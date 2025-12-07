using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RIMAPI.Core;
using RIMAPI.Models;
using RimWorld;
using UnityEngine;
using Verse;

namespace RIMAPI.Helpers
{
    public static class TextureHelper
    {
        public static ImageDto GetThingImage(int thingId)
        {
            ImageDto image = new ImageDto();

            return image;
        }

        public static ImageDto GetItemImageByName(string thingName)
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

        public static void SetItemImageByName(ImageUploadRequest imageUpload, string imageBase64)
        {
            Core.LogApi.Info($"imageUpload.ThingType {imageUpload.ThingType}");
            Core.LogApi.Info($"imageUpload.Name {imageUpload.Name}");
            string thingName = imageUpload.Name.ToLower();
            Texture2D newTexture = CreateTextureFromBase64(imageBase64);

            switch (imageUpload.ThingType.ToLower())
            {
                case "building":
                    /* Type - Building */
                    foreach (var building in Find.CurrentMap.listerBuildings.allBuildingsColonist)
                    {
                        if (building.def.defName.ToLower() != thingName)
                            continue;
                        BuildingUpdateTexture(building, newTexture, imageUpload.Direction);
                    }
                    break;
                case "linked":
                    /* Type - Linked */
                    UpdateTexture_Linked(imageUpload, newTexture);
                    break;
                case "item":
                    foreach (
                        var item in Find
                            .CurrentMap.listerThings.AllThings.Where(p =>
                                p.def.defName.ToLower() == thingName
                            )
                            .ToList()
                    )
                    {
                        ThingUpdateTexture(item, newTexture, imageUpload.Direction);
                    }
                    break;
                case "plant":
                    foreach (
                        var item in Find
                            .CurrentMap.listerThings.AllThings.Where(p =>
                                p.def.defName.ToLower() == thingName
                            )
                            .ToList()
                    )
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

        public static void ChangeDefTexture(string thingName, Texture2D newTexture)
        {
            var thingDef = DefDatabase<ThingDef>.GetNamed(thingName);

            thingDef.DrawMatSingle.mainTexture = newTexture;
        }

        public static void ThingUpdateTexture(Thing thing, Texture2D newTexture, string direction)
        {
            var thingDef = thing.def;

            thingDef.DrawMatSingle.mainTexture = newTexture;
            thing.DefaultGraphic.MatSingle.mainTexture = newTexture;

            if (thing.DefaultGraphic is Graphic_StackCount linkedGraphic)
            {
                var graphicCollection = thing.DefaultGraphic as Graphic_Collection;
                if (graphicCollection != null)
                {
                    Core.LogApi.Info($"set graphicCollection");
                    var subGraphicsField = typeof(Graphic_Collection).GetField(
                        "subGraphics",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );
                    Graphic[] subGraphics =
                        subGraphicsField.GetValue(graphicCollection) as Graphic[];

                    // Now you can iterate through or access individual subgraphics
                    foreach (var subGraphic in subGraphics)
                    {
                        subGraphic.MatSingle.mainTexture = newTexture;
                    }
                }
                else
                {
                    Core.LogApi.Info($"graphicCollection is null");
                }
            }

            if (thing.DefaultGraphic is Graphic_Multi multiGraphic)
            {
                var graphicMulti = thing.DefaultGraphic as Graphic_Multi;
                if (graphicMulti != null)
                {
                    var matsField = typeof(Graphic_Multi).GetField(
                        "mats",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );
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
                    var cacheField = typeof(Graphic).GetField(
                        "replacementInfoCache",
                        BindingFlags.NonPublic | BindingFlags.Static
                    );
                    var cache = cacheField.GetValue(null) as IDictionary;
                    cache.Clear();

                    // Regenerate map
                    Find.CurrentMap.mapDrawer.RegenerateEverythingNow();
                }
                else
                {
                    Core.LogApi.Info($"is null Graphic_Multi");
                }
            }
        }

        public static void BuildingUpdateTexture(
            Building building,
            Texture2D newTexture,
            string direction
        )
        {
            if (building == null)
            {
                Core.LogApi.Error("Building is null");
                return;
            }

            if (newTexture == null)
            {
                Core.LogApi.Error("newTexture is null");
                return;
            }

            if (building.def == null)
            {
                Core.LogApi.Error("building.def is null");
                return;
            }

            if (building.DefaultGraphic == null)
            {
                Core.LogApi.Error("building.DefaultGraphic is null");
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
                        Core.LogApi.Info($"set graphicCollection");
                        var subGraphicsField = typeof(Graphic_Collection).GetField(
                            "subGraphics",
                            BindingFlags.NonPublic | BindingFlags.Instance
                        );

                        if (subGraphicsField != null)
                        {
                            Graphic[] subGraphics =
                                subGraphicsField.GetValue(graphicCollection) as Graphic[];

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
                                        Core.LogApi.Info(
                                            $"subGraphic or MatSingle is null in Graphic_Collection"
                                        );
                                    }
                                }
                            }
                            else
                            {
                                Core.LogApi.Info($"subGraphics is null");
                            }
                        }
                        else
                        {
                            Core.LogApi.Info($"subGraphicsField is null");
                        }
                    }
                    else
                    {
                        Core.LogApi.Info($"graphicCollection is null");
                    }
                }

                // Handle Graphic_Linked
                if (building.DefaultGraphic is Graphic_Linked)
                {
                    var graphicLinked = building.DefaultGraphic as Graphic_Linked;
                    if (graphicLinked != null)
                    {
                        Core.LogApi.Info($"set Graphic_Linked");
                        var subGraphicsField = typeof(Graphic_Linked).GetField(
                            "subGraphic",
                            BindingFlags.NonPublic | BindingFlags.Instance
                        );

                        if (subGraphicsField != null)
                        {
                            Graphic subGraphic =
                                subGraphicsField.GetValue(graphicLinked) as Graphic;

                            if (subGraphic?.MatSingle != null)
                            {
                                string matName = subGraphic.MatSingle.name;
                                Core.LogApi.Info($"name: {matName}");
                                subGraphic.MatSingle.mainTexture = newTexture;
                                Core.LogApi.Info($"new name: {matName}");

                                try
                                {
                                    // Get the atlasDict field from MaterialAtlasPool
                                    var atlasDictField = typeof(MaterialAtlasPool).GetField(
                                        "atlasDict",
                                        BindingFlags.NonPublic | BindingFlags.Static
                                    );

                                    if (atlasDictField != null)
                                    {
                                        var atlasDict =
                                            atlasDictField.GetValue(null)
                                            as System.Collections.IDictionary;

                                        if (atlasDict != null)
                                        {
                                            foreach (
                                                System.Collections.DictionaryEntry entry in atlasDict
                                            )
                                            {
                                                Material keyMaterial = entry.Key as Material;
                                                Core.LogApi.Info($"materials: {keyMaterial.name}");

                                                if (
                                                    keyMaterial != null
                                                    && (
                                                        keyMaterial.name.Contains("Plank")
                                                        || keyMaterial.name.Contains("plank")
                                                    )
                                                )
                                                {
                                                    var materialAtlas = entry.Value;
                                                    var materialAtlasType = materialAtlas.GetType();
                                                    var subMatsField = materialAtlasType.GetField(
                                                        "subMats",
                                                        BindingFlags.NonPublic
                                                            | BindingFlags.Instance
                                                    );

                                                    if (subMatsField != null)
                                                    {
                                                        Material[] subMats =
                                                            subMatsField.GetValue(materialAtlas)
                                                            as Material[];

                                                        if (subMats != null)
                                                        {
                                                            // Change specific index
                                                            //int changeIndex = 0; // Set your desired index here
                                                            //
                                                            //if (changeIndex >= 0 && changeIndex < subMats.Length)
                                                            //{
                                                            //    if (subMats[changeIndex] != null)
                                                            //    {
                                                            //        subMats[changeIndex].mainTexture = newTexture;
                                                            //        DebugLogging.Info($"Updated subMats[{changeIndex}] with new texture");
                                                            //    }
                                                            //    else
                                                            //    {
                                                            //        DebugLogging.Info($"subMats[{changeIndex}] is null");
                                                            //    }
                                                            //}
                                                            //else
                                                            //{
                                                            //    DebugLogging.Info($"Index {changeIndex} is out of range (0-{subMats.Length - 1})");
                                                            //}

                                                            for (int i = 0; i < subMats.Length; i++)
                                                            {
                                                                if (subMats[i] != null)
                                                                {
                                                                    subMats[i].mainTexture =
                                                                        newTexture;
                                                                    Core.LogApi.Info(
                                                                        $"Updated subMats[{i}] with new texture"
                                                                    );
                                                                }
                                                                else
                                                                {
                                                                    Core.LogApi.Info(
                                                                        $"subMats[{i}] is null"
                                                                    );
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Core.LogApi.Info(
                                                                "subMats array is null"
                                                            );
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Core.LogApi.Info("subMatsField not found");
                                                    }
                                                    break; // Found our material, no need to continue looping
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Core.LogApi.Info("atlasDict is null");
                                        }
                                    }
                                    else
                                    {
                                        Core.LogApi.Info("atlasDictField not found");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Core.LogApi.Error(
                                        $"Failed to update subMat texture: {ex.Message}"
                                    );
                                    Core.LogApi.Error($"Exception type: {ex.GetType().Name}");
                                }
                            }

                            // Clear atlas cache
                            var cacheField = typeof(Graphic).GetField(
                                "replacementInfoCache",
                                BindingFlags.NonPublic | BindingFlags.Static
                            );
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
                            Core.LogApi.Info($"subGraphicsField is null for Graphic_Linked");
                        }
                    }
                    else
                    {
                        Core.LogApi.Info($"graphicLinked is null");
                    }
                }

                // Handle Graphic_Multi
                if (building.DefaultGraphic is Graphic_Multi)
                {
                    var graphicMulti = building.DefaultGraphic as Graphic_Multi;
                    if (graphicMulti != null)
                    {
                        var matsField = typeof(Graphic_Multi).GetField(
                            "mats",
                            BindingFlags.NonPublic | BindingFlags.Instance
                        );

                        if (matsField != null)
                        {
                            Material[] mats = matsField.GetValue(graphicMulti) as Material[];

                            if (mats != null)
                            {
                                Core.LogApi.Info($"direction is {direction}");

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
                                else if (
                                    direction?.ToLower() == "north"
                                    && mats.Length > 0
                                    && mats[0] != null
                                )
                                {
                                    mats[0].mainTexture = newTexture;
                                }
                                else if (
                                    direction?.ToLower() == "east"
                                    && mats.Length > 1
                                    && mats[1] != null
                                )
                                {
                                    mats[1].mainTexture = newTexture;
                                }
                                else if (
                                    direction?.ToLower() == "south"
                                    && mats.Length > 2
                                    && mats[2] != null
                                )
                                {
                                    mats[2].mainTexture = newTexture;
                                }
                                else if (
                                    direction?.ToLower() == "west"
                                    && mats.Length > 3
                                    && mats[3] != null
                                )
                                {
                                    mats[3].mainTexture = newTexture;
                                }
                                else
                                {
                                    Core.LogApi.Info(
                                        $"Invalid direction or material index: {direction}"
                                    );
                                }

                                // Clear atlas cache
                                var cacheField = typeof(Graphic).GetField(
                                    "replacementInfoCache",
                                    BindingFlags.NonPublic | BindingFlags.Static
                                );
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
                                Core.LogApi.Info($"mats array is null");
                            }
                        }
                        else
                        {
                            Core.LogApi.Info($"matsField is null");
                        }
                    }
                    else
                    {
                        Core.LogApi.Info($"graphicMulti is null");
                    }
                }

                Core.LogApi.Info($"Graphic type handled: {building.DefaultGraphic.GetType().Name}");
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error in BuildingUpdateTexture: {ex.Message}");
                Core.LogApi.Error($"Stack trace: {ex.StackTrace}");
            }
        }

        public static IDictionary GetAtlasDictionary()
        {
            try
            {
                var atlasDictField = typeof(MaterialAtlasPool).GetField(
                    "atlasDict",
                    BindingFlags.NonPublic | BindingFlags.Static
                );

                if (atlasDictField == null)
                {
                    Debug.LogError("atlasDict field not found in MaterialAtlasPool");
                    return null;
                }

                var atlasDict = atlasDictField.GetValue(null) as IDictionary;

                if (atlasDict == null)
                {
                    Debug.LogWarning("atlasDict is null - no materials loaded yet");
                    return new Hashtable(); // Return empty dictionary instead of null
                }

                return atlasDict;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting atlas dictionary: {ex.Message}");
                return new Hashtable(); // Return empty dictionary on error
            }
        }

        public static List<Material> GetAtlasDictionaryMaterials()
        {
            var atlasDict = GetAtlasDictionary();
            List<Material> materialAtlases = new List<Material>();

            if (atlasDict == null || atlasDict.Count == 0)
            {
                Debug.LogWarning("Atlas dictionary is null or empty");
                return materialAtlases; // Return empty list
            }

            try
            {
                foreach (DictionaryEntry entry in atlasDict)
                {
                    Material keyMaterial = entry.Key as Material;
                    if (keyMaterial != null)
                    {
                        materialAtlases.Add(keyMaterial);
                    }
                    else
                    {
                        Debug.LogWarning("Found null material in atlas dictionary");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing atlas materials: {ex.Message}");
            }

            return materialAtlases;
        }

        public static Vector2 StringToVec2(Vector2 origin, string str)
        {
            if (string.IsNullOrEmpty(str))
                return origin;

            string[] parts = str.Split(';');
            if (parts.Length != 2)
                return origin;

            float x = origin.x;
            float y = origin.y;

            // Parse X component
            if (parts[0].ToLower() != "same")
            {
                if (float.TryParse(parts[0], out float parsedX))
                {
                    x = parsedX;
                }
            }

            // Parse Y component
            if (parts[1].ToLower() != "same")
            {
                if (float.TryParse(parts[1], out float parsedY))
                {
                    y = parsedY;
                }
            }

            return new Vector2(x, y);
        }

        public static void UpdateTexture_Linked(
            ImageUploadRequest imageUpload,
            Texture2D newTexture
        )
        {
            try
            {
                var atlasDict = GetAtlasDictionary();
                List<object> materialAtlases = new List<object>();

                foreach (System.Collections.DictionaryEntry entry in atlasDict)
                {
                    Material keyMaterial = entry.Key as Material;
                    Core.LogApi.Info($"materials: {keyMaterial.name}");

                    if (
                        keyMaterial != null
                        && keyMaterial.name.ToLower().Contains(imageUpload.Name.ToLower())
                    )
                    {
                        materialAtlases.Add(entry.Value);
                    }
                }

                Core.LogApi.Info($"materialAtlases count: {materialAtlases.Count}");
                foreach (var atlas in materialAtlases)
                {
                    var materialAtlas = atlas;
                    var materialAtlasType = materialAtlas.GetType();
                    var subMatsField = materialAtlasType.GetField(
                        "subMats",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );

                    Material[] subMats = subMatsField.GetValue(materialAtlas) as Material[];

                    if (subMats != null)
                    {
                        // Change specific index
                        if (imageUpload.UpdateItemIndex != -1)
                        {
                            int changeIndex = imageUpload.UpdateItemIndex; // Set your desired index here

                            if (changeIndex >= 0 && changeIndex < subMats.Length)
                            {
                                if (subMats[changeIndex] != null)
                                {
                                    subMats[changeIndex].mainTexture = newTexture;
                                    Core.LogApi.Info(
                                        $"Updated subMats[{changeIndex}] with new texture"
                                    );
                                }
                                else
                                {
                                    Core.LogApi.Info($"subMats[{changeIndex}] is null");
                                }
                            }
                            else
                            {
                                Core.LogApi.Info(
                                    $"Index {changeIndex} is out of range (0-{subMats.Length - 1})"
                                );
                            }
                        }
                        else
                        {
                            for (int i = 0; i < subMats.Length; i++)
                            {
                                if (subMats[i] != null)
                                {
                                    subMats[i].mainTexture = newTexture;
                                    subMats[i].mainTextureOffset = StringToVec2(
                                        subMats[i].mainTextureOffset,
                                        imageUpload.Offset
                                    );
                                    subMats[i].mainTextureScale = StringToVec2(
                                        subMats[i].mainTextureScale,
                                        imageUpload.Scale
                                    );
                                    //subMats[i].color = imageUpload.Color;
                                    Core.LogApi.Info($"Updated subMats[{i}] with new texture");
                                }
                                else
                                {
                                    Core.LogApi.Info($"subMats[{i}] is null");
                                }
                            }
                        }
                    }
                    else
                    {
                        Core.LogApi.Info("subMats array is null");
                    }
                }
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Failed to update subMat texture: {ex.Message}");
                Core.LogApi.Error($"Exception type: {ex.GetType().Name}");
            }

            RefreshGraphics();
        }

        public static void RefreshGraphics()
        {
            // Clear atlas cache
            var cacheField = typeof(Graphic).GetField(
                "replacementInfoCache",
                BindingFlags.NonPublic | BindingFlags.Static
            );
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

        public static ImageDto GetPawnPortraitImage(
            Pawn pawn,
            int width,
            int height,
            string faceDir = "south"
        )
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

                renderTexture.Release();
            }
            catch (Exception ex)
            {
                image.Result = ex.Message;
                throw;
            }
            return image;
        }

        public static string TextureToBase64(Texture2D texture)
        {
            try
            {
                // Encode to PNG
                byte[] imageBytes = ImageConversion.EncodeToPNG(texture);
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                LogApi.Error($"TextureToBase64 error: {ex}");
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
