namespace MagicAndMyths
{
    public interface IEnergyProvider
    {
        float Energy { get; }
        float MaxEnergy { get; }
        bool TryUseEnergy(float amount);
        void AddEnergy(float amount);
        bool HasEnough(float amount);
        float GetEnergyPercent();
    }
}