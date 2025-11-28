using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RIMAPI.CameraStreamer
{
    [StaticConstructorOnStartup]
    public static class CameraStreamerUpdater
    {
        private static List<UdpCameraStream> activeStreamers = new List<UdpCameraStream>();
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

        public static void Register(UdpCameraStream streamer)
        {
            if (!activeStreamers.Contains(streamer))
            {
                activeStreamers.Add(streamer);
            }
        }

        public static void Unregister(UdpCameraStream streamer)
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
