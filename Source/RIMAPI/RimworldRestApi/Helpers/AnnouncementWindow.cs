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

        public override Vector2 InitialSize => new Vector2(Verse.UI.screenWidth, Verse.UI.screenHeight);
        protected override float Margin => 0f;

        public AnnouncementWindow(string text, float duration, string colorHex, float scale)
        {
            _text = text;
            _duration = duration;
            _scale = scale;
            _startTime = Time.realtimeSinceStartup;

            if (!ColorUtility.TryParseHtmlString(colorHex, out _color))
                _color = Color.white;

            this.layer = WindowLayer.Super;
            this.closeOnClickedOutside = false;
            this.doCloseButton = false;
            this.doCloseX = false;
            this.absorbInputAroundWindow = false;
            this.shadowAlpha = 0f;
            this.forcePause = false;
            this.preventCameraMotion = false;
        }

        // --- THE FIX ---
        // By overriding this and NOT calling base.WindowOnGUI(), 
        // we skip the default background texture drawing entirely.
        public override void WindowOnGUI()
        {
            // Just call our contents method directly
            this.DoWindowContents(this.windowRect);
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (Time.realtimeSinceStartup - _startTime > _duration)
            {
                this.Close();
                return;
            }

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;

            // Save state
            Color oldColor = GUI.color;
            Matrix4x4 oldMatrix = GUI.matrix;

            // Apply style
            GUI.color = _color;
            Vector2 pivot = new Vector2(Verse.UI.screenWidth / 2f, Verse.UI.screenHeight / 2f);
            GUIUtility.ScaleAroundPivot(new Vector2(_scale, _scale), pivot);

            // Draw
            Widgets.Label(inRect, _text);

            // Restore state
            GUI.matrix = oldMatrix;
            GUI.color = oldColor;
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}