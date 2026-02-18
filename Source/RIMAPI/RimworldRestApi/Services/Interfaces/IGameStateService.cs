
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IGameStateService
    {
        ApiResult<GameStateDto> GetGameState();
        ApiResult<List<ModInfoDto>> GetModsInfo();
        ApiResult ConfigureMods(ConfigureModsRequestDto body);
        ApiResult SelectArea(SelectAreaRequestDto body);
        ApiResult Select(string objectType, int id);
        ApiResult DeselectAll();
        ApiResult OpenTab(string tabName);
        ApiResult<DefsDto> GetAllDefs(AllDefsRequestDto body);
        ApiResult<MapTimeDto> GetCurrentMapDatetime();
        ApiResult<MapTimeDto> GetWorldTileDatetime(int tileID);
        ApiResult SendLetterSimple(SendLetterRequestDto body);
        ApiResult SetGameSpeed(int speed);
        ApiResult GameSave(string name);
        ApiResult GameLoad(string name);
        ApiResult GameDevQuickStart();
        ApiResult GameStart(NewGameStartRequestDto body);
        ApiResult<GameSettingsDto> GetCurrentSettings();
        ApiResult<bool> ToggleRunInBackground();
        ApiResult<bool> GetRunInBackground();
    }
}