using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IWindowService
    {
        ApiResult ShowMessage(WindowMessageRequestDto request);
        ApiResult ShowDialog(WindowDialogRequestDto request);
        ApiResult<WindowCloseResultDto> CloseWindows(WindowCloseRequestDto request);
        ApiResult<List<OpenWindowDto>> ListWindows();
    }
}