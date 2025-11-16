

namespace RimworldRestApi.SseModels
{
    public class IncidentLetterEvent
    {
        public string Label { get; set; }
        public string Text { get; set; }
        public string IncidentDef { get; set; }
        public string LetterDef { get; set; }
        public bool IsUrgent { get; set; }
        public FactionInfo Faction { get; set; }
        public QuestInfo Quest { get; set; }
        public TargetInfoLite Target { get; set; }
        public LookTargetsLite LookTargets { get; set; }
        public IncidentParmsLite Parms { get; set; }
    }

    public class FactionInfo
    {
        public string DefName { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public class QuestInfo
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public class TargetInfoLite
    {
        public string Type { get; set; }
        public int? MapId { get; set; }
        public int? Tile { get; set; }
        public string ParentName { get; set; }
    }

    public class LookTargetsLite
    {
        public int Count { get; set; }
        public string PrimaryLabel { get; set; }
        public string PrimaryThingDef { get; set; }
        public string PrimaryPosition { get; set; }
        public int? PrimaryMapId { get; set; }
    }

    public class IncidentParmsLite
    {
        public float points { get; set; }
        public bool forced { get; set; }
        public string SpawnCenter { get; set; }
        public string CustomLetterLabel { get; set; }
        public string CustomLetterText { get; set; }
    }


}