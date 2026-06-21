using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;
using RIMAPI.Core;
using RIMAPI.Models.UI;
using RIMAPI.Services.Interfaces;
using RIMAPI.Helpers;
using System.Linq;
using RIMAPI.Models;
using RimWorld.Planet;

namespace RIMAPI.Services
{
    public class UIService : IUIService
    {
        private static readonly FieldInfo ActiveAlertsField = typeof(AlertsReadout).GetField(
            "activeAlerts",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        public ApiResult<List<AlertDto>> GetActiveAlerts()
        {
            if (Current.ProgramState != ProgramState.Playing)
            {
                return ApiResult<List<AlertDto>>.Fail("Cannot fetch alerts: Game is not currently playing.");
            }

            if (Find.Alerts == null)
            {
                return ApiResult<List<AlertDto>>.Fail("Alerts system is currently unavailable.");
            }

            if (ActiveAlertsField == null)
            {
                return ApiResult<List<AlertDto>>.Fail("Reflection failed: Could not find the activeAlerts field.");
            }

            var alertDtos = new List<AlertDto>();

            try
            {
                var activeAlerts = (List<Alert>)ActiveAlertsField.GetValue(Find.Alerts);

                if (activeAlerts != null)
                {
                    foreach (Alert alert in activeAlerts)
                    {
                        try
                        {
                            var alertDto = new AlertDto
                            {
                                Label = alert.GetLabel(),
                                Explanation = alert.GetExplanation(),
                                Priority = alert.Priority.ToString(),
                                Targets = new List<int>(),
                                Cells = new List<string>()
                            };

                            AlertReport report = alert.GetReport();
                            if (report.AllCulprits != null)
                            {
                                foreach (GlobalTargetInfo target in report.AllCulprits)
                                {
                                    if (!target.IsValid) continue;

                                    if (target.HasThing)
                                    {
                                        alertDto.Targets.Add(target.Thing.thingIDNumber);
                                    }
                                    else if (target.HasWorldObject)
                                    {
                                        alertDto.Targets.Add(target.WorldObject.ID);
                                    }
                                    else if (target.Cell.IsValid)
                                    {
                                        alertDto.Cells.Add($"Cell_{target.Cell.x}_{target.Cell.z}");
                                    }
                                }
                            }

                            alertDtos.Add(alertDto);
                        }
                        catch (Exception ex)
                        {
                            Log.Warning($"[RIMAPI] Failed to parse alert of type {alert.GetType().Name}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[RIMAPI] Critical reflection error while reading alerts: {ex}");
                return ApiResult<List<AlertDto>>.Fail("Internal server error reading memory.");
            }

            return ApiResult<List<AlertDto>>.Ok(alertDtos);
        }

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