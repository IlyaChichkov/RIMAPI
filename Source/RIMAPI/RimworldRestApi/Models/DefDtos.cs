using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace RIMAPI.Models
{
    public class AllDefsRequestDto
    {
        public List<string> Filters { get; set; }
    }

    public class DefsDto
    {
        public List<ThingDefDto> ThingsDefs { get; set; }
        public List<IncidentDefDto> IncidentsDefs { get; set; }
        public List<GameConditionDefDto> ConditionsDefs { get; set; }
        public List<PawnKindDefDto> PawnKindDefs { get; set; }
        public List<TraitDefDto> TraitDefs { get; set; }
        public List<ResearchProjectDefDto> ResearchDefs { get; set; }
        public List<HediffDefDto> HediffDefs { get; set; }
        public List<SkillDefDto> SkillDefs { get; set; }
        public List<WorkTypeDefDto> WorkTypeDefs { get; set; }
        public List<NeedDefDto> NeedDefs { get; set; }
        public List<ThoughtDefDto> ThoughtDefs { get; set; }
        public List<StatDefDto> StatDefs { get; set; }
        public List<WorldObjectDefDto> WorldObjectDefs { get; set; }
        public List<BiomeDefDto> BiomeDefs { get; set; }
        public List<TerrainDefDto> TerrainDefs { get; set; }
        public List<RecipeDefDto> RecipeDefs { get; set; }
        public List<BodyDefDto> BodyDefs { get; set; }
        public List<BodyPartDefDto> BodyPartDefs { get; set; }
        public List<FactionDefDto> FactionDefs { get; set; }
        public List<SoundDefDto> SoundDefs { get; set; }
        public List<DesignationCategoryDefDto> DesignationCategoryDefs { get; set; }
        public List<JoyKindDefDto> JoyKindDefs { get; set; }
        public List<MemeDefDto> MemeDefs { get; set; }
        public List<PreceptDefDto> PreceptDefs { get; set; }
        public List<AbilityDefDto> AbilityDefs { get; set; }
        public List<GeneDefDto> GeneDefs { get; set; }
        public List<WeatherDefDto> WeatherDefs { get; set; }
        public List<RoomRoleDefDto> RoomRoleDefs { get; set; }
        public List<RoomStatDefDto> RoomStatDefs { get; set; }
        public List<MentalStateDefDto> MentalStateDefs { get; set; }
        public List<DrugPolicyDefDto> DrugPolicyDefs { get; set; }
        public List<PlantDefDto> PlantDefs { get; set; }
        public List<AnimalDefDto> AnimalDefs { get; set; }
    }

    public class ThingDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ThingClass { get; set; }
        public Dictionary<string, float> StatBase { get; set; }
        public List<ThingCostDto> CostList { get; set; }
        public bool IsWeapon { get; set; }
        public bool IsApparel { get; set; }
        public bool IsItem { get; set; }
        public bool IsPawn { get; set; }
        public bool IsPlant { get; set; }
        public bool IsBuilding { get; set; }
        public bool IsMedicine { get; set; }
        public bool IsDrug { get; set; }
        public float MarketValue { get; set; }
        public float Mass { get; set; }
        public float MaxHitPoints { get; set; }
        public float Flammability { get; set; }
        public int StackLimit { get; set; }
        public float Nutrition { get; set; }
        public float WorkToMake { get; set; }
        public float WorkToBuild { get; set; }
        public float Beauty { get; set; }
        public string TechLevel { get; set; }
        public List<string> TradeTags { get; set; }
        public List<string> StuffCategories { get; set; }
        public float MaxHealth { get; set; }
        public float ArmorRating_Sharp { get; set; }
        public float ArmorRating_Blunt { get; set; }
        public float ArmorRating_Heat { get; set; }
        public float Insulation_Cold { get; set; }
        public float Insulation_Heat { get; set; }
    }

    public class ThingCostDto
    {
        public string ThingDef { get; set; }
        public int Count { get; set; }
    }

    public class PawnKindDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Race { get; set; }
        public float CombatPower { get; set; }
        public List<string> WeaponTags { get; set; }
        public List<string> ApparelTags { get; set; }
        public float BaseHealthScale { get; set; }
        public float BaseBodySize { get; set; }
        public List<string> FactionTags { get; set; }
    }

    public class TraitDegreeDataDto
    {
        public int Degree { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public Dictionary<string, float> StatOffsets { get; set; }
        public float SocialFightChanceFactor { get; set; }
        public float ForcedMentalStateMtbDays { get; set; }
        public float HungerRateFactor { get; set; }
    }

    public class ResearchProjectDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public float Cost { get; set; }
        public List<string> Prerequisites { get; set; }
        public string RequiredResearchBuilding { get; set; }
        public string TechLevel { get; set; }
        public string Tab { get; set; }
        public bool Hidden { get; set; }
        public List<string> UnlockedDefs { get; set; }
    }

    public class HediffDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string HediffClass { get; set; }
        public bool IsAddiction { get; set; }
        public bool MakesSickThought { get; set; }
        public float SeverityPerDay { get; set; }
        public float MaxSeverity { get; set; }
        public bool Tendable { get; set; }
        public bool IsBad { get; set; }
        public List<HediffStageDto> Stages { get; set; }
        public string CompProps { get; set; }
    }

    public class HediffStageDto
    {
        public float MinSeverity { get; set; }
        public string Label { get; set; }
        public List<string> BecomeImmuneTo { get; set; }
        public float DeathMtbDays { get; set; }
        public float PainFactor { get; set; }
        public float ForgetMemoryThoughtMtbDays { get; set; }
        public float VomitMtbDays { get; set; }
        public float MentalBreakMtbDays { get; set; }
    }

    public class SkillDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public bool PassionSusceptible { get; set; }
        public string DisablingWorkTags { get; set; }
    }

    public class WorkTypeDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string WorkTags { get; set; }
        public List<string> RelevantSkills { get; set; }
        public int NaturalPriority { get; set; }
        public bool VisibleOnlyWithWork { get; set; }
    }

    public class NeedDefDto
    {
        public string DefName { get; set; }
        public string NeedClassName { get; set; }
        public bool FreezeInMentalState { get; set; }
        public bool FreezeWhileSleeping { get; set; }
        public float SeekerFallPerHour { get; set; }
        public float SeekerRisePerHour { get; set; }
        public float FallPerDay { get; set; }
        public bool ShowUnitTicks { get; set; }
        public bool ScaleBar { get; set; }
        public bool ShowForCaravanMembers { get; set; }
        public string TutorHighlightTag { get; set; }
        public int ListPriority { get; set; }
        public bool Major { get; set; }
        public float BaseLevel { get; set; }
        public bool ShowOnNeedList { get; set; }
        public string DevelopmentalStageFilter { get; set; }
        public List<string> NullifyingPrecepts { get; set; }
        public List<string> HediffRequiredAny { get; set; }
        public List<string> TitleRequiredAny { get; set; }
        public bool NeverOnSlave { get; set; }
        public bool NeverOnPrisoner { get; set; }
        public bool OnlyIfCausedByTrait { get; set; }
        public bool OnlyIfCausedByGene { get; set; }
        public bool OnlyIfCausedByHediff { get; set; }
        public bool SlavesOnly { get; set; }
        public bool ColonistsOnly { get; set; }
        public bool PlayerMechsOnly { get; set; }
        public bool ColonistAndPrisonersOnly { get; set; }
        public string MinIntelligence { get; set; }
        public List<string> RequiredComps { get; set; }

        public static NeedDefDto NeedDefToDto(NeedDef needDef)
        {
            return new NeedDefDto
            {
                DefName = needDef.defName,
                NeedClassName = needDef.needClass?.FullName,
                FreezeInMentalState = needDef.freezeInMentalState,
                FreezeWhileSleeping = needDef.freezeWhileSleeping,
                SeekerFallPerHour = needDef.seekerFallPerHour,
                SeekerRisePerHour = needDef.seekerRisePerHour,
                FallPerDay = needDef.fallPerDay,
                ShowUnitTicks = needDef.showUnitTicks,
                ScaleBar = needDef.scaleBar,
                ShowForCaravanMembers = needDef.showForCaravanMembers,
                TutorHighlightTag = needDef.tutorHighlightTag,
                ListPriority = needDef.listPriority,
                Major = needDef.major,
                BaseLevel = needDef.baseLevel,
                ShowOnNeedList = needDef.showOnNeedList,
                DevelopmentalStageFilter = needDef.developmentalStageFilter.ToString(),
                NullifyingPrecepts = needDef.nullifyingPrecepts?.Select(p => p.defName).ToList(),
                HediffRequiredAny = needDef.hediffRequiredAny?.Select(h => h.defName).ToList(),
                TitleRequiredAny = needDef.titleRequiredAny?.Select(t => t.defName).ToList(),
                NeverOnSlave = needDef.neverOnSlave,
                NeverOnPrisoner = needDef.neverOnPrisoner,
                OnlyIfCausedByTrait = needDef.onlyIfCausedByTrait,
                OnlyIfCausedByGene = needDef.onlyIfCausedByGene,
                OnlyIfCausedByHediff = needDef.onlyIfCausedByHediff,
                SlavesOnly = needDef.slavesOnly,
                ColonistsOnly = needDef.colonistsOnly,
                PlayerMechsOnly = needDef.playerMechsOnly,
                ColonistAndPrisonersOnly = needDef.colonistAndPrisonersOnly,
                MinIntelligence = needDef.minIntelligence.ToString(),
                RequiredComps = needDef.requiredComps?.Select(c => c.GetType().Name).ToList(),
            };
        }
    }

    public class ThoughtDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public float MoodOffset { get; set; }
        public float DurationDays { get; set; }
        public int StackLimit { get; set; }
        public List<ThoughtStageDto> Stages { get; set; }
    }

    public class ThoughtStageDto
    {
        public string Label { get; set; }
        public string Description { get; set; }
        public float BaseMoodEffect { get; set; }
        public float BaseOpinionOffset { get; set; }
    }

    public class StatDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float DefaultBaseValue { get; set; }
        public bool ShowOnPawns { get; set; }
        public bool ShowOnHumanlikes { get; set; }
        public bool ShowOnAnimals { get; set; }
        public bool ShowOnMechanoids { get; set; }
        public bool ShowOnNonWorkTables { get; set; }
        public bool AlwaysAllowBaseZero { get; set; }
    }

    public class WorldObjectDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public bool CanHaveFaction { get; set; }
        public bool Selectable { get; set; }
        public bool NeverMultiSelect { get; set; }
        public bool ExpandingIcon { get; set; }
        public bool CanBeRandomlyPlaced { get; set; }
    }

    public class BiomeDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public float AnimalDensity { get; set; }
        public float PlantDensity { get; set; }
        public float DiseaseMtbDays { get; set; }
        public string ForagedFood { get; set; }
        public float MovementDifficulty { get; set; }
        public List<string> AllWildPlants { get; set; }
    }

    public class TerrainDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public float Fertility { get; set; }
        public int PathCost { get; set; }
        public float ExtraDeteriorationFactor { get; set; }
        public bool Layerable { get; set; }
        public List<string> Affordances { get; set; }
        public List<string> ResearchPrerequisites { get; set; }
    }

    public class RecipeDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string WorkSkill { get; set; }
        public float WorkSkillLearnFactor { get; set; }
        public float WorkAmount { get; set; }
        public string WorkSpeedStat { get; set; }
        public string EfficiencyStat { get; set; }
        public List<string> Products { get; set; }
        public List<SkillRequirementDto> SkillRequirements { get; set; }
    }

    public class SkillRequirementDto
    {
        public string Skill { get; set; }
        public int MinLevel { get; set; }
    }

    public class IngredientDto
    {
        public string Filter { get; set; }
        public int Count { get; set; }
        public bool IsFixedIngredient { get; set; }
        public string FixedIngredient { get; set; }
    }

    public class BodyDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string CorePart { get; set; }
        public List<string> Parts { get; set; }
    }

    public class BodyPartDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public float BleedRate { get; set; }
        public float HitPoints { get; set; }
        public float PermanentInjuryChanceFactor { get; set; }
    }

    public class SoundDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public int MaxSimultaneous { get; set; }
    }

    public class DesignationCategoryDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
    }

    public class JoyKindDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
    }

    public class MemeDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
    }

    public class PreceptDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Impact { get; set; }
    }

    public class AbilityDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string VerbProperties { get; set; }
    }

    public class GeneDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public int BiostatCpx { get; set; }
        public int BiostatMet { get; set; }
        public string DisplayCategory { get; set; }
        public float MinAgeActive { get; set; }
    }

    public class WeatherDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public float MinTemperature { get; set; }
        public float MaxTemperature { get; set; }
        public float WindSpeedFactor { get; set; }
        public float MoveSpeedMultiplier { get; set; }
        public float AccuracyMultiplier { get; set; }
    }

    public class RoomRoleDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
    }

    public class RoomStatDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public List<RoomStatScoreStageDto> ScoreStages { get; set; }
    }

    public class RoomStatScoreStageDto
    {
        public string Label { get; set; }
        public float MinScore { get; set; }
    }

    public class MentalStateDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public float RecoveryMtbDays { get; set; }
        public bool IsAggro { get; set; }
    }

    public class DrugPolicyDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public List<DrugPolicyEntryDto> Entries { get; set; }
    }

    public class DrugPolicyEntryDto
    {
        public string Drug { get; set; }
        public bool AllowedForAddiction { get; set; }
        public bool AllowedForJoy { get; set; }
        public bool AllowScheduled { get; set; }
        public float DaysFrequency { get; set; }
        public float OnlyIfMoodBelow { get; set; }
        public float OnlyIfJoyBelow { get; set; }
    }

    public class PlantDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public float FertilityMin { get; set; }
        public float FertilitySensitivity { get; set; }
        public float GrowDays { get; set; }
        public string HarvestedThingDef { get; set; }
    }

    public class AnimalDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public float BaseBodySize { get; set; }
        public float BaseHealthScale { get; set; }
        public string FoodType { get; set; }
        public bool Predator { get; set; }
        public bool PackAnimal { get; set; }
        public float Petness { get; set; }
        public List<LifeStageAgeDto> LifeStages { get; set; }
    }

    public class LifeStageAgeDto
    {
        public string LifeStage { get; set; }
        public float MinAge { get; set; }
    }
}
