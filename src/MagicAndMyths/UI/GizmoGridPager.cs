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
        private static Texture2D FavoritesIcon => TexButton.Info;
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
            if (Event.current.type == EventType.Layout || Find.Selector.SelectedPawns.Count > 1 || Settings != null && !Settings.IsEnabled)
                return true;

            Pawn selectedPAwn = Find.Selector.SelectedPawns[0];
            var gizmoList = gizmos.ToList();
            var abilityGizmos = gizmoList.Where(x => IsAbilityGizmo(x)).Cast<Command>().ToList();
            var nonAbilityGizmos = gizmoList.Where(g => !IsAbilityGizmo(g)).ToList();

            if (abilityGizmos.Any())
            {
                var radialGizmo = CreateRadialMenuGizmo(selectedPAwn, abilityGizmos);
                nonAbilityGizmos.Add(radialGizmo);
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
                hotKey = Settings.radialMenuHotKey ?? KeyBindingDefOf.Misc1,
                action = () => OpenRadialMenu(pawn, abilityGizmos),
                Order = -100
            };
        }

        private static void OpenRadialMenu(Pawn pawn, List<Command> abilityGizmos)
        {
            var menuItems = BuildAbilityMenuItems(pawn, abilityGizmos);
            if (menuItems.Any())
            {
                RadialMenuWindow.Show(menuItems, false);
            }
        }

        private static List<RadialMenuItem> BuildAbilityMenuItems(Pawn pawn, List<Command> abilityGizmos)
        {
            var categoryGroups = abilityGizmos
                .GroupBy(g => GetAbilityCategory(g))
                .OrderBy(group => group.Key)
                .ToList();

            var menuItems = new List<RadialMenuItem>();

            foreach (var categoryGroup in categoryGroups)
            {
                var categoryItem = new RadialMenuItem(
                    pawn,
                    categoryGroup.Key,
                    "",
                   categoryGroup.First().icon as Texture2D
                );

                foreach (var abilityGizmo in categoryGroup.OrderBy(g => GetGizmoLabel(g)))
                {
                    RadialMenuItem abilityItem = new RadialMenuItem(
                        pawn,
                        GetGizmoLabel(abilityGizmo),
                        GetAbilityDescription(abilityGizmo),
                        abilityGizmo.icon as Texture2D,
                        () => ExecuteAbilityGizmo(abilityGizmo),
                        abilityGizmo)
                    {
                        enabled = !abilityGizmo.Disabled,
                        color = abilityGizmo.Disabled ? Color.gray : Color.white
                    };

                    categoryItem.subItems.Add(abilityItem);
                }

                if (categoryItem.subItems.Count == 1)
                {
                    var singleAbility = categoryItem.subItems.First();
                    singleAbility.label = categoryGroup.Key;
                    menuItems.Add(singleAbility);
                }
                else if (categoryItem.subItems.Any())
                {
                    menuItems.Add(categoryItem);
                }
            }

            return menuItems;
        }

        private static List<RadialMenuItem> BuildFavoriteMenuItems(Pawn pawn, List<Command> favoriteAbilities)
        {
            var menuItems = new List<RadialMenuItem>();

            foreach (var abilityGizmo in favoriteAbilities.OrderBy(g => GetGizmoLabel(g)))
            {
                RadialMenuItem abilityItem = new RadialMenuItem(
                    pawn,
                    GetGizmoLabel(abilityGizmo),
                    GetAbilityDescription(abilityGizmo),
                    abilityGizmo.icon as Texture2D,
                    () => ExecuteAbilityGizmo(abilityGizmo),
                    abilityGizmo)
                {
                    enabled = !abilityGizmo.Disabled,
                    color = abilityGizmo.Disabled ? Color.gray : Color.white
                };

                menuItems.Add(abilityItem);
            }

            return menuItems;
        }

        private static string GetGizmoLabel(Command gizmo)
        {
            return gizmo.Label;
        }

        private static string GetAbilityCategory(Command command)
        {
            if (command is CommandAbility commandAbility)
            {
                return commandAbility.Ability.def.abilityTrees.First().label;
            }
            else if (command is Command_Ability commandAbi)
            {
                if (commandAbi.Ability != null && commandAbi.Ability.def != null && commandAbi.Ability.def.category != null)
                {
                    return commandAbi.Ability.def.category.defName;
                }
            }
            return "";
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
    }

}