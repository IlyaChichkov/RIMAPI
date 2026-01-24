using System;
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class SendLetterRequestDto
    {
        public string Label { get; set; }
        public string Message { get; set; }
        public string LetterDef { get; set; }
        public string MapId { get; set; }
        public string LookTargetThingId { get; set; }
        public string FactionOrderId { get; set; }
        public List<string> HyperlinkThingDefs { get; set; }
        public string QuestId { get; set; }
        public string DebugInfo { get; set; }
        public int DelayTicks { get; set; }
        public bool PlaySound { get; set; }
    }
}
