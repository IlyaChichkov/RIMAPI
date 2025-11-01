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
        public float Hunger { get; set; }
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
        public float Sleep { get; set; }
        public float Comfort { get; set; }
        public float SurroundingBeauty { get; set; }
        public float FreshAir { get; set; }
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
        public int ChildrenCount { get; set; }
    }

    public class RelationDto
    {
        public string relationDefName { get; set; }
        public string otherPawnId { get; set; }
        public string otherPawnName { get; set; }
    }

    public class OpinionAboutPawnDto
    {
        public int Opinion { get; set; }
        public int OpinionAboutMe { get; set; }
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
        // Basic identification  
        public int LoadId { get; set; }
        public string DefName { get; set; }
        public string Label { get; set; }
        public string LabelCap { get; set; }
        public string LabelInBrackets { get; set; }

        // Severity and stage  
        public float Severity { get; set; }
        public string SeverityLabel { get; set; }
        public int CurStageIndex { get; set; }
        public string CurStageLabel { get; set; }

        // Body part information  
        public string PartLabel { get; set; }
        public string PartDefName { get; set; }

        // Age and timing  
        public int AgeTicks { get; set; }
        public string AgeString { get; set; }

        // Status flags  
        public bool Visible { get; set; }
        public bool IsPermanent { get; set; }
        public bool IsTended { get; set; }
        public bool TendableNow { get; set; }
        public bool Bleeding { get; set; }
        public float BleedRate { get; set; }
        public bool IsLethal { get; set; }
        public bool IsCurrentlyLifeThreatening { get; set; }
        public bool CanEverKill { get; set; }

        // Source information  
        public string SourceDefName { get; set; }
        public string SourceLabel { get; set; }
        public string SourceBodyPartGroupDefName { get; set; }
        public string SourceHediffDefName { get; set; }

        // Combat log  
        public string CombatLogText { get; set; }

        // Additional properties  
        public string TipStringExtra { get; set; }
        public float PainFactor { get; set; }
        public float PainOffset { get; set; }
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
