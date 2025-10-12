using System;

namespace RimworldRestApi.Models
{
    public class MapDto
    {
        public int ID { get; set; }
        public int Index { get; set; }
        public int Seed { get; set; }
        public string FactionID { get; set; }
        public bool IsPlayerHome { get; set; }
        public bool IsPocketMap { get; set; }
        public bool IsTempIncidentMap { get; set; }
        public string Size { get; set; }
    }

    public class MapPowerInfoDto
    {
        public int CurrentPower { get; set; }
        public int TotalPossiblePower { get; set; }
        public int CurrentlyStoredPower { get; set; }
        public int TotalPowerStorage { get; set; }
        public int TotalConsumption { get; set; }
        public int ConsumptionPowerOn { get; set; }
    }

    public class MapTimeDto
    {
        public string Datetime { get; set; }
    }
}