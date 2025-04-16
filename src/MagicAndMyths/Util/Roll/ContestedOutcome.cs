using RimWorld;
using Verse;

namespace MagicAndMyths
{
    /// <summary>
    /// the detailed result of an opposed RollCheck between two pawns.
    /// </summary>
    public readonly struct ContestedOutcome
    {
        public Pawn Contestor { get; }
        public Pawn Target { get; }
        public StatDef Stat { get; }
        public RollCheckOutcome ContestorOutcome { get; }
        public RollCheckOutcome TargetOutcome { get; }
        public Pawn Winner => ContestorOutcome.Total >= TargetOutcome.Total ? Contestor : Target;

        public ContestedOutcome(Pawn contestor, Pawn target, StatDef stat, RollCheckOutcome contestorOutcome, RollCheckOutcome targetOutcome)
        {
            Contestor = contestor;
            Target = target;
            Stat = stat;
            ContestorOutcome = contestorOutcome;
            TargetOutcome = targetOutcome;
        }

        public override string ToString()
        {
            return $"Opposed Check ({Stat.label}): {Contestor.LabelShort} {ContestorOutcome} vs {Target.LabelShort} {TargetOutcome}. Winner: {Winner.LabelShort}";
        }
    }
}
