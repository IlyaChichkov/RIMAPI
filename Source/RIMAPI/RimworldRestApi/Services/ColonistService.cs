using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RIMAPI.Services
{
    public class ColonistService : IColonistService
    {
        public ColonistService() { }

        public ApiResult<ColonistDto> GetColonist(int pawnId)
        {
            var allColonists = ColonistsHelper
                .GetColonists();

            try
            {
                var result = allColonists
                    .First(c => c.Id == pawnId);
                return ApiResult<ColonistDto>.Ok(result);
            }
            catch (Exception ex)
            {
                return ApiResult<ColonistDto>
                    .Fail($"Failed to find pawn with id: {pawnId} - {ex.Message}");
            }
        }

        public ApiResult<BodyPartsDto> GetColonistBodyParts(int pawnId)
        {
            BodyPartsDto bodyParts = new BodyPartsDto();

            try
            {
                Pawn colonist = PawnsFinder
                    .AllMaps_FreeColonists.Where(p => p.thingIDNumber == pawnId)
                    .FirstOrDefault();

                Material bodyMaterial = colonist.Drawer.renderer.BodyGraphic.MatAt(Rot4.South);
                Texture2D bodyTexture = (Texture2D)bodyMaterial.mainTexture;

                Material headMaterial = colonist.Drawer.renderer.HeadGraphic.MatAt(Rot4.South);
                Texture2D headTexture = (Texture2D)headMaterial.mainTexture;

                bodyParts.BodyImage = TextureHelper.TextureToBase64(bodyTexture);
                bodyParts.BodyColor = bodyMaterial.color.ToString();
                bodyParts.HeadImage = TextureHelper.TextureToBase64(headTexture);
                bodyParts.HeadColor = headMaterial.color.ToString();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error getting body image - {ex.Message}");
            }
            return ApiResult<BodyPartsDto>.Ok(bodyParts);
        }

        public ApiResult<ColonistDetailedDto> GetColonistDetailed(int pawnId)
        {
            var allColonists = ColonistsHelper
                .GetColonistsDetailed();

            try
            {
                var result = allColonists
                    .First(c => c.Colonist.Id == pawnId);
                return ApiResult<ColonistDetailedDto>.Ok(result);
            }
            catch (Exception ex)
            {
                return ApiResult<ColonistDetailedDto>
                    .Fail($"Failed to find pawn with id: {pawnId} - {ex.Message}");
            }
        }

        public ApiResult<ColonistInventoryDto> GetColonistInventory(int pawnId)
        {
            Pawn colonist = PawnsFinder
                .AllMaps_FreeColonists.Where(p => p.thingIDNumber == pawnId)
                .FirstOrDefault();

            try
            {
                List<ThingDto> Items = new List<ThingDto>();
                List<ThingDto> Apparels = new List<ThingDto>();
                List<ThingDto> Equipment = new List<ThingDto>();

                foreach (var item in colonist.inventory.innerContainer)
                {
                    Items.Add(ResourcesHelper.ThingToDto(item));
                }

                foreach (var apparel in colonist.apparel.WornApparel)
                {
                    Items.Add(ResourcesHelper.ThingToDto(apparel));
                }

                foreach (var equipment in colonist.equipment.AllEquipmentListForReading)
                {
                    Items.Add(ResourcesHelper.ThingToDto(equipment));
                }

                var result = new ColonistInventoryDto
                {
                    Items = Items,
                    Apparels = Apparels,
                    Equipment = Equipment,
                };
                return ApiResult<ColonistInventoryDto>.Ok(result);
            }
            catch (Exception ex)
            {
                return ApiResult<ColonistInventoryDto>.Fail(ex.Message);
            }
        }

        public ApiResult<List<ColonistDto>> GetColonists()
        {
            var result = ColonistsHelper.GetColonists();
            return ApiResult<List<ColonistDto>>.Ok(result);
        }

        public ApiResult<List<PawnPositionDto>> GetColonistPositions()
        {
            var result = ColonistsHelper.GetColonistPositions();
            return ApiResult<List<PawnPositionDto>>.Ok(result);
        }

        public ApiResult<List<ColonistDetailedDto>> GetColonistsDetailed()
        {
            var result = ColonistsHelper.GetColonistsDetailed();
            return ApiResult<List<ColonistDetailedDto>>.Ok(result);
        }

        public ApiResult<OpinionAboutPawnDto> GetOpinionAboutPawn(int pawnId, int otherPawnId)
        {
            Pawn pawn = PawnsFinder
                .AllMaps_FreeColonists.Where(p => p.thingIDNumber == pawnId)
                .FirstOrDefault();
            if (pawn == null)
                return ApiResult<OpinionAboutPawnDto>.Fail("Failed to find pawn by id");

            Pawn other = PawnsFinder
                .AllMaps_FreeColonists.Where(p => p.thingIDNumber == otherPawnId)
                .FirstOrDefault();
            if (other == null)
                return ApiResult<OpinionAboutPawnDto>.Fail("Failed to find other pawn by id");

            var result = new OpinionAboutPawnDto
            {
                Opinion = pawn.relations.OpinionOf(other),
                OpinionAboutMe = other.relations.OpinionOf(pawn),
            };
            return ApiResult<OpinionAboutPawnDto>.Ok(result);
        }

        public ApiResult<List<OutfitDto>> GetOutfits()
        {
            var result = ColonistsHelper.GetOutfits();
            return ApiResult<List<OutfitDto>>.Ok(result);
        }

        public ApiResult<ImageDto> GetPawnPortraitImage(
            int pawnId,
            int width,
            int height,
            string direction
        )
        {
            Pawn pawn = ColonistsHelper.GetPawnById(pawnId);
            var result = TextureHelper.GetPawnPortraitImage(pawn, width, height, direction);
            return ApiResult<ImageDto>.Ok(result);
        }

        public ApiResult<List<TimeAssignmentDto>> GetTimeAssignmentsList()
        {
            var result = DefDatabase<TimeAssignmentDef>
                .AllDefs.Select(s => new TimeAssignmentDto { Name = s.defName })
                .ToList();
            return ApiResult<List<TimeAssignmentDto>>.Ok(result);
        }

        public ApiResult<TraitDefDto> GetTraitDefDto(string traitName)
        {
            TraitDef trait = DefDatabase<TraitDef>.GetNamed(traitName, false);
            var result = DefDatabaseHelper.GetTraitDefDto(trait);
            return ApiResult<TraitDefDto>.Ok(result);
        }

        public ApiResult<WorkListDto> GetWorkList()
        {
            WorkListDto workList = new WorkListDto { Work = new List<string>() };

            foreach (WorkTypeDef workType in DefDatabase<WorkTypeDef>.AllDefs)
            {
                if (workType == null)
                    continue;
                workList.Work.Add(workType.defName);
            }

            return ApiResult<WorkListDto>.Ok(workList);
        }

        public ApiResult MakeJobEquip(int mapId, int pawnId, int equipmentId)
        {
            try
            {
                Map map = MapHelper.GetMapByID(mapId);
                if (map == null)
                {
                    return ApiResult.Fail($"Map with ID={mapId} not found");
                }
                Pawn pawn = map
                    .listerThings.AllThings.OfType<Pawn>()
                    .FirstOrDefault(p => p.thingIDNumber == pawnId);
                if (pawn == null)
                {
                    return ApiResult.Fail($"Pawn with ID={pawnId} not found");
                }

                Thing foundThing = map.listerThings.AllThings.FirstOrDefault(t =>
                    t.thingIDNumber == equipmentId
                );
                if (foundThing == null)
                {
                    return ApiResult.Fail($"Thing with ID={equipmentId} not found");
                }

                Job job = null;
                try
                {
                    if (EquipmentUtility.CanEquip(foundThing, pawn) == true)
                    {
                        job = JobMaker.MakeJob(JobDefOf.Equip, foundThing);
                    }

                    if (ApparelUtility.HasPartsToWear(pawn, foundThing.def) == true)
                    {
                        job = JobMaker.MakeJob(JobDefOf.Wear, foundThing);
                    }
                }
                catch
                {
                    return ApiResult.Fail($"Failed to determine equip job for item: {foundThing.def.defName}");
                }

                if (job == null)
                {
                    return ApiResult.Fail($"Failed to make a job for item: {foundThing.def.defName}");
                }

                bool result = pawn.jobs.TryTakeOrderedJob(job);
                if (!result)
                {
                    return ApiResult.Fail($"Failed to assign job to pawn: {pawn.thingIDNumber}");
                }
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult SetColonistsWorkPriority(ColonistsWorkPrioritiesRequestDto body)
        {
            try
            {
                var warnings = new List<string>();
                foreach (var colonistPriorityUpdate in body.Priorities)
                {
                    var result = SetColonistWorkPriority(colonistPriorityUpdate);
                    if (result.Success == false)
                    {
                        warnings.Add(result.Errors.ToString());
                    }
                }

                if (warnings.Count > 0)
                {
                    if (warnings.Count == body.Priorities.Count)
                    {
                        return ApiResult.Fail(warnings.ToString());
                    }
                    return ApiResult.Partial(warnings);
                }
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult SetColonistWorkPriority(WorkPriorityRequestDto body)
        {
            try
            {
                // Find the pawn by thingIDNumber
                Pawn pawn = ColonistsHelper.GetPawnById(body.Id);
                if (pawn == null)
                {
                    return ApiResult.Fail($"Could not find pawn with ID {body.Id}");
                }

                // Find the WorkTypeDef by defName
                WorkTypeDef workTypeDef = DefDatabase<WorkTypeDef>.GetNamedSilentFail(body.Work);
                if (workTypeDef == null)
                {
                    return ApiResult.Fail($"Could not find WorkTypeDef with defName {body.Work}");
                }

                // Check if pawn has work settings initialized
                if (pawn.workSettings == null || !pawn.workSettings.EverWork)
                {
                    return ApiResult.Fail(
                        $"Pawn {pawn.LabelShort} does not have work settings initialized"
                    );
                }

                // Check if the work type is disabled for this pawn
                if (body.Priority != 0 && pawn.WorkTypeIsDisabled(workTypeDef))
                {
                    return ApiResult.Fail(
                        $"Cannot set priority for disabled work type {workTypeDef.defName} on pawn {pawn.LabelShort}"
                    );
                }

                // Validate priority range (0-9)
                if (body.Priority < 0 || body.Priority > 9)
                {
                    return ApiResult.Fail($"Invalid priority {body.Priority}. Must be between 0 and 4");
                }

                // Set the priority
                pawn.workSettings.SetPriority(workTypeDef, body.Priority);

                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult SetTimeAssignment(PawnTimeAssignmentRequestDto body)
        {
            try
            {
                Pawn pawn = ColonistsHelper.GetPawnById(body.PawnId);
                TimeAssignmentDef assignmentDef = DefDatabase<TimeAssignmentDef>
                    .AllDefs.Where(p => p.defName.ToLower() == body.Assignment.ToLower())
                    .FirstOrDefault();
                if (assignmentDef == null)
                {
                    return ApiResult.Fail(
                        $"Failed to find assignment def with {body.Assignment} name"
                    );
                }
                pawn.timetable.SetAssignment(body.Hour, assignmentDef);

                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }
    }
}
