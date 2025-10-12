

using System;
using System.Collections.Generic;
using RimWorld;
using RimworldRestApi.Models;
using UnityEngine;
using Verse;

namespace RimworldRestApi.Helpers
{
    public class MapHelper
    {
        public Map FindMapByUniqueID(int uniqueID)
        {
            foreach (Map map in Find.Maps)
            {
                if (map.uniqueID == uniqueID)
                {
                    return map;
                }
            }
            return null;
        }

        public List<MapDto> GetMaps()
        {
            var maps = new List<MapDto>();

            try
            {
                foreach (var map in Current.Game.Maps)
                {
                    maps.Add(new MapDto
                    {
                        ID = map.uniqueID,
                        Index = map.Index,
                        Seed = map.ConstantRandSeed,
                        FactionID = map.ParentFaction.loadID.ToString(),
                        IsPlayerHome = map.IsPlayerHome,
                        IsPocketMap = map.IsPocketMap,
                        IsTempIncidentMap = map.IsTempIncidentMap,
                        Size = map.Size.ToString()
                    });
                }

                return maps;
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting game maps - {ex.Message}");
                return maps;
            }
        }

        public MapTimeDto GetDatetimeAt(int tileID)
        {
            MapTimeDto mapTimeDto = new MapTimeDto();
            try
            {
                var vector = Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
                mapTimeDto.Datetime = GenDate.DateFullStringWithHourAt(Find.TickManager.TicksAbs, vector);

                return mapTimeDto;
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting game maps - {ex.Message}");
                return mapTimeDto;
            }
        }

        public MapPowerInfoDto GetMapPowerInfoInternal(int mapId)
        {
            MapPowerInfoDto powerInfo = new MapPowerInfoDto();

            try
            {
                Map map = FindMapByUniqueID(mapId);

                foreach (Building building in map.listerBuildings.allBuildingsColonist)
                {
                    // Check if building is - Power Generator
                    CompPowerPlant powerPlant = building.TryGetComp<CompPowerPlant>();
                    if (powerPlant != null)
                    {
                        powerInfo.TotalPossiblePower += Mathf.RoundToInt(
                            Mathf.Abs(powerPlant.Props.PowerConsumption)
                        );
                        powerInfo.CurrentPower += Mathf.RoundToInt(powerPlant.PowerOutput);
                        continue;
                    }

                    // Check if building is - Battery
                    CompPowerBattery powerBattery = building.TryGetComp<CompPowerBattery>();
                    if (powerBattery != null)
                    {
                        powerInfo.CurrentlyStoredPower += Mathf.RoundToInt(powerBattery.StoredEnergy);
                        powerInfo.TotalPowerStorage += Mathf.RoundToInt(powerBattery.Props.storedEnergyMax);
                    }
                }

                // Calculate power consumption
                foreach (PowerNet net in map.powerNetManager.AllNetsListForReading)
                {
                    foreach (CompPowerTrader comp in net.powerComps)
                    {
                        if (comp.Props.PowerConsumption > 0f)
                        {
                            powerInfo.TotalConsumption += Mathf.RoundToInt(comp.Props.PowerConsumption);
                        }
                        if (comp.PowerOn && comp.PowerOutput < 0f)
                        {
                            powerInfo.ConsumptionPowerOn += Mathf.RoundToInt(Mathf.Abs(comp.PowerOutput));
                        }
                    }
                }

                return powerInfo;
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting game maps - {ex.Message}");
                return powerInfo;
            }
        }
    }
}