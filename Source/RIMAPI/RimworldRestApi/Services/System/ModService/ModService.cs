using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Models;
using Verse;

namespace RIMAPI.Services
{
    public class ModService : IModService
    {
        public ApiResult<List<ModInfoDto>> GetModsInfo()
        {
            try
            {
                var mods = LoadedModManager.RunningModsListForReading.Select(mod => new ModInfoDto
                {
                    Name = mod.Name,
                    PackageId = mod.PackageId,
                    LoadOrder = mod.loadOrder,
                }).ToList();

                return ApiResult<List<ModInfoDto>>.Ok(mods);
            }
            catch (Exception ex)
            {
                return ApiResult<List<ModInfoDto>>.Fail($"Failed to get mods info: {ex.Message}");
            }
        }

        public ApiResult ConfigureMods(ConfigureModsRequestDto body)
        {
            try
            {
                if (body == null || body.PackageIds == null)
                    return ApiResult.Fail("Invalid request. 'PackageIds' list is required.");

                if (!body.PackageIds.Contains("ludeon.rimworld", StringComparer.OrdinalIgnoreCase))
                    body.PackageIds.Insert(0, "ludeon.rimworld");

                ModsConfig.SetActiveToList(body.PackageIds);
                ModsConfig.Save();

                if (body.RestartGame)
                {
                    LogApi.Info("Restarting RimWorld to apply new mod configuration...");
                    GenCommandLine.Restart();
                }

                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"Failed to configure mods: {ex.Message}");
            }
        }
    }
}