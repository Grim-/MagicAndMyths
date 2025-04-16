using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public abstract class StatPart_DnDStatBase : StatPart
    {
        protected int BaseStatValue = 5;
        protected Dictionary<string, float> StatContributions = new Dictionary<string, float>();

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing is Pawn pawn)
            {
                StringBuilder explanation = new StringBuilder();
                foreach (KeyValuePair<string, float> contribution in StatContributions)
                {
                    string prefix = contribution.Value >= 0 ? "+" : "";
                    explanation.AppendLine($"{contribution.Key}: {prefix}{contribution.Value}");
                }

                float total = BaseStatValue;
                foreach (float value in StatContributions.Values)
                {
                    total += value;
                }
                explanation.AppendLine($"Total Bonus: {total}");

                return explanation.ToString();
            }
            return null;
        }
    }
}