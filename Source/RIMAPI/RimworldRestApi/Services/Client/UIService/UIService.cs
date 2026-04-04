using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;
using RIMAPI.Core;
using RIMAPI.Services.Interfaces;
using RIMAPI.Helpers;
using System.Linq;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public class UIService : IUIService
    {
        private static readonly FieldInfo ActiveAlertsField = typeof(AlertsReadout).GetField(
            "activeAlerts",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        public ApiResult OpenTab(string tabName)
        {
            try
            {
                switch (tabName.ToLower())
                {
                    case "health":
                        InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Health));
                        break;
                    case "character":
                    case "backstory":
                        InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Character));
                        break;
                    case "gear":
                    case "equipment":
                    case "inventory":
                        InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Gear));
                        break;
                    case "needs":
                    case "mood":
                        InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Needs));
                        break;
                    case "training":
                        InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Training));
                        break;
                    case "log":
                    case "combatlog":
                        InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Log));
                        break;
                    case "relations":
                    case "social":
                        InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Social));
                        break;
                    case "prisoner":
                        InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Prisoner));
                        break;
                    case "slave":
                        InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Slave));
                        break;
                    case "guest":
                        InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Guest));
                        break;
                    default:
                        return ApiResult.Fail($"Tried to open unknown tab menu: {tabName}");
                }
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error opening tab {tabName}: {ex}");
                return ApiResult.Fail($"Failed to open tab: {ex.Message}");
            }
        }

        public ApiResult SendLetterSimple(SendLetterRequestDto body)
        {
            try
            {
                List<string> warnings = new List<string>();
                const int MAX_LABEL_SIZE = 48;
                const int MAX_MESSAGE_SIZE = 500;
                var label = ApiSecurityHelper.SanitizeLetterInput(body.Label);
                var message = ApiSecurityHelper.SanitizeLetterInput(body.Message);

                if (string.IsNullOrEmpty(message))
                {
                    return ApiResult.Fail("Message is empty after sanitization");
                }

                if (message.Length > MAX_MESSAGE_SIZE)
                {
                    message = message.Substring(0, MAX_MESSAGE_SIZE) + "...";
                    warnings.Add($"Message has been truncated to {MAX_MESSAGE_SIZE} characters");
                }

                if (label.Length > MAX_LABEL_SIZE)
                {
                    message = message.Substring(0, MAX_LABEL_SIZE) + "...";
                    warnings.Add($"Label has been truncated to {MAX_LABEL_SIZE} characters");
                }

                LetterDef letterDef = GameTypesHelper.StringToLetterDef(body.LetterDef);
                LookTargets target = null;
                Faction faction = null;
                Quest quest = null;
                // TODO: Support for hyperlinkThingDefs & debugInfo
                List<ThingDef> hyperlinkThingDefs = null;
                string debugInfo = null;

                if (!string.IsNullOrEmpty(body.MapId))
                {
                    target = MapHelper.GetThingOnMapById(
                        int.Parse(body.MapId),
                        int.Parse(body.LookTargetThingId)
                    );
                }

                if (!string.IsNullOrEmpty(body.FactionOrderId))
                {
                    faction = FactionHelper.GetFactionByOrderId(int.Parse(body.FactionOrderId));
                }

                if (!string.IsNullOrEmpty(body.QuestId))
                {
                    int id = int.Parse(body.QuestId);
                    quest = Find
                        .QuestManager.QuestsListForReading.Where(s => s.id == id)
                        .FirstOrDefault();
                }

                Find.LetterStack.ReceiveLetter(
                    label,
                    message,
                    letterDef,
                    (LookTargets)target,
                    faction,
                    quest,
                    hyperlinkThingDefs,
                    debugInfo,
                    body.DelayTicks,
                    body.PlaySound
                );

                if (warnings.Count > 0)
                {
                    return ApiResult.Partial(warnings);
                }
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }
    }
}