// GameEventsDto.cs
using System.Collections.Generic;


namespace RimworldRestApi.Models
{
    public class QuestsDto
    {
        public List<ActiveQuestDto> ActiveQuests { get; set; } = new List<ActiveQuestDto>();
        public List<ActiveQuestDto> HistoricalQuests { get; set; } = new List<ActiveQuestDto>();
    }

    public class IncidentsDto
    {
        public List<IncidentDto> Incidents { get; set; } = new List<IncidentDto>();
    }

    public class IncidentDto
    {
        public string IncidentDef { get; set; }
        public string Label { get; set; }
        public string Category { get; set; }
        public float IncidentHour { get; set; }
        public float DaysSinceOccurred { get; set; }
    }

    public class ActiveQuestDto
    {
        public string QuestDef { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public float ExpiryHours { get; set; }
        public List<string> Reward { get; set; }
    }
}