using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    // Helper class for drawing color fields
    public static class ColorWidgets
    {
        public static Color ColorField(Rect rect, Color color)
        {
            Rect colorPreviewRect = new Rect(rect.x, rect.y, 30f, rect.height);
            Rect rRect = new Rect(rect.x + 35f, rect.y, (rect.width - 35f) / 4 - 5f, rect.height);
            Rect gRect = new Rect(rect.x + 35f + (rect.width - 35f) / 4, rect.y, (rect.width - 35f) / 4 - 5f, rect.height);
            Rect bRect = new Rect(rect.x + 35f + 2 * ((rect.width - 35f) / 4), rect.y, (rect.width - 35f) / 4 - 5f, rect.height);
            Rect aRect = new Rect(rect.x + 35f + 3 * ((rect.width - 35f) / 4), rect.y, (rect.width - 35f) / 4 - 5f, rect.height);

            // Color preview
            Widgets.DrawBoxSolid(colorPreviewRect, color);

            // RGB sliders
            string rBuffer = Mathf.RoundToInt(color.r * 255f).ToString();
            float r = color.r;
            Widgets.TextFieldNumeric(rRect, ref r, ref rBuffer, 0f, 1f);

            string gBuffer = Mathf.RoundToInt(color.g * 255f).ToString();
            float g = color.g;
            Widgets.TextFieldNumeric(gRect, ref g, ref gBuffer, 0f, 1f);

            string bBuffer = Mathf.RoundToInt(color.b * 255f).ToString();
            float b = color.b;
            Widgets.TextFieldNumeric(bRect, ref b, ref bBuffer, 0f, 1f);

            string aBuffer = Mathf.RoundToInt(color.a * 255f).ToString();
            float a = color.a;
            Widgets.TextFieldNumeric(aRect, ref a, ref aBuffer, 0f, 1f);

            return new Color(r, g, b, a);
        }
    }
}
