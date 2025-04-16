using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{

    /// <summary>
    /// calculates the various bonuses from properties, skills, stats, traits and other fields such as bodySize
    /// </summary>
    public class StatPart_Strength : StatPart_DnDStatBase
    {
        public Dictionary<BodyTypeDef, int> BodyTypeBonuses;
        public Dictionary<TraitDef, int> TraitBonuses;

        protected bool BonusesInitialized = false;

        protected void InitBonuses()
        {
            BodyTypeBonuses = new Dictionary<BodyTypeDef, int>()
            {
                { BodyTypeDefOf.Hulk,  2},
                { BodyTypeDefOf.Thin, -1},
                { BodyTypeDefOf.Baby, -5 },
                { BodyTypeDefOf.Child, -3 }
            };

            TraitBonuses = new Dictionary<TraitDef, int>()
            {
                { TraitDefOf.Brawler,  1},
                { TraitDefOf.Wimp, -1 }
            };

            BonusesInitialized = true;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing is Pawn pawn)
            {
                if (!BonusesInitialized)
                {
                    InitBonuses();
                }

                if (StatContributions == null)
                {
                    StatContributions = new Dictionary<string, float>();
                }

                StatContributions.Clear();

                int manipBonus = DCUtility.CalculateCapacityBonus(pawn, PawnCapacityDefOf.Manipulation, 0, 3);
                if (manipBonus > 0)
                    StatContributions["Manipulation"] = manipBonus;

                int movingBonus = DCUtility.CalculateCapacityBonus(pawn, PawnCapacityDefOf.Moving, 0, 2);
                if (movingBonus > 0)
                    StatContributions["Moving"] = movingBonus;

                float bodySizeBonus = 0;



                if (pawn.story != null && pawn.story.bodyType != null)
                {
                    if (BodyTypeBonuses.ContainsKey(pawn.story.bodyType))
                    {
                        bodySizeBonus += BodyTypeBonuses[pawn.story.bodyType];
                        StatContributions["Body Type"] = bodySizeBonus;
                    }
                }


                if (pawn.story != null && pawn.story.traits != null)
                {
                    foreach (var item in pawn.story.traits.allTraits)
                    {
                        if (TraitBonuses.ContainsKey(item.def))
                        {
                            StatContributions[item.def.defName] = TraitBonuses[item.def];
                        }
                    }
                }

                foreach (float contribution in StatContributions.Values)
                {
                    val += contribution;
                }
            }
        }
    }
}