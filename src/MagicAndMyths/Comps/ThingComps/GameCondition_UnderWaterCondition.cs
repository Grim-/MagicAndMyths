//using RimWorld;
//using System.Collections.Generic;
//using UnityEngine;
//using Verse;

//namespace MagicAndMyths
//{
//    public class GameCondition_UnderWaterCondition : GameCondition
//    {
//        private Color SkyColor = new Color(1f, 1f, 1f);
//        private Color SkyColorNight = Color.white;
//        private Color ShadowColor = new Color(0.4f, 0, 0, 0.2f);
//        private Color OverlayColor = new Color(0.5f, 0.5f, 0.5f);
//        private float Saturation = 0.75f;
//        private float Glow = 1;

//        public override int TransitionTicks => 120;
//        public override void Init()
//        {
//            base.Init();

//            UnderWaterGameConditionDef def = (UnderWaterGameConditionDef)this.def;

//            this.SkyColor = def.SkyColor;
//            this.SkyColorNight = def.SkyColorNight;
//            this.ShadowColor = def.ShadowColor;
//            this.OverlayColor = def.OverlayColor;
//            this.Saturation = def.SkyColorSaturation;
//            this.Glow = def.OverallGlowIntensityMultiplier;
//        }
//        public override void GameConditionTick()
//        {
//            base.GameConditionTick();
//            List<Map> affectedMaps = base.AffectedMaps;
//            foreach (var map in affectedMaps)
//            {
//                foreach (var item in SkyOverlays(map))
//                {
//                    item.TickOverlay(map);
//                }
//            }
//        }

//        public override void GameConditionDraw(Map map)
//        {
//            base.GameConditionDraw(map);

//            if (map == null)
//            {
//                return;
//            }

//            foreach (var item in this.SkyOverlays(map))
//            {
//                item.DrawOverlay(map);

//                if (item is CausticsOverlay causticsOverlay)
//                {
//                    causticsOverlay.UpdateZoom();
//                    causticsOverlay.UpdateMaterial();
//                }
//            }
//        }

//        public override List<SkyOverlay> SkyOverlays(Map map)
//        {
//            return new List<SkyOverlay>() { new CausticsOverlay() };
//        }

//        public override float SkyTargetLerpFactor(Map map)
//        {
//            return GameConditionUtility.LerpInOutValue(this, TransitionTicks);
//        }

//        public SkyColorSet TestSkyColors
//        {
//            get
//            {
//                float dayPercent = GenCelestial.CurCelestialSunGlow(Find.CurrentMap);
//                Color lerpedColor = Color.Lerp(SkyColorNight, SkyColor, dayPercent);
//                return new SkyColorSet(lerpedColor, ShadowColor, OverlayColor, Saturation);
//            }
//        }

//        public override SkyTarget? SkyTarget(Map map)
//        {
//            return new SkyTarget(Glow, TestSkyColors, 1f, 1f);
//        }
//    }
//}