using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Dialog_RenameGroup : Window
    {
        private Building_FireControlCenter controlCenter;
        private int groupId;
        private string groupName;

        public Dialog_RenameGroup(Building_FireControlCenter center, int id)
        {
            controlCenter = center;
            groupId = id;
            var group = center.GetAllGroups().FirstOrDefault(g => g.GroupId == id);
            groupName = group?.Name ?? "";

            forcePause = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(300f, 150f);

        public override void DoWindowContents(Rect inRect)
        {
            Widgets.Label(new Rect(0f, 0f, inRect.width, 30f), "Enter new group name:");
            groupName = Widgets.TextField(new Rect(0f, 40f, inRect.width, 30f), groupName);

            if (Widgets.ButtonText(new Rect(0f, 80f, 100f, 30f), "OK"))
            {
                controlCenter.RenameGroup(groupId, groupName);
                Close();
            }

            if (Widgets.ButtonText(new Rect(110f, 80f, 100f, 30f), "Cancel"))
            {
                Close();
            }
        }
    }
}
