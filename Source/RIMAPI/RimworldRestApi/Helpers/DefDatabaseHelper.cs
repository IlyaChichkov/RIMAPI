using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Models;
using RimWorld;
using Verse;

namespace RIMAPI.Helpers
{
    public static class DefDatabaseHelper
    {
        public static TraitDefDto GetTraitDefDto(TraitDef traitDef)
        {
            if (traitDef == null)
                return null;

            TraitDefDto dto = new TraitDefDto
            {
                DefName = traitDef.defName,
                Label = string.IsNullOrEmpty(traitDef.label) ? "" : traitDef.label,
                Description = string.IsNullOrEmpty(traitDef.description) ? "" : traitDef.description,
                DegreeDatas = new List<TraitDegreeDto>(),
                ConflictingTraits = new List<string>(),
                DisabledWorkTypes = new List<string>(),
                DisabledWorkTags = traitDef.disabledWorkTags.ToString(),
            };

            // Fill degree datas
            for (int i = 0; i < traitDef.degreeDatas.Count; i++)
            {
                TraitDegreeData degreeData = traitDef.degreeDatas[i];
                TraitDegreeDto degreeDto = new TraitDegreeDto
                {
                    Label = degreeData.label,
                    Description = degreeData.description,
                    Degree = degreeData.degree,
                    SkillGains = new Dictionary<string, int>(),
                    StatOffsets = new List<StatModifierDto>(),
                    StatFactors = new List<StatModifierDto>(),
                };

                // Fill skill gains
                foreach (var skillGain in degreeData.skillGains)
                {
                    degreeDto.SkillGains[skillGain.skill.defName] = skillGain.amount;
                }

                // Fill stat offsets
                if (degreeData.statOffsets != null)
                {
                    for (int j = 0; j < degreeData.statOffsets.Count; j++)
                    {
                        degreeDto.StatOffsets.Add(
                            new StatModifierDto
                            {
                                StatDefName = degreeData.statOffsets[j].stat.defName,
                                Value = degreeData.statOffsets[j].value,
                            }
                        );
                    }
                }

                // Fill stat factors
                if (degreeData.statFactors != null)
                {
                    for (int j = 0; j < degreeData.statFactors.Count; j++)
                    {
                        degreeDto.StatFactors.Add(
                            new StatModifierDto
                            {
                                StatDefName = degreeData.statFactors[j].stat.defName,
                                Value = degreeData.statFactors[j].value,
                            }
                        );
                    }
                }

                dto.DegreeDatas.Add(degreeDto);
            }

            // Fill conflicting traits
            for (int i = 0; i < traitDef.conflictingTraits.Count; i++)
            {
                dto.ConflictingTraits.Add(traitDef.conflictingTraits[i].defName);
            }

            // Fill disabled work types
            for (int i = 0; i < traitDef.disabledWorkTypes.Count; i++)
            {
                dto.DisabledWorkTypes.Add(traitDef.disabledWorkTypes[i].defName);
            }

            return dto;
        }

        public static List<ThoughtDefDto> GetThoughtDefDtoList()
        {
            return DefDatabase<ThoughtDef>
                .AllDefsListForReading.Select(t => new ThoughtDefDto
                {
                    DefName = t.defName,
                    Label = t.label,
                    Description = t.description,
                    MoodOffset = t.stages?.FirstOrDefault()?.baseMoodEffect ?? 0,
                    DurationDays = t.durationDays,
                    StackLimit = t.stackLimit,
                    Stages = t
                        .stages?.Select(s => new ThoughtStageDto
                        {
                            Label = s.LabelCap,
                            Description = s.description,
                            BaseMoodEffect = s.baseMoodEffect,
                            BaseOpinionOffset = s.baseOpinionOffset,
                        })
                        .ToList(),
                })
                .ToList();
        }

        public static List<StatDefDto> GetStatDefDtoList()
        {
            return DefDatabase<StatDef>
                .AllDefsListForReading.Select(s => new StatDefDto
                {
                    DefName = s.defName,
                    Label = s.LabelCap,
                    Description = s.description,
                    Category = s.category?.defName,
                    MinValue = s.minValue,
                    MaxValue = s.maxValue,
                    DefaultBaseValue = s.defaultBaseValue,
                    ShowOnPawns = s.showOnPawns,
                    ShowOnHumanlikes = s.showOnHumanlikes,
                    ShowOnAnimals = s.showOnAnimals,
                    ShowOnMechanoids = s.showOnMechanoids,
                    ShowOnNonWorkTables = s.showOnNonWorkTables,
                })
                .ToList();
        }

        public static List<WorldObjectDefDto> GetWorldObjectDefDtoList()
        {
            return DefDatabase<WorldObjectDef>
                .AllDefsListForReading.Select(w => new WorldObjectDefDto
                {
                    DefName = w.defName,
                    Label = w.LabelCap,
                    Description = w.description,
                    CanHaveFaction = w.canHaveFaction,
                    Selectable = w.selectable,
                    NeverMultiSelect = w.neverMultiSelect,
                    ExpandingIcon = w.expandingIcon,
                })
                .ToList();
        }

        public static List<BiomeDefDto> GetBiomeDefDtoList()
        {
            return DefDatabase<BiomeDef>
                .AllDefsListForReading.Select(b => new BiomeDefDto
                {
                    DefName = b.defName,
                    Label = b.LabelCap,
                    Description = b.description,
                    AnimalDensity = b.animalDensity,
                    PlantDensity = b.plantDensity,
                    DiseaseMtbDays = b.diseaseMtbDays,
                    ForagedFood = b.foragedFood?.defName,
                    MovementDifficulty = b.movementDifficulty,
                })
                .ToList();
        }

        public static List<TerrainDefDto> GetTerrainDefDtoList()
        {
            return DefDatabase<TerrainDef>
                .AllDefsListForReading.Select(t => new TerrainDefDto
                {
                    DefName = t.defName,
                    Label = t.LabelCap,
                    Fertility = t.fertility,
                    PathCost = t.pathCost,
                    ExtraDeteriorationFactor = t.extraDeteriorationFactor,
                    Layerable = t.layerable,
                    Affordances = t.affordances?.Select(a => a.defName).ToList(),
                    ResearchPrerequisites = t
                        .researchPrerequisites?.Select(r => r.defName)
                        .ToList(),
                })
                .ToList();
        }

        public static List<RecipeDefDto> GetRecipeDefDtoList()
        {
            return DefDatabase<RecipeDef>
                .AllDefsListForReading.Select(r => new RecipeDefDto
                {
                    DefName = r.defName,
                    Label = r.LabelCap,
                    Description = r.description,
                    WorkSkill = r.workSkill?.defName,
                    WorkSkillLearnFactor = r.workSkillLearnFactor,
                    WorkAmount = r.workAmount,
                    WorkSpeedStat = r.workSpeedStat?.defName,
                    EfficiencyStat = r.efficiencyStat?.defName,
                    Products = r.products?.Select(p => p.thingDef.defName).ToList(),
                    SkillRequirements = r
                        .skillRequirements?.Select(s => new SkillRequirementDto
                        {
                            Skill = s.skill.defName,
                            MinLevel = s.minLevel,
                        })
                        .ToList(),
                })
                .ToList();
        }

        public static List<BodyDefDto> GetBodyDefDtoList()
        {
            return DefDatabase<BodyDef>
                .AllDefsListForReading.Select(b => new BodyDefDto
                {
                    DefName = b.defName,
                    Label = b.LabelCap,
                    CorePart = b.corePart?.def.defName,
                    Parts = b.AllParts?.Select(p => p.def.defName).ToList(),
                })
                .ToList();
        }

        public static List<BodyPartDefDto> GetBodyPartDefDtoList()
        {
            return DefDatabase<BodyPartDef>
                .AllDefsListForReading.Select(bp => new BodyPartDefDto
                {
                    DefName = bp.defName,
                    Label = bp.LabelCap,
                    BleedRate = bp.bleedRate,
                    HitPoints = bp.hitPoints,
                    PermanentInjuryChanceFactor = bp.permanentInjuryChanceFactor,
                })
                .ToList();
        }

        public static List<FactionDefDto> GetFactionDefDtoList()
        {
            return DefDatabase<FactionDef>
                .AllDefsListForReading.Select(s =>
                    FactionDefDto.FromFactionDef(FactionHelper.GetFactionDef(s.defName))
                )
                .ToList();
        }

        public static List<SoundDefDto> GetSoundDefDtoList()
        {
            return DefDatabase<SoundDef>
                .AllDefsListForReading.Select(s => new SoundDefDto
                {
                    DefName = s.defName,
                    Label = s.LabelCap,
                    MaxSimultaneous = s.maxSimultaneous,
                })
                .ToList();
        }

        public static List<DesignationCategoryDefDto> GetDesignationCategoryDefDtoList()
        {
            return DefDatabase<DesignationCategoryDef>
                .AllDefsListForReading.Select(dc => new DesignationCategoryDefDto
                {
                    DefName = dc.defName,
                    Label = dc.LabelCap,
                    Description = dc.description,
                })
                .ToList();
        }

        public static List<JoyKindDefDto> GetJoyKindDefDtoList()
        {
            return DefDatabase<JoyKindDef>
                .AllDefsListForReading.Select(jk => new JoyKindDefDto
                {
                    DefName = jk.defName,
                    Label = jk.LabelCap,
                })
                .ToList();
        }

        public static List<MemeDefDto> GetMemeDefDtoList()
        {
            return DefDatabase<MemeDef>
                .AllDefsListForReading.Select(m => new MemeDefDto
                {
                    DefName = m.defName,
                    Label = m.LabelCap,
                    Description = m.description,
                })
                .ToList();
        }

        public static List<PreceptDefDto> GetPreceptDefDtoList()
        {
            return DefDatabase<PreceptDef>
                .AllDefsListForReading.Select(p => new PreceptDefDto
                {
                    DefName = p.defName,
                    Label = p.LabelCap,
                    Description = p.description,
                    Impact = p.impact.ToString(),
                })
                .ToList();
        }

        public static List<AbilityDefDto> GetAbilityDefDtoList()
        {
            return DefDatabase<AbilityDef>
                .AllDefsListForReading.Select(a => new AbilityDefDto
                {
                    DefName = a.defName,
                    Label = a.LabelCap,
                    Description = a.description,
                    VerbProperties = a.verbProperties?.ToString(),
                })
                .ToList();
        }

        public static List<GeneDefDto> GetGeneDefDtoList()
        {
            return DefDatabase<GeneDef>
                .AllDefsListForReading.Select(g => new GeneDefDto
                {
                    DefName = g.defName,
                    Label = g.LabelCap,
                    Description = g.description,
                    BiostatCpx = g.biostatCpx,
                    BiostatMet = g.biostatMet,
                    DisplayCategory = g.displayCategory.ToString(),
                    MinAgeActive = g.minAgeActive,
                })
                .ToList();
        }

        public static List<WeatherDefDto> GetWeatherDefDtoList()
        {
            return DefDatabase<WeatherDef>
                .AllDefsListForReading.Select(w => new WeatherDefDto
                {
                    DefName = w.defName,
                    Label = w.LabelCap,
                    Description = w.description,
                    MinTemperature = w.temperatureRange.min,
                    MaxTemperature = w.temperatureRange.max,
                    WindSpeedFactor = w.windSpeedFactor,
                    MoveSpeedMultiplier = w.moveSpeedMultiplier,
                    AccuracyMultiplier = w.accuracyMultiplier,
                })
                .ToList();
        }

        public static List<RoomRoleDefDto> GetRoomRoleDefDtoList()
        {
            return DefDatabase<RoomRoleDef>
                .AllDefsListForReading.Select(rr => new RoomRoleDefDto
                {
                    DefName = rr.defName,
                    Label = rr.LabelCap,
                    Description = rr.description,
                })
                .ToList();
        }

        public static List<RoomStatDefDto> GetRoomStatDefDtoList()
        {
            return DefDatabase<RoomStatDef>
                .AllDefsListForReading.Select(rs => new RoomStatDefDto
                {
                    DefName = rs.defName,
                    Label = rs.LabelCap,
                    Description = rs.description,
                    ScoreStages = rs
                        .scoreStages?.Select(ss => new RoomStatScoreStageDto
                        {
                            Label = ss.label,
                            MinScore = ss.minScore,
                        })
                        .ToList(),
                })
                .ToList();
        }

        public static List<MentalStateDefDto> GetMentalStateDefDtoList()
        {
            return DefDatabase<MentalStateDef>
                .AllDefsListForReading.Select(ms => new MentalStateDefDto
                {
                    DefName = ms.defName,
                    Label = ms.LabelCap,
                    Description = ms.description,
                    RecoveryMtbDays = ms.recoveryMtbDays,
                    IsAggro = ms.IsAggro,
                })
                .ToList();
        }

        public static List<DrugPolicyDefDto> GetDrugPolicyDefDtoList()
        {
            return DefDatabase<DrugPolicyDef>
                .AllDefsListForReading.Select(dp => new DrugPolicyDefDto
                {
                    DefName = dp.defName,
                    Label = dp.LabelCap,
                    Entries = dp
                        .entries?.Select(e => new DrugPolicyEntryDto
                        {
                            Drug = e.drug?.defName,
                            AllowedForAddiction = e.allowedForAddiction,
                            AllowedForJoy = e.allowedForJoy,
                            AllowScheduled = e.allowScheduled,
                            DaysFrequency = e.daysFrequency,
                            OnlyIfMoodBelow = e.onlyIfMoodBelow,
                            OnlyIfJoyBelow = e.onlyIfJoyBelow,
                        })
                        .ToList(),
                })
                .ToList();
        }

        public static List<PlantDefDto> GetPlantDefDtoList()
        {
            return DefDatabase<ThingDef>
                .AllDefsListForReading.Where(t => t.category == ThingCategory.Plant)
                .Select(p => new PlantDefDto
                {
                    DefName = p.defName,
                    Label = p.LabelCap,
                    Description = p.description,
                    FertilityMin = p.plant?.fertilityMin ?? 0,
                    FertilitySensitivity = p.plant?.fertilitySensitivity ?? 0,
                    GrowDays = p.plant?.growDays ?? 0,
                    HarvestedThingDef = p.plant?.harvestedThingDef?.defName,
                })
                .ToList();
        }

        public static List<AnimalDefDto> GetAnimalDefDtoList()
        {
            return DefDatabase<ThingDef>
                .AllDefsListForReading.Where(t => t.race?.Animal == true)
                .Select(a => new AnimalDefDto
                {
                    DefName = a.defName,
                    Label = a.LabelCap,
                    Description = a.description,
                    BaseBodySize = a.race?.baseBodySize ?? 0,
                    BaseHealthScale = a.race?.baseHealthScale ?? 0,
                    FoodType = a.race?.foodType.ToString(),
                    Predator = a.race?.predator ?? false,
                    PackAnimal = a.race?.packAnimal ?? false,
                    Petness = a.race?.petness ?? 0,
                    LifeStages = a
                        .race?.lifeStageAges?.Select(ls => new LifeStageAgeDto
                        {
                            LifeStage = ls.def?.defName,
                            MinAge = ls.minAge,
                        })
                        .ToList(),
                })
                .ToList();
        }

        public static List<NeedDefDto> GetNeedDefDtoList()
        {
            return DefDatabase<NeedDef>
                .AllDefsListForReading.Select(n => NeedDefDto.NeedDefToDto(n))
                .ToList();
        }

        public static List<WorkTypeDefDto> GetWorkTypeDefDtoList()
        {
            return DefDatabase<WorkTypeDef>
                .AllDefsListForReading.Select(w => new WorkTypeDefDto
                {
                    DefName = w.defName,
                    Label = w.LabelCap,
                    Description = w.description,
                    WorkTags = w.workTags.ToString(),
                    RelevantSkills = w.relevantSkills?.Select(s => s.defName).ToList(),
                    NaturalPriority = w.naturalPriority,
                })
                .ToList();
        }

        public static List<SkillDefDto> GetSkillDefDtoList()
        {
            return DefDatabase<SkillDef>
                .AllDefsListForReading.Select(s => new SkillDefDto
                {
                    DefName = s.defName,
                    Label = s.LabelCap,
                    Description = s.description,
                    DisablingWorkTags = s.disablingWorkTags.ToString(),
                })
                .ToList();
        }

        public static List<HediffDefDto> GetHediffDefsList()
        {
            return DefDatabase<HediffDef>
                .AllDefsListForReading.Select(h => new HediffDefDto
                {
                    DefName = h.defName,
                    Label = h.LabelCap,
                    Description = h.description,
                    HediffClass = h.hediffClass?.Name,
                    IsAddiction = h.IsAddiction,
                    MakesSickThought = h.makesSickThought,
                    MaxSeverity = h.maxSeverity,
                    Tendable = h.tendable,
                    IsBad = h.isBad,
                    Stages = h
                        .stages?.Select(s => new HediffStageDto
                        {
                            MinSeverity = s.minSeverity,
                            Label = s.label,
                            BecomeImmuneTo = s.makeImmuneTo?.Select(m => m.defName).ToList(),
                            DeathMtbDays = s.deathMtbDays,
                            PainFactor = s.painFactor,
                            ForgetMemoryThoughtMtbDays = s.forgetMemoryThoughtMtbDays,
                            VomitMtbDays = s.vomitMtbDays,
                            MentalBreakMtbDays = s.mentalBreakMtbDays,
                        })
                        .ToList(),
                })
                .ToList();
        }

        public static List<ResearchProjectDefDto> GetResearchProjectDefDtoList()
        {
            return DefDatabase<ResearchProjectDef>
                .AllDefsListForReading.Select(r => new ResearchProjectDefDto
                {
                    DefName = r.defName,
                    Label = r.LabelCap,
                    Description = r.description,
                    Cost = r.baseCost,
                    Prerequisites = r.prerequisites?.Select(p => p.defName).ToList(),
                    RequiredResearchBuilding = r.requiredResearchBuilding?.defName,
                    TechLevel = r.techLevel.ToString(),
                    Tab = r.tab?.defName,
                    Hidden = r.IsHidden,
                })
                .ToList();
        }

        public static List<TraitDefDto> GetTraitDefDtoList()
        {
            return DefDatabase<TraitDef>
                .AllDefsListForReading.Select(t => DefDatabaseHelper.GetTraitDefDto(t))
                .ToList();
        }

        public static List<PawnKindDefDto> GetPawnKindDefDtoList()
        {
            return DefDatabase<PawnKindDef>
                .AllDefsListForReading.Select(p => new PawnKindDefDto
                {
                    DefName = p.defName,
                    Label = p.LabelCap,
                    Race = p.race?.defName,
                    CombatPower = p.combatPower,
                    WeaponTags = p.weaponTags?.ToList() ?? new List<string>(),
                    ApparelTags = p.apparelTags?.ToList() ?? new List<string>(),
                })
                .ToList();
        }

        public static List<GameConditionDefDto> GetConditionsDefDtoList()
        {
            return DefDatabase<GameConditionDef>
                .AllDefsListForReading.Select(g => new GameConditionDefDto
                {
                    DefName = g.defName,
                    Label = g.LabelCap,
                    Description = g.description,
                    LetterText = g.letterText,
                    CanBePermanent = g.canBePermanent,
                    TemperatureOffset = g.temperatureOffset,
                })
                .ToList();
        }

        public static List<IncidentDefDto> GetIncidentDefDtoList()
        {
            return DefDatabase<IncidentDef>
                .AllDefsListForReading.Where(i => i != null)
                .Select(i => new IncidentDefDto
                {
                    DefName = i.defName,
                    Label = i.LabelCap,
                    Description = i.description,
                    Category = i.category?.defName,
                    BaseChance = i.baseChance,
                    BaseChanceWithRoyalty = i.baseChanceWithRoyalty,
                    LetterDefName = i.letterDef?.defName,
                    PopulationEffect = ((int)i.populationEffect).ToString(),
                    LetterText = i.letterText,
                    MinPopulation = i.minPopulation,
                    MinThreatPoints = i.minThreatPoints,
                    MaxThreatPoints = i.maxThreatPoints,
                    ShouldIgnoreRecentWeighting = i.ShouldIgnoreRecentWeighting,
                    Tags = i.tags?.ToList() ?? new List<string>(),
                    DisallowedBiomes =
                        i.disallowedBiomes?.Select(b => b.defName).ToList() ?? new List<string>(),
                    AllowedBiomes =
                        i.allowedBiomes?.Select(b => b.defName).ToList() ?? new List<string>(),
                    RequireColonistsPresent = i.requireColonistsPresent,
                })
                .ToList();
        }

        public static List<ThingDefDto> GetThingDefDtoList()
        {
            return DefDatabase<ThingDef>
                .AllDefsListForReading.Where(t => t != null && !string.IsNullOrEmpty(t.defName))
                .Select(t => ThingDefDto.ToDto(t))
                .ToList();
        }

        public static IncidentParms IncidentParmsFromDto(
            IncidentParmsDto dto,
            IIncidentTarget target = null
        )
        {
            if (dto == null)
                return null;

            // Create base parms using DefaultParmsNow if we have a category and target
            IncidentParms parms;

            if (target != null)
            {
                // If you have incident category available, use DefaultParmsNow
                // parms = StorytellerUtility.DefaultParmsNow(incidentCategory, target);
                // For now, create basic parms
                parms = new IncidentParms();
                parms.target = target;
            }
            else
            {
                parms = new IncidentParms();
            }

            // Basic parameters
            parms.points = dto.Points;
            parms.forced = dto.Forced;

            // Letter parameters
            parms.customLetterLabel = dto.CustomLetterLabel;
            parms.customLetterText = dto.CustomLetterText;
            parms.sendLetter = dto.SendLetter;
            parms.inSignalEnd = dto.InSignalEnd;
            parms.silent = dto.Silent;

            // // Spawn parameters
            // if (!string.IsNullOrEmpty(dto.SpawnCenter) && IntVec3.TryParse(dto.SpawnCenter, out IntVec3 spawnCenter))
            // {
            //     parms.spawnCenter = spawnCenter;
            // }

            // if (!string.IsNullOrEmpty(dto.SpawnRotation) && Rot4.TryParse(dto.SpawnRotation, out Rot4 spawnRotation))
            // {
            //     parms.spawnRotation = spawnRotation;
            // }

            // Raid and combat parameters
            parms.generateFightersOnly = dto.GenerateFightersOnly;
            parms.dontUseSingleUseRocketLaunchers = dto.DontUseSingleUseRocketLaunchers;
            parms.raidForceOneDowned = dto.RaidForceOneDowned;
            parms.raidNeverFleeIndividual = dto.RaidNeverFleeIndividual;
            parms.raidArrivalModeForQuickMilitaryAid = dto.RaidArrivalModeForQuickMilitaryAid;

            // Biocode parameters
            parms.biocodeWeaponsChance = dto.BiocodeWeaponsChance;
            parms.biocodeApparelChance = dto.BiocodeApparelChance;

            // Pawn parameters
            parms.pawnGroupMakerSeed = dto.PawnGroupMakerSeed;
            parms.pawnCount = dto.PawnCount;
            parms.podOpenDelay = dto.PodOpenDelay;

            // Quest parameters
            parms.questTag = dto.QuestTag;

            // Behavior parameters
            parms.canTimeoutOrFlee = dto.CanTimeoutOrFlee;
            parms.canSteal = dto.CanSteal;
            parms.canKidnap = dto.CanKidnap;

            // Body size and multiplier parameters
            parms.totalBodySize = dto.TotalBodySize;
            parms.pointMultiplier = dto.PointMultiplier;
            parms.bypassStorytellerSettings = dto.BypassStorytellerSettings;

            // Debug faction lookup
            if (!string.IsNullOrEmpty(dto.Faction))
            {
                LogApi.Message($"Looking up faction: {dto.Faction}");

                parms.faction = Find.FactionManager.AllFactions.FirstOrDefault(f =>
                    f.def.defName.ToLower() == dto.Faction.ToLower()
                );
                LogApi.Message($"Case-insensitive result: {parms.faction?.def?.defName}");
            }

            if (!string.IsNullOrEmpty(dto.CustomLetterDef))
            {
                parms.customLetterDef = DefDatabase<LetterDef>.GetNamedSilentFail(
                    dto.CustomLetterDef
                );
            }

            if (!string.IsNullOrEmpty(dto.RaidStrategy))
            {
                parms.raidStrategy = DefDatabase<RaidStrategyDef>.GetNamedSilentFail(
                    dto.RaidStrategy
                );
            }

            if (!string.IsNullOrEmpty(dto.RaidArrivalMode))
            {
                parms.raidArrivalMode = DefDatabase<PawnsArrivalModeDef>.GetNamedSilentFail(
                    dto.RaidArrivalMode
                );
            }

            if (!string.IsNullOrEmpty(dto.RaidAgeRestriction))
            {
                parms.raidAgeRestriction = DefDatabase<RaidAgeRestrictionDef>.GetNamedSilentFail(
                    dto.RaidAgeRestriction
                );
            }

            if (!string.IsNullOrEmpty(dto.PawnKind))
            {
                parms.pawnKind = DefDatabase<PawnKindDef>.GetNamedSilentFail(dto.PawnKind);
            }

            if (!string.IsNullOrEmpty(dto.PawnGroupKind))
            {
                parms.pawnGroupKind = DefDatabase<PawnGroupKindDef>.GetNamedSilentFail(
                    dto.PawnGroupKind
                );
            }

            if (!string.IsNullOrEmpty(dto.TraderKind))
            {
                parms.traderKind = DefDatabase<TraderKindDef>.GetNamed(dto.TraderKind, false);
            }

            if (!string.IsNullOrEmpty(dto.QuestScriptDef))
            {
                parms.questScriptDef = DefDatabase<QuestScriptDef>.GetNamedSilentFail(
                    dto.QuestScriptDef
                );
            }

            if (!string.IsNullOrEmpty(dto.PsychicRitualDef))
            {
                parms.psychicRitualDef = DefDatabase<PsychicRitualDef>.GetNamedSilentFail(
                    dto.PsychicRitualDef
                );
            }

            // Look up collections of defs
            if (dto.LetterHyperlinkThingDefs != null)
            {
                parms.letterHyperlinkThingDefs = dto
                    .LetterHyperlinkThingDefs.Select(defName =>
                        DefDatabase<ThingDef>.GetNamedSilentFail(defName)
                    )
                    .Where(def => def != null)
                    .ToList();
            }

            if (dto.LetterHyperlinkHediffDefs != null)
            {
                parms.letterHyperlinkHediffDefs = dto
                    .LetterHyperlinkHediffDefs.Select(defName =>
                        DefDatabase<HediffDef>.GetNamedSilentFail(defName)
                    )
                    .Where(def => def != null)
                    .ToList();
            }

            // Look up pawns by ID (this is more complex and may require searching)
            if (dto.ControllerPawn != null)
            {
                parms.controllerPawn = FindPawnById(dto.ControllerPawn);
            }

            // Look up infestation location
            // if (!string.IsNullOrEmpty(dto.InfestationLocOverride) &&
            //     IntVec3.TryParse(dto.InfestationLocOverride, out IntVec3 infestationLoc))
            // {
            //     parms.infestationLocOverride = infestationLoc;
            // }

            return parms;
        }

        public static Pawn FindPawnById(string pawnId)
        {
            if (string.IsNullOrEmpty(pawnId))
                return null;

            // Search in maps
            foreach (var map in Find.Maps)
            {
                var pawn = map.mapPawns.AllPawns.FirstOrDefault(p => p.GetUniqueLoadID() == pawnId);
                if (pawn != null)
                    return pawn;
            }

            // Search in world pawns
            return Find.World.worldPawns.AllPawnsAlive.FirstOrDefault(p =>
                p.GetUniqueLoadID() == pawnId
            );
        }
    }
}
