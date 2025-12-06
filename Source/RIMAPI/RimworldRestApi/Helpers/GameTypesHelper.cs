using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RIMAPI.Helpers
{
    public static class GameTypesHelper
    {
        public static float TicksToDays(this int numTicks)
        {
            return (float)numTicks / 60000f;
        }

        public static Color HexToColor(string hex)
        {
            // Remove # if present and trim whitespace
            hex = hex.Trim().Replace("#", "");

            // Handle different hex formats
            if (hex.Length == 3)
            {
                // Short format (RGB) - expand to full format
                hex = string.Format("{0}{0}{1}{1}{2}{2}", hex[0], hex[1], hex[2]);
            }
            else if (hex.Length != 6)
            {
                RIMAPI.Core.LogApi.Warning($"Invalid hex color format: {hex}");
                return Color.white; // Return default color
            }

            try
            {
                // Parse hex components
                byte r = byte.Parse(
                    hex.Substring(0, 2),
                    System.Globalization.NumberStyles.HexNumber
                );
                byte g = byte.Parse(
                    hex.Substring(2, 2),
                    System.Globalization.NumberStyles.HexNumber
                );
                byte b = byte.Parse(
                    hex.Substring(4, 2),
                    System.Globalization.NumberStyles.HexNumber
                );

                // Convert to Unity Color (0-1 range)
                return new Color(r / 255f, g / 255f, b / 255f);
            }
            catch (System.Exception e)
            {
                RIMAPI.Core.LogApi.Warning($"Failed to parse hex color: {hex}. Error: {e.Message}");
                return Color.white; // Return default color
            }
        }

        public static LetterDef StringToLetterDef(string def)
        {
            switch (def)
            {
                case "ThreatBig":
                    return LetterDefOf.ThreatBig;
                case "AcceptCreepJoiner":
                    return LetterDefOf.AcceptCreepJoiner;
                case "Bossgroup":
                    return LetterDefOf.Bossgroup;
                case "ChildBirthday":
                    return LetterDefOf.ChildBirthday;
                case "ChildToAdult":
                    return LetterDefOf.ChildToAdult;
                case "BabyToChild":
                    return LetterDefOf.BabyToChild;
                case "BabyBirth":
                    return LetterDefOf.BabyBirth;
                case "RelicHuntInstallationFound":
                    return LetterDefOf.RelicHuntInstallationFound;
                case "RitualOutcomePositive":
                    return LetterDefOf.RitualOutcomePositive;
                case "EntityDiscovered":
                    return LetterDefOf.EntityDiscovered;
                case "RitualOutcomeNegative":
                    return LetterDefOf.RitualOutcomeNegative;
                case "GameEnded":
                    return LetterDefOf.GameEnded;
                case "AcceptJoiner":
                    return LetterDefOf.AcceptJoiner;
                case "AcceptVisitors":
                    return LetterDefOf.AcceptVisitors;
                case "Death":
                    return LetterDefOf.Death;
                case "PositiveEvent":
                    return LetterDefOf.PositiveEvent;
                case "NeutralEvent":
                    return LetterDefOf.NeutralEvent;
                case "NegativeEvent":
                    return LetterDefOf.NegativeEvent;
                case "ThreatSmall":
                    return LetterDefOf.ThreatSmall;
                case "ChoosePawn":
                    return LetterDefOf.ChoosePawn;
                case "BundleLetter":
                    return LetterDefOf.BundleLetter;
            }
            throw new Exception($"Failed to find LetterDef by def name: {def}");
        }
    }
}
