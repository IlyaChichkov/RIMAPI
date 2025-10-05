using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RIMAPI
{
    public class ApiHandler
    {
        public static readonly object TickSyncLock = new object();

        private List<Designator> allDesignatorsCache;

        public Pawn GetPawn(int id)
        {
            Pawn q = PawnsFinder.AllMaps_Spawned.FirstOrDefault(p => p.thingIDNumber == id);
            if (q == null)
            {
                throw new Exception($"pawn {id} not found.");
            }
            return q;
        }

        public Thing GetThing(int id)
        {
            var q = Find.CurrentMap.listerThings.AllThings.FirstOrDefault(t => t.thingIDNumber == id);
            if (q == null)
            {
                throw new Exception($"thing {id} not found.");
            }
            return q;
        }

        public object PawnToObject(Pawn p)
        {
            return new
            {
                id = p.thingIDNumber,
                name = p.Name.ToStringShort,
                age = p.ageTracker.AgeBiologicalYears,
                gender = p.gender.ToString(),
                position = new { x = p.Position.x, y = p.Position.z },
                mood = p.needs?.mood?.CurLevelPercentage * 100 ?? -1f,
                health = p.health?.summaryHealth?.SummaryHealthPercent ?? 1f,
                hediff = p.health?.hediffSet?.hediffs?.Select(x => new { part = x.Part?.Label, label = x.Label }).ToList(),
                currentJob = p.CurJob?.def?.defName ?? "",
                traits = p.story?.traits?.allTraits.Select(t => t.def.defName).ToList() ?? new List<string>(),
                workPriorities = DefDatabase<WorkTypeDef>.AllDefs
                    .Select(wt => new { workType = wt.defName, priority = p.workSettings.GetPriority(wt) })
                    .Where(x => x.priority > 0)
                    .OrderBy(x => x.priority)
                    .ToList()
            };
        }

        public List<Designator> GetAllDesignators()
        {
            if (allDesignatorsCache == null)
            {
                allDesignatorsCache = new List<Designator>();
                foreach (DesignationCategoryDef categoryDef in DefDatabase<DesignationCategoryDef>.AllDefs)
                {
                    allDesignatorsCache.AddRange(categoryDef.ResolvedAllowedDesignators);
                }
            }
            return allDesignatorsCache;
        }

        public void InputBlueprint(int x, int y, string thing, string stuff, string rotation)
        {
            ThingDef thingToBuild = DefDatabase<ThingDef>.GetNamed(thing);
            ThingDef stuffToUse = null;
            if (stuff != "")
                stuffToUse = DefDatabase<ThingDef>.GetNamed(stuff);
            IntVec3 desiredPosition = new IntVec3(x, 0, y);
            Faction playerFaction = Faction.OfPlayer;
            if (!GenConstruct.CanPlaceBlueprintAt(thingToBuild, desiredPosition, Rot4.FromString(rotation), Find.CurrentMap, stuffDef: stuffToUse))
            {
                throw new Exception("cannot not place blueprint.");
            }
            GenConstruct.PlaceBlueprintForBuild(thingToBuild, desiredPosition, Find.CurrentMap, Rot4.FromString(rotation), playerFaction, stuff: stuffToUse);
        }

        public void input_surgery(int id, string recipe, string body_part)
        {
            Pawn p = GetPawn(id);
            // RecipeDefOf
            RecipeDef r = DefDatabase<RecipeDef>.GetNamed(recipe);

            BodyPartRecord bodyPart = p.health.hediffSet.GetNotMissingParts().FirstOrDefault(part => part.Label == body_part);
            if (bodyPart == null)
            {
                string hint = String.Join(", ", p.health.hediffSet.GetNotMissingParts().Select(part => part.Label));
                throw new Exception($"Available are {hint}");
            }

            Bill_Medical newBill = new Bill_Medical(r, null);
            newBill.Part = bodyPart;
            p.health.surgeryBills.AddBill(newBill);
        }

        public string CreateZone(string json)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<ZoneCreateRequest>(json);
                if (data == null)
                    return "{\"error\": \"Invalid JSON data\"}";

                string label = data.label ?? "New Zone";
                int x = data.x;
                int y = data.y;

                Map map = Find.CurrentMap;
                if (map == null)
                    return "{\"error\": \"No current map found\"}";

                Zone_Stockpile zone = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);
                zone.label = label;

                IntVec3 cell = new IntVec3(x, 0, y);
                if (!cell.InBounds(map))
                    return "{\"error\": \"Coordinates out of map bounds\"}";

                zone.AddCell(cell);
                map.zoneManager.RegisterZone(zone);

                return $"{{\"zoneId\": {zone.ID}, \"label\": \"{label}\", \"x\": {x}, \"y\": {y}, \"status\": \"created\"}}";
            }
            catch (Exception e)
            {
                Log.Error($"Error in CreateZone: {e}");
                return "{\"error\": \"Failed to create zone\"}";
            }
        }

        private class ZoneCreateRequest
        {
            public string label { get; set; }
            public int x { get; set; }
            public int y { get; set; }
        }

        public void input_zone_add_cell(int zone_id, int x, int y)
        {
            Map map = Find.CurrentMap;
            Zone zone = map.zoneManager.AllZones.FirstOrDefault(z => z.ID == zone_id);
            if (zone == null)
                throw new Exception("zone not found.");
            zone.AddCell(new IntVec3(x, 0, y));
        }

        public void input_command(int id, string label, int target_id)
        {
            Pawn p = GetPawn(id);
            Thing target_p = null;
            if (target_id != 0)
                target_p = GetThing(target_id);
            foreach (Gizmo g in p.GetGizmos())
            {
                if ((g as Command)?.Label != label && g.GetType().FullName != label)    // 开火命令没有label，只能这样代替1下了
                    continue;
                if (g is Command_Toggle g1)
                {
                    g1.toggleAction();
                    return;
                }
                if (g is Command_Action g2)
                {
                    g2.action();
                    return;
                }
                if (g is Command_VerbTarget g3)
                {
                    g3.verb.TryStartCastOn(target_p);
                    return;
                }
                if (g is Command_Ability g4)
                {
                    Ability ability = g4.Ability;
                    LocalTargetInfo l = new LocalTargetInfo(target_p);
                    if (!ability.CanCast)
                    {
                        throw new Exception("cannot cast.");
                    }
                    ability.QueueCastingJob(l, l);
                    return;
                }
                throw new Exception("command type error.");
            }
            throw new Exception("command not found.");
        }

        public void input_set_forbidden(int id, bool value)
        {
            GetThing(id).SetForbidden(value);
        }

        public void input_add_designation(int id, string type)
        {
            Thing target = GetThing(id);
            DesignationManager g = Find.CurrentMap.designationManager;
            Designator dr = GetAllDesignators().FirstOrDefault(d => d.GetType().Name == type);
            if (dr == null)
            {
                string hint = String.Join(", ", GetAllDesignators().Select(d => d.GetType().Name));
                throw new Exception($"Available are {hint}");
            }
            if (g.HasMapDesignationOn(target))
                g.RemoveAllDesignationsOn(target);
            if (!dr.CanDesignateThing(target))
            {
                throw new Exception($"can not designate.");
            }
            dr.DesignateThing(target);
        }

        public void input_remove_all_designation(int id)
        {
            Thing target = GetThing(id);
            Find.CurrentMap.designationManager.RemoveAllDesignationsOn(target);
        }

        public string things()
        {
            var result = Find.CurrentMap.listerThings.AllThings.Select(p => new
            {
                id = p.thingIDNumber,
                type = p.GetType().FullName,
                def = p.def?.defName,
                position = new { x = p.Position.x, y = p.Position.z },
                is_forbidden = p.IsForbidden(Faction.OfPlayer),
            }
            ).ToList();
            return JsonConvert.SerializeObject(result);
        }

        public string zones()
        {
            Map map = Find.CurrentMap;
            var result = map.zoneManager.AllZones.Select(z => new { id = z.ID, label = z.label });
            return JsonConvert.SerializeObject(result);
        }

        public string prisoners()
        {
            var result = Find.CurrentMap?.mapPawns?.PrisonersOfColony.Select(PawnToObject).ToList();
            return JsonConvert.SerializeObject(result);
        }

        public string animals()
        {
            var animals = Find
                .CurrentMap?.mapPawns?.AllPawns.Where(p => p.RaceProps?.Animal == true)
                .Select(p => new
                {
                    id = p.thingIDNumber,
                    name = p.LabelShortCap,
                    def = p.def?.defName,
                    faction = p.Faction?.ToString(),
                    position = new { x = p.Position.x, y = p.Position.z },
                    trainer = p
                        .relations?.DirectRelations.Where(r => r.def == PawnRelationDefOf.Bond)
                        .Select(r => r.otherPawn?.thingIDNumber)
                        .FirstOrDefault(),
                    trainings = DefDatabase<TrainableDef>.AllDefsListForReading.ToDictionary(
                        td => td.defName,
                        td =>
                        {
                            if (p.training == null)
                                return 0;
                            var mi = typeof(Pawn_TrainingTracker).GetMethod(
                                "GetSteps",
                                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                            );
                            return mi != null ? (int)mi.Invoke(p.training, new object[] { td }) : 0;
                        }
                    ),
                    pregnant = p.health?.hediffSet?.HasHediff(HediffDefOf.Pregnant) ?? false,
                })
                .ToList();

            return JsonConvert.SerializeObject(animals);
        }

        public string storage_detail()
        {
            Map map = Find.CurrentMap;
            var storages = new List<object>();

            var zones = map.zoneManager?.AllZones?.OfType<Zone_Stockpile>() ?? Enumerable.Empty<Zone_Stockpile>();
            foreach (var zone in zones)
            {
                var items = zone?.slotGroup.HeldThings.Select(t => new { id = t.thingIDNumber, def = t.def?.defName, stack_count = t.stackCount });
                storages.Add(new { name = zone.label, items });
            }

            var buildings = map.listerBuildings?.allBuildingsColonist?.OfType<Building_Storage>() ?? Enumerable.Empty<Building_Storage>();
            foreach (var building in buildings)
            {
                var items = building?.slotGroup.HeldThings.Select(t => new { id = t.thingIDNumber, def = t.def?.defName, stack_count = t.stackCount });
                storages.Add(new { name = building.LabelCap, items = CountThings(building?.slotGroup) });
            }

            return JsonConvert.SerializeObject(storages);
        }


        public string storage()
        {
            Map map = Find.CurrentMap;
            var storages = new List<object>();

            var zones = map.zoneManager?.AllZones?.OfType<Zone_Stockpile>() ?? Enumerable.Empty<Zone_Stockpile>();
            foreach (var zone in zones)
            {
                storages.Add(new { name = zone.label, items = CountThings(zone?.slotGroup) });
            }

            var buildings = map.listerBuildings?.allBuildingsColonist?.OfType<Building_Storage>() ?? Enumerable.Empty<Building_Storage>();
            foreach (var building in buildings)
            {
                storages.Add(new { name = building.LabelCap, items = CountThings(building?.slotGroup) });
            }

            return JsonConvert.SerializeObject(storages);
        }

        private static Dictionary<string, int> CountThings(SlotGroup group)
        {
            var dict = new Dictionary<string, int>();
            if (group == null)
                return dict;

            foreach (var thing in group.HeldThings)
            {
                string def = thing.def?.defName;
                if (string.IsNullOrEmpty(def))
                    continue;

                if (dict.ContainsKey(def))
                    dict[def] += thing.stackCount;
                else
                    dict[def] = thing.stackCount;
            }

            return dict;
        }


        public string ModsList()
        {
            var mods = LoadedModManager.RunningModsListForReading
                .Select(m => new { name = m.Name, packageId = m.PackageId });
            return JsonConvert.SerializeObject(mods);
        }

        public string factions()
        {
            // When no game is loaded (e.g. the main menu) Find.FactionManager is
            // null which would cause a NullReferenceException. Return an empty
            // list in that case.
            if (Current.ProgramState != ProgramState.Playing || Find.FactionManager == null)
            {
                return "[]";
            }

            var factions = Find.FactionManager.AllFactionsListForReading
                .Select(f => new
                {
                    name = f.Name,
                    def = f.def?.defName,
                    is_player = f.IsPlayer,
                    relation = f.IsPlayer ? string.Empty :
                        (
                            Find.FactionManager?.OfPlayer != null
                            ? Find.FactionManager.OfPlayer.RelationKindWith(f).ToString()
                            : string.Empty
                        ),
                    goodwill = f.IsPlayer ? 0 :
                        (Find.FactionManager?.OfPlayer?.GoodwillWith(f) ?? 0),
                })
                .ToList();

            return JsonConvert.SerializeObject(factions);
        }

        private IEnumerable<object> gizmo_to_dict(Thing b)
        {
            try
            {
                return b.GetGizmos()
                    .Select(m => new
                    {
                        type = m.GetType().FullName,
                        label = (m as Command)?.Label,
                    }).ToList(); ;
            }
            catch
            {
                return null;
            }
        }

        public string Research()
        {
            // When no game is loaded (e.g. the main menu), Current.Game is null.
            if (Current.ProgramState != ProgramState.Playing || Current.Game == null)
            {
                var empty = new
                {
                    currentProject = string.Empty,
                    progress = 0f,
                    finishedProjects = new List<string>()
                };

                return JsonConvert.SerializeObject(empty);
            }

            var manager = Find.ResearchManager;
            ResearchProjectDef current = null;

            if (manager != null)
            {
                // R�cup�re le champ priv� 'currentProj'
                var fld = typeof(ResearchManager)
                    .GetField("currentProj", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fld != null)
                    current = (ResearchProjectDef)fld.GetValue(manager);
            }

            float progress = 0f;
            if (current != null)
                progress = manager.GetProgress(current) / current.baseCost;

            var data = new
            {
                currentProject = current?.defName ?? string.Empty,
                progress,
                finishedProjects = DefDatabase<ResearchProjectDef>.AllDefsListForReading
                    .Where(p => p.IsFinished)
                    .Select(p => p.defName)
                    .ToList()
            };

            return JsonConvert.SerializeObject(data);
        }

        public string buildings()
        {
            var result = Find.CurrentMap.listerThings.AllThings.Where(t => t.def.building != null).Select(b => new
            {
                id = b.thingIDNumber,
                type = b.GetType().FullName,
                def = b.def?.defName,
                position = new { x = b.Position.x, y = b.Position.z },
                is_forbidden = b.IsForbidden(Faction.OfPlayer),
                faction = b.Faction?.ToString(),
            }).ToList();
            return JsonConvert.SerializeObject(result);
        }

        public string map()
        {
            Map map = Find.CurrentMap;
            if (map == null) return "{}";

            var info = new
            {
                weather = map.weatherManager?.curWeather?.defName,
                temperature = map.mapTemperature?.OutdoorTemp ?? 0f,
                hour = GenLocalDate.HourOfDay(map),
                size = new { x = map.Size.x, y = map.Size.z },
                season = GenLocalDate.Season(map).ToString(),
                total_ticks = Find.TickManager.TicksGame,
            };

            return JsonConvert.SerializeObject(info);
        }

        public string alerts()
        {
            // When no game is loaded (example on the main menu) Find.Alerts throws
            // an InvalidCastException. Simply return an empty list in that case
            // instead of logging an error.
            if (Current.ProgramState != ProgramState.Playing)
                return "[]";

            var alertsField = typeof(AlertsReadout).GetField("activeAlerts", BindingFlags.Instance | BindingFlags.NonPublic);

            IEnumerable<Alert> active = null;
            try
            {
                active = alertsField?.GetValue(Find.Alerts) as IEnumerable<Alert>;
            }
            catch
            {
                return "[]";
            }

            List<object> list;
            if (active == null)
            {
                list = new List<object>();
            }
            else
            {
                list = active.Where(a => a.Active)
                    .Select(a => (object)new { label = a.Label, priority = a.Priority.ToString() })
                    .ToList();
            }

            return JsonConvert.SerializeObject(list);
        }

        public string jobs()
        {
            var colonists = Find.CurrentMap?.mapPawns?.FreeColonists ?? Enumerable.Empty<Pawn>();

            var list = colonists.Select(p => new
            {
                id = p.thingIDNumber,
                name = p.Name.ToStringShort,
                current = p.CurJob?.def?.defName,
                queue = p.jobs?.jobQueue?.Select(q => q.job?.def?.defName).ToList() ?? new List<string>()
            }).ToList();

            return JsonConvert.SerializeObject(list);
        }

        public string Ping()
        {
            return "Pong!";
        }

        public string GetColonyInfo()
        {
            Map map = Find.CurrentMap;
            if (map == null)
            {
                return "{\"error\": \"No active game found\", \"debug\": {\"programState\": \"" + Current.ProgramState + "\"}}";
            }

            Dictionary<string, object> info = new Dictionary<string, object>()
            {
                ["colonyName"] = map?.info?.parent?.LabelCap ?? "Unnamed",
                ["colonistCount"] = map?.mapPawns?.FreeColonistsCount ?? 0,
                ["wealth"] = map?.wealthWatcher?.WealthTotal ?? 0,
                ["debug"] = new Dictionary<string, string>()
                {
                    ["programState"] = Current.ProgramState.ToString(),
                    ["hasGame"] = (Current.Game != null).ToString(),
                    ["mapId"] = (map?.uniqueID ?? -1).ToString(),
                }
            };
            return JsonConvert.SerializeObject(info);
        }

        public string GetLetters()
        {
            var letters = Find.LetterStack?.LettersListForReading
                .Select(l => new
                {
                    label = l.GetType().GetProperty("LabelCap")?.GetValue(l)?.ToString(),
                    type = l.def?.letterClass?.ToString(),
                    arrivalTime = l.arrivalTime
                })
                .ToList<object>() ?? new List<object>();

            return JsonConvert.SerializeObject(letters);
        }

        public string GetColonists()
        {
            var colonists = Find.CurrentMap?.mapPawns?.FreeColonists.Select(PawnToObject).ToList()
                ?? new List<object>();
            return JsonConvert.SerializeObject(colonists);
        }

        public string GetColonistById(string idStr)
        {
            string json = "{}";

            if (!int.TryParse(idStr, out int id))
            {
                return json;
            }

            Pawn p = Find.CurrentMap?.mapPawns?.FreeColonists.FirstOrDefault(x => x.thingIDNumber == id);
            if (p == null)
            {
                return json;
            }

            try
            {
                var colonistData = PawnToObject(p);
                json = JsonConvert.SerializeObject(colonistData);
            }
            catch (Exception e)
            {
                Log.Error($"Error serializing pawn: {e}");
            }
            return json;
        }

        public string GetAllMapTiles()
        {
            var map = Find.CurrentMap;
            if (map == null) return "[]";

            var tiles = new List<object>();
            for (int x = 0; x < map.Size.x; x++)
            {
                for (int y = 0; y < map.Size.z; y++)
                {
                    tiles.Add(new { x, y });
                }
            }

            return JsonConvert.SerializeObject(tiles);
        }

        public string GetMapTile(int x, int y)
        {
            Map map = Find.CurrentMap;
            if (map == null) return "{}";

            IntVec3 cell = new IntVec3(x, 0, y);
            if (!cell.InBounds(map)) return "{}";

            var info = new
            {
                terrain = map.terrainGrid?.TerrainAt(cell)?.defName,
                zone = map.zoneManager?.ZoneAt(cell)?.label,
                things = map.thingGrid?.ThingsListAt(cell)?.Select(t => t.def?.defName).ToList() ?? new List<string>()
            };

            return JsonConvert.SerializeObject(info);
        }
    }
}
