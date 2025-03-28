using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Dialog_AreaCaptureSettings : Window
    {
        private Zone_AreaCapture zone;
        private string zoneName;
        private Vector2 scrollPosition = Vector2.zero;
        private bool captureFloors = true;
        private bool captureTerrain = true;
        private bool captureThings = true;
        private IntVec2 originSize;

        public override Vector2 InitialSize => new Vector2(500f, 400f);

        public Dialog_AreaCaptureSettings(Zone_AreaCapture zone)
        {
            this.zone = zone;
            this.zoneName = zone.label;
            this.originSize = zone.OriginSize;
            this.forcePause = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(0f, 0f, inRect.width, 42f);
            Widgets.Label(titleRect, "Area Capture Settings");
            Text.Font = GameFont.Small;

            float curY = 50f;
            Rect settingsRect = new Rect(0f, curY, inRect.width, inRect.height - curY - 50f);
            Widgets.DrawBox(settingsRect);

            Rect innerRect = settingsRect.ContractedBy(10f);
            curY = innerRect.y;

            // Zone name
            Rect nameRect = new Rect(innerRect.x, curY, innerRect.width, 30f);
            Widgets.Label(nameRect.LeftHalf(), "Zone name:");
            zoneName = Widgets.TextField(nameRect.RightHalf(), zoneName);
            curY += 40f;

            // Origin size input
            Rect originSizeRect = new Rect(innerRect.x, curY, innerRect.width, 30f);
            Widgets.Label(originSizeRect.LeftHalf(), "Origin size (X, Z):");

            Rect xRect = new Rect(originSizeRect.width * 0.5f, curY, originSizeRect.width * 0.2f, 30f);
            Rect zRect = new Rect(originSizeRect.width * 0.75f, curY, originSizeRect.width * 0.2f, 30f);

            string xBuffer = originSize.x.ToString();
            Widgets.TextFieldNumeric(xRect, ref originSize.x, ref xBuffer, 1, 50);

            string zBuffer = originSize.z.ToString();
            Widgets.TextFieldNumeric(zRect, ref originSize.z, ref zBuffer, 1, 50);

            curY += 40f;

            // Capture options
            Rect optionsRect = new Rect(innerRect.x, curY, innerRect.width, 105f);
            Widgets.CheckboxLabeled(new Rect(optionsRect.x, optionsRect.y, optionsRect.width, 30f), "Capture floors", ref captureFloors);
            Widgets.CheckboxLabeled(new Rect(optionsRect.x, optionsRect.y + 35f, optionsRect.width, 30f), "Capture terrain", ref captureTerrain);
            Widgets.CheckboxLabeled(new Rect(optionsRect.x, optionsRect.y + 70f, optionsRect.width, 30f), "Capture things (buildings, furniture, etc.)", ref captureThings);
            curY += 115f;

            // Bottom buttons
            Rect buttonsRect = new Rect(0f, inRect.height - 40f, inRect.width, 40f);

            if (Widgets.ButtonText(new Rect(buttonsRect.width - 180f, buttonsRect.y, 80f, 30f), "Cancel"))
            {
                Close();
            }

            if (Widgets.ButtonText(new Rect(buttonsRect.width - 90f, buttonsRect.y, 80f, 30f), "Accept"))
            {
                SaveSettings();
                Close();
            }
        }

        private void SaveSettings()
        {
            zone.label = zoneName;
            zone.OriginSize = originSize;
            zone.CaptureFloors = captureFloors;
            zone.CaptureTerrain = captureTerrain;
            zone.CaptureThings = captureThings;
        }
    }
}
