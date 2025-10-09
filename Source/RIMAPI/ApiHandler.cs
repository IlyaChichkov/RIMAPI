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
//using FollowMe; // Uses FollowMe Mod

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


        public void InputZoneAddCell(int zoneId, int x, int y)
        {
            Map map = Find.CurrentMap;
            Zone zone = map.zoneManager.AllZones.FirstOrDefault(z => z.ID == zoneId);
            if (zone == null)
                throw new Exception("zone not found.");
            zone.AddCell(new IntVec3(x, 0, y));
        }


        public void InputSetForbidden(int id, bool value)
        {
            GetThing(id).SetForbidden(value);
        }

        public void InputAddDesignation(int id, string type)
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

        public void InputRemoveAllDesignation(int id)
        {
            Thing target = GetThing(id);
            Find.CurrentMap.designationManager.RemoveAllDesignationsOn(target);
        }

        public string Things()
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

        public string Zones()
        {
            Map map = Find.CurrentMap;
            var result = map.zoneManager.AllZones.Select(z => new { id = z.ID, label = z.label });
            return JsonConvert.SerializeObject(result);
        }

        public string Prisoners()
        {
            var result = Find.CurrentMap?.mapPawns?.PrisonersOfColony.Select(PawnToObject).ToList();
            return JsonConvert.SerializeObject(result);
        }

        public string Animals()
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

        public string StorageDetail()
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


        public string Storage()
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

        public string Factions()
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

        private IEnumerable<object> GizmoToDict(Thing b)
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

        public string Buildings()
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

        public string Map()
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

        public string Alerts()
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

        public string Jobs()
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

        public string GetMapInfo()
        {
            Map map = Find.CurrentMap;
            if (map == null) return "{}";

            var info = new
            {
                id = map.uniqueID.ToString(),
                size = map.Size.ToString(),
                center = map.Center.ToString(),
                is_player_home = map.IsPlayerHome,
                biome = map.Biome.defName,
            };

            return JsonConvert.SerializeObject(info);
        }

        public string GetCameraInfo()
        {
            var info = new
            {
                is_following = FollowMe.FollowMe.FollowedLabel
            };

            return JsonConvert.SerializeObject(info);

        }

        public string GetPowerProduction()
        {
            Map map = Find.CurrentMap;
            float currentPower = 0f;
            float totalPossiblePower = 0f;
            float currentlyStoredPower = 0f;
            float totalPowerStorage = 0f;

            foreach (Building building in map.listerBuildings.allBuildingsColonist)
            {
                CompPowerPlant powerPlant = building.TryGetComp<CompPowerPlant>();
                if (powerPlant != null)
                {
                    totalPossiblePower += powerPlant.Props.PowerConsumption;
                    currentPower += powerPlant.PowerOutput;
                    continue;
                }

                CompPowerBattery powerBattery = building.TryGetComp<CompPowerBattery>();
                if (powerBattery != null)
                {
                    currentlyStoredPower += powerBattery.StoredEnergy;
                    totalPowerStorage += powerBattery.Props.storedEnergyMax;
                }
            }


            var info = new
            {
                currentPower = Mathf.Round(currentPower),
                totalPossiblePower = Mathf.Round(Mathf.Abs(totalPossiblePower)),
                currentlyStoredPower = Mathf.Round(currentlyStoredPower),
                totalPowerStorage = Mathf.Round(totalPowerStorage)
            };

            return JsonConvert.SerializeObject(info);
        }

        public string GetColonyBuildings()
        {
            Map map = Find.CurrentMap;
            List<string> buildings = new List<string>();

            foreach (Building building in map.listerBuildings.allBuildingsColonist)
            {
                buildings.Add(building.Label);
            }


            var info = new
            {
                buildings_list = buildings,
            };

            return JsonConvert.SerializeObject(info);
        }

        public string GetMapPlants()
        {
            Map map = Find.CurrentMap;
            if (map == null) return "{}";

            try
            {
                var plants = map.listerThings.ThingsInGroup(ThingRequestGroup.Plant);
                var plantInfo = plants.Where(p => p is Plant)
                                    .Cast<Plant>()
                                    .Select(p => new
                                    {
                                        id = p.thingIDNumber,
                                        defName = p.def.defName,
                                        label = p.Label,
                                        position = new { x = p.Position.x, y = p.Position.y, z = p.Position.z },
                                        growth = p.Growth,
                                        lifeStage = p.LifeStage.ToString(),
                                        health = p.HitPoints / (float)p.MaxHitPoints,
                                        harvested = p.HarvestableNow,
                                        blighted = p.Blighted,
                                        sowable = p.def.plant?.Sowable ?? false
                                    })
                                    .ToList();

                return JsonConvert.SerializeObject(plantInfo);
            }
            catch (Exception e)
            {
                Log.Error($"Error in GetMapPlants: {e}");
                return "{\"error\": \"Failed to get plants data\"}";
            }
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

        public string GetHardTerrainTiles()
        {
            Map map = Find.CurrentMap;
            if (map == null) return "[]";

            try
            {
                var hardTerrainTiles = new List<object>();

                for (int x = 0; x < map.Size.x; x++)
                {
                    for (int z = 0; z < map.Size.z; z++)
                    {
                        IntVec3 cell = new IntVec3(x, 0, z);
                        TerrainDef terrain = map.terrainGrid.TerrainAt(cell);

                        if (terrain != null && (terrain.edgeType == TerrainDef.TerrainEdgeType.Hard))
                        {
                            hardTerrainTiles.Add(new
                            {
                                x = x,
                                z = z,
                                terrainDef = terrain.defName,
                                label = terrain.label,
                                passability = terrain.passability.ToString(),
                                fertility = terrain.fertility,
                                movementCost = terrain.pathCost,
                                isWater = terrain.IsWater,
                            });
                        }
                    }
                }

                return JsonConvert.SerializeObject(hardTerrainTiles);
            }
            catch (Exception e)
            {
                Log.Error($"Error in GetHardTerrainTiles: {e}");
                return "{\"error\": \"Failed to get hard terrain data\"}";
            }
        }

        private class ZoneCreateRequest
        {
            public string label { get; set; }
            public int x { get; set; }
            public int y { get; set; }
        }

        private class FollowPawnRequest
        {
            public int pawn_id { get; set; }
        }

        private class CameraZoomRequest
        {
            public int zoom { get; set; }
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

        public string MoveCamera(string json)
        {
            Map map = Find.CurrentMap;
            var data = JsonConvert.DeserializeObject<MoveCameraRequest>(json);
            GlobalTargetInfo targetInfo = new GlobalTargetInfo(
                new IntVec3(data.x, 1, data.y),
                map
            );
            CameraJumper.TryJump(targetInfo);
            return $"{{\"status\": \"done\"}}";
        }

        //public string CameraFollowPawn(string json)
        //{
        //    var data = JsonConvert.DeserializeObject<FollowPawnRequest>(json);
        //    if (data == null)
        //        return "{\"error\": \"Invalid JSON data\"}";
        //
        //    int pawn_id = data.pawn_id;
        //    Pawn p = Find.CurrentMap?.mapPawns?.FreeColonists.FirstOrDefault(x => x.thingIDNumber == pawn_id);
        //    if (p == null)
        //    {
        //        return json;
        //    }
        //    FollowMe.FollowMe.TryStartFollow(p);
        //    return $"{{\"status\": \"done\"}}";
        //}

        public string SetCameraZoom(string json)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<ZoomRequest>(json);
                if (data == null)
                    return "{\"error\": \"Invalid JSON data\"}";

                string cameraType = data.cameraType?.ToLower() ?? "map";
                float zoomLevel = data.zoomLevel;

                if (cameraType == "world")
                {
                    // Set altitude for world camera using both altitude and desiredAltitude
                    var worldCamera = Find.WorldCameraDriver;
                    if (worldCamera != null)
                    {
                        // Clamp zoom level to valid range (MinAltitude to 1100f)
                        float minAltitude = WorldCameraDriver.MinAltitude;
                        float clampedZoom = Mathf.Clamp(zoomLevel, minAltitude, 1100f);

                        // Set altitude directly for immediate zoom
                        worldCamera.altitude = clampedZoom;

                        // Use reflection to set desiredAltitude for smooth zoom
                        var desiredAltitudeField = typeof(WorldCameraDriver).GetField("desiredAltitude",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (desiredAltitudeField != null)
                        {
                            desiredAltitudeField.SetValue(worldCamera, clampedZoom);
                        }

                        return $"{{\"status\": \"success\", \"camera\": \"world\", \"zoom\": {clampedZoom}}}";
                    }
                    return "{\"error\": \"World camera not available\"}";
                }
                else if (cameraType == "map")
                {
                    var mapCamera = Find.CameraDriver;
                    if (mapCamera != null)
                    {
                        // Clamp to engine-configured size range to match CameraDriver behavior
                        float minSize = mapCamera.config.sizeRange.min;
                        float maxSize = mapCamera.config.sizeRange.max;
                        float clampedZoom = Mathf.Clamp(zoomLevel, minSize, maxSize);

                        // Preserve current root position and avoid RootSize setter side effects (zoom-to-mouse)
                        var rootPosField = typeof(CameraDriver).GetField("rootPos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var desiredSizeField = typeof(CameraDriver).GetField("desiredSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var rootSizeField = typeof(CameraDriver).GetField("rootSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var applyMethod = typeof(CameraDriver).GetMethod("ApplyPositionToGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        if (rootPosField != null && desiredSizeField != null && rootSizeField != null && applyMethod != null)
                        {
                            // Read current root position to keep it unchanged
                            Vector3 currentRootPos = (Vector3)rootPosField.GetValue(mapCamera);

                            // Set sizes directly to avoid RootSize property's zoom-to-mouse side-effect
                            desiredSizeField.SetValue(mapCamera, clampedZoom);
                            rootSizeField.SetValue(mapCamera, clampedZoom);

                            // Restore rootPos x/z before applying so position is preserved
                            Vector3 currentRootPosNow = (Vector3)rootPosField.GetValue(mapCamera);
                            rootPosField.SetValue(mapCamera, new Vector3(currentRootPos.x, currentRootPosNow.y, currentRootPos.z));

                            // Apply transforms (will recompute y and set orthographic size)
                            applyMethod.Invoke(mapCamera, null);

                            return $"{{\"status\": \"success\", \"camera\": \"map\", \"zoom\": {clampedZoom}}}";
                        }

                        return "{\"error\": \"Unable to set map camera zoom (reflection)\"}";
                    }
                    return "{\"error\": \"Map camera not available\"}";
                }
                else
                {
                    return "{\"error\": \"Invalid camera type. Use 'world' or 'map'\"}";
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error in SetCameraZoom: {e}");
                return "{\"error\": \"Failed to set zoom\"}";
            }
        }

        public string GetCameraZoom()
        {
            try
            {
                float worldZoom = Find.WorldCameraDriver?.altitude ?? 0f;
                float mapZoom = Find.CameraDriver?.ZoomRootSize ?? 0f;

                var data = new GetZoomRequest();
                data.worldZoom = worldZoom;
                data.mapZoom = mapZoom;

                return JsonConvert.SerializeObject(data);
            }
            catch (Exception e)
            {
                Log.Error($"Error in GetCameraZoom: {e}");
                return "{\"error\": \"Failed to get zoom levels\"}";
            }
        }

        private class ZoomRequest
        {
            public string cameraType { get; set; }
            public float zoomLevel { get; set; }
        }

        private class GetZoomRequest
        {
            public float worldZoom { get; set; }
            public float mapZoom { get; set; }
        }

        private class MoveCameraRequest
        {
            public int x { get; set; }
            public int y { get; set; }
        }


        public string GetSpawnableItems()
        {
            try
            {
                Log.Message("[RIMAPI] Getting spawnable items list");

                var spawnableItems = new List<object>();

                // Get all thingDefs that can be spawned as items
                foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
                {
                    // Filter for spawnable items (not terrain, not abstract, not blueprint, etc.)
                    if (thingDef.forceDebugSpawnable)
                    {
                        var itemInfo = new Dictionary<string, object>
                        {
                            ["defName"] = thingDef.defName,
                            ["label"] = thingDef.label,
                            ["description"] = thingDef.description,
                            ["category"] = thingDef.category.ToString(),
                            ["stackLimit"] = thingDef.stackLimit,
                            ["baseMarketValue"] = thingDef.BaseMarketValue,
                            ["techLevel"] = thingDef.techLevel.ToString(),
                            ["isWeapon"] = thingDef.IsWeapon,
                            ["isApparel"] = thingDef.IsApparel,
                            ["isMedicine"] = thingDef.IsMedicine,
                            ["isDrug"] = thingDef.IsDrug,
                            ["isBuilding"] = thingDef.IsBuildingArtificial,
                            ["isResource"] = thingDef.CountAsResource
                        };

                        spawnableItems.Add(itemInfo);
                    }
                }

                // Sort by category and label
                spawnableItems = spawnableItems
                    .OrderBy(item => ((Dictionary<string, object>)item)["category"])
                    .ThenBy(item => ((Dictionary<string, object>)item)["label"])
                    .ToList();

                string json = JsonConvert.SerializeObject(new { items = spawnableItems });
                Log.Message($"[RIMAPI] Found {spawnableItems.Count} spawnable items");

                return json;
            }
            catch (Exception ex)
            {
                Log.Error("[RIMAPI] GetSpawnableItems error: " + ex);
                return "{\"error\": \"Failed to get spawnable items: " + ex.Message + "\"}";
            }
        }



        public string SpawnPawn(string requestBody)
        {
            try
            {
                Log.Message("[RIMAPI] SpawnPawn request: " + requestBody);

                // Parse JSON request
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBody);

                // Extract parameters with defaults
                string name = data.ContainsKey("name") ? data["name"].ToString() : "Visitor";
                string kindDef = data.ContainsKey("kindDef") ? data["kindDef"].ToString() : "Colonist";
                int tileX = data.ContainsKey("tileX") ? Convert.ToInt32(data["tileX"]) : -1;
                int tileY = data.ContainsKey("tileY") ? Convert.ToInt32(data["tileY"]) : -1;
                string faction = data.ContainsKey("faction") ? data["faction"].ToString() : "PlayerColony";

                // Get current map
                Map currentMap = Find.CurrentMap;
                if (currentMap == null)
                {
                    return "{\"error\": \"No map available\"}";
                }

                // Determine spawn position
                IntVec3 spawnPos;
                if (tileX >= 0 && tileY >= 0)
                {
                    spawnPos = new IntVec3(tileX, 0, tileY);
                    if (!spawnPos.InBounds(currentMap))
                    {
                        spawnPos = FindRandomSpawnPosition(currentMap);
                    }
                }
                else
                {
                    spawnPos = FindRandomSpawnPosition(currentMap);
                }

                // Create pawn request
                PawnGenerationRequest request = new PawnGenerationRequest(
                    kind: PawnKindDef.Named(kindDef),
                    faction: Faction.OfPlayer, // Spawn as colonist
                    context: PawnGenerationContext.NonPlayer,
                    tile: -1,
                    forceGenerateNewPawn: true,
                    developmentalStages: DevelopmentalStage.Adult,
                    allowDead: false,
                    allowDowned: false,
                    canGeneratePawnRelations: true,
                    mustBeCapableOfViolence: false,
                    colonistRelationChanceFactor: 1f,
                    forceAddFreeWarmLayerIfNeeded: false,
                    allowGay: true,
                    allowFood: true,
                    allowAddictions: true,
                    fixedBiologicalAge: null,
                    fixedChronologicalAge: null,
                    fixedGender: null,
                    fixedLastName: null
                );

                // Generate the pawn
                Pawn newPawn = PawnGenerator.GeneratePawn(request);

                // Set custom name if provided
                if (!string.IsNullOrEmpty(name) && name != "Visitor")
                {
                    newPawn.Name = new NameSingle(name);
                }

                // Spawn the pawn in the world
                GenSpawn.Spawn(newPawn, spawnPos, currentMap);

                Log.Message($"[RIMAPI] Spawned pawn: {name} at {spawnPos}");

                return "{\"success\": true, \"pawn\": \"" + name + "\", \"position\": \"" + spawnPos + "\"}";
            }
            catch (Exception ex)
            {
                Log.Error("[RIMAPI] SpawnPawn error: " + ex);
                return "{\"error\": \"Failed to spawn pawn: " + ex.Message + "\"}";
            }
        }

        public string SpawnItem(string requestBody)
        {
            try
            {
                Log.Message("[RIMAPI] SpawnItem request: " + requestBody);

                // Parse JSON request
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBody);

                // Extract parameters
                string thingDef = data.ContainsKey("thingDef") ? data["thingDef"].ToString() : "WoodLog";
                int stackCount = data.ContainsKey("stackCount") ? Convert.ToInt32(data["stackCount"]) : 1;
                int tileX = data.ContainsKey("tileX") ? Convert.ToInt32(data["tileX"]) : -1;
                int tileY = data.ContainsKey("tileY") ? Convert.ToInt32(data["tileY"]) : -1;
                string quality = data.ContainsKey("quality") ? data["quality"].ToString() : "Normal";

                // Get current map
                Map currentMap = Find.CurrentMap;
                if (currentMap == null)
                {
                    return "{\"error\": \"No map available\"}";
                }

                // Find thing def
                ThingDef thingDefToSpawn = DefDatabase<ThingDef>.GetNamedSilentFail(thingDef);
                if (thingDefToSpawn == null)
                {
                    return "{\"error\": \"Unknown item type: " + thingDef + "\"}";
                }

                // Determine spawn position
                IntVec3 spawnPos;
                if (tileX >= 0 && tileY >= 0)
                {
                    spawnPos = new IntVec3(tileX, 0, tileY);
                    if (!spawnPos.InBounds(currentMap) || !spawnPos.Walkable(currentMap))
                    {
                        spawnPos = FindRandomItemSpawnPosition(currentMap);
                    }
                }
                else
                {
                    spawnPos = FindRandomItemSpawnPosition(currentMap);
                }

                // Create the item
                Thing newItem = ThingMaker.MakeThing(thingDefToSpawn);

                // Set stack count
                if (newItem.def.stackLimit > 1 && stackCount > 1)
                {
                    newItem.stackCount = Math.Min(stackCount, newItem.def.stackLimit);
                }

                // Set quality if applicable
                CompQuality compQuality = newItem.TryGetComp<CompQuality>();
                if (compQuality != null)
                {
                    QualityCategory qualityCategory;
                    if (Enum.TryParse<QualityCategory>(quality, out qualityCategory))
                    {
                        compQuality.SetQuality(qualityCategory, ArtGenerationContext.Outsider);
                    }
                }

                // Spawn the item
                GenSpawn.Spawn(newItem, spawnPos, currentMap);

                Log.Message($"[RIMAPI] Spawned item: {thingDef} x{stackCount} at {spawnPos}");

                return "{\"success\": true, \"item\": \"" + thingDef + "\", \"count\": " + stackCount + ", \"position\": \"" + spawnPos + "\"}";
            }
            catch (Exception ex)
            {
                Log.Error("[RIMAPI] SpawnItem error: " + ex);
                return "{\"error\": \"Failed to spawn item: " + ex.Message + "\"}";
            }
        }

        public string TriggerEvent(string requestBody)
        {
            try
            {
                Log.Message("[RIMAPI] TriggerEvent request: " + requestBody);

                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBody);
                string eventType = data.ContainsKey("eventType") ? data["eventType"].ToString() : "visitor";
                string intensity = data.ContainsKey("intensity") ? data["intensity"].ToString() : "medium";

                Map currentMap = Find.CurrentMap;
                if (currentMap == null)
                {
                    return "{\"error\": \"No map available\"}";
                }

                switch (eventType.ToLower())
                {
                    case "raid_small":
                        TriggerRaid(currentMap, 1, 3);
                        break;

                    case "raid_medium":
                        TriggerRaid(currentMap, 2, 5);
                        break;

                    case "trader_visit":
                        TriggerTraderVisit(currentMap);
                        break;

                    case "animal_attack":
                        TriggerAnimalAttack(currentMap, intensity);
                        break;

                    case "mental_break":
                        TriggerMentalBreak();
                        break;

                    case "visitor":
                        TriggerVisitorGroup(currentMap);
                        break;

                    default:
                        return "{\"error\": \"Unknown event type: " + eventType + "\"}";
                }

                return "{\"success\": true, \"event\": \"" + eventType + "\", \"intensity\": \"" + intensity + "\"}";
            }
            catch (Exception ex)
            {
                Log.Error("[RIMAPI] TriggerEvent error: " + ex);
                return "{\"error\": \"Failed to trigger event: " + ex.Message + "\"}";
            }
        }

        // Helper methods for the spawn functions
        private IntVec3 FindRandomSpawnPosition(Map map)
        {
            CellRect cellRect = CellRect.WholeMap(map);
            for (int i = 0; i < 50; i++)
            {
                IntVec3 randomCell = cellRect.RandomCell;
                if (randomCell.InBounds(map) && randomCell.Standable(map) && !randomCell.Fogged(map))
                {
                    return randomCell;
                }
            }
            return DropCellFinder.TradeDropSpot(map);
        }

        private IntVec3 FindRandomItemSpawnPosition(Map map)
        {
            // Try to find a spot near colonists first
            Pawn randomColonist = map.mapPawns.FreeColonists.RandomElementWithFallback();
            if (randomColonist != null)
            {
                CellRect aroundColonist = CellRect.CenteredOn(randomColonist.Position, 5);
                for (int i = 0; i < 30; i++)
                {
                    IntVec3 randomCell = aroundColonist.RandomCell;
                    if (randomCell.InBounds(map) && randomCell.Standable(map) && !randomCell.Fogged(map))
                    {
                        return randomCell;
                    }
                }
            }

            // Fallback to general random position
            return FindRandomSpawnPosition(map);
        }

        // Event triggering helper methods
        private void TriggerRaid(Map map, int pointsFactor, int pawnCount)
        {
            try
            {
                IncidentParms parms = new IncidentParms
                {
                    target = map,
                    points = StorytellerUtility.DefaultThreatPointsNow(map) * pointsFactor,
                    pawnCount = pawnCount,
                    faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined),
                    raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn
                };

                IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
            }
            catch (Exception ex)
            {
                Log.Error("[RIMAPI] TriggerRaid error: " + ex);
            }
        }

        private void TriggerTraderVisit(Map map)
        {
            try
            {
                IncidentParms parms = new IncidentParms
                {
                    target = map,
                    faction = Find.FactionManager.RandomNonHostileFaction(false, false, true, TechLevel.Undefined)
                };

                IncidentDefOf.OrbitalTraderArrival.Worker.TryExecute(parms);
            }
            catch (Exception ex)
            {
                Log.Error("[RIMAPI] TriggerTraderVisit error: " + ex);
            }
        }

        private void TriggerAnimalAttack(Map map, string intensity)
        {
            try
            {
                PawnKindDef animalDef = DefDatabase<PawnKindDef>.AllDefs
                    .Where(def => def.RaceProps.Animal && def.RaceProps.predator)
                    .RandomElement();

                if (animalDef != null)
                {
                    int count = intensity == "high" ? 3 : intensity == "medium" ? 2 : 1;

                    for (int i = 0; i < count; i++)
                    {
                        Pawn animal = PawnGenerator.GeneratePawn(animalDef);
                        IntVec3 spawnPos = FindRandomSpawnPosition(map);
                        GenSpawn.Spawn(animal, spawnPos, map);

                        // Make them manhunter
                        animal.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[RIMAPI] TriggerAnimalAttack error: " + ex);
            }
        }

        private void TriggerMentalBreak()
        {
            try
            {
                Pawn colonist = Find.CurrentMap?.mapPawns?.FreeColonists?.RandomElementWithFallback();
                if (colonist != null)
                {
                    MentalStateDef[] breakDefs = new MentalStateDef[]
                    {
                        MentalStateDefOf.Berserk,
                        MentalStateDefOf.Rebellion,
                        MentalStateDefOf.Wander_Sad,
                    };

                    MentalStateDef randomBreak = breakDefs.RandomElement();
                    colonist.mindState.mentalStateHandler.TryStartMentalState(randomBreak);
                }
            }
            catch (Exception ex)
            {
                Log.Error("[RIMAPI] TriggerMentalBreak error: " + ex);
            }
        }

        private void TriggerVisitorGroup(Map map)
        {
            try
            {
                IncidentParms parms = new IncidentParms
                {
                    target = map,
                    faction = Find.FactionManager.RandomNonHostileFaction(false, false, true, TechLevel.Undefined),
                    pawnCount = Rand.Range(1, 4)
                };

                IncidentDefOf.TravelerGroup.Worker.TryExecute(parms);
            }
            catch (Exception ex)
            {
                Log.Error("[RIMAPI] TriggerVisitorGroup error: " + ex);
            }
        }

        public string GetForbiddenItems()
        {
            try
            {
                Log.Message("[RIMAPI] Getting forbidden items list");

                Map currentMap = Find.CurrentMap;
                if (currentMap == null)
                {
                    return "{\"error\": \"No map available\"}";
                }

                var forbiddenItems = new List<object>();

                // Get all items on the map
                foreach (Thing thing in currentMap.listerThings.AllThings)
                {
                    // Skip if not an item, or if it's a plant, terrain, etc.
                    if (thing.def.category != ThingCategory.Item &&
                        thing.def.category != ThingCategory.Building &&
                        thing.def.category != ThingCategory.Plant)
                        continue;

                    // Skip if it's a pawn or corpse
                    if (thing is Pawn || thing.def.IsCorpse)
                        continue;

                    bool isForbidden = thing.IsForbidden(Faction.OfPlayer);
                    bool isInStorage = thing.Spawned && thing.Position.GetSlotGroup(currentMap) != null;
                    bool isAccessible = IsItemAccessible(thing);

                    // Only include items that are forbidden OR in storage but still useful to report
                    if (isForbidden || isInStorage)
                    {
                        var itemInfo = new Dictionary<string, object>
                        {
                            ["thingId"] = thing.thingIDNumber,
                            ["defName"] = thing.def.defName,
                            ["label"] = thing.Label,
                            ["category"] = thing.def.category.ToString(),
                            ["isForbidden"] = isForbidden,
                            ["isInStorage"] = isInStorage,
                            ["isAccessible"] = isAccessible,
                            ["stackCount"] = thing.stackCount,
                            ["position"] = new { x = thing.Position.x, z = thing.Position.z },
                            ["marketValue"] = thing.MarketValue * thing.stackCount,
                            ["hitPoints"] = thing.HitPoints,
                            ["maxHitPoints"] = thing.MaxHitPoints
                        };

                        // Add specific properties based on item type
                        if (thing.def.IsWeapon)
                        {
                            itemInfo["weaponType"] = thing.def.defName;
                            itemInfo["isRanged"] = thing.def.IsRangedWeapon;
                            itemInfo["isMelee"] = thing.def.IsMeleeWeapon;
                        }

                        if (thing.def.IsApparel)
                        {
                            itemInfo["apparelType"] = thing.def.apparel?.ToString() ?? "Unknown";
                            itemInfo["wornBy"] = (thing as Apparel)?.Wearer?.LabelShortCap ?? "None";
                        }

                        if (thing.def.IsMedicine)
                        {
                            itemInfo["medicalPotency"] = thing.def.GetStatValueAbstract(StatDefOf.MedicalPotency);
                        }

                        forbiddenItems.Add(itemInfo);
                    }
                }

                // Sort by value and importance
                forbiddenItems = forbiddenItems
                    .OrderByDescending(item => ((Dictionary<string, object>)item)["isForbidden"])
                    .ThenByDescending(item => ((Dictionary<string, object>)item)["marketValue"])
                    .ToList();

                string json = JsonConvert.SerializeObject(new
                {
                    items = forbiddenItems,
                    totalCount = forbiddenItems.Count,
                    forbiddenCount = forbiddenItems.Count(item => (bool)((Dictionary<string, object>)item)["isForbidden"])
                });

                Log.Message($"[RIMAPI] Found {forbiddenItems.Count} items ({forbiddenItems.Count(item => (bool)((Dictionary<string, object>)item)["isForbidden"])} forbidden)");

                return json;
            }
            catch (Exception ex)
            {
                Log.Error("[RIMAPI] GetForbiddenItems error: " + ex);
                return "{\"error\": \"Failed to get forbidden items: " + ex.Message + "\"}";
            }
        }

        public string SetItemForbidden(string requestBody)
        {
            try
            {
                Log.Message("[RIMAPI] SetItemForbidden request: " + requestBody);

                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBody);
                int thingId = data.ContainsKey("thingId") ? Convert.ToInt32(data["thingId"]) : -1;
                bool forbidden = data.ContainsKey("forbidden") ? Convert.ToBoolean(data["forbidden"]) : false;

                Map currentMap = Find.CurrentMap;
                if (currentMap == null)
                {
                    return "{\"error\": \"No map available\"}";
                }

                // Find the thing by ID
                Thing thing = currentMap.listerThings.AllThings.FirstOrDefault(t => t.thingIDNumber == thingId);
                if (thing == null)
                {
                    return "{\"error\": \"Item not found with ID: " + thingId + "\"}";
                }

                // Set forbidden state
                thing.SetForbidden(forbidden, false);

                Log.Message($"[RIMAPI] Set item {thing.Label} (ID: {thingId}) forbidden = {forbidden}");

                return "{\"success\": true, \"thingId\": " + thingId + ", \"forbidden\": " + forbidden.ToString().ToLower() + ", \"label\": \"" + thing.Label + "\"}";
            }
            catch (Exception ex)
            {
                Log.Error("[RIMAPI] SetItemForbidden error: " + ex);
                return "{\"error\": \"Failed to set item forbidden state: " + ex.Message + "\"}";
            }
        }

        public string SetMultipleItemsForbidden(string requestBody)
        {
            try
            {
                Log.Message("[RIMAPI] SetMultipleItemsForbidden request");

                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBody);
                var thingIds = data.ContainsKey("thingIds") ?
                    JsonConvert.DeserializeObject<List<int>>(data["thingIds"].ToString()) : new List<int>();
                bool forbidden = data.ContainsKey("forbidden") ? Convert.ToBoolean(data["forbidden"]) : false;
                string filterType = data.ContainsKey("filterType") ? data["filterType"].ToString() : "specific";

                Map currentMap = Find.CurrentMap;
                if (currentMap == null)
                {
                    return "{\"error\": \"No map available\"}";
                }

                int successCount = 0;
                int totalCount = 0;

                if (filterType == "specific" && thingIds.Any())
                {
                    // Set specific items by ID
                    foreach (int thingId in thingIds)
                    {
                        Thing thing = currentMap.listerThings.AllThings.FirstOrDefault(t => t.thingIDNumber == thingId);
                        if (thing != null)
                        {
                            thing.SetForbidden(forbidden, false);
                            successCount++;
                        }
                        totalCount++;
                    }
                }
                else if (filterType == "category")
                {
                    // Set all items of a specific category
                    string category = data.ContainsKey("category") ? data["category"].ToString() : "Resource";

                    // Get all items in the specified category
                    var itemsInCategory = currentMap.listerThings.AllThings
                        .Where(t => t.def.category.ToString() == category && t.def.EverStorable(false))
                        .ToList();

                    foreach (Thing thing in itemsInCategory)
                    {
                        thing.SetForbidden(forbidden, false);
                        successCount++;
                    }
                    totalCount = itemsInCategory.Count;

                    Log.Message($"[RIMAPI] Set {successCount} items in category '{category}' to forbidden = {forbidden}");
                }
                else if (filterType == "all")
                {
                    // Set all forbidden items to allowed (or vice versa)
                    var allItems = currentMap.listerThings.AllThings
                        .Where(t => t.def.EverStorable(false) && t.IsForbidden(Faction.OfPlayer) != forbidden)
                        .ToList();

                    foreach (Thing thing in allItems)
                    {
                        thing.SetForbidden(forbidden, false);
                        successCount++;
                    }
                    totalCount = allItems.Count;
                }

                Log.Message($"[RIMAPI] Set {successCount}/{totalCount} items forbidden = {forbidden}");

                return "{\"success\": true, \"processed\": " + successCount + ", \"total\": " + totalCount + ", \"forbidden\": " + forbidden.ToString().ToLower() + "}";
            }
            catch (Exception ex)
            {
                Log.Error("[RIMAPI] SetMultipleItemsForbidden error: " + ex);
                return "{\"error\": \"Failed to set multiple items forbidden state: " + ex.Message + "\"}";
            }
        }

        public string GetResourceSummary()
        {
            try
            {
                Log.Message("[RIMAPI] Getting resource summary");

                Map currentMap = Find.CurrentMap;
                if (currentMap == null)
                {
                    return "{\"error\": \"No map available\"}";
                }

                var resourceSummary = new Dictionary<string, object>();
                var categories = new Dictionary<string, object>();

                // Get all storable items
                var allItems = currentMap.listerThings.AllThings
                    .Where(t => t.def.EverStorable(false))
                    .ToList();

                // Group by category and forbidden state
                foreach (var categoryGroup in allItems.GroupBy(t => t.def.category.ToString()))
                {
                    string category = categoryGroup.Key;
                    var categoryItems = categoryGroup.ToList();

                    int totalCount = categoryItems.Count;
                    int forbiddenCount = categoryItems.Count(t => t.IsForbidden(Faction.OfPlayer));
                    int accessibleCount = categoryItems.Count(t => !t.IsForbidden(Faction.OfPlayer) && IsItemAccessible(t));
                    double totalValue = categoryItems.Sum(t => t.MarketValue * t.stackCount);

                    categories[category] = new Dictionary<string, object>
                    {
                        ["totalItems"] = totalCount,
                        ["forbiddenItems"] = forbiddenCount,
                        ["accessibleItems"] = accessibleCount,
                        ["totalValue"] = totalValue,
                        ["forbiddenValue"] = categoryItems.Where(t => t.IsForbidden(Faction.OfPlayer)).Sum(t => t.MarketValue * t.stackCount)
                    };
                }

                // Overall statistics
                int totalAllItems = allItems.Count;
                int totalForbidden = allItems.Count(t => t.IsForbidden(Faction.OfPlayer));
                int totalAccessible = allItems.Count(t => !t.IsForbidden(Faction.OfPlayer) && IsItemAccessible(t));
                double totalAllValue = allItems.Sum(t => t.MarketValue * t.stackCount);
                double forbiddenValue = allItems.Where(t => t.IsForbidden(Faction.OfPlayer)).Sum(t => t.MarketValue * t.stackCount);

                resourceSummary["categories"] = categories;
                resourceSummary["overall"] = new Dictionary<string, object>
                {
                    ["totalItems"] = totalAllItems,
                    ["forbiddenItems"] = totalForbidden,
                    ["accessibleItems"] = totalAccessible,
                    ["totalValue"] = totalAllValue,
                    ["forbiddenValue"] = forbiddenValue,
                    ["forbiddenPercentage"] = totalAllItems > 0 ? (double)totalForbidden / totalAllItems * 100 : 0
                };

                // Priority items (weapons, medicine, food)
                var priorityItems = allItems
                    .Where(t => t.def.IsWeapon || t.def.IsMedicine || t.def.IsIngestible)
                    .Where(t => t.IsForbidden(Faction.OfPlayer))
                    .OrderByDescending(t => t.MarketValue * t.stackCount)
                    .Take(10)
                    .Select(t => new Dictionary<string, object>
                    {
                        ["thingId"] = t.thingIDNumber,
                        ["defName"] = t.def.defName,
                        ["label"] = t.Label,
                        ["category"] = t.def.category.ToString(),
                        ["value"] = t.MarketValue * t.stackCount,
                        ["stackCount"] = t.stackCount
                    })
                    .ToList();

                resourceSummary["priorityForbiddenItems"] = priorityItems;

                string json = JsonConvert.SerializeObject(resourceSummary);
                return json;
            }
            catch (Exception ex)
            {
                Log.Error("[RIMAPI] GetResourceSummary error: " + ex);
                return "{\"error\": \"Failed to get resource summary: " + ex.Message + "\"}";
            }
        }

        private bool IsItemAccessible(Thing thing)
        {
            try
            {
                if (thing?.Map == null) return false;

                // Check if any colonist can path to the item
                foreach (Pawn colonist in thing.Map.mapPawns.FreeColonists)
                {
                    if (colonist.CanReach(thing, Verse.AI.PathEndMode.Touch, Danger.Some))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public string CreateStorageZone(string requestBody)
        {
            try
            {
                Log.Message("[RIMAPI] CreateStorageZone request: " + requestBody);

                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBody);
                string purpose = data.ContainsKey("purpose") ? data["purpose"].ToString() : "general";
                var locationData = data.ContainsKey("location") ?
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(data["location"].ToString()) :
                    new Dictionary<string, object>();

                Map currentMap = Find.CurrentMap;
                if (currentMap == null)
                {
                    return "{\"error\": \"No map available\"}";
                }

                // Extract location parameters with defaults
                int x = locationData.ContainsKey("x") ? Convert.ToInt32(locationData["x"]) : 125;
                int z = locationData.ContainsKey("z") ? Convert.ToInt32(locationData["z"]) : 125;
                int width = locationData.ContainsKey("width") ? Convert.ToInt32(locationData["width"]) : 7;
                int height = locationData.ContainsKey("height") ? Convert.ToInt32(locationData["height"]) : 7;

                // Create zone bounds - ensure they're within map
                IntVec3 centerCell = new IntVec3(x, 0, z);
                if (!centerCell.InBounds(currentMap))
                {
                    // Find a safe center point
                    centerCell = currentMap.Center;
                }

                // Create a storage zone using RimWorld's Zone_Stockpile
                Zone_Stockpile stockpile = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, currentMap.zoneManager);

                // Configure based on purpose
                ConfigureStorageZone(stockpile, purpose, data);

                // Create a rectangular area around the center point
                CellRect zoneRect = CellRect.CenteredOn(centerCell, width / 2, height / 2);
                zoneRect.ClipInsideMap(currentMap);

                // Add cells to the zone
                foreach (IntVec3 cell in zoneRect)
                {
                    if (cell.InBounds(currentMap) &&
                        cell.Standable(currentMap) &&
                        cell.GetEdifice(currentMap) == null) // No buildings in the way
                    {
                        stockpile.AddCell(cell);
                    }
                }

                // Register the zone
                currentMap.zoneManager.RegisterZone(stockpile);

                Log.Message($"[RIMAPI] Created storage zone: {purpose} at {x},{z} size {width}x{height}");

                return "{\"success\": true, \"zone\": \"" + purpose + "\", \"location\": \"" + zoneRect + "\", \"cells\": " + stockpile.CellCount + "}";
            }
            catch (Exception ex)
            {
                Log.Error("[RIMAPI] CreateStorageZone error: " + ex);
                return "{\"error\": \"Failed to create storage zone: " + ex.Message + "\"}";
            }
        }

        private void ConfigureStorageZone(Zone_Stockpile stockpile, string purpose, Dictionary<string, object> data)
        {
            StorageSettings settings = stockpile.settings;

            // Clear default settings
            settings.filter.SetDisallowAll();

            // Configure based on purpose
            switch (purpose)
            {
                case "secure_storage":
                    // Weapons, armor, medicine
                    settings.filter.SetAllow(ThingCategoryDef.Named("Weapons"), true);
                    settings.filter.SetAllow(ThingCategoryDef.Named("Apparel"), true);
                    settings.filter.SetAllow(ThingCategoryDef.Named("Medicine"), true);
                    settings.filter.SetAllow(ThingCategoryDef.Named("Drugs"), true);
                    settings.Priority = StoragePriority.Critical;
                    stockpile.label = "Secure Storage";
                    break;

                case "food_storage":
                    // All food items
                    settings.filter.SetAllow(ThingCategoryDef.Named("Foods"), true);
                    settings.filter.SetAllow(ThingCategoryDef.Named("Meals"), true);
                    settings.filter.SetAllow(ThingCategoryDef.Named("PlantFoodRaw"), true);
                    settings.filter.SetAllow(ThingCategoryDef.Named("PlantMatter"), true);
                    settings.Priority = StoragePriority.Important;
                    stockpile.label = "Food Storage";
                    break;

                case "resource_storage":
                    // Raw resources and building materials
                    settings.filter.SetAllow(ThingCategoryDef.Named("ResourcesRaw"), true);
                    settings.filter.SetAllow(ThingCategoryDef.Named("Manufactured"), true);
                    settings.filter.SetAllow(ThingCategoryDef.Named("StoneBlocks"), true);
                    settings.filter.SetAllow(ThingCategoryDef.Named("Items"), true);
                    settings.Priority = StoragePriority.Normal;
                    stockpile.label = "Resource Storage";
                    break;

                case "general_storage":
                default:
                    // General items catch-all
                    settings.filter.SetAllow(ThingCategoryDef.Named("Items"), true);
                    settings.filter.SetAllow(ThingCategoryDef.Named("ResourcesRaw"), true);
                    settings.filter.SetAllow(ThingCategoryDef.Named("Manufactured"), true);
                    settings.Priority = StoragePriority.Low;
                    stockpile.label = "General Storage";
                    break;
            }

            // Apply any custom filters from the request
            if (data.ContainsKey("filters"))
            {
                var filters = JsonConvert.DeserializeObject<List<string>>(data["filters"].ToString());
                foreach (string filter in filters)
                {
                    ThingCategoryDef category = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(filter);
                    if (category != null)
                    {
                        settings.filter.SetAllow(category, true);
                    }
                }
            }
        }

        public string CreateBasicStorageZone(string requestBody)
        {
            try
            {
                Log.Message("[RIMAPI] CreateBasicStorageZone request: " + requestBody);

                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBody);
                var locationData = data.ContainsKey("location") ?
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(data["location"].ToString()) :
                    new Dictionary<string, object>();

                Map currentMap = Find.CurrentMap;
                if (currentMap == null)
                {
                    return "{\"error\": \"No map available\"}";
                }

                // Extract location parameters with defaults
                int x = locationData.ContainsKey("x") ? Convert.ToInt32(locationData["x"]) : 125;
                int z = locationData.ContainsKey("z") ? Convert.ToInt32(locationData["z"]) : 125;
                int radius = locationData.ContainsKey("radius") ? Convert.ToInt32(locationData["radius"]) : 3;

                // Create center cell
                IntVec3 centerCell = new IntVec3(x, 0, z);
                if (!centerCell.InBounds(currentMap))
                {
                    centerCell = currentMap.Center;
                }

                // Create a simple stockpile
                Zone_Stockpile stockpile = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, currentMap.zoneManager);

                // Add cells in a radius around center
                int cellsAdded = 0;
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(centerCell, radius, true))
                {
                    if (cell.InBounds(currentMap) &&
                        cell.Standable(currentMap) &&
                        cell.GetEdifice(currentMap) == null)
                    {
                        stockpile.AddCell(cell);
                        cellsAdded++;
                    }
                }

                if (cellsAdded == 0)
                {
                    return "{\"error\": \"No valid cells found for storage zone\"}";
                }

                currentMap.zoneManager.RegisterZone(stockpile);

                Log.Message($"[RIMAPI] Created basic storage zone with {cellsAdded} cells at {centerCell}");

                return "{\"success\": true, \"cells\": " + cellsAdded + ", \"center\": \"" + centerCell + "\"}";
            }
            catch (Exception ex)
            {
                Log.Error("[RIMAPI] CreateBasicStorageZone error: " + ex);
                return "{\"error\": \"Failed to create storage zone: " + ex.Message + "\"}";
            }
        }
    }
}
