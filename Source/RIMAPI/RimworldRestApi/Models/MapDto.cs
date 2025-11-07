using System;
using System.Collections.Generic;

namespace RimworldRestApi.Models
{
    public class MapDto
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public int Seed { get; set; }
        public string FactionId { get; set; }
        public bool IsPlayerHome { get; set; }
        public bool IsPocketMap { get; set; }
        public bool IsTempIncidentMap { get; set; }
        public string Size { get; set; }
    }

    public class MapWeatherDto
    {
        public string Weather { get; set; }
        public float Temperature { get; set; }
    }

    public class MapPowerInfoDto
    {
        public int CurrentPower { get; set; }
        public int TotalPossiblePower { get; set; }
        public int CurrentlyStoredPower { get; set; }
        public int TotalPowerStorage { get; set; }
        public int TotalConsumption { get; set; }
        public int ConsumptionPowerOn { get; set; }
        public List<int> ProducePowerBuildings = new List<int>();
        public List<int> ConsumePowerBuildings = new List<int>();
        public List<int> StorePowerBuildings = new List<int>();
    }

    public class MapTimeDto
    {
        public string Datetime { get; set; }
    }

    public class AnimalDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Def { get; set; }
        public string Faction { get; set; }
        public PositionDto Position { get; set; }
        public int? Trainer { get; set; }
        public bool Pregnant { get; set; }
    }

    public class MapCreaturesSummaryDto
    {
        public int ColonistsCount { get; set; }
        public int PrisonersCount { get; set; }
        public int EnemiesCount { get; set; }
        public int AnimalsCount { get; set; }
        public int InsectoidsCount { get; set; }
        public int MechanoidsCount { get; set; }
    }

    public class MapFarmSummaryDto
    {
        public int TotalGrowingZones { get; set; }
        public int TotalPlants { get; set; }
        public int TotalExpectedYield { get; set; }
        public int TotalInfectedPlants { get; set; }
        public float GrowthProgressAverage { get; set; }
        public List<CropTypeDto> CropTypes { get; set; } = new List<CropTypeDto>();
    }

    public class CropTypeDto
    {
        public string PlantDefName { get; set; }
        public string PlantLabel { get; set; }
        public string PlantCategory { get; set; }
        public int TotalPlants { get; set; }
        public int HarvestablePlants { get; set; }
        public int ExpectedYield { get; set; }
        public int InfectedCount { get; set; }
        public float GrowthProgressAverage { get; set; }
        public float DaysUntilHarvest { get; set; }
        public bool IsFullyGrown { get; set; }
        public bool IsHarvestable { get; set; }
        public int ZoneId { get; set; }
    }

    public class GrowingZoneDto
    {
        public ZoneDto Zone { get; set; }
        public string PlantDefName { get; set; }
        public int PlantCount { get; set; }
        public int DefExpectedYield { get; set; }
        public int ExpectedYield { get; set; }
        public int InfectedCount { get; set; }
        public float GrowthProgress { get; set; }
        public bool IsSowing { get; set; }
        public string SoilType { get; set; }
        public float Fertility { get; set; }
        public bool HasDying { get; set; }
        public bool HasDyingFromPollution { get; set; }
        public bool HasDyingFromNoPollution { get; set; }
    }

    public class MapZonesDto
    {
        public List<ZoneDto> Zones { get; set; }
        public List<ZoneDto> Areas { get; set; }
    }

    public class ZoneDto
    {
        public int Id { get; set; }
        public int CellsCount { get; set; }
        public string Label { get; set; }
        public string BaseLabel { get; set; }
        public string Type { get; set; }
    }
}