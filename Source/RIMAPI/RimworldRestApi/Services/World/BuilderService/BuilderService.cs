using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld;
using UnityEngine;
using Verse;

namespace RIMAPI.Services
{
    public class BuilderService : IBuilderService
    {
        public ApiResult<BlueprintDto> CopyArea(CopyAreaRequestDto request)
        {
            try
            {
                var map = MapHelper.GetMapByID(request.MapId);
                if (map == null) return ApiResult<BlueprintDto>.Fail($"Map {request.MapId} not found.");

                // Normalize Coordinates (ensure min/max are correct regardless of A/B order)
                int minX = Mathf.Min(request.PointA.X, request.PointB.X);
                int minZ = Mathf.Min(request.PointA.Z, request.PointB.Z);
                int maxX = Mathf.Max(request.PointA.X, request.PointB.X);
                int maxZ = Mathf.Max(request.PointA.Z, request.PointB.Z);

                var blueprint = new BlueprintDto
                {
                    Width = (maxX - minX) + 1,
                    Height = (maxZ - minZ) + 1
                };

                // Track added things to avoid duplicates for multi-tile buildings (like Geothermal Generators)
                HashSet<Thing> addedThings = new HashSet<Thing>();

                CellRect rect = new CellRect(minX, minZ, blueprint.Width, blueprint.Height);

                // Iterate every cell
                foreach (IntVec3 cell in rect)
                {
                    if (!cell.InBounds(map)) continue;

                    // 1. Save Terrain (Floor)
                    TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                    if (terrain != null && terrain.Removable) // Only copy constructed floors, not soil/sand
                    {
                        blueprint.Floors.Add(new SavedTerrainDto
                        {
                            DefName = terrain.defName,
                            RelX = cell.x - minX,
                            RelZ = cell.z - minZ
                        });
                    }

                    // 2. Save Buildings
                    List<Thing> things = cell.GetThingList(map);
                    foreach (var thing in things)
                    {
                        // We only want Buildings, created by players, that we haven't added yet
                        if (thing.def.category == ThingCategory.Building &&
                            thing.def.saveCompressible == false && // Skip filth/motes
                            !addedThings.Contains(thing))
                        {
                            // Important: For multi-tile buildings, only add them if their "InteractionCell" or "Position" is inside or near our rect.
                            // To be safe, we just check if it's the first time we see it.
                            addedThings.Add(thing);

                            blueprint.Buildings.Add(new SavedBuildingDto
                            {
                                DefName = thing.def.defName,
                                StuffDefName = thing.Stuff?.defName, // Material (WoodLog, Steel, etc)
                                RelX = thing.Position.x - minX, // Save relative to anchor
                                RelZ = thing.Position.z - minZ,
                                Rotation = thing.Rotation.AsInt
                            });
                        }
                    }
                }

                return ApiResult<BlueprintDto>.Ok(blueprint);
            }
            catch (Exception ex)
            {
                LogApi.Error($"Copy Error: {ex}");
                return ApiResult<BlueprintDto>.Fail(ex.Message);
            }
        }

        public ApiResult PasteArea(PasteAreaRequestDto request)
        {
            try
            {
                if (request.Blueprint == null) return ApiResult.Fail("Blueprint is null");

                var map = MapHelper.GetMapByID(request.MapId);
                if (map == null) return ApiResult.Fail($"Map {request.MapId} not found.");

                int anchorX = request.Position.X;
                int anchorZ = request.Position.Z;

                // 1. Paste Floors
                foreach (var floorDto in request.Blueprint.Floors)
                {
                    IntVec3 pos = new IntVec3(anchorX + floorDto.RelX, 0, anchorZ + floorDto.RelZ);
                    if (pos.InBounds(map))
                    {
                        var terrainDef = DefDatabase<TerrainDef>.GetNamedSilentFail(floorDto.DefName);
                        if (terrainDef != null)
                        {
                            map.terrainGrid.SetTerrain(pos, terrainDef);
                        }
                    }
                }

                // 2. Paste Buildings
                foreach (var buildDto in request.Blueprint.Buildings)
                {
                    IntVec3 pos = new IntVec3(anchorX + buildDto.RelX, 0, anchorZ + buildDto.RelZ);

                    if (!pos.InBounds(map)) continue;

                    // Resolve Definitions
                    ThingDef thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(buildDto.DefName);
                    if (thingDef == null) continue;

                    ThingDef stuffDef = null;
                    if (!string.IsNullOrEmpty(buildDto.StuffDefName))
                    {
                        stuffDef = DefDatabase<ThingDef>.GetNamedSilentFail(buildDto.StuffDefName);
                    }

                    // Optional: Clear obstacles in the spot before spawning
                    if (request.ClearObstacles)
                    {
                        var obstacles = pos.GetThingList(map).ToList(); // Copy list
                        foreach (var obs in obstacles)
                        {
                            // Destroy items/buildings in the way. Don't destroy pawns.
                            if (obs.def.category == ThingCategory.Building || obs.def.category == ThingCategory.Item || obs.def.category == ThingCategory.Plant)
                            {
                                obs.Destroy();
                            }
                        }
                    }

                    // Create & Spawn
                    Thing thing = ThingMaker.MakeThing(thingDef, stuffDef);
                    thing.Rotation = new Rot4(buildDto.Rotation);

                    // Force the faction to be the player's
                    if (thing.def.CanHaveFaction)
                    {
                        thing.SetFaction(Faction.OfPlayer);
                    }

                    GenSpawn.Spawn(thing, pos, map, thing.Rotation);
                }

                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Paste Error: {ex}");
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult PlaceBlueprints(PasteAreaRequestDto request)
        {
            try
            {
                if (request.Blueprint == null) return ApiResult.Fail("Blueprint is null");

                var map = MapHelper.GetMapByID(request.MapId);
                if (map == null) return ApiResult.Fail($"Map {request.MapId} not found.");

                int anchorX = request.Position.X;
                int anchorZ = request.Position.Z;
                int count = 0;

                // 1. Place Floor Blueprints
                foreach (var floorDto in request.Blueprint.Floors)
                {
                    IntVec3 pos = new IntVec3(anchorX + floorDto.RelX, 0, anchorZ + floorDto.RelZ);
                    if (pos.InBounds(map))
                    {
                        TerrainDef terrainDef = DefDatabase<TerrainDef>.GetNamedSilentFail(floorDto.DefName);
                        if (terrainDef != null)
                        {
                            // PlaceBlueprintForBuild works for TerrainDefs too
                            GenConstruct.PlaceBlueprintForBuild(terrainDef, pos, map, Rot4.North, Faction.OfPlayer, null);
                            count++;
                        }
                    }
                }

                // 2. Place Building Blueprints
                foreach (var buildDto in request.Blueprint.Buildings)
                {
                    IntVec3 pos = new IntVec3(anchorX + buildDto.RelX, 0, anchorZ + buildDto.RelZ);

                    if (!pos.InBounds(map)) continue;

                    // Resolve Definitions
                    ThingDef thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(buildDto.DefName);
                    if (thingDef == null) continue;

                    ThingDef stuffDef = null;
                    if (!string.IsNullOrEmpty(buildDto.StuffDefName))
                    {
                        stuffDef = DefDatabase<ThingDef>.GetNamedSilentFail(buildDto.StuffDefName);
                    }

                    // Create Blueprint
                    // Note: GenConstruct handles checking if it can be placed, checking affordance, etc.
                    GenConstruct.PlaceBlueprintForBuild(
                        thingDef,
                        pos,
                        map,
                        new Rot4(buildDto.Rotation),
                        Faction.OfPlayer,
                        stuffDef
                    );
                    count++;
                }

                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Blueprint Error: {ex}");
                return ApiResult.Fail(ex.Message);
            }
        }
    }
}