using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Models;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RIMAPI.Services
{
    public class FactionService : IFactionService
    {
        public FactionService() { }

        public ApiResult<FactionDto> GetFaction(int id)
        {
            try
            {
                Faction faction = GetFactionById(id);
                return ApiResult<FactionDto>.Ok(FactionDto.ToDto(faction));
            }
            catch (Exception ex)
            {
                return ApiResult<FactionDto>.Fail(ex.Message);
            }
        }

        public ApiResult<FactionDefDto> GetFactionDef(string defName)
        {
            try
            {
                var result = FactionDefDto.FromFactionDef(
                    DefDatabase<FactionDef>.GetNamed(defName)
                );
                return ApiResult<FactionDefDto>.Ok(result);
            }
            catch (Exception ex)
            {
                return ApiResult<FactionDefDto>.Fail(ex.Message);
            }
        }

        private FactionRelationDto GetFactionRelationWithDto(int id, int otherId)
        {
            Faction faction = GetFactionById(id);
            Faction otherFaction = GetFactionById(otherId);
            FactionRelation relation = faction.RelationWith(otherFaction);
            string relationKind = "Unknown";

            switch (relation.kind)
            {
                case FactionRelationKind.Hostile:
                    relationKind = "Hostile";
                    break;
                case FactionRelationKind.Neutral:
                    relationKind = "Neutral";
                    break;
                case FactionRelationKind.Ally:
                    relationKind = "Ally";
                    break;
            }
            return new FactionRelationDto
            {
                Goodwill = relation.baseGoodwill,
                RelationKind = relationKind,
            };
        }

        public ApiResult<FactionRelationDto> GetFactionRelationWith(int id, int otherId)
        {
            try
            {
                return ApiResult<FactionRelationDto>.Ok(GetFactionRelationWithDto(id, otherId));
            }
            catch (Exception ex)
            {
                return ApiResult<FactionRelationDto>.Fail(ex.Message);
            }
        }

        public ApiResult<FactionChangeRelationResponceDto> ChangeFactionRelationWith(
            int id,
            int otherId,
            int goodwillChange,
            bool sendMessage,
            bool canSendHostilityLetter
        )
        {
            try
            {
                Faction faction = GetFactionById(id);
                Faction otherFaction = GetFactionById(otherId);

                if (!faction.CanChangeGoodwillFor(otherFaction, goodwillChange))
                {
                    List<string> reasons = new List<string>();
                    if (!faction.HasGoodwill)
                    {
                        reasons.Add("Faction hasn't Goodwill");
                    }
                    if (!otherFaction.HasGoodwill)
                    {
                        reasons.Add("Other faction hasn't Goodwill");
                    }
                    if (faction.def.permanentEnemy)
                    {
                        reasons.Add("Permanent enemy");
                    }
                    if (otherFaction.def.permanentEnemy)
                    {
                        reasons.Add("Other faction is permanent enemy");
                    }
                    if (faction.defeated)
                    {
                        reasons.Add("Faction is defeated");
                    }
                    if (otherFaction.defeated)
                    {
                        reasons.Add("Other faction is defeated");
                    }
                    if (faction == otherFaction)
                    {
                        reasons.Add("Other faction is equal to faction");
                    }
                    if (
                        goodwillChange > 0
                        && (
                            (
                                faction.IsPlayer
                                && SettlementUtility.IsPlayerAttackingAnySettlementOf(otherFaction)
                            )
                            || (
                                otherFaction.IsPlayer
                                && SettlementUtility.IsPlayerAttackingAnySettlementOf(faction)
                            )
                        )
                    )
                    {
                        reasons.Add("Attacking settlement");
                    }
                    if (QuestUtility.IsGoodwillLockedByQuest(faction, otherFaction))
                    {
                        reasons.Add("Locked by quest");
                    }

                    string formattedReasons = $"[{string.Join(", ", reasons)}]";
                    return ApiResult<FactionChangeRelationResponceDto>.Fail(
                        $"Can't change goodwill of {faction.Name} towards {otherFaction.Name}. Reasons: {formattedReasons}"
                    );
                }

                if (faction.TryAffectGoodwillWith(otherFaction, goodwillChange, sendMessage))
                {
                    FactionRelation relation = faction.RelationWith(otherFaction);
                    FactionChangeRelationResponceDto responce = new FactionChangeRelationResponceDto
                    {
                        Id = id,
                        OtherId = otherId,
                        Value = relation.baseGoodwill,
                    };
                    return ApiResult<FactionChangeRelationResponceDto>.Ok(responce);
                }
                return ApiResult<FactionChangeRelationResponceDto>.Fail(
                    "Try to affect goodwill has failed"
                );
            }
            catch (Exception ex)
            {
                return ApiResult<FactionChangeRelationResponceDto>.Fail(ex.Message);
            }
        }

        public ApiResult<List<FactionsDto>> GetFactions()
        {
            List<FactionsDto> factions = new List<FactionsDto>();
            try
            {
                Faction playerFaction = Find.FactionManager?.OfPlayer;

                if (Current.ProgramState != ProgramState.Playing || Find.FactionManager == null)
                {
                    return ApiResult<List<FactionsDto>>.Fail(
                        "Current program state is not \"Playing\""
                    );
                }

                factions = Find
                    .FactionManager.AllFactionsListForReading.Select(f => new FactionsDto
                    {
                        LoadId = f.loadID,
                        DefName = f.def?.defName,
                        Name = f.Name,
                        IsPlayer = f.IsPlayer,
                        Relation = f.IsPlayer
                            ? string.Empty
                            : (
                                playerFaction != null
                                    ? playerFaction.RelationKindWith(f).ToString()
                                    : string.Empty
                            ),
                        Goodwill = f.IsPlayer ? 0 : (playerFaction?.GoodwillWith(f) ?? 0),
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                return ApiResult<List<FactionsDto>>.Fail(ex.Message);
            }
            return ApiResult<List<FactionsDto>>.Ok(factions);
        }

        private Faction GetFactionById(int id)
        {
            return Find.FactionManager.AllFactionsListForReading.FirstOrDefault(s =>
                s.loadID == id
            );
        }

        public ApiResult<FactionRelationsDto> GetFactionRelations(int id)
        {
            try
            {
                var relations = Find
                    .FactionManager.AllFactionsListForReading.Where(f => f.loadID != id)
                    .ToDictionary(f => f.loadID, f => GetFactionRelationWithDto(id, f.loadID));

                FactionRelationsDto factionRelations = new FactionRelationsDto
                {
                    Id = id,
                    Relations = relations,
                };
                return ApiResult<FactionRelationsDto>.Ok(factionRelations);
            }
            catch (Exception ex)
            {
                return ApiResult<FactionRelationsDto>.Fail(ex.Message);
            }
        }

        public ApiResult<FactionDto> GetPlayerFaction()
        {
            try
            {
                var playerFaction = Find
                    .FactionManager.AllFactionsListForReading.Where(s => s.IsPlayer == true)
                    .FirstOrDefault();
                if (playerFaction == null)
                {
                    return ApiResult<FactionDto>.Fail("Couldn't find player faction");
                }

                return ApiResult<FactionDto>.Ok(FactionDto.ToDto(playerFaction));
            }
            catch (Exception ex)
            {
                return ApiResult<FactionDto>.Fail(ex.Message);
            }
        }
    }
}
