using UnityEngine;
using Verse;
using RIMAPI;

/// <summary>
/// The main entry point for the RIMAPI system upon game launch.
/// <para>Marked with [StaticConstructorOnStartup], this class initializes the global
/// server process immediately after mods are loaded, ensuring the API is available
/// at the Main Menu before any save game is loaded.</para>
/// </summary>
[StaticConstructorOnStartup]
public static class RimApiStartup
{
    static RimApiStartup()
    {
        InitializeApiProcess();
    }

    /// <summary>
    /// Initializes the persistent server process and auxiliary components.
    /// <para>This method performs a safety check to prevent duplicate initialization
    /// and spawns the "RimApi_ServerProcess" GameObject into the DontDestroyOnLoad scene.</para>
    /// </summary>
    private static void InitializeApiProcess()
    {
        if (GameObject.Find("RimApi_ServerProcess") == null)
        {
            Log.Message("[RIMAPI] Initializing Global API Server Process...");

            GameObject sp = new GameObject("RimApi_ServerProcess");
            sp.AddComponent<RimApiServerProcess>();
            UnityEngine.Object.DontDestroyOnLoad(sp);
        }

        if (GameObject.Find("TextureExporter") == null)
        {
            GameObject te = new GameObject("TextureExporter");
            te.AddComponent<TextureExportManager>();
            UnityEngine.Object.DontDestroyOnLoad(te);
        }

        RIMAPI_GameComponent.StartServer();
    }
}