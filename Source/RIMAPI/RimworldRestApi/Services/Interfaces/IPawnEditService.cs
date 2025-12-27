using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IPawnEditService
    {
        // Core Info
        ApiResult UpdateBasicInfo(PawnBasicRequest request);

        // Stats & Conditions
        ApiResult UpdateHealth(PawnHealthRequest request);
        ApiResult UpdateNeeds(PawnNeedsRequest request);
        ApiResult UpdateSkills(PawnSkillsRequest request);
        ApiResult UpdateTraits(PawnTraitsRequest request);

        // Items
        ApiResult UpdateInventory(PawnInventoryRequest request);
        ApiResult UpdateApparel(PawnApparelRequest request); // Separated for clarity

        // World State
        ApiResult UpdateStatus(PawnStatusRequest request); // Draft, Kill, Resurrect
        ApiResult UpdatePosition(PawnPositionRequest request);
        ApiResult UpdateFaction(PawnFactionRequest request);
    }
}