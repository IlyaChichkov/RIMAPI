using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using RIMAPI.CameraStreamer;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Models.Camera;
using UnityEngine;
using Verse;

namespace RIMAPI.Services
{
    public class CameraService : ICameraService
    {
        public CameraService() { }

        public ApiResult ChangeZoom(int zoom)
        {
            try
            {
                Find.CameraDriver.SetRootPosAndSize(
                    Find.CameraDriver.MapPosition.ToVector3(),
                    zoom
                );
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
            return ApiResult.Ok();
        }

        private static void SetScreenshotMode(bool state)
        {
            if (Find.UIRoot == null || Find.UIRoot.screenshotMode == null) return;
            var handler = Find.UIRoot.screenshotMode;

#if RIMWORLD_1_6
            handler.Active = state;
#else
            // In 1.5, we must use Reflection to write to the private 'active' backing field
            FieldInfo field = typeof(ScreenshotModeHandler).GetField("active", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(handler, state);
            }
            else
            {
                // Fallback for auto-properties
                PropertyInfo prop = typeof(ScreenshotModeHandler).GetProperty("Active", BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(handler, state, null);
                }
            }
#endif
        }

        private static void TakeNativeScreenshotDirect(string fileName)
        {
            string folderPath = GenFilePaths.ScreenshotFolderPath;

            DirectoryInfo dir = new DirectoryInfo(folderPath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            string fullPath = Path.Combine(folderPath, fileName + ".png");
            ScreenCapture.CaptureScreenshot(fullPath);
        }

        public ApiResult<string> TakeNativeScreenshot(NativeScreenshotRequestDto request)
        {
            if (Find.CurrentMap == null)
            {
                return ApiResult<string>.Fail("Cannot take a screenshot. No map is currently loaded.");
            }

            // Start a coroutine to handle the asynchronous screenshot delay
            Find.CameraDriver.StartCoroutine(NativeScreenshotRoutine(request));

            string savedName = string.IsNullOrEmpty(request.FileName) ? "Auto-generated" : request.FileName;
            return ApiResult<string>.Ok($"Native screenshot '{savedName}' queued successfully.");
        }

        private IEnumerator NativeScreenshotRoutine(NativeScreenshotRequestDto request)
        {
            bool originalUIState = false;

            // 1. Move the camera instantly
            if (request.CenterX.HasValue && request.CenterZ.HasValue && request.ZoomLevel.HasValue)
            {
                Vector3 targetPos = new Vector3(request.CenterX.Value, 0, request.CenterZ.Value);
                Find.CameraDriver.SetRootPosAndSize(targetPos, request.ZoomLevel.Value);

                // Yield for one frame to let the game engine render the new camera position
                yield return new WaitForEndOfFrame();
            }

            // 2. Hide UI
            if (request.HideUI && Find.UIRoot != null)
            {
                originalUIState = Find.UIRoot.screenshotMode.Active;
                SetScreenshotMode(true);
            }

            // Trigger the native screenshot tool
            string fileName = string.IsNullOrEmpty(request.FileName) ? $"RIMAPI_{DateTime.Now.Ticks}" : request.FileName;
            TakeNativeScreenshotDirect(fileName);

            // WAIT for the frame to finish rendering so the screenshot actually captures!
            yield return new WaitForEndOfFrame();

            // 5. Restore UI
            if (request.HideUI && Find.UIRoot != null)
            {
                SetScreenshotMode(originalUIState);
            }
        }

        public ApiResult MoveToPosition(int x, int y)
        {
            try
            {
                IntVec3 position = new IntVec3(x, 0, y);
                Find.CameraDriver.JumpToCurrentMapLoc(position);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
            return ApiResult.Ok();
        }
        private static CameraScreenshotResponseDto CaptureScreenshotAsDto(CameraScreenshotRequestDto request)
        {
            request = request ?? new CameraScreenshotRequestDto();

            string format = (request.Format ?? "jpeg").ToLower();
            int quality = Mathf.Clamp(request.Quality, 1, 100);

            int targetWidth = request.Width ?? Screen.width;
            int targetHeight = request.Height ?? Screen.height;

            targetWidth = Mathf.Clamp(targetWidth, 16, 7680);
            targetHeight = Mathf.Clamp(targetHeight, 16, 4320);

            RenderTexture tempRT = RenderTexture.GetTemporary(targetWidth, targetHeight, 24, RenderTextureFormat.ARGB32);
            RenderTexture flippedRT = null;
            Texture2D texture = null;

            try
            {
                bool needsFlip = false;

                // 1. SYNCHRONOUS UI HIDE (Direct Camera Render)
                if (request.HideUI && Find.Camera != null)
                {
                    // Hijack the main world camera. 
                    // This natively renders right-side-up, so we DO NOT need to flip it later.
                    RenderTexture oldTarget = Find.Camera.targetTexture;
                    Find.Camera.targetTexture = tempRT;
                    Find.Camera.Render();
                    Find.Camera.targetTexture = oldTarget;

                    needsFlip = false;
                }
                else
                {
                    // Capture the screen buffer (includes UI).
                    // This grabs the raw display memory, which usually needs flipping.
                    ScreenCapture.CaptureScreenshotIntoRenderTexture(tempRT);

                    needsFlip = true;
                }

                // 2. GPU Blit (Conditionally flip based on the render method)
                flippedRT = RenderTexture.GetTemporary(targetWidth, targetHeight, 24, RenderTextureFormat.ARGB32);

                if (needsFlip)
                {
                    // Invert the Y-axis for ScreenCapture
                    Graphics.Blit(tempRT, flippedRT, new Vector2(1, -1), new Vector2(0, 1));
                }
                else
                {
                    // Copy exactly as-is for the Camera Render
                    Graphics.Blit(tempRT, flippedRT);
                }

                // 3. Read the pixels from the processed texture
                texture = new Texture2D(flippedRT.width, flippedRT.height, TextureFormat.RGB24, false);
                RenderTexture.active = flippedRT;
                texture.ReadPixels(new Rect(0, 0, flippedRT.width, flippedRT.height), 0, 0);
                texture.Apply();
                RenderTexture.active = null;

                // 4. Encode based on requested format
                byte[] imageBytes;
                string actualFormat;

                if (format == "png")
                {
                    imageBytes = texture.EncodeToPNG();
                    actualFormat = "png";
                }
                else
                {
                    imageBytes = texture.EncodeToJPG(quality);
                    actualFormat = "jpeg";
                }

                string base64String = Convert.ToBase64String(imageBytes);

                var dto = new CameraScreenshotResponseDto
                {
                    Image = new ImageData
                    {
                        DataUri = $"data:image/{actualFormat};base64,{base64String}"
                    },
                    Metadata = new ImageMetadata
                    {
                        Format = actualFormat,
                        Width = flippedRT.width,
                        Height = flippedRT.height,
                        SizeBytes = imageBytes.Length
                    },
                    GameContext = new GameContext
                    {
                        CurrentTick = Find.TickManager != null ? Find.TickManager.TicksGame : 0
                    }
                };

                return dto;
            }
            finally
            {
                // 5. Cleanup memory
                if (tempRT != null) RenderTexture.ReleaseTemporary(tempRT);
                if (flippedRT != null) RenderTexture.ReleaseTemporary(flippedRT);
                if (texture != null) UnityEngine.Object.Destroy(texture);
            }
        }

        public ApiResult<CameraScreenshotResponseDto> MakeScreenshot(CameraScreenshotRequestDto request)
        {
            try
            {
                var dto = CaptureScreenshotAsDto(request);
                return ApiResult<CameraScreenshotResponseDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                Log.Error($"[RIMAPI] Failed to capture screenshot: {ex}");
                return ApiResult<CameraScreenshotResponseDto>.Fail(ex.Message);
            }
        }

        public ApiResult<StreamStatusDto> GetStreamStatus(ICameraStream stream)
        {
            var result = stream.GetStatus();
            return ApiResult<StreamStatusDto>.Ok(result);
        }

        public ApiResult SetupStream(ICameraStream stream, StreamConfigDto config)
        {
            try
            {
                stream.SetConfig(config);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
            return ApiResult.Ok();
        }

        public ApiResult StartStream(ICameraStream stream)
        {
            try
            {
                stream.StartStreaming();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
            return ApiResult.Ok();
        }

        public ApiResult StopStream(ICameraStream stream)
        {
            try
            {
                stream.StopStreaming();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
            return ApiResult.Ok();
        }

        public Task<ApiResult<CameraScreenshotResponseDto>> MakeScreenshotAsync(CameraScreenshotRequestDto request)
        {
            var tcs = new TaskCompletionSource<ApiResult<CameraScreenshotResponseDto>>();

            // Ensure the coroutine is launched from the main Unity thread
            GameThreadUtility.InvokeAsync(() =>
            {
                if (Find.CurrentMap == null || Find.CameraDriver == null)
                {
                    tcs.TrySetResult(ApiResult<CameraScreenshotResponseDto>.Fail("Cannot take a screenshot. No map is currently loaded."));
                    return;
                }

                // Start the Coroutine, passing the request and the TaskCompletionSource
                Find.CameraDriver.StartCoroutine(CaptureScreenshotCoroutine(request, tcs));
            });

            // The HTTP Controller will await this task, pausing until TrySetResult is called below!
            return tcs.Task;
        }

        private IEnumerator CaptureScreenshotCoroutine(CameraScreenshotRequestDto request, TaskCompletionSource<ApiResult<CameraScreenshotResponseDto>> tcs)
        {
            request = request ?? new CameraScreenshotRequestDto();

            string format = (request.Format ?? "jpeg").ToLower();
            int quality = Mathf.Clamp(request.Quality, 1, 100);

            int targetWidth = request.Width ?? Screen.width;
            int targetHeight = request.Height ?? Screen.height;

            targetWidth = Mathf.Clamp(targetWidth, 16, 7680);
            targetHeight = Mathf.Clamp(targetHeight, 16, 4320);

            bool originalUIState = false;

            // 1. Hide the UI BEFORE the try-catch block
            if (request.HideUI && Find.UIRoot != null)
            {
                originalUIState = Find.UIRoot.screenshotMode.Active;
                SetScreenshotMode(true);
            }

            // Yield to let Unity render a brand new frame WITHOUT the UI!
            // We must do this outside the try-catch to satisfy C# compiler rules (CS1626).
            yield return new WaitForEndOfFrame();

            try
            {
                // Read the raw screen buffer directly! 
                // Because we waited for the end of the frame, the IMGUI (RimWorld UI) is guaranteed to be here.
                RenderTexture.active = null; // Ensure we are reading from the screen, not an RT
                Texture2D screenTex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                screenTex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                screenTex.Apply();

                Texture2D finalTexture = screenTex;
                RenderTexture resizedRT = null;

                // Handle Resizing (If the API client requested a smaller/larger width than the game window)
                bool needsResize = (targetWidth != Screen.width || targetHeight != Screen.height);
                if (needsResize)
                {
                    resizedRT = RenderTexture.GetTemporary(targetWidth, targetHeight, 24, RenderTextureFormat.ARGB32);

                    // Copy the full screen into the resized target
                    Graphics.Blit(screenTex, resizedRT);

                    // Read the resized pixels into a new texture
                    finalTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
                    RenderTexture.active = resizedRT;
                    finalTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
                    finalTexture.Apply();
                    RenderTexture.active = null;
                }

                byte[] imageBytes;
                string actualFormat;

                if (format == "png")
                {
                    imageBytes = finalTexture.EncodeToPNG();
                    actualFormat = "png";
                }
                else
                {
                    imageBytes = finalTexture.EncodeToJPG(quality);
                    actualFormat = "jpeg";
                }

                string base64String = Convert.ToBase64String(imageBytes);

                var dto = new CameraScreenshotResponseDto
                {
                    Image = new ImageData
                    {
                        DataUri = $"data:image/{actualFormat};base64,{base64String}"
                    },
                    Metadata = new ImageMetadata
                    {
                        Format = actualFormat,
                        Width = targetWidth,
                        Height = targetHeight,
                        SizeBytes = imageBytes.Length
                    },
                    GameContext = new GameContext
                    {
                        CurrentTick = Find.TickManager != null ? Find.TickManager.TicksGame : 0
                    }
                };

                // Cleanup memory to prevent massive memory leaks
                UnityEngine.Object.Destroy(screenTex);
                if (needsResize)
                {
                    UnityEngine.Object.Destroy(finalTexture);
                    RenderTexture.ReleaseTemporary(resizedRT);
                }

                tcs.TrySetResult(ApiResult<CameraScreenshotResponseDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                Log.Error($"[RIMAPI] Failed to capture DTO screenshot: {ex}");
                tcs.TrySetResult(ApiResult<CameraScreenshotResponseDto>.Fail(ex.Message));
            }
            finally
            {
                if (request.HideUI && Find.UIRoot != null)
                {
                    SetScreenshotMode(originalUIState);
                }
            }
        }
    }
}
