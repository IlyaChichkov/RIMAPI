

using System;
using UnityEngine;
using Verse;

namespace RimworldRestApi.Helpers
{
    public class TextureHelper
    {
        public string GetItemImage(int thingId)
        {
            try
            {
                Log.Message($"[RIMAPI] GetItemImage request for thingId: {thingId}");

                Map currentMap = Find.CurrentMap;
                if (currentMap == null)
                {
                    return "{\"error\": \"No map available\"}";
                }

                // Find the thing by ID
                Thing thing = currentMap.listerThings.AllThings.FirstOrDefault(t => t.thingIDNumber == thingId);
                if (thing == null)
                {
                    return "{\"error\": \"Item not found\"}";
                }

                // Get the texture
                Texture2D icon;

                if (!thing.def.uiIconPath.NullOrEmpty())
                {
                    icon = thing.def.uiIcon;
                }
                else
                {
                    // Use the thing's graphic
                    icon = (Texture2D)thing.Graphic.MatSingle.mainTexture;
                }

                if (icon == null)
                {
                    return "{\"error\": \"No icon available for this item\"}";
                }

                // Convert texture to base64
                string base64Image = TextureToBase64(icon);

                return $"{{\"success\": true, \"thingId\": {thingId}, \"image\": \"{base64Image}\", \"format\": \"png\", \"width\": {icon.width}, \"height\": {icon.height}, \"label\": \"{thing.Label}\"}}";
            }
            catch (Exception ex)
            {
                Log.Error($"[RIMAPI] GetItemImage error: {ex}");
                return "{\"error\": \"Failed to get item image: " + ex.Message + "\"}";
            }
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
                Log.Error($"[RIMAPI] TextureToBase64 error: {ex}");
                return "";
            }
        }

    }
}