using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Verse;

namespace RimworldRestApi.CameraStreamer
{
    public class StreamingSetup
    {
        public int Port = 5007;
        public string Address = "127.0.0.1";
        public int FrameWidth = 1280;
        public int FrameHeight = 720;
        public int TargetFPS = 15;
        public int JpegQuality = 50;
    }

    public class UdpCameraStreamer
    {
        private Camera _cachedCamera;
        private UdpClient udpClient;
        private Texture2D captureTexture;
        private RenderTexture renderTexture;
        private int streamPort = 5007;
        private string targetIP = "127.0.0.1";
        private int frameWidth = 1280;
        private int frameHeight = 720;
        private int targetFPS = 15;
        private int jpegQuality = 50;
        private float lastFrameTime;
        public bool IsStreaming = false;

        // UDP packet settings
        private const int MAX_PACKET_SIZE = 60000; // Safe size below 65,507 limit
        private const int HEADER_SIZE = 10; // Increased header for chunking info

        public void Setup(StreamingSetup setup)
        {
            Log.Message("[CameraStreamer] Setup");
            streamPort = setup.Port;
            targetIP = setup.Address;
            frameWidth = setup.FrameWidth;
            frameHeight = setup.FrameHeight;
            targetFPS = setup.TargetFPS;
            jpegQuality = setup.JpegQuality;
        }

        public StreamingSetup GetCurrentSetup()
        {
            Log.Message("[CameraStreamer] GetCurrentSetup");
            return new StreamingSetup
            {
                Port = streamPort,
                Address = targetIP,
                FrameWidth = frameWidth,
                FrameHeight = frameHeight,
                TargetFPS = targetFPS,
                JpegQuality = jpegQuality,
            };
        }

        public void StartStreaming()
        {
            try
            {
                udpClient = new UdpClient();
                captureTexture = new Texture2D(frameWidth, frameHeight, TextureFormat.RGB24, false);
                renderTexture = new RenderTexture(frameWidth, frameHeight, 24);

                Log.Message("[CameraStreamer] Started UDP streaming to " + targetIP + ":" + streamPort);

                CameraStreamerUpdater.Register(this);
                IsStreaming = true;
            }
            catch (Exception e)
            {
                Log.Error("[CameraStreamer] Failed to start streaming: " + e.Message);
                throw;
            }
        }

        public void StopStreaming()
        {
            CameraStreamerUpdater.Unregister(this);
            IsStreaming = false;

            udpClient?.Close();
            udpClient = null;

            if (renderTexture != null)
            {
                renderTexture.Release();
                UnityEngine.Object.Destroy(renderTexture);
            }

            if (captureTexture != null)
            {
                UnityEngine.Object.Destroy(captureTexture);
            }

            Log.Message("[CameraStreamer] Stopped UDP streaming");
        }

        public void Update()
        {
            if (Time.unscaledTime - lastFrameTime < 1f / targetFPS)
                return;

            try
            {
                CaptureAndSendFrame();
                lastFrameTime = Time.unscaledTime;
            }
            catch (Exception e)
            {
                Log.Error("[CameraStreamer] Error capturing frame: " + e.Message);
            }
        }

        private void CaptureAndSendFrame()
        {
            Camera camera = _cachedCamera ?? Find.Camera ?? Camera.current;

            if (camera == null && Camera.allCameras.Length > 0)
            {
                camera = Camera.allCameras[0];
                _cachedCamera = camera; // Cache for future use
            }

            if (camera == null) return;

            try
            {
                // Use existing render texture instead of reassigning
                RenderTexture previousTarget = camera.targetTexture;
                camera.targetTexture = renderTexture;
                camera.Render();
                camera.targetTexture = previousTarget;

                RenderTexture.active = renderTexture;
                captureTexture.ReadPixels(new Rect(0, 0, frameWidth, frameHeight), 0, 0);
                captureTexture.Apply();
                RenderTexture.active = null;

                // Encode to JPEG
                byte[] jpegData = ImageConversion.EncodeToJPG(captureTexture, jpegQuality);

                // Send data based on size
                if (jpegData.Length > MAX_PACKET_SIZE - HEADER_SIZE)
                {
                    SendDataInChunks(jpegData);
                }
                else
                {
                    SendSinglePacket(jpegData);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Frame capture failed: {ex.Message}");
            }
        }

        private void SendSinglePacket(byte[] imageData)
        {
            byte[] packet = CreateDataPacket(imageData, 0, 1); // chunk 0 of 1
            udpClient?.Send(packet, packet.Length, targetIP, streamPort);
        }

        private void SendDataInChunks(byte[] imageData)
        {
            int maxChunkSize = MAX_PACKET_SIZE - HEADER_SIZE;
            int totalChunks = (imageData.Length + maxChunkSize - 1) / maxChunkSize;

            for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
            {
                int chunkOffset = chunkIndex * maxChunkSize;
                int chunkSize = Math.Min(maxChunkSize, imageData.Length - chunkOffset);

                byte[] chunkData = new byte[chunkSize];
                Buffer.BlockCopy(imageData, chunkOffset, chunkData, 0, chunkSize);

                byte[] packet = CreateDataPacket(chunkData, chunkIndex, totalChunks);
                udpClient?.Send(packet, packet.Length, targetIP, streamPort);
            }
        }

        private byte[] CreateDataPacket(byte[] imageData, int chunkIndex, int totalChunks)
        {
            // Packet structure: [3 bytes header][4 bytes data length][2 bytes chunk info][image data]
            byte[] header = System.Text.Encoding.ASCII.GetBytes("CAM");
            byte[] lengthBytes = BitConverter.GetBytes(imageData.Length);
            byte[] chunkInfo = new byte[] { (byte)chunkIndex, (byte)totalChunks };

            int headerSize = header.Length + lengthBytes.Length + chunkInfo.Length;
            byte[] packet = new byte[headerSize + imageData.Length];

            int offset = 0;
            Buffer.BlockCopy(header, 0, packet, offset, header.Length);
            offset += header.Length;

            Buffer.BlockCopy(lengthBytes, 0, packet, offset, lengthBytes.Length);
            offset += lengthBytes.Length;

            Buffer.BlockCopy(chunkInfo, 0, packet, offset, chunkInfo.Length);
            offset += chunkInfo.Length;

            Buffer.BlockCopy(imageData, 0, packet, offset, imageData.Length);
            return packet;
        }


        // Method to adjust quality dynamically if needed
        public void SetQuality(int quality)
        {
            jpegQuality = Mathf.Clamp(quality, 10, 100);
        }

        public void SetResolution(int width, int height)
        {
            frameWidth = width;
            frameHeight = height;
        }
    }

    [StaticConstructorOnStartup]
    public static class CameraStreamerUpdater
    {
        private static List<UdpCameraStreamer> activeStreamers = new List<UdpCameraStreamer>();
        private static GameObject updaterObject;
        private static CameraStreamMonoBehaviour updaterComponent;

        static CameraStreamerUpdater()
        {
            // Create update handler when game starts
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                updaterObject = new GameObject("CameraStreamUpdater");
                updaterComponent = updaterObject.AddComponent<CameraStreamMonoBehaviour>();
                UnityEngine.Object.DontDestroyOnLoad(updaterObject);
            });
        }

        public static void Register(UdpCameraStreamer streamer)
        {
            if (!activeStreamers.Contains(streamer))
            {
                activeStreamers.Add(streamer);
            }
        }

        public static void Unregister(UdpCameraStreamer streamer)
        {
            activeStreamers.Remove(streamer);
        }

        public static void UpdateAll()
        {
            for (int i = activeStreamers.Count - 1; i >= 0; i--)
            {
                activeStreamers[i]?.Update();
            }
        }
    }

    public class CameraStreamMonoBehaviour : MonoBehaviour
    {
        void Update()
        {
            CameraStreamerUpdater.UpdateAll();
        }
    }
}
