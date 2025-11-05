

using System;
using HarmonyLib;
using RimWorld;
using RimworldRestApi.Core;
using RimworldRestApi.Models;
using UnityEngine;
using Verse;

namespace RimworldRestApi.Helpers
{
    public class TextureHelper
    {
        public ImageDto GetItemImageByName(string thingName)
        {
            ImageDto image = new ImageDto();
            try
            {
                var thingDef = DefDatabase<ThingDef>.GetNamed(thingName);
                Texture2D icon = null;

                if (!thingDef.uiIconPath.NullOrEmpty())
                {
                    icon = thingDef.uiIcon;
                }
                else
                {
                    icon = (Texture2D)thingDef.DrawMatSingle.mainTexture;
                }

                if (icon == null)
                {
                    image.Result = $"No icon available for item - {thingName}";
                }
                else
                {
                    image.Result = "success";
                    image.ImageBase64 = TextureToBase64(icon);
                }
            }
            catch (Exception ex)
            {
                image.Result = ex.Message;
                throw;
            }
            return image;
        }

        public ImageDto GetPawnPortraitImage(Pawn pawn, int width, int height, string faceDir = "south")
        {
            ImageDto image = new ImageDto();
            try
            {
                var dir = Rot4.South;
                switch (faceDir)
                {
                    case "north":
                        dir = Rot4.North;
                        break;
                    case "east":
                        dir = Rot4.East;
                        break;
                    case "south":
                        dir = Rot4.South;
                        break;
                    case "west":
                        dir = Rot4.West;
                        break;
                }

                RenderTexture renderTexture = PortraitsCache.Get(
                    pawn,
                    new Vector2(width, height),
                    dir
                );

                // Convert to Texture2D  
                RenderTexture.active = renderTexture;
                Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height);
                texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                texture.Apply();
                RenderTexture.active = null;

                image.Result = "success";
                image.ImageBase64 = TextureToBase64(texture);
            }
            catch (Exception ex)
            {
                image.Result = ex.Message;
                throw;
            }
            return image;
        }

        public string TextureToBase64(Texture2D texture)
        {
            try
            {
                // Create a temporary RenderTexture
                RenderTexture renderTexture = RenderTexture.GetTemporary(
                    texture.width,
                    texture.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear
                );

                // Blit the texture to RenderTexture
                Graphics.Blit(texture, renderTexture);

                // Set active render texture
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = renderTexture;

                // Create Texture2D to read pixels into
                Texture2D readableTexture = new Texture2D(texture.width, texture.height);
                readableTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                readableTexture.Apply();

                // Reset active render texture
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(renderTexture);

                // Encode to PNG
                byte[] imageBytes = ImageConversion.EncodeToPNG(readableTexture);

                // Clean up
                UnityEngine.Object.Destroy(readableTexture);

                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"TextureToBase64 error: {ex}");
                throw;
            }
        }

    }
}