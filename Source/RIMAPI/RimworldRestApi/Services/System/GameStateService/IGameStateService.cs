using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IGameStateService
    {
        ApiResult<GameStateDto> GetGameState();
        ApiResult SetGameSpeed(int speed);
        ApiResult GameSave(GameSaveRequestDto body);
        ApiResult GameLoad(GameLoadRequestDto body);
        ApiResult GameDevQuickStart();
        ApiResult GoToMainMenu();
        ApiResult QuitGame();
        ApiResult GameStart(NewGameStartRequestDto body);
        ApiResult<GameSettingsDto> GetCurrentSettings();
        ApiResult<bool> ToggleRunInBackground();
        ApiResult<bool> GetRunInBackground();
    }
}