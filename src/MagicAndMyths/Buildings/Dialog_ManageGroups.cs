using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Dialog_ManageGroups : Window
    {
        private Building_FireControlCenter controlCenter;

        public Dialog_ManageGroups(Building_FireControlCenter center)
        {
            controlCenter = center;
            forcePause = true;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(400f, 500f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 40f), "Manage Battery Groups");
            Text.Font = GameFont.Small;

            var groups = controlCenter.GetAllGroups();
            var listRect = new Rect(0f, 50f, inRect.width, inRect.height - 100f);

            float y = 0f;
            foreach (var group in groups)
            {
                var groupRect = new Rect(0f, y, inRect.width, 60f);
                Widgets.DrawBoxSolid(groupRect, Color.grey);

                Widgets.Label(new Rect(10f, y + 5f, 200f, 25f), $"{group.Name} ({group.Turrets.Count} turrets)");

                if (Widgets.ButtonText(new Rect(inRect.width - 120f, y + 5f, 50f, 25f), "Rename"))
                {
                    Find.WindowStack.Add(new Dialog_RenameGroup(controlCenter, group.GroupId));
                }

                if (Widgets.ButtonText(new Rect(inRect.width - 60f, y + 5f, 50f, 25f), "Delete"))
                {
                    controlCenter.DeleteGroup(group.GroupId);
                }

                y += 70f;
            }
        }
    }
}
