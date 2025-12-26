using System;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.UI;
using Verse;

namespace RIMAPI.Services
{
    public interface IOverlayService
    {
        ApiResult ShowAnnouncement(OverlayRequestDto request);
    }

    public class OverlayService : IOverlayService
    {
        public ApiResult ShowAnnouncement(OverlayRequestDto request)
        {
            // Push the UI creation to the Main Game Thread
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                // Remove existing announcements so they don't overlap messy
                var existing = Find.WindowStack.WindowOfType<AnnouncementWindow>();
                if (existing != null) existing.Close();

                // Add new one
                Find.WindowStack.Add(new AnnouncementWindow(
                    request.Text,
                    request.Duration,
                    request.Color,
                    request.Scale
                ));
            });

            return ApiResult.Ok();
        }
    }
}