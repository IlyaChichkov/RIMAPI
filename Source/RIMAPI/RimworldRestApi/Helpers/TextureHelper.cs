

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
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
        public ImageDto GetThingImage(int thingId)
        {
            ImageDto image = new ImageDto();

            return image;
        }

        public ImageDto GetItemImageByName(string thingName)
        {
            ImageDto image = new ImageDto();
            try
            {
                var thingDef = DefDatabase<ThingDef>.GetNamed(thingName);
                Texture2D texture = null;

                if (!thingDef.uiIconPath.NullOrEmpty())
                {
                    texture = thingDef.uiIcon;
                }
                else
                {
                    texture = (Texture2D)thingDef.DrawMatSingle.mainTexture;
                }

                if (texture == null)
                {
                    image.Result = $"No texture available for item - {thingName}";
                }
                else
                {
                    image.Result = "success";
                    image.ImageBase64 = TextureToBase64(texture);
                }
            }
            catch (Exception ex)
            {
                image.Result = ex.Message;
                throw;
            }
            return image;
        }

        public void SetItemImageByName(string thingName, string imageBase64)
        {
            var thingDef = DefDatabase<ThingDef>.GetNamed(thingName);


            DebugLogging.Info($"Got thingDef: {thingDef.defName}");
            Texture2D newTexture = CreateTextureFromBase64(imageBase64);
            DebugLogging.Info($"Setting texture for {thingName}, Graphic: {thingDef.graphic.GetType().Name}");

            thingDef.DrawMatSingle.mainTexture = newTexture;

            if (thingDef.graphic is Graphic_StackCount linkedGraphic)
            {
                var graphicCollection = thingDef.graphic as Graphic_Collection;
                if (graphicCollection != null)
                {
                    DebugLogging.Info($"set graphicCollection");
                    var subGraphicsField = typeof(Graphic_Collection).GetField("subGraphics",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                    Graphic[] subGraphics = subGraphicsField.GetValue(graphicCollection) as Graphic[];

                    // Now you can iterate through or access individual subgraphics  
                    foreach (var subGraphic in subGraphics)
                    {
                        subGraphic.MatSingle.mainTexture = newTexture;
                    }
                }
                else
                {
                    DebugLogging.Info($"graphicCollection is null");
                }
            }

            if (thingDef.graphic is Graphic_Multi multiGraphic)
            {
                var graphicMulti = thingDef.graphic as Graphic_Multi;
                if (graphicMulti != null)
                {
                    DebugLogging.Info($"set Graphic_Multi");
                    var subGraphicsField = typeof(Graphic_Multi).GetField("mats",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                    Material[] mats = subGraphicsField.GetValue(graphicMulti) as Material[];
                    Material northMat = mats[0];
                    DebugLogging.Info($"Name: {northMat.name}");
                    DebugLogging.Info($"Shader: {northMat.shader.name}");
                    DebugLogging.Info($"Main Texture: {northMat.mainTexture?.name}");
                    DebugLogging.Info($"Color: {northMat.color}");
                    DebugLogging.Info($"Render Queue: {northMat.renderQueue}");
                    mats[0].mainTexture = null;
                    mats[1].mainTexture = null;
                    mats[2].mainTexture = null;
                    mats[3].mainTexture = null;
                }
                else
                {

                    DebugLogging.Info($"is null Graphic_Multi");
                }
            }


            var cacheField = typeof(Graphic).GetField("replacementInfoCache",
                BindingFlags.NonPublic | BindingFlags.Static);
            var cache = cacheField.GetValue(null) as IDictionary;
            cache.Clear();

            // Regenerate map to update visuals  
            Find.CurrentMap.mapDrawer.RegenerateEverythingNow();

            GlobalTextureAtlasManager.rebakeAtlas = true;
        }

        private void UpdateLinkedGraphicSubTexture(Graphic_Linked linkedGraphic, Texture2D newTexture)
        {
            try
            {
                // Get the subGraphic via reflection
                var subGraphicField = typeof(Graphic_Linked).GetField("subGraphic",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (subGraphicField == null)
                {
                    DebugLogging.Error("Could not find subGraphic field");
                    return;
                }

                var subGraphic = subGraphicField.GetValue(linkedGraphic) as Graphic;
                if (subGraphic == null)
                {
                    DebugLogging.Error("subGraphic is null");
                    return;
                }

                DebugLogging.Info($"Found subGraphic: {subGraphic.GetType().Name}");

                // Update the subGraphic's texture - this is the key!
                // The linked graphic will use this subGraphic's texture when generating materials
                subGraphic.MatSingle.mainTexture = newTexture;

                DebugLogging.Info("Updated subGraphic texture");

                // For Graphic_Single subgraphics, we might need to update the material directly
                if (subGraphic is Graphic_Single singleSubGraphic)
                {
                    UpdateGraphicSingleMaterial(singleSubGraphic, newTexture);
                }

            }
            catch (Exception ex)
            {
                DebugLogging.Error($"Failed to update linked graphic sub texture: {ex}");
            }
        }

        private void UpdateGraphicSingleMaterial(Graphic_Single graphicSingle, Texture2D newTexture)
        {
            try
            {
                // For Graphic_Single, we can try to replace the entire material
                var newMaterial = MaterialPool.MatFrom(newTexture, graphicSingle.Shader, Color.white);

                // Use reflection to set the private 'mat' field on Graphic_Single
                var matField = typeof(Graphic_Single).GetField("mat",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (matField != null)
                {
                    matField.SetValue(graphicSingle, newMaterial);
                    DebugLogging.Info("Replaced Graphic_Single material via reflection");
                }
                else
                {
                    // Fallback: just update the texture
                    graphicSingle.MatSingle.mainTexture = newTexture;
                    DebugLogging.Info("Updated Graphic_Single texture directly");
                }
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"Failed to update Graphic_Single material: {ex}");
                graphicSingle.MatSingle.mainTexture = newTexture;
            }
        }


        private void UpdateStackCountGraphics(Graphic_StackCount stackGraphic, Texture2D newTexture)
        {
            try
            {
                // Use reflection to access the private subGraphics field
                var subGraphicsField = typeof(Graphic_StackCount).GetField("subGraphics",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (subGraphicsField != null)
                {
                    var subGraphics = subGraphicsField.GetValue(stackGraphic) as Graphic[];
                    if (subGraphics != null)
                    {
                        foreach (var subGraphic in subGraphics)
                        {
                            subGraphic.MatSingle.mainTexture = newTexture;
                        }
                        DebugLogging.Info($"Updated {subGraphics.Length} sub-graphics for stack count");
                    }
                }

                // Update the base MatSingle
                stackGraphic.MatSingle.mainTexture = newTexture;
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"Failed to update stack graphics: {ex}");
                stackGraphic.MatSingle.mainTexture = newTexture;
            }
        }

        private void UpdateMultiGraphics(Graphic_Multi multiGraphic, Texture2D newTexture)
        {
            // Update all directional materials
            multiGraphic.MatSouth.mainTexture = newTexture;
            multiGraphic.MatNorth.mainTexture = newTexture;
            multiGraphic.MatEast.mainTexture = newTexture;
            multiGraphic.MatWest.mainTexture = newTexture;
            multiGraphic.MatSingle.mainTexture = newTexture;
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

        public static Texture2D CreateTextureFromBase64(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, true);
            texture.LoadImage(imageBytes);
            texture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
            return texture;
        }
    }
}