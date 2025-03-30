using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CustomGateAddress : Def
    {
        public List<GateSymbolDef> address;
        public MapGeneratorDef mapGeneratorDef;
        public bool IsValidAddress(GateAddress gateAddress)
        {
            if (gateAddress.Symbols.Count != address.Count)
                return false;

            for (int i = 0; i < address.Count; i++)
            {
                if (gateAddress.Symbols[i] != address[i])
                    return false;
            }

            return true;
        }
    }
}
