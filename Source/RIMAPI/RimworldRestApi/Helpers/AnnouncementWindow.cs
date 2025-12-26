using UnityEngine;
using Verse;

namespace RIMAPI.UI
{
    public class AnnouncementWindow : Window
    {
        private readonly string _text;
        private readonly float _duration;
        private readonly Color _color;
        private readonly float _scale;
        private readonly float _startTime;

        // FIX 1: Explicitly use 'Verse.UI' to avoid conflict with your namespace 'RIMAPI.UI'
        public override Vector2 InitialSize => new Vector2(Verse.UI.screenWidth, Verse.UI.screenHeight);

        public AnnouncementWindow(string text, float duration, string colorHex, float scale)
        {
            _text = text;
            _duration = duration;
            _scale = scale;
            _startTime = Time.realtimeSinceStartup;

            // Parse Color
            if (!ColorUtility.TryParseHtmlString(colorHex, out _color))
                _color = Color.white;

            // Window Properties
            this.layer = WindowLayer.Super;
            this.absorbInputAroundWindow = false;
            this.closeOnClickedOutside = false;
            this.doCloseButton = false;
            this.doCloseX = false;

            // FIX 2: Remove 'backgroundColor' (it doesn't exist). 
            // Setting shadowAlpha to 0 removes the dark overlay.
            // By default, a Window is transparent unless you draw a background in DoWindowContents.
            this.shadowAlpha = 0f;

            // Ensure the game doesn't pause when this text pops up
            this.forcePause = false;
            this.preventCameraMotion = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            // 1. Check Timeout
            if (Time.realtimeSinceStartup - _startTime > _duration)
            {
                this.Close();
                return;
            }

            // 2. Setup Text Style
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = _color;

            // 3. Draw Scaled Text
            Matrix4x4 oldMatrix = GUI.matrix;

            // FIX 3: Explicitly use Verse.UI again here
            Vector2 pivot = new Vector2(Verse.UI.screenWidth / 2f, Verse.UI.screenHeight / 2f);
            GUIUtility.ScaleAroundPivot(new Vector2(_scale, _scale), pivot);

            // Draw Label
            Widgets.Label(inRect, _text);

            // 4. Restore
            GUI.matrix = oldMatrix;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
    }
}