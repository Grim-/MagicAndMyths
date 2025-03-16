using System;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Dialog_AreaCaptured : Window
    {
        private string xmlContent;
        private Vector2 scrollPosition;
        private bool copySuccessful = false;
        private float copyMessageTimer = 0f;

        public Dialog_AreaCaptured(string xml)
        {
            xmlContent = xml;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(600f, 600f);

        public override void DoWindowContents(Rect inRect)
        {
            // Title
            Rect titleRect = inRect.TopPartPixels(40f);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "Captured Area Layout");
            Text.Font = GameFont.Small;

            // Copy button
            Rect buttonRect = new Rect(inRect.width - 150f, inRect.height - 40f, 130f, 35f);
            if (Widgets.ButtonText(buttonRect, "Copy to Clipboard"))
            {
                GUIUtility.systemCopyBuffer = xmlContent;
                copySuccessful = true;
                copyMessageTimer = 3f; // Show message for 3 seconds
            }

            // Display copy success message
            if (copySuccessful && copyMessageTimer > 0)
            {
                Rect messageRect = new Rect(10f, inRect.height - 40f, 200f, 35f);
                GUI.color = Color.green;
                Widgets.Label(messageRect, "Copied to clipboard!");
                GUI.color = Color.white;
            }

            // XML content in scrollable area
            Rect contentRect = new Rect(inRect.x, titleRect.yMax + 10f, inRect.width, inRect.height - titleRect.height - 60f);
            Widgets.BeginScrollView(contentRect, ref scrollPosition, new Rect(0, 0, contentRect.width - 16f, Math.Max(Text.CalcHeight(xmlContent, contentRect.width - 20f), 500f)));

            // Draw the XML
            Widgets.TextArea(new Rect(0, 0, contentRect.width - 20f, Math.Max(Text.CalcHeight(xmlContent, contentRect.width - 20f), 500f)), xmlContent, true);

            Widgets.EndScrollView();
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();
            if (copyMessageTimer > 0)
            {
                copyMessageTimer -= Time.deltaTime;
            }
        }
    }
}
