using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using TaranMagicFramework;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public static class AbilityRadialPager
    {
        private static Texture2D RadialIcon => TexCommand.Draft;
        private static Texture2D FavoritesIcon => TexButton.Plus;
        private static AbilityRadialPagerSettings Settings => MagicAndMythsMod.Settings;

        static AbilityRadialPager()
        {
            var harmony = new Harmony("test.mod.abilityradialpager");
            harmony.Patch(
                original: AccessTools.Method(typeof(GizmoGridDrawer), "DrawGizmoGrid"),
                prefix: new HarmonyMethod(typeof(AbilityRadialPager), nameof(GizmoGridPatchPrefix))
            );
        }

        public static bool GizmoGridPatchPrefix(ref IEnumerable<Gizmo> gizmos, float startX, out Gizmo mouseoverGizmo,
            Func<Gizmo, bool> customActivatorFunc, Func<Gizmo, bool> highlightFunc, Func<Gizmo, bool> lowlightFunc, bool multipleSelected)
        {
            mouseoverGizmo = null;
            if (Event.current.type == EventType.Layout || !(Find.Selector.SingleSelectedObject is Pawn) || Find.Selector.SelectedPawns.Count > 1 || Settings != null && !Settings.IsEnabled)
                return true;

            Pawn selectedPawn = Find.Selector.SelectedPawns[0];
            if (selectedPawn == null)
            {
                return true;
            }

            var gizmoList = gizmos.ToList();
            var abilityGizmos = gizmoList.Where(x => IsAbilityGizmo(x)).Cast<Command>().ToList();
            var nonAbilityGizmos = gizmoList.Where(g => !IsAbilityGizmo(g)).ToList();

            if (abilityGizmos.Any())
            {
                var radialGizmo = CreateRadialMenuGizmo(selectedPawn, abilityGizmos);
                nonAbilityGizmos.Add(radialGizmo);
            }

            if (Current.Game.GetComponent<GameComp_RadialFavouritesTracker>().HasAnyFavourites(selectedPawn))
            {
                var favoritesGizmo = CreateFavouriteRadialMenuGizmo(selectedPawn, abilityGizmos);
                nonAbilityGizmos.Add(favoritesGizmo);
            }

            gizmos = nonAbilityGizmos;
            return true;
        }

        private static bool IsAbilityGizmo(Gizmo gizmo)
        {
            return gizmo is Command_Ability || gizmo is CommandAbility;
        }

        private static Command_Action CreateRadialMenuGizmo(Pawn pawn, List<Command> abilityGizmos)
        {
            return new Command_Action
            {
                defaultLabel = "Abilities",
                defaultDesc = "Open radial ability menu",
                icon = RadialIcon,
                hotKey = KeyBindingDefOf.Misc1,
                action = () => OpenRadialMenu(pawn, abilityGizmos),
                Order = -100
            };
        }

        private static Command_Action CreateFavouriteRadialMenuGizmo(Pawn pawn, List<Command> abilityGizmos)
        {
            return new Command_Action
            {
                defaultLabel = "Favourites",
                defaultDesc = "Open favorite abilities",
                icon = FavoritesIcon,
                hotKey = KeyBindingDefOf.Misc2,
                action = () => OpenFavoritesMenu(pawn, abilityGizmos),
                Order = -99
            };
        }

        private static void OpenRadialMenu(Pawn pawn, List<Command> abilityGizmos)
        {
            if (abilityGizmos.Any())
            {
                RadialMenuWindow.ShowFromGizmos(pawn, abilityGizmos, false);
            }
        }

        private static void OpenFavoritesMenu(Pawn pawn, List<Command> abilityGizmos)
        {
            var favoritesTracker = Current.Game.GetComponent<GameComp_RadialFavouritesTracker>();
            var favoriteDefNames = favoritesTracker.PawnAbilityFavourites.ContainsKey(pawn)
                ? favoritesTracker.PawnAbilityFavourites[pawn]
                : new List<string>();

            List<RadialMenuItem> favoriteItems = new List<RadialMenuItem>();

            foreach (var gizmo in abilityGizmos)
            {
                string defName = GetAbilityDefName(gizmo);
                if (!string.IsNullOrEmpty(defName) && favoriteDefNames.Contains(defName))
                {
                    RadialMenuItem favoriteItem = new RadialMenuItem(
                        pawn,
                        GetGizmoLabel(gizmo),
                        GetAbilityDescription(gizmo),
                        gizmo.icon as Texture2D,
                        () => ExecuteAbilityGizmo(gizmo))
                    {
                        sourceGizmo = gizmo,
                        defName = defName
                    };

                    favoriteItems.Add(favoriteItem);
                }
            }

            if (favoriteItems.Any())
            {
                RadialMenuWindow.Show(favoriteItems, true);
            }
            else
            {
                Messages.Message("No favorite abilities found.", MessageTypeDefOf.RejectInput);
            }
        }

        private static string GetGizmoLabel(Command gizmo)
        {
            return gizmo.Label;
        }

        private static string GetAbilityDescription(Command command)
        {
            if (command is CommandAbility commandAbility)
            {
                return commandAbility.Ability.def.description;
            }
            else if (command is Command_Ability commandAbi)
            {
                if (commandAbi.Ability != null && commandAbi.Ability.def != null)
                {
                    return commandAbi.Ability.def.description;
                }
            }
            return "";
        }

        private static void ExecuteAbilityGizmo(Command abilityGizmo)
        {
            if (!abilityGizmo.Disabled)
            {
                abilityGizmo.ProcessInput(Event.current);
            }
            else
            {
                Log.Message(abilityGizmo.disabledReason);
            }
        }

        private static string GetAbilityDefName(Command command)
        {
            if (command is CommandAbility commandAbility)
            {
                return commandAbility.Ability.def.defName;
            }
            else if (command is Command_Ability commandAbi)
            {
                if (commandAbi.Ability != null && commandAbi.Ability.def != null)
                {
                    return commandAbi.Ability.def.defName;
                }
            }
            return "";
        }
    }
}