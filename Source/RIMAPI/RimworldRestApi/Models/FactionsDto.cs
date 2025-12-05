using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RIMAPI.Models
{
    public class FactionsDto
    {
        public int LoadId { get; set; }
        public string DefName { get; set; }
        public string Name { get; set; }
        public bool IsPlayer { get; set; }
        public string Relation { get; set; }
        public int Goodwill { get; set; }
    }

    public class FactionDto
    {
        public int LoadId { get; set; }
        public string DefName { get; set; }
        public string Name { get; set; }
        public bool IsPlayer { get; set; }
        public string LeaderTitle { get; set; }
        public int LeaderId { get; set; }

        public static FactionDto ToDto(Faction f)
        {
            if (f == null)
                return null;
            int leaderId = f.leader != null ? f.leader.thingIDNumber : 0;

            return new FactionDto
            {
                LoadId = f.loadID,
                DefName = f.def?.defName,
                Name = f.Name,
                IsPlayer = f.IsPlayer,
                LeaderTitle = f.LeaderTitle,
                LeaderId = leaderId,
            };
        }
    }

    public class FactionRelationDto
    {
        public int Goodwill { get; set; }
        public string RelationKind { get; set; }
    }

    public class FactionChangeRelationRequestDto
    {
        public int Id { get; set; }
        public int OtherId { get; set; }
        public int Value { get; set; }
        public bool SendMessage { get; set; }
        public bool CanSendHostilityLetter { get; set; }
    }

    public class FactionRelationsDto
    {
        public int Id { get; set; }
        public Dictionary<int, FactionRelationDto> Relations { get; set; }
    }

    public class FactionChangeRelationResponceDto
    {
        public int Id { get; set; }
        public int OtherId { get; set; }
        public int Value { get; set; }
    }

    public class FactionDefDto
    {
        public string DefName { get; set; }
        public string PawnSingular { get; set; }
        public string PawnsPlural { get; set; }
        public string LeaderTitle { get; set; }
        public string LeaderTitleFemale { get; set; }
        public string CategoryTag { get; set; }
        public bool HostileToFactionlessHumanlikes { get; set; }
        public string StartingCountAtWorldCreation { get; set; }
        public bool PermanentEnemy { get; set; }
        public bool NaturalEnemy { get; set; }
        public bool ClassicIdeo { get; set; }
        public bool HiddenIdeo { get; set; }
        public string IdeoName { get; set; }
        public bool CanSiege { get; set; }
        public bool CanStageAttacks { get; set; }
        public bool CanUseAvoidGrid { get; set; }
        public bool CanPsychicRitualSiege { get; set; }
        public float EarliestRaidDays { get; set; }

        public static FactionDefDto FromFactionDef(FactionDef f)
        {
            if (f == null)
                return null;

            return new FactionDefDto
            {
                DefName = f.defName,
                PawnSingular = f.pawnSingular,
                PawnsPlural = f.pawnsPlural,
                LeaderTitle = f.leaderTitle,
                LeaderTitleFemale = f.leaderTitleFemale,
                CategoryTag = f.categoryTag,
                HostileToFactionlessHumanlikes = f.hostileToFactionlessHumanlikes,
                StartingCountAtWorldCreation = f.startingCountAtWorldCreation.ToString() ?? "0",
                PermanentEnemy = f.permanentEnemy,
                NaturalEnemy = f.naturalEnemy,
                ClassicIdeo = f.classicIdeo,
                HiddenIdeo = f.hidden,
                IdeoName = f.ideoName,
                CanSiege = f.canSiege,
                CanStageAttacks = f.canStageAttacks,
                CanUseAvoidGrid = f.canUseAvoidGrid,
                CanPsychicRitualSiege = f.canPsychicRitualSiege,
                EarliestRaidDays = f.earliestRaidDays,
            };
        }
    }
}
