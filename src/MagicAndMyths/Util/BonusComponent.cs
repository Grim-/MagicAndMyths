namespace MagicAndMyths
{
    //holds information on where a bonus came from
    public struct BonusComponent
    {
        public string Label { get; }
        public int Value { get; }

        public BonusComponent(string label, int value)
        {
            Label = label;
            Value = value;
        }
    }
}
