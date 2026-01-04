using RIMAPI;
using UnityEngine;

/// <summary>
/// A persistent MonoBehaviour that bridges Unity's Main Thread Event Loop with the API system.
/// <para>This component lives in the "DontDestroyOnLoad" scene, allowing it to execute logic
/// every frame regardless of whether the player is in-game or at the Main Menu.</para>
/// </summary>
public class RimApiServerProcess : MonoBehaviour
{
    /// <summary>
    /// Called once per frame by the Unity Engine.
    /// <para>This method triggers the processing of server queues (HTTP requests and broadcasts)
    /// on the main thread. Using Update() instead of OnGUI() prevents input misalignment bugs.</para>
    /// </summary>
    void Update()
    {
        RIMAPI_GameComponent.ProcessServerQueues();
    }

    /// <summary>
    /// Called when the application is shutting down.
    /// <para>Triggers the safe shutdown of the API server and releases network ports.</para>
    /// </summary>
    void OnApplicationQuit()
    {
        RIMAPI_GameComponent.Shutdown();
    }
}