using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class Window_PortalUI : Window
    {
        private const float ADDRESS_BOX_SIZE = 40f;
        private const float SYMBOL_BUTTON_SIZE = 50f;
        private const float PADDING = 10f;
        private const float BUTTON_WIDTH = 150f;
        private const float BUTTON_HEIGHT = 40f;
        private const int ADDRESS_LENGTH = 3;

        private List<GateSymbolDef> currentAddress = new List<GateSymbolDef>();
        private Vector2 savedAddressesScrollPosition = Vector2.zero;
        private Building_PortalGate portalBuilding;

        public Window_PortalUI()
        {
            forcePause = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnAccept = false;
        }

        public Window_PortalUI(Building_PortalGate building)
        {
            this.portalBuilding = building;
            forcePause = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnAccept = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var controlBarHeight = BUTTON_HEIGHT;
            var symbolGridHeight = 200f;
            var margin = PADDING;

            // Control bar at top
            var controlBarRect = new Rect(margin, margin, inRect.width - (margin * 2), controlBarHeight);
            DrawControlBar(controlBarRect);

            // Symbol grid below control bar with background
            var symbolSectionRect = new Rect(margin, controlBarHeight + (margin * 2), inRect.width - (margin * 2), symbolGridHeight + (margin * 2));
            Widgets.DrawMenuSection(symbolSectionRect);
            var symbolGridRect = new Rect(symbolSectionRect.x + margin, symbolSectionRect.y + margin,
                symbolSectionRect.width - (margin * 2), symbolGridHeight);
            DrawAddressSymbols(symbolGridRect);

            // Saved addresses list taking remaining space
            var addressListRect = new Rect(
                margin,
                symbolSectionRect.yMax + margin,
                inRect.width - (margin * 2),
                inRect.height - symbolSectionRect.yMax - (margin * 2)
            );
            DrawAddressColumn(addressListRect);
        }

        protected virtual void DrawControlBar(Rect inRect)
        {
            float addressBoxesWidth = ADDRESS_LENGTH * (ADDRESS_BOX_SIZE + PADDING) - PADDING;

            // Address boxes on the left
            float addressStartX = inRect.x;
            for (int i = 0; i < ADDRESS_LENGTH; i++)
            {
                Rect boxRect = new Rect(
                    addressStartX + i * (ADDRESS_BOX_SIZE + PADDING),
                    inRect.y,
                    ADDRESS_BOX_SIZE,
                    ADDRESS_BOX_SIZE
                );

                GUI.DrawTexture(boxRect, BaseContent.WhiteTex);

                if (i < currentAddress.Count)
                {
                    Texture2D symbolTex = ContentFinder<Texture2D>.Get(currentAddress[i].texPath);
                    GUI.DrawTexture(boxRect.ContractedBy(2f), symbolTex);
                }
            }

            // Buttons to the right of the address boxes
            float buttonsStartX = addressStartX + addressBoxesWidth + PADDING;

            // Lock In Address button
            if (currentAddress != null && currentAddress.Count > 2 && Widgets.ButtonText(
                new Rect(buttonsStartX, inRect.y, BUTTON_WIDTH, BUTTON_HEIGHT),
                "Lock In Address"))
            {
                portalBuilding.AttemptDialAddress(currentAddress);
                this.Close();
            }

            // Clear Address button
            if (Widgets.ButtonText(
                new Rect(buttonsStartX + BUTTON_WIDTH + PADDING, inRect.y, BUTTON_WIDTH, BUTTON_HEIGHT),
                "Clear Address"))
            {
                currentAddress.Clear();
            }
        }

        protected virtual void DrawAddressSymbols(Rect inRect)
        {
            var availableSymbols = DefDatabase<GateSymbolDef>.AllDefsListForReading;
            float buttonsPerRow = Mathf.Floor(inRect.width / (SYMBOL_BUTTON_SIZE + PADDING));
            float startX = (inRect.width - (buttonsPerRow * (SYMBOL_BUTTON_SIZE + PADDING))) / 2;

            for (int i = 0; i < availableSymbols.Count; i++)
            {
                int row = Mathf.FloorToInt(i / buttonsPerRow);
                int col = i % (int)buttonsPerRow;

                Rect buttonRect = new Rect(
                    inRect.x + startX + col * (SYMBOL_BUTTON_SIZE + PADDING),
                    inRect.y + row * (SYMBOL_BUTTON_SIZE + PADDING),
                    SYMBOL_BUTTON_SIZE,
                    SYMBOL_BUTTON_SIZE
                );

                if (Widgets.ButtonImage(buttonRect, availableSymbols[i].Texture))
                {
                    if (currentAddress.Count < ADDRESS_LENGTH)
                    {
                        currentAddress.Add(availableSymbols[i]);
                    }
                }
            }
        }

        protected virtual void DrawAddressColumn(Rect inRect)
        {
            Widgets.DrawMenuSection(inRect);

            // Add inner padding for the content
            var innerRect = inRect.ContractedBy(PADDING);

            Rect viewRect = new Rect(0f, 0f, innerRect.width - 16f,
                portalBuilding.savedAddresses.Count * 30f);

            Widgets.BeginScrollView(innerRect, ref savedAddressesScrollPosition, viewRect);

            float curY = 0f;
            foreach (var address in portalBuilding.savedAddresses)
            {
                // Add right margin to the row
                Rect rowRect = new Rect(0f, curY, viewRect.width - PADDING, 28f);

                if (Widgets.ButtonText(rowRect, address.ToString()))
                {
                    currentAddress = new List<GateSymbolDef>(address.Symbols);
                }

                curY += 30f;
            }

            Widgets.EndScrollView();
        }
    }
}
