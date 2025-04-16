using UnityEngine;

namespace MagicAndMyths
{
    //handles a roll and holds the information on that roll
    public class RollCheckOutcome
    {
        public int LastRoll;
        public int Bonus;
        public int Total => LastRoll + Bonus;

        public RollCheckOutcome(int bonus, int diceValue = 20)
        {
            Bonus = bonus;
            Roll(diceValue);
        }

        public void Roll(int diceValue = 20)
        {
            LastRoll = Random.Range(1, diceValue + 1);
        }

        public override string ToString()
        {
            string bonusStr = Bonus >= 0 ? $"+{Bonus}" : Bonus.ToString();
            return $"Roll: {LastRoll} + {bonusStr} = {Total}";
        }
    }
}
