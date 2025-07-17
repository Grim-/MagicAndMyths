using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{


    //[StaticConstructorOnStartup]
    //public static class AbilityRadialPager
    //{
    //    private static Texture2D RadialIcon => TexCommand.Draft;
    //    private static Texture2D FavoritesIcon => TexButton.Plus;
    //    private static AbilityRadialPagerSettings Settings => MagicAndMythsMod.Settings;

    //    static AbilityRadialPager()
    //    {
    //        var harmony = new Harmony("com.emo.radialmenu");
    //        harmony.Patch(
    //            original: AccessTools.Method(typeof(GizmoGridDrawer), "DrawGizmoGrid"),
    //            prefix: new HarmonyMethod(typeof(AbilityRadialPager), nameof(GizmoGridPatchPrefix))
    //        );
    //    }


    //    public static bool GizmoGridPatchPrefix(ref IEnumerable<Gizmo> gizmos, float startX, out Gizmo mouseoverGizmo,
    //        Func<Gizmo, bool> customActivatorFunc, Func<Gizmo, bool> highlightFunc, Func<Gizmo, bool> lowlightFunc, bool multipleSelected)
    //    {
    //        mouseoverGizmo = null;
    //        if (Event.current.type == EventType.Layout || Find.CurrentMap == null || (Settings != null && !Settings.IsEnabled))
    //            return true;

    //        if (!(Find.Selector.SingleSelectedObject is Pawn) || Find.Selector.SelectedPawns.Count > 1)
    //            return true;

    //        Pawn selectedPawn = Find.Selector.SelectedPawns[0];

    //        var gizmoList = gizmos.ToList();
    //        var abilityGizmos = gizmoList.Where(x => IsAbilityGizmo(x)).Cast<Command_Ability>().ToList();
    //        var nonAbilityGizmos = gizmoList.Where(g => !IsAbilityGizmo(g)).ToList();

    //        if (abilityGizmos.Any())
    //        {
    //            var radialGizmo = CreateRadialMenuGizmo(selectedPawn, abilityGizmos);
    //            nonAbilityGizmos.Add(radialGizmo);
    //        }

    //        if (Current.Game.GetComponent<GameComp_RadialFavouritesTracker>().HasAnyFavourites(selectedPawn))
    //        {
    //            var favoritesGizmo = CreateFavouriteRadialMenuGizmo(selectedPawn, abilityGizmos);
    //            nonAbilityGizmos.Add(favoritesGizmo);
    //        }

    //        gizmos = nonAbilityGizmos;
    //        return true;
    //    }

    //    private static bool IsAbilityGizmo(Gizmo gizmo)
    //    {
    //        return gizmo is Command_Ability || (TMFAbilityHelper.IsTMFLoaded && TMFAbilityHelper.IsTMFCommand(gizmo));
    //    }

    //    private static Command_Action CreateRadialMenuGizmo(Pawn pawn, List<Command_Ability> abilityGizmos)
    //    {
    //        return new Command_Action
    //        {
    //            defaultLabel = "Abilities",
    //            defaultDesc = "Open radial ability menu",
    //            icon = RadialIcon,
    //            hotKey = KeyBindingDefOf.Misc1,
    //            action = () => OpenRadialMenu(pawn, abilityGizmos),
    //            Order = -100
    //        };
    //    }

    //    private static Command_Action CreateFavouriteRadialMenuGizmo(Pawn pawn, List<Command_Ability> abilityGizmos)
    //    {
    //        return new Command_Action
    //        {
    //            defaultLabel = "Favourites",
    //            defaultDesc = "Open favorite abilities",
    //            icon = FavoritesIcon,
    //            hotKey = KeyBindingDefOf.Misc2,
    //            action = () => OpenFavoritesMenu(pawn, abilityGizmos),
    //            Order = -99
    //        };
    //    }

    //    private static void OpenRadialMenu(Pawn pawn, List<Command_Ability> abilityGizmos)
    //    {
    //        if (abilityGizmos.Any())
    //        {
    //            RadialMenuWindow.ShowFromGizmos(pawn, abilityGizmos, false);
    //        }
    //    }

    //    private static void OpenFavoritesMenu(Pawn pawn, List<Command_Ability> abilityGizmos)
    //    {
    //        var favoritesTracker = Current.Game.GetComponent<GameComp_RadialFavouritesTracker>();
    //        var favoriteDefNames = favoritesTracker.PawnAbilityFavourites.ContainsKey(pawn)
    //            ? favoritesTracker.PawnAbilityFavourites[pawn]
    //            : new List<string>();

    //        List<RadialMenuItem> favoriteItems = new List<RadialMenuItem>();

    //        foreach (var gizmo in abilityGizmos)
    //        {
    //            string defName = GetAbilityDefName(gizmo);
    //            if (!string.IsNullOrEmpty(defName) && favoriteDefNames.Contains(defName))
    //            {
    //                RadialMenuItem favoriteItem = new RadialMenuItem(
    //                    pawn,
    //                    GetGizmoLabel(gizmo),
    //                    GetAbilityDescription(gizmo),
    //                    gizmo.icon as Texture2D,
    //                    () => ExecuteAbilityGizmo(gizmo))
    //                {
    //                    sourceGizmo = gizmo,
    //                    defName = defName
    //                };

    //                favoriteItems.Add(favoriteItem);
    //            }
    //        }

    //        if (favoriteItems.Any())
    //        {
    //            RadialMenuWindow.Show(favoriteItems, true);
    //        }
    //        else
    //        {
    //            Messages.Message("No favorite abilities found.", MessageTypeDefOf.RejectInput);
    //        }
    //    }

    //    private static string GetGizmoLabel(Command_Ability gizmo) => gizmo.Label;

    //    public static string GetAbilityDescription(Command_Ability command)
    //    {
    //        if (TMFAbilityHelper.IsTMFLoaded && TMFAbilityHelper.IsTMFCommand(command))
    //        {
    //            return TMFAbilityHelper.GetTMFDescription(command);
    //        }
    //        if (command is Command_Ability commandAbi)
    //        {
    //            return commandAbi.Ability?.def?.description ?? "";
    //        }
    //        return "";
    //    }

    //    public static void ExecuteAbilityGizmo(Command_Ability abilityGizmo)
    //    {
    //        if (!abilityGizmo.Disabled)
    //        {
    //            abilityGizmo.ProcessInput(Event.current);
    //        }
    //        else
    //        {
    //            Log.Message(abilityGizmo.disabledReason);
    //        }
    //    }

    //    public static string GetAbilityCategory(Command_Ability command)
    //    {
    //        if (TMFAbilityHelper.IsTMFLoaded && TMFAbilityHelper.IsTMFCommand(command))
    //        {
    //            return TMFAbilityHelper.GetTMFAbilityTreeLabel(command);
    //        }

    //        if (command is Command_Ability commandAbi)
    //        {
    //            return commandAbi.Ability?.def?.category?.defName ?? "Base Game";
    //        }
    //        return "Unknown";
    //    }

    //    public static string GetAbilityDefName(Command_Ability command)
    //    {
    //        if (TMFAbilityHelper.IsTMFLoaded && TMFAbilityHelper.IsTMFCommand(command))
    //        {
    //            return TMFAbilityHelper.GetTMFDefName(command);
    //        }
    //        if (command is Command_Ability commandAbi)
    //        {
    //            return commandAbi.Ability?.def?.defName ?? "";
    //        }
    //        return "";
    //    }
    //}


    //public static class TMFAbilityHelper
    //{
    //    private static readonly Type TMFCommandAbilityType;
    //    private static readonly PropertyInfo AbilityProperty;
    //    private static readonly FieldInfo DefField;
    //    private static readonly FieldInfo AbilityTreesField;

    //    static TMFAbilityHelper()
    //    {
    //        TMFCommandAbilityType = AccessTools.TypeByName("TaranMagicFramework.CommandAbility");
    //        if (TMFCommandAbilityType == null) return;

    //        AbilityProperty = AccessTools.Property(TMFCommandAbilityType, "Ability");
    //        if (AbilityProperty == null) return;

    //        Type abilityType = AbilityProperty.PropertyType;

    //        DefField = AccessTools.Field(abilityType, "def");
    //        if (DefField == null) return;

    //        Type defType = DefField.FieldType;

    //        AbilityTreesField = AccessTools.Field(defType, "abilityTrees");
    //    }

    //    public static bool IsTMFLoaded => TMFCommandAbilityType != null && AbilityProperty != null && DefField != null;

    //    public static bool IsTMFCommand(Gizmo gizmo)
    //    {
    //        return TMFCommandAbilityType.IsAssignableFrom(gizmo.GetType());
    //    }

    //    private static object GetTMFDef(Command command)
    //    {
    //        object ability = AbilityProperty.GetValue(command);
    //        if (ability == null) return null;

    //        return DefField.GetValue(ability);
    //    }

    //    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    //    public static string GetTMFDefName(Command command)
    //    {
    //        object def = GetTMFDef(command);
    //        return (def as Def)?.defName;
    //    }

    //    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    //    public static string GetTMFDescription(Command command)
    //    {
    //        object def = GetTMFDef(command);
    //        return (def as Def)?.description;
    //    }

    //    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    //    public static string GetTMFAbilityTreeLabel(Command command)
    //    {
    //        if (AbilityTreesField == null) return "TMF (No Tree)";

    //        object def = GetTMFDef(command);
    //        if (def == null) return "TMF (No Def)";

    //        var trees = AbilityTreesField.GetValue(def) as IEnumerable;
    //        if (trees == null) return "TMF (Tree Null)";

    //        var firstTree = trees.Cast<object>().FirstOrDefault();
    //        if (firstTree == null) return "TMF (Tree Empty)";

    //        var labelProp = AccessTools.Property(firstTree.GetType(), "label");
    //        return labelProp?.GetValue(firstTree) as string ?? "TMF (No Label)";
    //    }
    //}
}