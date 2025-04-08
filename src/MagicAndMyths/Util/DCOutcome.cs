namespace MagicAndMyths
{
    public struct DCOutcome
    {
        public bool Success;
        public int total;
        public int roll;

        public DCOutcome(bool success, int total, int roll)
        {
            Success = success;
            this.total = total;
            this.roll = roll;
        }
    }
}
