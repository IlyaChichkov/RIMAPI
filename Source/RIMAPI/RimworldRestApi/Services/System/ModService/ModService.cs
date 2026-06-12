using System;
using System.Collections.Generic;
using System.IO;
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
                var mods = LoadedModManager.RunningModsListForReading.Select(mod => MapToDto(mod)).ToList();

                return ApiResult<List<ModInfoDto>>.Ok(mods);
            }
            catch (Exception ex)
            {
                return ApiResult<List<ModInfoDto>>.Fail($"Failed to get mods info: {ex.Message}");
            }
        }

        public ApiResult<ModInfoDto> GetModInfo(string packageId)
        {
            try
            {
                var mod = LoadedModManager.RunningModsListForReading.FirstOrDefault(m =>
                    m.PackageId.Equals(packageId, StringComparison.OrdinalIgnoreCase));

                if (mod == null)
                    return ApiResult<ModInfoDto>.Fail($"Mod with packageId '{packageId}' not found or not active.");

                return ApiResult<ModInfoDto>.Ok(MapToDto(mod));
            }
            catch (Exception ex)
            {
                return ApiResult<ModInfoDto>.Fail($"Failed to get mod info for '{packageId}': {ex.Message}");
            }
        }

        public ApiResult<string> GetModPreview(string packageId)
        {
            try
            {
                var mod = LoadedModManager.RunningModsListForReading.FirstOrDefault(m =>
                    m.PackageId.Equals(packageId, StringComparison.OrdinalIgnoreCase));

                if (mod == null)
                    return ApiResult<string>.Fail($"Mod with packageId '{packageId}' not found or not active.");

                string previewPath = Path.Combine(mod.RootDir, "About", "preview.png");
                if (!File.Exists(previewPath))
                    return ApiResult<string>.Fail($"Preview image not found for mod '{packageId}' at {previewPath}");

                byte[] imageBytes = File.ReadAllBytes(previewPath);
                string base64Image = Convert.ToBase64String(imageBytes);

                return ApiResult<string>.Ok(base64Image);
            }
            catch (Exception ex)
            {
                return ApiResult<string>.Fail($"Failed to get mod preview for '{packageId}': {ex.Message}");
            }
        }

        private ModInfoDto MapToDto(ModContentPack mod)
        {
            return new ModInfoDto
            {
                Name = mod.Name,
                PackageId = mod.PackageId,
                LoadOrder = mod.loadOrder,
                Author = mod.ModMetaData?.AuthorsString,
                Description = mod.ModMetaData?.Description,
                Url = mod.ModMetaData?.Url,
                Version = mod.ModMetaData?.ModVersion,
                SupportedVersions = mod.ModMetaData?.SupportedVersionsReadOnly.Select(v => v.ToString()).ToList(),
                RootDir = mod.RootDir
            };
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