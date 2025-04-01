using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class CompProperties_PortalGenerator : CompProperties
    {
        public MapGeneratorDef mapGeneratorDef;
        public IntVec3 mapSize = new IntVec3(75, 1, 75); // Default map size
        public bool oneTimeUse = false;
        public int cooldownTicks = -1; // -1 for no cooldown
        public List<ThingDef> requiredFuel = null; // Optional fuel requirements
        public int fuelAmountRequired = 0;

        public CompProperties_PortalGenerator()
        {
            this.compClass = typeof(CompPortalGenerator);
        }
    }

    public class CompPortalGenerator : ThingComp, IPortalProvider
    {
        private bool isPortalOpen = false;
        private Map linkedMap = null;
        private int lastUsedTick = -1;
        private Effecter portalEffect = null;
        private int uniqueMapId = -1;

        private WorldCustomSiteManager _siteManager;
        private WorldCustomSiteManager SiteManager
        {
            get
            {
                if (_siteManager == null)
                {
                    _siteManager = Find.World.GetComponent<WorldCustomSiteManager>();
                }
                return _siteManager;
            }
        }

        public CompProperties_PortalGenerator Props => (CompProperties_PortalGenerator)props;
        private bool CooldownActive => Props.cooldownTicks > 0 &&
                             lastUsedTick > 0 &&
                             (Find.TickManager.TicksGame - lastUsedTick) < Props.cooldownTicks;

        public bool IsPortalActive => isPortalOpen;
        public Map LinkedMap => linkedMap;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (uniqueMapId == -1)
            {
                // Generate a unique ID for this portal that will persist
                uniqueMapId = Find.TickManager.TicksGame + this.parent.thingIDNumber;
            }
        }

        public void OpenPortal()
        {
            if (isPortalOpen)
                return;

            if (CooldownActive)
            {
                Messages.Message("Cannot open portal: Cooldown active", MessageTypeDefOf.RejectInput);
                return;
            }

            if (Props.mapGeneratorDef == null)
            {
                return;
            }

            if (Props.requiredFuel != null && !HasSufficientFuel())
            {
                Messages.Message("Cannot open portal: Insufficient fuel", MessageTypeDefOf.RejectInput);
                return;
            }

            // Get or create map based on the MapGeneratorDef
            linkedMap = GetOrCreatePortalMap();

            if (linkedMap == null)
            {
                Messages.Message("Failed to create destination map", MessageTypeDefOf.RejectInput);
                return;
            }

            isPortalOpen = true;
            lastUsedTick = Find.TickManager.TicksGame;

            // Consume fuel if required
            if (Props.requiredFuel != null && Props.fuelAmountRequired > 0)
            {
                ConsumeFuel();
            }

            Messages.Message("Portal opened successfully", MessageTypeDefOf.PositiveEvent);
        }

        public void ClosePortal()
        {
            if (!isPortalOpen)
                return;

            // Clean up portal effect
            if (portalEffect != null)
            {
                portalEffect.Cleanup();
                portalEffect = null;
            }

            isPortalOpen = false;
            linkedMap = null;

            if (SiteManager != null && SiteManager.HasComponentPortalData(uniqueMapId))
            {
                SiteManager.RemoveComponentPortalData(uniqueMapId);
            }

            Messages.Message("Portal closed", MessageTypeDefOf.NeutralEvent);
        }

        private Map GetOrCreatePortalMap()
        {
            // Use the site manager to create or get the map
            return SiteManager.GetOrCreateComponentPortalMap(
                uniqueMapId,
                Props.mapGeneratorDef,
                Props.mapSize,
                this.parent.Map.Tile
            );
        }

        public bool TeleportPawn(Pawn pawn)
        {
            if (!isPortalOpen || linkedMap == null)
                return false;

            IntVec3 spawnLoc = PortalUtils.FindTeleportLocation(pawn, linkedMap);
            if (!spawnLoc.IsValid)
                return false;

            pawn.DeSpawn(DestroyMode.Vanish);
            GenSpawn.Spawn(pawn, spawnLoc, linkedMap);

            // Create a return portal if needed
            TrySpawnReturnPortal(spawnLoc, linkedMap);

            return true;
        }





        private void TrySpawnReturnPortal(IntVec3 spawnLoc, Map map)
        {

            if (MagicAndMythDefOf.MagicAndMythsReturnRune != null)
            {
                Building_ReturnPortal returnPortal = (Building_ReturnPortal)ThingMaker.MakeThing(MagicAndMythDefOf.MagicAndMythsReturnRune);
                GenSpawn.Spawn(returnPortal, spawnLoc, map);
                returnPortal.SetHomeMap(this.parent.Map);
                returnPortal.SetPortalThingOrigin(this.parent);
            }
        }

        private bool HasSufficientFuel()
        {
            if (Props.requiredFuel == null || Props.fuelAmountRequired <= 0)
                return true;

            int totalAvailable = 0;
            foreach (ThingDef fuelDef in Props.requiredFuel)
            {
                totalAvailable += this.parent.Map.listerThings.ThingsOfDef(fuelDef)
                    .Where(t => t.Position.InHorDistOf(this.parent.Position, 5f))
                    .Sum(t => t.stackCount);
            }

            return totalAvailable >= Props.fuelAmountRequired;
        }

        private void ConsumeFuel()
        {
            if (Props.requiredFuel == null || Props.fuelAmountRequired <= 0)
                return;

            int remaining = Props.fuelAmountRequired;

            foreach (ThingDef fuelDef in Props.requiredFuel)
            {
                List<Thing> availableFuel = this.parent.Map.listerThings.ThingsOfDef(fuelDef)
                    .Where(t => t.Position.InHorDistOf(this.parent.Position, 5f))
                    .ToList();

                foreach (Thing fuel in availableFuel)
                {
                    int toConsume = Mathf.Min(remaining, fuel.stackCount);
                    fuel.SplitOff(toConsume).Destroy();
                    remaining -= toConsume;

                    if (remaining <= 0)
                        break;
                }

                if (remaining <= 0)
                    break;
            }
        }



        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Open portal command
            if (!isPortalOpen)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Open Portal",
                    defaultDesc = "Open a portal to the destination map.",
                    icon = TexButton.Play,
                    action = OpenPortal,
                    Disabled = CooldownActive,
                    disabledReason = CooldownActive ? "Portal on cooldown" : null
                };
            }
            else
            {
                yield return new Command_Action
                {
                    defaultLabel = "Close Portal",
                    defaultDesc = "Close the active portal.",
                    icon = TexCommand.ForbidOn,
                    action = ClosePortal
                };
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (IsPortalActive)
            {
                yield return new FloatMenuOption("Enter Portal", () =>
                {
                    Job job = JobMaker.MakeJob(MagicAndMythDefOf.Portals_UsePortalJob, this.parent);
                    selPawn.jobs.StartJob(job, JobCondition.InterruptForced);
                });
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            if (portalEffect != null && isPortalOpen)
            {
                portalEffect.EffectTick(this.parent, this.parent);
            }
        }

        public override void PostDeSpawn(Map map)
        {
            if (isPortalOpen)
            {
                ClosePortal();
            }

            base.PostDeSpawn(map);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (isPortalOpen)
            {
                ClosePortal();
            }

            // Clean up the map data if this portal is destroyed
            if (SiteManager != null && SiteManager.HasComponentPortalData(uniqueMapId))
            {
                SiteManager.RemoveComponentPortalData(uniqueMapId);
            }

            base.PostDestroy(mode, previousMap);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref isPortalOpen, "isPortalOpen", false);
            Scribe_References.Look(ref linkedMap, "linkedMap");
            Scribe_Values.Look(ref lastUsedTick, "lastUsedTick", -1);
            Scribe_Values.Look(ref uniqueMapId, "uniqueMapId", -1);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (isPortalOpen && uniqueMapId != -1)
                {
                    // Re-establish portal connection on load
                    linkedMap = GetOrCreatePortalMap();

                    // Re-establish portal effect on load
                    if (MagicAndMythDefOf.Portal_Effect != null)
                    {
                        portalEffect = MagicAndMythDefOf.Portal_Effect.Spawn(this.parent.Position, this.parent.Map, 1.5f);
                    }
                }
            }
        }
    }

    //public class GameCondition_UnderWaterCondition : GameCondition
    //{
    //    private Color SkyColor = new Color(1f, 1f, 1f);
    //    private Color SkyColorNight = Color.white;
    //    private Color ShadowColor = new Color(0.4f, 0, 0, 0.2f);
    //    private Color OverlayColor = new Color(0.5f, 0.5f, 0.5f);
    //    private float Saturation = 0.75f;
    //    private float Glow = 1;

    //    public override int TransitionTicks => 120;
    //    public override void Init()
    //    {
    //        base.Init();

    //        UnderWaterGameConditionDef def = (UnderWaterGameConditionDef)this.def;

    //        this.SkyColor = def.SkyColor;
    //        this.SkyColorNight = def.SkyColorNight;
    //        this.ShadowColor = def.ShadowColor;
    //        this.OverlayColor = def.OverlayColor;
    //        this.Saturation = def.SkyColorSaturation;
    //        this.Glow = def.OverallGlowIntensityMultiplier;
    //    }
    //    public override void GameConditionTick()
    //    {
    //        base.GameConditionTick();
    //        List<Map> affectedMaps = base.AffectedMaps;
    //        foreach (var map in affectedMaps)
    //        {
    //            foreach (var item in SkyOverlays(map))
    //            {
    //                item.TickOverlay(map);
    //            }
    //        }
    //    }

    //    public override void GameConditionDraw(Map map)
    //    {
    //        base.GameConditionDraw(map);

    //        if (map == null)
    //        {
    //            return;
    //        }

    //        foreach (var item in this.SkyOverlays(map))
    //        {
    //            item.DrawOverlay(map);

    //            if (item is CausticsOverlay causticsOverlay)
    //            {
    //                causticsOverlay.UpdateZoom();
    //                causticsOverlay.UpdateMaterial();
    //            }
    //        }
    //    }

    //    public override List<SkyOverlay> SkyOverlays(Map map)
    //    {
    //        return new List<SkyOverlay>() { new CausticsOverlay() };
    //    }

    //    public override float SkyTargetLerpFactor(Map map)
    //    {
    //        return GameConditionUtility.LerpInOutValue(this, TransitionTicks);
    //    }

    //    public SkyColorSet TestSkyColors
    //    {
    //        get
    //        {
    //            float dayPercent = GenCelestial.CurCelestialSunGlow(Find.CurrentMap);
    //            Color lerpedColor = Color.Lerp(SkyColorNight, SkyColor, dayPercent);
    //            return new SkyColorSet(lerpedColor, ShadowColor, OverlayColor, Saturation);
    //        }
    //    }

    //    public override SkyTarget? SkyTarget(Map map)
    //    {
    //        return new SkyTarget(Glow, TestSkyColors, 1f, 1f);
    //    }
    //}
    //public class CausticsOverlay : SkyOverlay
    //{
    //    public Shader Shader;
    //    public Texture2D MainTex;
    //    public Texture2D SecondTex;
    //    public Texture2D DistortTex;
    //    public Material Material;

    //    public const string CausticShaderAssetName = "causticsshader";

    //    public CausticsOverlay()
    //    {
    //        //this.MainTex = ContentFinder<Texture2D>.Get("Layer1");
    //        //this.SecondTex = ContentFinder<Texture2D>.Get("Layer2");
    //        //this.DistortTex = ContentFinder<Texture2D>.Get("DistortionNoise");
    //        //this.Shader = LoadedModManager.GetMod<BiomesUnderwater>().GetShaderFromAssets(CausticShaderAssetName);

    //        //if (this.Shader == null)
    //        //{
    //        //    Log.Error($"Could not find shader {CausticShaderAssetName} in assets.");
    //        //    return;
    //        //}

    //        //this.Material = new Material(this.Shader);
    //        //this.Material.SetTexture("_MainTex", this.MainTex);
    //        //this.Material.SetTexture("_LayerTwo", this.SecondTex);
    //        //this.Material.SetTexture("_DistortMap", this.DistortTex);


    //        //this.Material.SetFloat("_Opacity", 0.14f);
    //        //this.Material.SetFloat("_ScrollSpeed", 0.3f);

    //        //this.Material.SetFloat("_DistortionSpeed", 0.04f);
    //        //this.Material.SetFloat("_DistortionStrR", 0.06f);
    //        //this.Material.SetFloat("_DistortionStrG", 0.06f);


    //        //this.Material.SetColor("_Color", new Color(1, 1, 1));
    //        //this.Material.SetColor("_Color2", new Color(1, 1, 1));


    //        //this.worldOverlayMat = this.Material;
    //    }



    //    public void UpdateMaterial()
    //    {

    //    }
    //}
}