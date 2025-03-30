using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class GateAddress : IExposable
    {
        public List<GateSymbolDef> Symbols;



        public GateAddress(List<GateSymbolDef> symbols)
        {
            Symbols = symbols;
        }

        public GateAddress()
        {

        }

        public override string ToString()
        {
            return string.Join("-", Symbols.Select(s => s.label));
        }

        public int ToInt()
        {
            int result = 0;
            var baseSize = DefDatabase<GateSymbolDef>.AllDefs.Count();

            foreach (var symbol in Symbols)
            {
                result = result * baseSize + symbol.symbolIndex;
            }
            return result;
        }
        public static GateAddress GetAddressForSettlement(Settlement settlement)
        {
            if (settlement == null) return null;
            return CreateAddressForTile(settlement.Tile);
        }

        public static GateAddress CreateAddressForTile(int tileId)
        {
            if (tileId < 0)
                return null;

            return GateAddress.FromInt(tileId);
        }

        public static GateAddress FromInt(int number)
        {
            var result = new List<GateSymbolDef>();
            var availableSymbols = DefDatabase<GateSymbolDef>
                .AllDefs
                .OrderBy(def => def.symbolIndex)
                .ToList();
            int baseSize = availableSymbols.Count();

            while (result.Count < 3)
            {
                int index = number % baseSize;
                result.Insert(0, availableSymbols[index]);
                number /= baseSize;
            }

            return new GateAddress(result);
        }

        public static GateAddress FromString(string address)
        {
            var symbolNames = address.Split('-');
            var symbols = symbolNames
                .Select(name => DefDatabase<GateSymbolDef>.GetNamed(name, false))
                .Where(def => def != null)
                .ToList();

            return new GateAddress(symbols);
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref Symbols, "addressSymbols");
        }
    }
}
