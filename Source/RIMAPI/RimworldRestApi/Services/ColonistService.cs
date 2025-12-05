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

        public ApiResult EditPawn(PawnEditRequest request)
        {
            return ApiResult.Unimplemented();
        }

        public ApiResult<ColonistDto> GetColonist(int pawnId)
        {
            var result = ColonistsHelper.GetColonists().FirstOrDefault(c => c.Id == pawnId);
            return ApiResult<ColonistDto>.Ok(result);
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
            var result = ColonistsHelper
                .GetColonistsDetailed()
                .FirstOrDefault(c => c.Colonist.Id == pawnId);
            return ApiResult<ColonistDetailedDto>.Ok(result);
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
            var result = DefHelper.GetTraitDefDto(trait);
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

        public ApiResult MakeJobEquip(int mapId, int pawnId, int equipmentId, string equipmentType)
        {
            try
            {
                Map map = MapHelper.GetMapByID(mapId);
                if (map == null)
                {
                    throw new Exception($"Map with ID={mapId} not found");
                }
                Pawn pawn = map
                    .listerThings.AllThings.OfType<Pawn>()
                    .FirstOrDefault(p => p.thingIDNumber == pawnId);
                if (pawn == null)
                {
                    throw new Exception($"Pawn with ID={pawnId} not found");
                }

                Thing foundThing = map.listerThings.AllThings.FirstOrDefault(t =>
                    t.thingIDNumber == equipmentId
                );
                if (foundThing == null)
                {
                    throw new Exception($"Thing with ID={equipmentId} not found");
                }

                Job job = null;
                switch (equipmentType)
                {
                    case "weapon":
                        if (EquipmentUtility.CanEquip(foundThing, pawn) == false)
                        {
                            throw new Exception($"Can't equip this weapon");
                        }

                        job = JobMaker.MakeJob(JobDefOf.Equip, foundThing);
                        break;
                    case "apparel":
                        if (ApparelUtility.HasPartsToWear(pawn, foundThing.def) == false)
                        {
                            throw new Exception($"Can't equip this apparel");
                        }

                        job = JobMaker.MakeJob(JobDefOf.Wear, foundThing);
                        break;
                }

                if (job == null)
                {
                    throw new Exception($"Failed to make a job");
                }

                bool result = pawn.jobs.TryTakeOrderedJob(job);
                if (!result)
                {
                    throw new Exception($"Failed to assign job to pawn");
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
                foreach (var item in body.Priorities)
                {
                    var result = SetColonistWorkPriority(item.Id, item.Work, item.Priority);
                    if (result.Success == false)
                    {
                        return result;
                    }
                }
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult SetColonistWorkPriority(int pawnId, string workDef, int priority)
        {
            try
            {
                // Find the pawn by thingIDNumber
                Pawn pawn = ColonistsHelper.GetPawnById(pawnId);
                if (pawn == null)
                {
                    throw new Exception($"Could not find pawn with ID {pawnId}");
                }

                // Find the WorkTypeDef by defName
                WorkTypeDef workTypeDef = DefDatabase<WorkTypeDef>.GetNamedSilentFail(workDef);
                if (workTypeDef == null)
                {
                    throw new Exception($"Could not find WorkTypeDef with defName {workDef}");
                }

                // Check if pawn has work settings initialized
                if (pawn.workSettings == null || !pawn.workSettings.EverWork)
                {
                    throw new Exception(
                        $"Pawn {pawn.LabelShort} does not have work settings initialized"
                    );
                }

                // Check if the work type is disabled for this pawn
                if (priority != 0 && pawn.WorkTypeIsDisabled(workTypeDef))
                {
                    throw new Exception(
                        $"Cannot set priority for disabled work type {workTypeDef.defName} on pawn {pawn.LabelShort}"
                    );
                }

                // Validate priority range (0-9)
                if (priority < 0 || priority > 9)
                {
                    throw new Exception($"Invalid priority {priority}. Must be between 0 and 4");
                }

                // Set the priority
                pawn.workSettings.SetPriority(workTypeDef, priority);

                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult SetTimeAssignment(int pawnId, int hour, string assignmentName)
        {
            try
            {
                Pawn pawn = ColonistsHelper.GetPawnById(pawnId);
                TimeAssignmentDef assignmentDef = DefDatabase<TimeAssignmentDef>
                    .AllDefs.Where(p => p.defName.ToLower() == assignmentName.ToLower())
                    .FirstOrDefault();
                if (assignmentDef == null)
                {
                    throw new Exception(
                        $"Failed to find assignment def with {assignmentName} name"
                    );
                }
                pawn.timetable.SetAssignment(hour, assignmentDef);

                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }
    }
}
