using System.Collections.Generic;
using RimWorld;

namespace RimworldRestApi.Models
{
    public class ColonistDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public float Health { get; set; }
        public float Mood { get; set; }
        public PositionDto Position { get; set; }
    }

    public class ColonistInventoryDto
    {
        public List<InventoryThingDto> Items { get; set; }
        public List<InventoryThingDto> Apparels { get; set; }
        public List<InventoryThingDto> Equipment { get; set; }
    }

    public class BodyPartsDto
    {
        public string BodyImage { get; set; }
        public string BodyColor { get; set; }
        public string HeadImage { get; set; }
        public string HeadColor { get; set; }
    }

    public class PositionDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }

    public class ColonistDetailedDto
    {
        public ColonistDto Colonist { get; set; }
        public ColonistWorkInfoDto ColonistWorkInfo { get; set; }
        public ColonistPoliciesInfoDto ColonistPoliciesInfo { get; set; }
        public ColonistMedicalInfoDto ColonistMedicalInfo { get; set; }
        public ColonistSocialInfoDto ColonistSocialInfo { get; set; }
    }

    public class ColonistWorkInfoDto
    {
        public List<SkillDto> Skills { get; set; }
        public string CurrentJob { get; set; }
        public List<string> Traits { get; set; }
        public List<WorkPriorityDto> WorkPriorities { get; set; }
    }

    public class ColonistPoliciesInfoDto
    {
        public int FoodPolicyId { get; set; }
        public int HostilityResponse { get; set; }
    }

    public class ColonistSocialInfoDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<RelationDto> DirectRelations { get; set; }
        public List<RelationDto> VirtualRelations { get; set; }
        public int ChildrenCount { get; set; }
    }

    public class RelationDto
    {
        public string relationDefName;
        public string otherPawnId;
        public string otherPawnName;
    }

    public class ColonistMedicalInfoDto
    {
        public float Health { get; set; }
        public List<HediffDto> Hediffs { get; set; }
        public int MedicalPolicyId { get; set; }
        public bool IsSelfTendAllowed { get; set; }
    }

    public class HediffDto
    {
        public string Part { get; set; }
        public string Label { get; set; }
    }

    public class SkillDto
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public string LevelDescriptor { get; set; }
        public bool PermanentlyDisabled { get; set; }
        public bool TotallyDisabled { get; set; }
        public float XpTotalEarned { get; set; }
        public float XpProgressPercent { get; set; }
        public float XpRequiredForLevelUp { get; set; }
        public int Aptitude { get; set; }
    }

    public class WorkPriorityDto
    {
        public string WorkType { get; set; }
        public int Priority { get; set; }
    }

    public class PawnWorkPrioritiesResponseDto
    {
        public List<PawnWorkPrioritiesDto> Pawns { get; set; } = new List<PawnWorkPrioritiesDto>();
        public int TotalPawns { get; set; }
        public string LastUpdated { get; set; }
    }

    public class PawnWorkPrioritiesDto
    {
        public int PawnId { get; set; }
        public string PawnName { get; set; }
        public List<WorkPriorityDto> WorkPriorities { get; set; } = new List<WorkPriorityDto>();
    }
}
