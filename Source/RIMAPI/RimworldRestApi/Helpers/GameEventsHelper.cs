

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimworldRestApi.Models;
using Verse;

namespace RimworldRestApi.Helpers
{
    class GameEventsHelper
    {
        public QuestsDto GetQuestsDto(Map map)
        {
            var dto = new QuestsDto();

            List<Quest> allQuests = Find.QuestManager.QuestsListForReading;

            dto.HistoricalQuests.AddRange(allQuests
                .Where(quest => quest.Historical)
                .Select(quest => new ActiveQuestDto
                {
                    QuestDef = quest.root?.defName ?? "Unknown",
                    Name = quest.name,
                    Description = quest.description.ToString(),
                    State = quest.State.ToString(),
#if RIMWORLD_1_5
                    ExpiryHours = TimeHelper.TicksToDays(quest.ticksUntilAcceptanceExpiry) * 24,
#elif RIMWORLD_1_6
                    ExpiryHours = TimeHelper.TicksToDays(quest.TicksUntilExpiry) * 24,
#endif
                    Reward = GetQuestRewardString(quest)
                }));

            dto.ActiveQuests.AddRange(allQuests
                .Where(quest => !quest.Historical)
                .Select(quest => new ActiveQuestDto
                {
                    QuestDef = quest.root?.defName ?? "Unknown",
                    Name = quest.name,
                    Description = quest.description.ToString(),
                    State = quest.State.ToString(),
#if RIMWORLD_1_5
                    ExpiryHours = TimeHelper.TicksToDays(quest.ticksUntilAcceptanceExpiry) * 24,
#elif RIMWORLD_1_6
                    ExpiryHours = TimeHelper.TicksToDays(quest.TicksUntilExpiry) * 24,
#endif
                    Reward = GetQuestRewardString(quest)
                }));
            return dto;
        }

        private List<string> GetQuestRewardString(Quest quest)
        {
            return quest.PartsListForReading
                    .OfType<QuestPart_Choice>()
                    .SelectMany(choicePart => choicePart.choices)
                    .SelectMany(choice => choice.rewards)
                    .Select(reward => reward?.ToString() ?? "Unknown")
                    .ToList();
        }

        private string GetIncidentImportance(IncidentDef def)
        {
            if (def.category == IncidentCategoryDefOf.ThreatBig)
                return "ThreatBig";
            if (def.category == IncidentCategoryDefOf.ThreatSmall)
                return "ThreatSmall";
            if (def.category == IncidentCategoryDefOf.DeepDrillInfestation)
                return "DeepDrillInfestation";
            if (def.category == IncidentCategoryDefOf.DiseaseHuman)
                return "DiseaseHuman";
            if (def.category == IncidentCategoryDefOf.GiveQuest)
                return "GiveQuest";
            if (def.category == IncidentCategoryDefOf.Misc)
                return "Misc";
            if (def.category == IncidentCategoryDefOf.Special)
                return "Special";
            return "None";
        }

        public List<IncidentDto> GetIncidentsLog(Map map)  
        {  
            List<IncidentDto> incidentLog = new List<IncidentDto>();  
            
            if (map?.storyState?.lastFireTicks == null)  
                return incidentLog;  
            
            foreach (KeyValuePair<IncidentDef, int> entry in map.storyState.lastFireTicks)  
            {  
                incidentLog.Add(new IncidentDto  
                {  
                    IncidentDef = entry.Key.defName,  
                    Label = entry.Key.label,  
                    Category = GetIncidentImportance(entry.Key), 
                    IncidentHour = TimeHelper.TicksToDays(entry.Value) * 24,
                    DaysSinceOccurred = (Find.TickManager.TicksGame - entry.Value).TicksToDays(),
                });  
            }  
            
            // Sort by most recent first  
            incidentLog.Sort((a, b) => b.IncidentHour.CompareTo(a.IncidentHour));  
            
            return incidentLog;  
        }  
    }
}