

using System;
using System.Collections;
using System.Collections.Generic;
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

        public void SetItemImageByName(ImageUploadRequest imageUpload, string imageBase64)
        {
            DebugLogging.Info($"imageUpload.ThingType {imageUpload.ThingType}");
            DebugLogging.Info($"imageUpload.Name {imageUpload.Name}");
            string thingName = imageUpload.Name.ToLower();
            Texture2D newTexture = CreateTextureFromBase64(imageBase64);

            switch (imageUpload.ThingType.ToLower())
            {
                case "building":
                    /* Type - Building */
                    foreach (var building in Find.CurrentMap.listerBuildings.allBuildingsColonist)
                    {
                        if (building.def.defName.ToLower() != thingName) continue;
                        BuildingUpdateTexture(building, newTexture, imageUpload.Direction);
                    }
                    break;
                case "item":
                    foreach (var item in Find.CurrentMap.listerThings.AllThings
                        .Where(p => p.def.defName.ToLower() == thingName)
                        .ToList())
                    {
                        ThingUpdateTexture(item, newTexture, imageUpload.Direction);
                    }
                    break;
                case "plant":
                    foreach (var item in Find.CurrentMap.listerThings.AllThings
                        .Where(p => p.def.defName.ToLower() == thingName)
                        .ToList())
                    {
                        ThingUpdateTexture(item, newTexture, imageUpload.Direction);
                    }
                    break;
                case "def":
                    ChangeDefTexture(imageUpload.ThingType, newTexture);
                    break;
                default:
                    throw new Exception("Unknown thing type");
            }
        }

        private void ChangeDefTexture(string thingName, Texture2D newTexture)
        {
            var thingDef = DefDatabase<ThingDef>.GetNamed(thingName);

            thingDef.DrawMatSingle.mainTexture = newTexture;
        }

        private void ThingUpdateTexture(Thing thing, Texture2D newTexture, string direction)
        {
            var thingDef = thing.def;

            thingDef.DrawMatSingle.mainTexture = newTexture;
            thing.DefaultGraphic.MatSingle.mainTexture = newTexture;

            if (thing.DefaultGraphic is Graphic_StackCount linkedGraphic)
            {
                var graphicCollection = thing.DefaultGraphic as Graphic_Collection;
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

            if (thing.DefaultGraphic is Graphic_Multi multiGraphic)
            {
                var graphicMulti = thing.DefaultGraphic as Graphic_Multi;
                if (graphicMulti != null)
                {
                    var matsField = typeof(Graphic_Multi).GetField("mats",
         BindingFlags.NonPublic | BindingFlags.Instance);
                    Material[] mats = matsField.GetValue(graphicMulti) as Material[];

                    // Modify existing materials directly - don't create new ones  
                    for (int i = 0; i < 4; i++)
                    {
                        if (mats[i] != null)
                        {
                            mats[i].mainTexture = newTexture;
                        }
                    }

                    // Clear atlas cache  
                    var cacheField = typeof(Graphic).GetField("replacementInfoCache",
                        BindingFlags.NonPublic | BindingFlags.Static);
                    var cache = cacheField.GetValue(null) as IDictionary;
                    cache.Clear();

                    // Regenerate map  
                    Find.CurrentMap.mapDrawer.RegenerateEverythingNow();

                }
                else
                {

                    DebugLogging.Info($"is null Graphic_Multi");
                }
            }

        }
        private void BuildingUpdateTexture(Building building, Texture2D newTexture, string direction)
        {
            if (building == null)
            {
                DebugLogging.Error("Building is null");
                return;
            }

            if (newTexture == null)
            {
                DebugLogging.Error("newTexture is null");
                return;
            }

            if (building.def == null)
            {
                DebugLogging.Error("building.def is null");
                return;
            }

            if (building.DefaultGraphic == null)
            {
                DebugLogging.Error("building.DefaultGraphic is null");
                return;
            }

            try
            {
                var thingDef = building.def;

                // Handle Graphic_StackCount
                if (building.DefaultGraphic is Graphic_StackCount)
                {
                    var graphicCollection = building.DefaultGraphic as Graphic_Collection;
                    if (graphicCollection != null)
                    {
                        DebugLogging.Info($"set graphicCollection");
                        var subGraphicsField = typeof(Graphic_Collection).GetField("subGraphics",
                            BindingFlags.NonPublic | BindingFlags.Instance);

                        if (subGraphicsField != null)
                        {
                            Graphic[] subGraphics = subGraphicsField.GetValue(graphicCollection) as Graphic[];

                            if (subGraphics != null)
                            {
                                foreach (var subGraphic in subGraphics)
                                {
                                    if (subGraphic?.MatSingle != null)
                                    {
                                        subGraphic.MatSingle.mainTexture = newTexture;
                                    }
                                    else
                                    {
                                        DebugLogging.Info($"subGraphic or MatSingle is null in Graphic_Collection");
                                    }
                                }
                            }
                            else
                            {
                                DebugLogging.Info($"subGraphics is null");
                            }
                        }
                        else
                        {
                            DebugLogging.Info($"subGraphicsField is null");
                        }
                    }
                    else
                    {
                        DebugLogging.Info($"graphicCollection is null");
                    }
                }

                // Handle Graphic_Linked
                if (building.DefaultGraphic is Graphic_Linked)
                {
                    var graphicLinked = building.DefaultGraphic as Graphic_Linked;
                    if (graphicLinked != null)
                    {
                        DebugLogging.Info($"Setting Graphic_Linked texture");

                        try
                        {
                            // STEP 1: Get the base material from the linked graphic
                            var subGraphicsField = typeof(Graphic_Linked).GetField("subGraphic",
                                BindingFlags.NonPublic | BindingFlags.Instance);

                            if (subGraphicsField != null)
                            {
                                Graphic subGraphic = subGraphicsField.GetValue(graphicLinked) as Graphic;

                                if (subGraphic?.MatSingle != null)
                                {
                                    Material baseMaterial = subGraphic.MatSingle;
                                    string matName = baseMaterial.name;
                                    DebugLogging.Info($"Base material name: {matName}");

                                    // STEP 3: Clear MaterialAtlasPool cache
                                    var atlasDictField = typeof(MaterialAtlasPool).GetField("atlasDict",
                                        BindingFlags.NonPublic | BindingFlags.Static);

                                    if (atlasDictField != null)
                                    {
                                        var atlasDict = atlasDictField.GetValue(null) as System.Collections.IDictionary;
                                        if (atlasDict != null)
                                        {
                                            atlasDict.Clear();
                                            DebugLogging.Info("Cleared MaterialAtlasPool cache");
                                        }
                                    }

                                    // STEP 4: Clear standard graphic cache
                                    var cacheField = typeof(Graphic).GetField("replacementInfoCache",
                                        BindingFlags.NonPublic | BindingFlags.Static);
                                    if (cacheField != null)
                                    {
                                        var cache = cacheField.GetValue(null) as IDictionary;
                                        cache?.Clear();
                                        DebugLogging.Info("Cleared graphic replacement cache");
                                    }

                                    // STEP 2: Change the base material's texture
                                    baseMaterial.mainTexture = newTexture;
                                    DebugLogging.Info($"Updated base material texture");

                                    ThingDef smoothWallDef = DefDatabase<ThingDef>.GetNamed("Wall");
                                    DebugLogging.Info($"Got def: {smoothWallDef.defName}, {smoothWallDef.description}");
                                    var graphicLinkedDef = smoothWallDef.graphic as Graphic_Linked;
                                    Graphic subGraphicDef = subGraphicsField.GetValue(graphicLinkedDef) as Graphic;
                                    DebugLogging.Info($"subGraphicDef.MatSingle.mainTexture: {subGraphicDef.MatSingle.mainTexture.name}");
                                    subGraphicDef.MatSingle.mainTexture = newTexture;

                                    MaterialAtlasPool.SubMaterialFromAtlas(subGraphicDef.MatSingle, LinkDirections.None);

                                    // STEP 5: Regenerate map
                                    if (Find.CurrentMap != null)
                                    {
                                        Find.CurrentMap.mapDrawer?.RegenerateEverythingNow();
                                        DebugLogging.Info("Regenerated map");
                                    }

                                    DebugLogging.Info($"Successfully updated smooth wall texture");
                                }
                            }
                            else
                            {
                                DebugLogging.Info($"subGraphicsField is null for Graphic_Linked");
                            }
                        }
                        catch (Exception ex)
                        {
                            DebugLogging.Error($"Failed to update Graphic_Linked texture: {ex.Message}");
                        }
                    }
                    else
                    {
                        DebugLogging.Info($"graphicLinked is null");
                    }
                }

                // Handle Graphic_Multi
                if (building.DefaultGraphic is Graphic_Multi)
                {
                    var graphicMulti = building.DefaultGraphic as Graphic_Multi;
                    if (graphicMulti != null)
                    {
                        var matsField = typeof(Graphic_Multi).GetField("mats",
                            BindingFlags.NonPublic | BindingFlags.Instance);

                        if (matsField != null)
                        {
                            Material[] mats = matsField.GetValue(graphicMulti) as Material[];

                            if (mats != null)
                            {
                                DebugLogging.Info($"direction is {direction}");

                                if (direction?.ToLower() == "all")
                                {
                                    for (int i = 0; i < Math.Min(4, mats.Length); i++)
                                    {
                                        if (mats[i] != null)
                                        {
                                            mats[i].mainTexture = newTexture;
                                        }
                                    }
                                }
                                else if (direction?.ToLower() == "north" && mats.Length > 0 && mats[0] != null)
                                {
                                    mats[0].mainTexture = newTexture;
                                }
                                else if (direction?.ToLower() == "east" && mats.Length > 1 && mats[1] != null)
                                {
                                    mats[1].mainTexture = newTexture;
                                }
                                else if (direction?.ToLower() == "south" && mats.Length > 2 && mats[2] != null)
                                {
                                    mats[2].mainTexture = newTexture;
                                }
                                else if (direction?.ToLower() == "west" && mats.Length > 3 && mats[3] != null)
                                {
                                    mats[3].mainTexture = newTexture;
                                }
                                else
                                {
                                    DebugLogging.Info($"Invalid direction or material index: {direction}");
                                }

                                // Clear atlas cache  
                                var cacheField = typeof(Graphic).GetField("replacementInfoCache",
                                    BindingFlags.NonPublic | BindingFlags.Static);
                                if (cacheField != null)
                                {
                                    var cache = cacheField.GetValue(null) as IDictionary;
                                    cache?.Clear();
                                }

                                // Regenerate map  
                                if (Find.CurrentMap != null)
                                {
                                    Find.CurrentMap.mapDrawer?.RegenerateEverythingNow();
                                }
                            }
                            else
                            {
                                DebugLogging.Info($"mats array is null");
                            }
                        }
                        else
                        {
                            DebugLogging.Info($"matsField is null");
                        }
                    }
                    else
                    {
                        DebugLogging.Info($"graphicMulti is null");
                    }
                }

                DebugLogging.Info($"Graphic type handled: {building.DefaultGraphic.GetType().Name}");
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"Error in BuildingUpdateTexture: {ex.Message}");
                DebugLogging.Error($"Stack trace: {ex.StackTrace}");
            }
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