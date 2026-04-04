
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IOrderService
    {
        ApiResult DesignateArea(DesignateRequestDto request);
    }
}
