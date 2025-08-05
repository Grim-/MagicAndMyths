using LudeonTK;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace MagicAndMyths
{
    public static class MagicDebugActions
    {
        [DebugAction("Magic And Myths", "Add Thing Property", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void AddThingProperty()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetPawns = true,
                canTargetAnimals = true,
                canTargetBuildings = true,
                canTargetCorpses = true,
                canTargetHumans = true,
                canTargetItems = true,
                mustBeSelectable = true,
            },
            (LocalTargetInfo target) =>
            {
                if (target.Thing != null && target.Thing is ThingWithComps withComps)
                {
                    List<FloatMenuOption> Options = new List<FloatMenuOption>();

                    foreach (var item in DefDatabase<ThingPropertyDef>.AllDefs)
                    {
                        Options.Add(new FloatMenuOption($"Add {item.label} Property to {target.Thing.Label}", () =>
                        {
                            if (withComps.TryGetComp(out Comp_ThingProperties _ThingProperties))
                            {
                                Log.Message("Adding prop to thing");
                                _ThingProperties.AddProperty(item);
                            }
                            else
                            {
                                Log.Message("thing has no comp_thingproperties");
                            }
                        }));
                    }

                    Find.WindowStack.Add(new FloatMenu(Options));
                }
            }
            );
        }

        [DebugAction("Magic And Myths", "Add Enchant To Item", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void AddEnchant()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetPawns = false,
                canTargetAnimals = false,
                canTargetBuildings = false,
                canTargetCorpses = false,
                canTargetHumans = false,
                canTargetItems = true,
                mustBeSelectable = true,
                mapObjectTargetsMustBeAutoAttackable = false
            },
            (LocalTargetInfo target) =>
            {
                if (target.Thing != null && target.Thing is ThingWithComps withComps)
                {

                    if (withComps.TryGetComp(out Comp_EnchantProvider _EnchantProvider))
                    {
                        List<FloatMenuOption> Options = new List<FloatMenuOption>();

                        foreach (var item in DefDatabase<EnchantDef>.AllDefs)
                        {
                            Options.Add(new FloatMenuOption($"Add {item.label} to {target.Thing.Label}", () =>
                            {
                                _EnchantProvider.AddEnchant(item);
                            }));
                        }

                        if (Options.Count > 0)
                        {
                            Find.WindowStack.Add(new FloatMenu(Options));
                        }
                    }
                }
            }
            );
        }

        [DebugAction("Magic And Myths", "Test Spawn Orbital Laser", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnOrbitalLaser()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetLocations = true
            },
            (LocalTargetInfo target) =>
            {
                if (target.Cell.IsValid && target.Cell.InBounds(Find.CurrentMap))
                {
                    OrbitalLaser meteor = (OrbitalLaser)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("MagicAndMyths_OrbitalLaser"));
                    GenSpawn.Spawn(meteor, target.Cell, Find.CurrentMap);

                    meteor.Fire(target.Cell);
                }
            }
            );
        }

        [DebugAction("Magic And Myths", "DebugPawnShaderProperties", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void DebugPawnShaderProperties()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetPawns = true
            },
            (LocalTargetInfo target) =>
            {
                if (target.Pawn?.Graphic?.MatSouth == null) return;

                var material = target.Pawn.Drawer.renderer.BodyGraphic.MatAt(target.Pawn.Rotation);
                var shader = material.shader;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"=== Shader Properties for '{target.Pawn.LabelShort}' ===");
                sb.AppendLine($"Shader: {shader.name}");
                sb.AppendLine($"Total Properties: {shader.GetPropertyCount()}");
                sb.AppendLine();

                for (int i = 0; i < shader.GetPropertyCount(); i++)
                {
                    string propName = shader.GetPropertyName(i);
                    ShaderPropertyType propType = shader.GetPropertyType(i);
                    string propDesc = shader.GetPropertyDescription(i);

                    sb.AppendLine($"[{i}] {propName}");
                    sb.AppendLine($"    Type: {propType}");
                    if (!string.IsNullOrEmpty(propDesc))
                        sb.AppendLine($"    Description: {propDesc}");

                    sb.Append("    Value: ");
                    switch (propType)
                    {
                        case ShaderPropertyType.Color:
                            sb.AppendLine(material.GetColor(propName).ToString());
                            break;
                        case ShaderPropertyType.Vector:
                            sb.AppendLine(material.GetVector(propName).ToString());
                            break;
                        case ShaderPropertyType.Float:
                        case ShaderPropertyType.Range:
                            sb.AppendLine(material.GetFloat(propName).ToString("F3"));
                            break;
                        case ShaderPropertyType.Texture:
                            var tex = material.GetTexture(propName);
                            sb.AppendLine(tex != null ? $"{tex.name} ({tex.width}x{tex.height})" : "null");
                            break;
                        default:
                            sb.AppendLine($"Unknown type: {propType}");
                            break;
                    }

                    if (propType == ShaderPropertyType.Range)
                    {
                        Vector2 range = shader.GetPropertyRangeLimits(i);
                        sb.AppendLine($"    Range: [{range.x}, {range.y}]");
                    }

                    if (propType == ShaderPropertyType.Texture)
                    {
                        Vector2 offset = material.GetTextureOffset(propName);
                        Vector2 scale = material.GetTextureScale(propName);
                        sb.AppendLine($"    Offset: {offset}, Scale: {scale}");
                    }

                    sb.AppendLine();
                }

                Log.Message(sb.ToString());
            });
        }

        [DebugAction("Magic And Myths", "Test Transmute Lightning", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void FireTransmutationLightning()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetLocations = true
            },
            (LocalTargetInfo target) =>
            {
                if (target.Cell.IsValid && target.Cell.InBounds(Find.CurrentMap))
                {
                    LightningStrike.GenerateLightningStrike(Find.CurrentMap, target.Cell, 5, out IEnumerable<IntVec3> affectedCells);
                    TerrainDef goldTile = DefDatabase<TerrainDef>.AllDefs.RandomElement();


                    List<ThingDef> naturalRockDefs = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.building.isNaturalRock).ToList();

                    foreach (var item in affectedCells)
                    {
                        Find.CurrentMap.terrainGrid.SetTerrain(item, goldTile);



                        foreach (var thing in item.GetThingList(Find.CurrentMap))
                        {
                            if (thing.def.building != null && thing.def.building.isNaturalRock)
                            {
                                IntVec3 position = thing.Position;

                                thing.Destroy();

                                Thing replacementRock = ThingMaker.MakeThing(naturalRockDefs.RandomElement());
                                GenSpawn.Spawn(replacementRock, position, Find.CurrentMap);


                            }

                        }
                    }

                }
            }
            );


        }

        [DebugAction("Magic And Myth", "Test Spawn Meteor", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnMeteor()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetLocations = true
            },
            (LocalTargetInfo target) =>
            {
                if (target.Cell.IsValid && target.Cell.InBounds(Find.CurrentMap))
                {
                    Meteor meteor = (Meteor)ThingMaker.MakeThing(MagicAndMythDefOf.MagicAndMyths_Meteor);
                    GenSpawn.Spawn(meteor, target.Cell, Find.CurrentMap);

                    meteor.Launch(target.Cell);
                }
            }
            );
        }

        [DebugAction("Magic And Myths", "Spawn in grid", false, false, false, false, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
        private static List<DebugActionNode> SetTerrainRect()
        {
            List<DebugActionNode> list = new List<DebugActionNode>();
            foreach (ThingDef localDef2 in DefDatabase<ThingDef>.AllDefs)
            {
                ThingDef localDef = localDef2;
                if (localDef2.BuildableByPlayer)
                {
                    list.Add(new DebugActionNode(localDef.defName, DebugActionType.Action, () =>
                    {
                        ThingDef defName = localDef;

                        DebugToolsGeneral.GenericRectTool(defName.defName, (CellRect cellRect) =>
                        {
                            IntVec2 sizePerCell = defName.Size;
                            int stepX = sizePerCell.x + 1;
                            int stepZ = sizePerCell.z + 1;

                            for (int x = cellRect.minX; x + sizePerCell.x <= cellRect.maxX + 1; x += stepX)
                            {
                                for (int z = cellRect.minZ; z + sizePerCell.z <= cellRect.maxZ + 1; z += stepZ)
                                {
                                    IntVec3 spawnPos = new IntVec3(x, 0, z);
                                    if (cellRect.Contains(spawnPos))
                                    {
                                        Thing thing = ThingMaker.MakeThing(defName, defName.MadeFromStuff ? ThingDefOf.Steel : null);
                                        thing.SetFaction(Faction.OfPlayer);
                                        GenSpawn.Spawn(thing, spawnPos, Find.CurrentMap);
                                    }
                                }
                            }
                        });
                    }));
                }
            }
            return list;
        }
    }
}
