using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class InteractionDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
    }

    public class PawnInteractionStatusDto
    {
        public bool CanInteract { get; set; }
        public int CooldownTicks { get; set; }
        public float CooldownDays { get; set; }
        public string LastInteractionDef { get; set; }
        public int LastInteractionTicks { get; set; }
    }

    public class PawnInteractionLogDto
    {
        public int PawnId { get; set; }
        public List<InteractionLogEntryDto> Interactions { get; set; }
        public int Count { get; set; }
    }

    public class InteractionLogEntryDto
    {
        public int InitiatorId { get; set; }
        public string InitiatorName { get; set; }
        public int RecipientId { get; set; }
        public string RecipientName { get; set; }
        public string InteractionDefName { get; set; }
        public string InteractionLabel { get; set; }
        public string Text { get; set; }
        public int Ticks { get; set; }
        public string TimeAgo { get; set; }
    }

    public class PawnOpinionDto
    {
        public int TargetPawnId { get; set; }
        public string TargetPawnName { get; set; }
        public int Opinion { get; set; }
        public List<OpinionBreakdownDto> Breakdown { get; set; }
    }

    public class OpinionBreakdownDto
    {
        public string ThoughtDefName { get; set; }
        public string Label { get; set; }
        public float Score { get; set; }
    }

    public class ForceInteractionRequestDto
    {
        public int InitiatorId { get; set; }
        public int RecipientId { get; set; }
        public string InteractionDefName { get; set; }
    }

    public class AddRelationRequestDto
    {
        public int Pawn1Id { get; set; }
        public int Pawn2Id { get; set; }
        public string RelationDefName { get; set; }
    }

    public class RemoveRelationRequestDto
    {
        public int Pawn1Id { get; set; }
        public int Pawn2Id { get; set; }
        public string RelationDefName { get; set; }
    }

    public class PawnRelationsDto
    {
        public int PawnId { get; set; }
        public List<PawnRelationEntryDto> Relations { get; set; }
    }

    public class PawnRelationEntryDto
    {
        public int OtherPawnId { get; set; }
        public string OtherPawnName { get; set; }
        public string RelationDefName { get; set; }
        public string RelationLabel { get; set; }
    }
}
