using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using TaranMagicFramework;

namespace MagicAndMyths
{
    public class RadialMenuItem
    {
        public string label;
        public string description;
        public Texture2D icon;
        public Action action;
        public List<RadialMenuItem> subItems;
        public Color color = Color.white;
        public bool enabled = true;
        public Command originalCommand;
        public string abilityId;
        public Pawn parentPawn;

        public RadialMenuItem(Pawn pawn, string label, string description = "", Texture2D icon = null, Action action = null, Command originalCommand = null)
        {
            this.parentPawn = pawn;
            this.label = label;
            this.description = description;
            this.icon = icon;
            this.action = action;
            this.originalCommand = originalCommand;
            this.subItems = new List<RadialMenuItem>();
        }

        public bool HasSubItems => subItems != null && subItems.Count > 0;
    }


    public class RadialMenuWindow : Window
    {
        private List<RadialMenuItem> allMenuItems;
        private List<RadialMenuItem> currentPageItems;
        private Stack<List<RadialMenuItem>> menuStack;
        private Stack<int> pageStack;
        private Vector2 currentWindowSize;
        private Vector2 centerPosition => new Vector2(this.windowRect.size.x / 2, this.windowRect.size.y / 2 - Settings.heightOffset);
        private bool isFavoritesMenu;

        private AbilityRadialPagerSettings Settings => MagicAndMythsMod.Settings;

        private int itemsPerPage => Settings.itemsPerPage;
        private int currentPage = 0;
        private int totalPages => Mathf.CeilToInt((float)allMenuItems.Count / itemsPerPage);

        private bool hasMultiplePages => totalPages > 1;

        private float SpacePerItem => Mathf.Lerp(Settings.maxSpacePerItem, Settings.minSpacePerItem,
            Mathf.InverseLerp(Settings.minPageCount, Settings.maxPageCount, currentPageItems.Count));


        private float baseRadius = 50f;

        private float radius => baseRadius + (currentPageItems.Count * SpacePerItem);

        private float itemSize => Mathf.Lerp(Settings.maxItemSize, Settings.minItemSize,
            Mathf.InverseLerp(Settings.minPageCount, Settings.maxPageCount, currentPageItems.Count));

        private int hoveredIndex = -1;


        protected static Texture2D BackgroundTex = ContentFinder<Texture2D>.Get("UI/RadialBG");

        public RadialMenuWindow(List<RadialMenuItem> menuItems, bool isFavoritesMenu = false)
        {
            this.allMenuItems = menuItems;
            this.isFavoritesMenu = isFavoritesMenu;
            this.menuStack = new Stack<List<RadialMenuItem>>();
            this.pageStack = new Stack<int>();
            this.currentPage = 0;
            UpdateCurrentPageItems();
            this.currentWindowSize = CalculateWindowSize();
            this.doWindowBackground = false;
            this.doCloseX = false;
            this.doCloseButton = false;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = true;
            this.forcePause = false;
            this.preventCameraMotion = false;
            this.layer = WindowLayer.Super;
            this.drawShadow = false;
        }

        public override Vector2 InitialSize => currentWindowSize;

        private void UpdateCurrentPageItems()
        {
            int startIndex = currentPage * itemsPerPage;
            int endIndex = Mathf.Min(startIndex + itemsPerPage, allMenuItems.Count);
            currentPageItems = allMenuItems.GetRange(startIndex, endIndex - startIndex);
        }

        private Vector2 CalculateWindowSize()
        {
            float menuRadius = radius;
            float labelHeight = Text.CalcHeight("Sample", 200f);
            float pageIndicatorHeight = hasMultiplePages ? 20f : 0f;
            float totalRadius = menuRadius + itemSize / 2f + labelHeight + pageIndicatorHeight + 30f;
            float size = (totalRadius * 2f) + 10f;
            return new Vector2(size, size);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Vector2 mousePos = Event.current.mousePosition;
            hoveredIndex = GetHoveredItemIndex(mousePos);


            Vector2 newSize = CalculateWindowSize();
            if (newSize != currentWindowSize)
            {
                ResizeWindow(newSize);
            }

            if (BackgroundTex != null)
            {
                float bgSize = (radius + itemSize / 2f) * 2f;
                Rect bgRect = new Rect(
                    centerPosition.x - bgSize / 2f,
                    centerPosition.y - bgSize / 2f,
                    bgSize,
                    bgSize
                );
                Widgets.DrawTextureFitted(bgRect, BackgroundTex, 1);
            }


            DrawRadialMenu(inRect);
            HandleInput();
        }

        private void DrawRadialMenu(Rect rect)
        {


            for (int i = 0; i < currentPageItems.Count; i++)
            {
                DrawMenuItem(i, currentPageItems[i]);
            }

            if (menuStack.Count > 0)
            {
                Rect backButton = new Rect(centerPosition.x - Settings.backButtonSize / 2,
                    centerPosition.y - Settings.backButtonSize / 2, Settings.backButtonSize, Settings.backButtonSize);
                GUI.color = hoveredIndex == -2 ? Color.yellow : Color.white;

                if (hoveredIndex >= 0 && hoveredIndex < currentPageItems.Count && currentPageItems[hoveredIndex].icon != null)
                {
                    GUI.DrawTexture(backButton, currentPageItems[hoveredIndex].icon);
                }
                else
                {
                    GUI.DrawTexture(backButton, TexButton.CloseXBig);
                }
                GUI.color = Color.white;
            }

            if (hasMultiplePages)
            {
                DrawPageNavigation();
            }

            if (hoveredIndex >= 0 && hoveredIndex < currentPageItems.Count)
            {
                RadialMenuItem hoveredItem = currentPageItems[hoveredIndex];
                string displayText = hoveredItem.label;
                Vector2 labelSize = Text.CalcSize(displayText);
                float yOffset = hasMultiplePages ? 40f : 20f;
                Rect hoveredItemLabel = new Rect(centerPosition.x - labelSize.x / 2f, centerPosition.y + yOffset, labelSize.x, labelSize.y);
                GUI.Label(hoveredItemLabel, displayText);
            }

            if (isFavoritesMenu)
            {
                string favText = "Favorites Menu";
                Vector2 favTextSize = Text.CalcSize(favText);
                Rect favTextRect = new Rect(centerPosition.x - favTextSize.x / 2f, centerPosition.y - radius - 30f, favTextSize.x, favTextSize.y);
                GUI.color = Color.yellow;
                GUI.Label(favTextRect, favText);
                GUI.color = Color.white;
            }
        }

        private void DrawPageNavigation()
        {
            string pageText = $"{currentPage + 1} / {totalPages}";
            Vector2 pageTextSize = Text.CalcSize(pageText);
            Rect pageTextRect = new Rect(centerPosition.x - pageTextSize.x / 2f, centerPosition.y + 20f, pageTextSize.x, pageTextSize.y);
            GUI.Label(pageTextRect, pageText);

            if (currentPage > 0)
            {
                Rect prevButton = new Rect(centerPosition.x - 60f, centerPosition.y + 18f, Settings.navButtonsSize, Settings.navButtonsSize);
                GUI.color = hoveredIndex == -3 ? Color.yellow : Color.white;
                GUI.DrawTexture(prevButton, TexUI.ArrowTexLeft);
                GUI.color = Color.white;
            }

            if (currentPage < totalPages - 1)
            {
                Rect nextButton = new Rect(centerPosition.x + 40f, centerPosition.y + 18f, Settings.navButtonsSize, Settings.navButtonsSize);
                GUI.color = hoveredIndex == -4 ? Color.yellow : Color.white;
                GUI.DrawTexture(nextButton, TexUI.ArrowTexRight);
                GUI.color = Color.white;
            }
        }

        private void ResizeWindow(Vector2 newSize)
        {
            currentWindowSize = newSize;
            Vector2 center = GetCenterScreenPosition();
            windowRect = new Rect(center.x, center.y, newSize.x, newSize.y);
        }

        private Vector2 GetCenterScreenPosition()
        {
            return new Vector2(UI.screenWidth / 2f - this.currentWindowSize.x / 2f,
                (UI.screenHeight / 2f - this.currentWindowSize.y / 2f) - Settings.heightOffset);
        }

        private void DrawMenuItem(int index, RadialMenuItem item)
        {
            float angle = (360f / currentPageItems.Count) * index - 90f;
            Vector2 itemPos = GetItemPosition(angle);

            float extraHoverSize = index == hoveredIndex ? Settings.hoverSizeIncrease : 1f;

            Rect itemRect = new Rect(itemPos.x - itemSize * extraHoverSize / 2f, itemPos.y - itemSize * extraHoverSize / 2f,
                itemSize * extraHoverSize, itemSize * extraHoverSize);

            GUI.color = item.enabled ? item.color : Color.gray;

            if (index == hoveredIndex)
            {
                GUI.color = Color.yellow;
                string tooltip = currentPageItems[hoveredIndex].description;
                TooltipHandler.TipRegion(itemRect, tooltip);
            }

            if (item.icon != null)
            {
                GUI.DrawTexture(itemRect, item.icon);
            }
            else
            {
                GUI.DrawTexture(itemRect, TexButton.Infinity);
            }

            GUI.color = Color.white;

            if (Settings.showLabels)
            {
                Vector2 labelSize = Text.CalcSize(item.label);
                Rect labelRect = new Rect(itemPos.x - labelSize.x / 2f, itemPos.y + itemSize * extraHoverSize / 2f + 5f,
                                         labelSize.x, labelSize.y);
                GUI.Label(labelRect, item.label);
            }

            if (item.HasSubItems)
            {
                Rect arrowRect = new Rect(itemPos.x + itemSize * extraHoverSize / 2f - 8f, itemPos.y - itemSize * extraHoverSize / 2f, 8f, 8f);
                GUI.DrawTexture(arrowRect, BaseContent.WhiteTex);
            }
        }

        private Vector2 GetItemPosition(float angleDegrees)
        {
            float angleRad = angleDegrees * Mathf.Deg2Rad;
            float x = centerPosition.x + Mathf.Cos(angleRad) * radius;
            float y = centerPosition.y + Mathf.Sin(angleRad) * radius;
            return new Vector2(x, y);
        }

        private int GetHoveredItemIndex(Vector2 mousePos)
        {
            if (menuStack.Count > 0)
            {
                Rect backButton = new Rect(centerPosition.x - Settings.backButtonSize / 2,
                    centerPosition.y - Settings.backButtonSize / 2, Settings.backButtonSize, Settings.backButtonSize);
                if (backButton.Contains(mousePos))
                {
                    return -2;
                }
            }

            if (hasMultiplePages)
            {
                if (currentPage > 0)
                {
                    Rect prevButton = new Rect(centerPosition.x - 60f, centerPosition.y + 18f, Settings.navButtonsSize, Settings.navButtonsSize);
                    if (prevButton.Contains(mousePos))
                    {
                        return -3;
                    }
                }

                if (currentPage < totalPages - 1)
                {
                    Rect nextButton = new Rect(centerPosition.x + 40f, centerPosition.y + 18f, Settings.navButtonsSize, Settings.navButtonsSize);
                    if (nextButton.Contains(mousePos))
                    {
                        return -4;
                    }
                }
            }

            for (int i = 0; i < currentPageItems.Count; i++)
            {
                float angle = (360f / currentPageItems.Count) * i - 90f;
                Vector2 itemPos = GetItemPosition(angle);
                Rect itemRect = new Rect(itemPos.x - itemSize / 2f, itemPos.y - itemSize / 2f, itemSize, itemSize);

                if (itemRect.Contains(mousePos))
                {
                    return i;
                }
            }

            return -1;
        }

        private void HandleInput()
        {
           // OnFavoriteInput();
            OnConfirmInput();
            OnGoBackInput();
            OnCloseInput();
        }

        //private void OnFavoriteInput()
        //{
        //    if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        //    {
        //        if (hoveredIndex >= 0 && hoveredIndex < currentPageItems.Count)
        //        {
        //            RadialMenuItem item = currentPageItems[hoveredIndex];
        //            if (!string.IsNullOrEmpty(item.abilityId))
        //            {
        //                Settings.ToggleAbilityFavorite(item.parentPawn, item.abilityId);
        //                if (isFavoritesMenu && !item.IsFavorite(item.parentPawn))
        //                {
        //                    allMenuItems.Remove(item);
        //                    UpdateCurrentPageItems();
        //                    if (currentPageItems.Count == 0 && currentPage > 0)
        //                    {
        //                        currentPage--;
        //                        UpdateCurrentPageItems();
        //                    }
        //                    if (allMenuItems.Count == 0)
        //                    {
        //                        Close();
        //                        return;
        //                    }
        //                }
        //                Event.current.Use();
        //                return;
        //            }
        //        }

        //        if (menuStack.Count > 0)
        //        {
        //            GoBack();
        //        }
        //        else
        //        {
        //            Close();
        //        }
        //        Event.current.Use();
        //    }
        //}

        private void OnGoBackInput()
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                if (hoveredIndex >= 0 && hoveredIndex < currentPageItems.Count)
                {
                    return;
                }

                if (menuStack.Count > 0)
                {
                    GoBack();
                }
                else
                {
                    Close();
                }
                Event.current.Use();
            }
        }

        private void OnConfirmInput()
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                if (hoveredIndex == -2)
                {
                    GoBack();
                    Event.current.Use();
                }
                else if (hoveredIndex == -3)
                {
                    PreviousPage();
                    Event.current.Use();
                }
                else if (hoveredIndex == -4)
                {
                    NextPage();
                    Event.current.Use();
                }
                else if (hoveredIndex >= 0 && hoveredIndex < currentPageItems.Count)
                {
                    RadialMenuItem item = currentPageItems[hoveredIndex];

                    if (item.enabled)
                    {
                        if (item.HasSubItems)
                        {
                            OpenSubmenu(item.subItems);
                        }
                        else if (item.action != null)
                        {
                            item.action();
                            Close();
                        }
                    }

                    Event.current.Use();
                }
            }
        }

        private void OnCloseInput()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                if (menuStack.Count > 0)
                {
                    GoBack();
                }
                else
                {
                    Close();
                }
                Event.current.Use();
            }
        }

        private void PreviousPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                UpdateCurrentPageItems();
                hoveredIndex = -1;
            }
        }

        private void NextPage()
        {
            if (currentPage < totalPages - 1)
            {
                currentPage++;
                UpdateCurrentPageItems();
                hoveredIndex = -1;
            }
        }

        private void OpenSubmenu(List<RadialMenuItem> subItems)
        {
            menuStack.Push(allMenuItems);
            pageStack.Push(currentPage);
            allMenuItems = subItems;
            currentPage = 0;
            UpdateCurrentPageItems();
            hoveredIndex = -1;
        }

        private void GoBack()
        {
            if (menuStack.Count > 0)
            {
                allMenuItems = menuStack.Pop();
                currentPage = pageStack.Count > 0 ? pageStack.Pop() : 0;
                UpdateCurrentPageItems();
                hoveredIndex = -1;
            }
        }

        public static void Show(List<RadialMenuItem> menuItems, bool isFavoritesMenu = false)
        {
            RadialMenuWindow window = new RadialMenuWindow(menuItems, isFavoritesMenu);
            Find.WindowStack.Add(window);
            window.windowRect.x = UI.screenWidth / 2f - window.currentWindowSize.x / 2f;
            window.windowRect.y = (UI.screenHeight / 2f - window.currentWindowSize.y / 2f) - window.Settings.heightOffset;
        }
    }
}