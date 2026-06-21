using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld;
using Verse;

namespace RIMAPI.Services
{
    public class PawnSocialService : IPawnSocialService
    {
        public ApiResult<List<InteractionDefDto>> GetInteractionDefs()
        {
            try
            {
                var defs = DefDatabase<InteractionDef>.AllDefsListForReading
                    .Select(d => new InteractionDefDto
                    {
                        DefName = d.defName,
                        Label = d.label,
                        Description = d.description
                    })
                    .ToList();
                return ApiResult<List<InteractionDefDto>>.Ok(defs);
            }
            catch (Exception ex)
            {
                return ApiResult<List<InteractionDefDto>>.Fail(ex.Message);
            }
        }

        public ApiResult<PawnInteractionStatusDto> GetPawnInteractionStatus(int pawnId)
        {
            try
            {
                var pawn = PawnHelper.FindPawnById(pawnId);
                if (pawn == null) return ApiResult<PawnInteractionStatusDto>.Fail($"Pawn {pawnId} not found.");

                var interactions = pawn.interactions;
                if (interactions == null) return ApiResult<PawnInteractionStatusDto>.Fail("Pawn has no interaction tracker.");

                var traverse = Traverse.Create(interactions);
                int lastInteractionTicks = traverse.Field("lastInteractionTime").GetValue<int>();

                bool tooRecently = interactions.InteractedTooRecentlyToInteract();
                int cooldownTicks = 0;
                if (tooRecently)
                {
                    // 120 ticks is a hardcoded cooldown in RimWorld's Pawn_InteractionsTracker.InteractedTooRecentlyToInteract()
                    cooldownTicks = (lastInteractionTicks + 120) - Find.TickManager.TicksGame;
                    if (cooldownTicks < 0) cooldownTicks = 0;
                }

                var status = new PawnInteractionStatusDto
                {
                    CanInteract = !tooRecently && pawn.Awake() && !pawn.Downed,
                    LastInteractionTicks = lastInteractionTicks,
                    CooldownTicks = cooldownTicks,
                    CooldownDays = GameTypesHelper.TicksToDays(cooldownTicks)
                };

                return ApiResult<PawnInteractionStatusDto>.Ok(status);
            }
            catch (Exception ex)
            {
                return ApiResult<PawnInteractionStatusDto>.Fail(ex.Message);
            }
        }
        public ApiResult<PawnInteractionLogDto> GetPawnInteractionLog(int pawnId, int limit = 50)
        {
            try
            {
                var pawn = PawnHelper.FindPawnById(pawnId);
                if (pawn == null)
                {
                    return ApiResult<PawnInteractionLogDto>.Fail($"Pawn {pawnId} not found.");
                }

                var playLog = Find.PlayLog;

                // Return a graceful empty DTO if the log is null (e.g., game just started)
                if (playLog == null || playLog.AllEntries == null)
                {
                    return ApiResult<PawnInteractionLogDto>.Ok(new PawnInteractionLogDto
                    {
                        PawnId = pawnId,
                        Interactions = new List<InteractionLogEntryDto>(),
                        Count = 0
                    });
                }

                var logEntries = new List<InteractionLogEntryDto>();

                // Cache reflection flags outside the loop for maximum performance
                var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

                var interactionType = typeof(PlayLogEntry_Interaction);
                var initField = interactionType.GetField("initiator", flags);
                var recField = interactionType.GetField("recipient", flags);
                var intDefField = interactionType.GetField("intDef", flags);

                var singleType = typeof(PlayLogEntry_InteractionSinglePawn);
                var sInitField = singleType.GetField("initiator", flags);
                var sIntDefField = singleType.GetField("intDef", flags);

                // Reflection check: If field resolution fails, RimWorld internals might have changed.
                if (initField == null || recField == null || intDefField == null || sInitField == null || sIntDefField == null)
                {
                    Log.WarningOnce("RIMAPI: Failed to resolve PlayLogEntry_Interaction fields. Interaction logs may be incomplete.", 1948301);
                }

                foreach (var entry in playLog.AllEntries)
                {
                    // Fast native check: skip logs that don't involve this pawn to save CPU
                    if (!entry.Concerns(pawn)) continue;

                    if (entry is PlayLogEntry_Interaction interaction)
                    {
                        Pawn initiator = initField?.GetValue(interaction) as Pawn;
                        Pawn recipient = recField?.GetValue(interaction) as Pawn;

                        bool isInit = initiator != null && initiator.thingIDNumber == pawnId;
                        bool isRec = recipient != null && recipient.thingIDNumber == pawnId;

                        if (isInit || isRec)
                        {
                            InteractionDef intDef = intDefField?.GetValue(interaction) as InteractionDef;
                            Thing povPawn = isInit ? initiator : recipient;

                            logEntries.Add(new InteractionLogEntryDto
                            {
                                InitiatorId = initiator?.thingIDNumber ?? 0,
                                InitiatorName = initiator?.LabelShort ?? "Unknown",
                                RecipientId = recipient?.thingIDNumber ?? 0,
                                RecipientName = recipient?.LabelShort ?? "Unknown",
                                InteractionDefName = intDef?.defName,
                                InteractionLabel = intDef?.label,
                                Text = entry.ToGameStringFromPOV(povPawn),
                                Ticks = entry.Tick,
                                TimeAgo = (Find.TickManager.TicksGame - entry.Tick).ToStringTicksToPeriod()
                            });
                        }
                    }
                    // Add back SinglePawn check for things like speeches or solo social interactions
                    else if (entry is PlayLogEntry_InteractionSinglePawn singleInteraction)
                    {
                        Pawn initiator = sInitField?.GetValue(singleInteraction) as Pawn;

                        if (initiator != null && initiator.thingIDNumber == pawnId)
                        {
                            InteractionDef intDef = sIntDefField?.GetValue(singleInteraction) as InteractionDef;

                            logEntries.Add(new InteractionLogEntryDto
                            {
                                InitiatorId = initiator.thingIDNumber,
                                InitiatorName = initiator.LabelShort,
                                RecipientId = 0,
                                RecipientName = "None",
                                InteractionDefName = intDef?.defName,
                                InteractionLabel = intDef?.label,
                                Text = entry.ToGameStringFromPOV(initiator),
                                Ticks = entry.Tick,
                                TimeAgo = (Find.TickManager.TicksGame - entry.Tick).ToStringTicksToPeriod()
                            });
                        }
                    }
                }

                var orderedInteractions = logEntries.OrderByDescending(e => e.Ticks).Take(limit).ToList();

                var resultDto = new PawnInteractionLogDto
                {
                    PawnId = pawnId,
                    Interactions = orderedInteractions,
                    Count = orderedInteractions.Count
                };

                return ApiResult<PawnInteractionLogDto>.Ok(resultDto);
            }
            catch (Exception ex)
            {
                return ApiResult<PawnInteractionLogDto>.Fail(ex.Message);
            }
        }

        public ApiResult<PawnRelationsDto> GetPawnRelations(int pawnId)
        {
            try
            {
                var pawn = PawnHelper.FindPawnById(pawnId);
                if (pawn == null) return ApiResult<PawnRelationsDto>.Fail($"Pawn {pawnId} not found.");

                var result = new PawnRelationsDto
                {
                    PawnId = pawn.thingIDNumber,
                    Relations = new List<PawnRelationEntryDto>()
                };

                if (pawn.relations != null)
                {
                    // 1. Cast a wider net to bypass cache limitations for Modded/DLC pets
                    var pawnsToCheck = new HashSet<Pawn>();

                    // Add explicitly cached family/social relations
                    pawnsToCheck.UnionWith(pawn.relations.RelatedPawns);

                    // Add everyone on the current map to catch missing Familiars and standard Friends
                    if (pawn.Map != null)
                    {
                        foreach (var p in pawn.Map.mapPawns.AllPawns)
                        {
                            if (p != pawn) pawnsToCheck.Add(p);
                        }
                    }

                    // 2. Evaluate relationships for everyone in our net
                    foreach (var otherPawn in pawnsToCheck)
                    {
                        // A: Explicit Relations (Brother, Ex-Lover, Bonded, Familiar)
                        foreach (PawnRelationDef relDef in pawn.GetRelations(otherPawn))
                        {
                            result.Relations.Add(new PawnRelationEntryDto
                            {
                                OtherPawnId = otherPawn.thingIDNumber,
                                OtherPawnName = otherPawn.LabelShort,
                                RelationDefName = relDef.defName,
                                RelationLabel = relDef.GetGenderSpecificLabel(otherPawn)
                            });
                        }

                        // B: Implicit Social Situations (Friend / Rival)
                        // Animals don't have opinions of colonists in vanilla, so we ensure both are humanlike
                        if (pawn.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike)
                        {
                            int opinion = pawn.relations.OpinionOf(otherPawn);

                            if (opinion >= 20)
                            {
                                result.Relations.Add(new PawnRelationEntryDto
                                {
                                    OtherPawnId = otherPawn.thingIDNumber,
                                    OtherPawnName = otherPawn.LabelShort,
                                    RelationDefName = "Friend",
                                    RelationLabel = "Friend".Translate()
                                });
                            }
                            else if (opinion <= -20)
                            {
                                result.Relations.Add(new PawnRelationEntryDto
                                {
                                    OtherPawnId = otherPawn.thingIDNumber,
                                    OtherPawnName = otherPawn.LabelShort,
                                    RelationDefName = "Rival",
                                    RelationLabel = "Rival".Translate()
                                });
                            }
                        }
                    }
                }

                return ApiResult<PawnRelationsDto>.Ok(result);
            }
            catch (Exception ex)
            {
                return ApiResult<PawnRelationsDto>.Fail(ex.Message);
            }
        }

        public ApiResult<List<PawnOpinionDto>> GetPawnOpinions(int pawnId)
        {
            try
            {
                var pawn = PawnHelper.FindPawnById(pawnId);
                if (pawn == null) return ApiResult<List<PawnOpinionDto>>.Fail($"Pawn {pawnId} not found.");

                var opinions = new List<PawnOpinionDto>();
                var allPawns = ColonistsHelper.GetColonistsList();

                foreach (var otherPawn in allPawns)
                {
                    if (otherPawn == pawn) continue;

                    var opinionDto = new PawnOpinionDto
                    {
                        TargetPawnId = otherPawn.thingIDNumber,
                        TargetPawnName = otherPawn.LabelShort,
                        Opinion = pawn.relations.OpinionOf(otherPawn),
                        Breakdown = new List<OpinionBreakdownDto>()
                    };

                    // Get thoughts breakdown
                    if (pawn.needs?.mood?.thoughts != null)
                    {
                        List<ISocialThought> socialThoughts = new List<ISocialThought>();
                        pawn.needs.mood.thoughts.GetSocialThoughts(otherPawn, socialThoughts);

                        foreach (var st in socialThoughts)
                        {
                            if (st is Thought_SituationalSocial ss)
                            {
                                if (ss.OtherPawn() == otherPawn)
                                {
                                    opinionDto.Breakdown.Add(new OpinionBreakdownDto
                                    {
                                        ThoughtDefName = ss.def.defName,
                                        Label = ss.LabelCap,
                                        Score = ss.OpinionOffset()
                                    });
                                }
                            }
                        }

                        // Memories
                        foreach (var memory in pawn.needs.mood.thoughts.memories.Memories)
                        {
                            if (memory is Thought_MemorySocial ms)
                            {
                                if (ms.OtherPawn() == otherPawn)
                                {
                                    opinionDto.Breakdown.Add(new OpinionBreakdownDto
                                    {
                                        ThoughtDefName = ms.def.defName,
                                        Label = ms.LabelCap,
                                        Score = ms.OpinionOffset()
                                    });
                                }
                            }
                        }
                    }

                    opinions.Add(opinionDto);
                }

                return ApiResult<List<PawnOpinionDto>>.Ok(opinions);
            }
            catch (Exception ex)
            {
                return ApiResult<List<PawnOpinionDto>>.Fail(ex.Message);
            }
        }

        public ApiResult ForceInteraction(ForceInteractionRequestDto request)
        {
            try
            {
                var initiator = PawnHelper.FindPawnById(request.InitiatorId);
                if (initiator == null) return ApiResult.Fail($"Initiator {request.InitiatorId} not found.");

                var recipient = PawnHelper.FindPawnById(request.RecipientId);
                if (recipient == null) return ApiResult.Fail($"Recipient {request.RecipientId} not found.");

                var intDef = DefDatabase<InteractionDef>.GetNamed(request.InteractionDefName, false);
                if (intDef == null) return ApiResult.Fail($"InteractionDef {request.InteractionDefName} not found.");

                bool success = initiator.interactions.TryInteractWith(recipient, intDef);

                if (success)
                    return ApiResult.Ok();
                else
                    return ApiResult.Fail("Interaction failed (pawns might be too far, unconscious, or busy).");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult AddRelation(AddRelationRequestDto request)
        {
            try
            {
                var pawn1 = PawnHelper.FindPawnById(request.Pawn1Id);
                if (pawn1 == null) return ApiResult.Fail($"Pawn {request.Pawn1Id} not found.");

                var pawn2 = PawnHelper.FindPawnById(request.Pawn2Id);
                if (pawn2 == null) return ApiResult.Fail($"Pawn {request.Pawn2Id} not found.");

                var relDef = DefDatabase<PawnRelationDef>.GetNamed(request.RelationDefName, false);
                if (relDef == null) return ApiResult.Fail($"PawnRelationDef {request.RelationDefName} not found.");

                pawn1.relations.AddDirectRelation(relDef, pawn2);
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult RemoveRelation(RemoveRelationRequestDto request)
        {
            try
            {
                var pawn1 = PawnHelper.FindPawnById(request.Pawn1Id);
                if (pawn1 == null) return ApiResult.Fail($"Pawn {request.Pawn1Id} not found.");

                var pawn2 = PawnHelper.FindPawnById(request.Pawn2Id);
                if (pawn2 == null) return ApiResult.Fail($"Pawn {request.Pawn2Id} not found.");

                var relDef = DefDatabase<PawnRelationDef>.GetNamed(request.RelationDefName, false);
                if (relDef == null) return ApiResult.Fail($"PawnRelationDef {request.RelationDefName} not found.");

                if (pawn1.relations.DirectRelationExists(relDef, pawn2))
                {
                    pawn1.relations.RemoveDirectRelation(relDef, pawn2);
                    return ApiResult.Ok();
                }
                else
                {
                    return ApiResult.Fail("Relation does not exist.");
                }
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }
    }
}
