using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Verse;

public class TextureExportManager : MonoBehaviour
{
    public static TextureExportManager Instance;
    private Queue<ExportRequest> exportQueue = new Queue<ExportRequest>();
    private bool isProcessing = false;

    private class ExportRequest
    {
        public string Label;
        public Texture2D Texture;
        public Action<string> OnComplete;
    }

    void Awake()
    {
        Instance = this;
    }

    public void QueueExtract(string label, Texture2D texture, Action<string> callback)
    {
        exportQueue.Enqueue(new ExportRequest
        {
            Label = label,
            Texture = texture,
            OnComplete = callback
        });

        // Self-Healing: If queue has items but we aren't processing, start.
        // OR if we think we are processing but the queue was empty before this add (stuck state), restart.
        if (!isProcessing || (isProcessing && exportQueue.Count == 1))
        {
            // Ensure we don't start double coroutines if it is actually running
            if (exportQueue.Count == 1 && isProcessing)
            {
                // This is a heuristic; usually just !isProcessing is enough with the fix above.
                // But strictly speaking, just relying on the fixed Coroutine is safer.
            }

            if (!isProcessing)
            {
                StartCoroutine(ProcessQueue());
            }
        }
    }

    private IEnumerator ProcessQueue()
    {
        isProcessing = true;

        while (exportQueue.Count > 0)
        {
            ExportRequest req = exportQueue.Dequeue();

            // 1. Safety Check: Is the texture still valid? 
            // Objects can be destroyed/unloaded while waiting in the queue.
            if (req.Texture == null)
            {
                continue; // Skip this item, move to the next
            }

            // 2. Wrap the work in a try-catch block
            // This ensures the Coroutine NEVER crashes, even if conversion fails.
            try
            {
                string base64 = TextureToBase64(req.Texture);
                req.OnComplete?.Invoke(base64);
            }
            catch (Exception e)
            {
                Log.Warning($"[TextureExport] Failed to export {req.Label}: {e.Message}");
            }

            yield return null;
            Log.Message($"Texture export for {req.Label} has finished.");
        }
        isProcessing = false;
    }


    public static string TextureToBase64(Texture2D texture)
    {
        if (texture == null) return null;

        byte[] bytes = null;

        // OPTIMIZATION: Try to read directly if allowed
        // We check isReadable AND if the format is uncompressed (ARGB32/RGB24/RGBA32)
        // EncodeToPNG fails on compressed formats (like DXT1) even if isReadable is true.
        bool isSafeFormat = texture.format == TextureFormat.ARGB32 ||
                            texture.format == TextureFormat.RGB24 ||
                            texture.format == TextureFormat.RGBA32;

        if (texture.isReadable && isSafeFormat)
        {
            try
            {
                bytes = texture.EncodeToPNG();
            }
            catch
            {
                // If direct read fails for any reason, silently fall back to the Blit method below
                bytes = null;
            }
        }

        // FALLBACK: If bytes are still null (not readable, compressed, or error above), use the Blit method
        if (bytes == null)
        {
            RenderTexture tmp = RenderTexture.GetTemporary(
                texture.width, texture.height, 0,
                RenderTextureFormat.Default, RenderTextureReadWrite.Linear
            );

            RenderTexture previous = RenderTexture.active;

            try
            {
                // Blit decompresses the texture and puts it in a readable Render Texture
                Graphics.Blit(texture, tmp);
                RenderTexture.active = tmp;

                Texture2D readableTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
                readableTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                readableTexture.Apply();

                bytes = readableTexture.EncodeToPNG();
                UnityEngine.Object.Destroy(readableTexture);
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(tmp);
            }
        }

        return Convert.ToBase64String(bytes);
    }
}