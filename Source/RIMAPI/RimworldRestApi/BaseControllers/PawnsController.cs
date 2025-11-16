using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RimworldRestApi.Core;
using RimworldRestApi.Models;
using RimworldRestApi.Services;
using Verse;

namespace RimworldRestApi.Controllers
{
    public class PawnsController : BaseController
    {
        private readonly IGameDataService _gameDataService;

        public PawnsController(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        public async Task GetColonists(HttpListenerContext context)
        {
            try
            {
                object colonists = _gameDataService.GetColonists();
                HandleFiltering(context, ref colonists);
                await ResponseBuilder.Success(context.Response, colonists);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetColonist(HttpListenerContext context)
        {
            try
            {
                int pawnId = GetIntProperty(context, "id");
                object colonist = _gameDataService.GetColonist(pawnId);
                if (colonist == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist not found");
                    return;
                }
                HandleFiltering(context, ref colonist);
                await ResponseBuilder.Success(context.Response, colonist);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetColonistsDetailed(HttpListenerContext context)
        {
            try
            {
                object colonistsDetailed = _gameDataService.GetColonistsDetailed();
                HandleFiltering(context, ref colonistsDetailed);
                await ResponseBuilder.Success(context.Response, colonistsDetailed);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetColonistDetailed(HttpListenerContext context)
        {
            try
            {
                int pawnId = GetIntProperty(context, "id");
                object colonistDetailed = _gameDataService.GetColonistDetailed(pawnId);
                if (colonistDetailed == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist not found");
                    return;
                }
                HandleFiltering(context, ref colonistDetailed);
                await ResponseBuilder.Success(context.Response, colonistDetailed);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task SetColonistWorkPriority(HttpListenerContext context)
        {
            try
            {
                int pawnId = GetIntProperty(context, "id");
                string workDef = GetStringProperty(context, "work");
                int priority = GetIntProperty(context, "priority");
                _gameDataService.SetColonistWorkPriority(pawnId, workDef, priority);
                var result = new
                {
                    Result = "success"
                };
                await ResponseBuilder.Success(context.Response, result);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task SetColonistsWorkPriority(HttpListenerContext context)
        {
            try
            {
                var updates = await ReadJsonArrayBodyAsync<WorkPriorityUpdateDto>(context);

                int applied = 0;
                var errors = new List<object>();

                foreach (var u in updates)
                {
                    try
                    {
                        if (u == null) throw new ArgumentException("Null item.");
                        if (u.Id <= 0) throw new ArgumentException("Invalid 'id'.");
                        if (string.IsNullOrWhiteSpace(u.Work)) throw new ArgumentException("Missing 'work'.");

                        _gameDataService.SetColonistWorkPriority(u.Id, u.Work, u.Priority);
                        applied++;
                    }
                    catch (Exception itemEx)
                    {
                        errors.Add(new { id = u?.Id, work = u?.Work, error = itemEx.Message });
                    }
                }

                await ResponseBuilder.Success(context.Response, new { Result = "success", Applied = applied, Errors = errors });
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response, HttpStatusCode.BadRequest, ex.Message);
            }
        }

        public async Task GetColonistInventory(HttpListenerContext context)
        {
            try
            {
                int pawnId = GetIntProperty(context, "id");
                object colonistInventory = _gameDataService.GetColonistInventory(pawnId);
                if (colonistInventory == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist's inventory not found");
                    return;
                }
                HandleFiltering(context, ref colonistInventory);
                await ResponseBuilder.Success(context.Response, colonistInventory);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetPawnPortraitImage(HttpListenerContext context)
        {
            try
            {
                int pawnId = GetIntProperty(context, "pawn_id");
                int width = GetIntProperty(context, "width");
                int height = GetIntProperty(context, "height");
                string direction = GetStringProperty(context, "direction");

                object itemImage = _gameDataService.GetPawnPortraitImage(pawnId, width, height, direction);
                if (itemImage == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Failed to get item's image");
                    return;
                }
                await ResponseBuilder.Success(context.Response, itemImage);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetColonistBody(HttpListenerContext context)
        {
            try
            {
                string idStr = context.Request.QueryString["id"];
                if (string.IsNullOrEmpty(idStr))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Missing colonist ID parameter");
                    return;
                }

                if (!int.TryParse(idStr, out int colonistId))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid colonist ID format");
                    return;
                }

                object colonistBodyParts = _gameDataService.GetColonistBodyParts(colonistId);
                if (colonistBodyParts == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist's inventory not found");
                    return;
                }
                HandleFiltering(context, ref colonistBodyParts);
                await ResponseBuilder.Success(context.Response, colonistBodyParts);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetPawnOpinionAboutPawn(HttpListenerContext context)
        {
            try
            {
                string pawnIdStr = context.Request.QueryString["id"];
                if (string.IsNullOrEmpty(pawnIdStr))
                {
                    throw new Exception("Missing 'id' parameter");
                }

                if (!int.TryParse(pawnIdStr, out int pawnId))
                {
                    throw new Exception("Invalid 'id' format");
                }

                string otherIdStr = context.Request.QueryString["other_id"];
                if (string.IsNullOrEmpty(otherIdStr))
                {
                    throw new Exception("Missing 'id' parameter");
                }

                if (!int.TryParse(otherIdStr, out int otherId))
                {
                    throw new Exception("Invalid 'id' format");
                }

                object opinion = _gameDataService.GetOpinionAboutPawn(pawnId, otherId);
                await ResponseBuilder.Success(context.Response, opinion);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task MakeJobEquip(HttpListenerContext context)
        {
            try
            {
                var mapId = GetMapIdProperty(context);
                var pawnId = GetIntProperty(context, "pawn_id");
                var itemId = GetIntProperty(context, "item_id");
                var itemType = GetStringProperty(context, "item_type");
                _gameDataService.MakeJobEquip(mapId, pawnId, itemId, itemType);
                var result = new
                {
                    Result = "success"
                };
                await ResponseBuilder.Success(context.Response, result);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        internal async Task GetWorkList(HttpListenerContext context)
        {
            try
            {
                object incidentsData = _gameDataService.GetWorkList();
                HandleFiltering(context, ref incidentsData);
                await ResponseBuilder.Success(context.Response, incidentsData);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        internal async Task GetTraitDef(HttpListenerContext context)
        {
            try
            {
                string traitName = GetStringProperty(context, "name");

                object traitDef = _gameDataService.GetTraitDefDto(traitName);
                HandleFiltering(context, ref traitDef);
                await ResponseBuilder.Success(context.Response, traitDef);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        internal async Task GetTimeAssignmentsList(HttpListenerContext context)
        {
            try
            {
                object timeAssignment = _gameDataService.GetTimeAssignmentsList();
                HandleFiltering(context, ref timeAssignment);
                await ResponseBuilder.Success(context.Response, timeAssignment);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task SetTimeAssignment(HttpListenerContext context)
        {
            try
            {
                var pawnId = GetIntProperty(context, "pawn_id");
                var hour = GetIntProperty(context, "hour");
                var assignment = GetStringProperty(context, "assignment");
                _gameDataService.SetTimeAssignment(pawnId, hour, assignment);
                var result = new
                {
                    Result = "success"
                };
                await ResponseBuilder.Success(context.Response, result);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        internal async Task GetOutfitsList(HttpListenerContext context)
        {
            try
            {
                var outfitsList = _gameDataService.GetOutfits();
                await ResponseBuilder.Success(context.Response, outfitsList);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}