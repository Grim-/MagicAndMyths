using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    /// <summary>
    /// Utility class for standardized D&D-style difficulty check calculations using XML-defined curves
    /// </summary>
    public static class DCUtility
    {
        public const int DC_TRIVIAL = 5;
        public const int DC_EASY = 10;
        public const int DC_MODERATE = 15;
        public const int DC_HARD = 20;
        public const int DC_VERY_HARD = 25;
        public const int DC_NEARLY_IMPOSSIBLE = 30;

        private static SimpleCurve skillCurve = null;
        private static SimpleCurve capacityCurve = null;
        private static SimpleCurve statCurve = null;

        public static void Initialize()
        {
            if (skillCurve == null)
            {
                skillCurve = LoadCurveFromXML("MagicAndMyths_SkillToBonus");
                if (skillCurve == null)
                {
                    skillCurve = new SimpleCurve
                    {
                        {0f, 0f},    // Level 0 = +0
                        {4f, 1f},    // Level 4 = +1
                        {8f, 2f},    // Level 8 = +2
                        {12f, 3f},   // Level 12 = +3
                        {16f, 4f},   // Level 16 = +4
                        {20f, 5f}    // Level 20 = +5
                    };
                }
            }

            if (capacityCurve == null)
            {
                capacityCurve = LoadCurveFromXML("MagicAndMyths_CapacityToBonus");
                if (capacityCurve == null)
                {
                    capacityCurve = new SimpleCurve
                    {
                        {0f, -3f},   // 0% = -3
                        {0.25f, -1f}, // 25% = -1
                        {0.5f, 0f},   // 50% = +0
                        {0.75f, 2f},  // 75% = +2
                        {1f, 4f},      // 100% = +4
                        {1.2f, 5f}      // 100% = +4
                    };
                }
            }

            if (statCurve == null)
            {
                statCurve = LoadCurveFromXML("MagicAndMyths_StatToBonus");

                if (statCurve == null)
                {
                    statCurve = new SimpleCurve
                    {
                        {0f, -3f},   // 0% = -3
                        {0.25f, -1f}, // 25% = -1
                        {0.5f, 0f},   // 50% = +0
                        {0.75f, 2f},  // 75% = +2
                        {1f, 4f},      // 100% = +4
                        {1.2f, 5f}
                    };
                }
            }
        }

        private static SimpleCurve LoadCurveFromXML(string defName)
        {
            var def = DefDatabase<CurveDefMagicMyths>.GetNamed(defName, false);
            return def?.curve;
        }

        public static int GetSkillBonus(Pawn pawn, SkillDef skill)
        {
            if (skillCurve == null) Initialize();

            if (pawn?.skills == null) 
                return -1;

            SkillRecord skillRecord = pawn.skills.GetSkill(skill);
            if (skillRecord == null) 
                return -1;

            float level = skillRecord.Level;
            return Mathf.RoundToInt(skillCurve.Evaluate(level));
        }

        public static int GetCapacityBonus(Pawn pawn, PawnCapacityDef capacity)
        {
            if (capacityCurve == null) Initialize();

            if (pawn?.health?.capacities == null) 
                return -3;

            float capacityValue = pawn.health.capacities.GetLevel(capacity);
            return Mathf.RoundToInt(capacityCurve.Evaluate(capacityValue));
        }

   
        public static int GetStatBonus(Pawn pawn, StatDef stat)
        {
            if (statCurve == null) Initialize();

            if (pawn == null)
                return -3;

            float statValue = pawn.GetStatValue(stat);
            float maxValue = stat.maxValue;

            float percentage = Mathf.Clamp01(statValue / maxValue);
            return Mathf.RoundToInt(statCurve.Evaluate(percentage));
        }

        // Calculate success chance from DC and bonus (for internal use)
        public static float CalculateSuccessChance(int dc, int bonus)
        {
            // In D&D, you need to roll dc or higher on d20
            // So success chance is (21 - (dc - bonus)) / 20
            float successChance = (21f - dc + bonus) / 20f;
            return Mathf.Clamp(successChance, 0.05f, 0.95f);
        }

        // Roll a d20 and check against DC (for visual logging)
        public static DCOutcome RollAgainstDC(int dc, int bonus)
        {
            int roll = Rand.RangeInclusive(1, 20);
            int total = roll + bonus;
            bool success = total >= dc;
            return new DCOutcome(success, roll, total);
        }

        public static string FormatDCCheck(int dc, int bonus)
        {
            string bonusStr = bonus >= 0 ? $"+{bonus}" : bonus.ToString();
            return $"[DC {dc} ({bonusStr})]";
        }
    }
}
