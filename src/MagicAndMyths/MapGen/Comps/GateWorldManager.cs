using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{

    /// <summary>
    /// Saves resolved addresses
    /// </summary>
    public class GateWorldManager : WorldComponent
    {
        private Dictionary<int, int> ResolvedAddresses = new Dictionary<int, int>();
        private int worldSeed;
        private const int MAX_ATTEMPTS = 100;
        public GateWorldManager(World world) : base(world)
        {
            this.worldSeed = world.ConstantRandSeed;
        }


        public List<GateSymbolDef> NumberToSymbols(int number)
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

            return result;
        }

        public int SymbolsToNumber(List<GateSymbolDef> symbols)
        {
            int result = 0;
            var baseSize = DefDatabase<GateSymbolDef>
                .AllDefs
                .Count();

            foreach (var symbol in symbols)
            {
                result = result * baseSize + symbol.symbolIndex;
            }

            return result;
        }

        public CustomGateAddress GetCustomAddressMap(GateAddress gateAddress)
        {
            return DefDatabase<CustomGateAddress>.AllDefsListForReading.FirstOrDefault(x => x.IsValidAddress(gateAddress));
        }
        public bool HasCustomAddressMap(GateAddress gateAddress)
        {
            return DefDatabase<CustomGateAddress>.AllDefsListForReading.Any(x => x.IsValidAddress(gateAddress));
        }

        public int ResolveTileFromAddress(GateAddress address)
        {
            ///TO DO LEAVE FOR NOW
            //if (HasCustomAddressMap(address))
            //{
            //    //return tileID
            //    return GetCustomAddressMap(address);
            //}

            int addressValue = SymbolsToNumber(address.Symbols);
            System.Random rand = new System.Random(HashCombine(worldSeed, addressValue));

            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                int tileId = rand.Next(0, Find.WorldGrid.TilesCount);
                if (IsValidTile(tileId))
                {
                    return tileId;
                }
            }

            return -1;
        }

        private bool IsValidTile(int tileId)
        {
            var tile = Find.WorldGrid[tileId];
            return tile != null
                && !tile.WaterCovered
                && tile.hilliness != Hilliness.Impassable
                && tile.biome != null;
        }

        private int HashCombine(int hash1, int hash2)
        {
            unchecked
            {
                int roll1 = ((hash1 << 5) + hash1) ^ hash2;
                int roll2 = ((hash2 << 5) + hash2) ^ hash1;
                return roll1 * 31 + roll2;
            }
        }


        public bool HasResolvedTile(GateAddress gateAddress)
        {
            return ResolvedAddresses.ContainsKey(gateAddress.ToInt());
        }
    }
}
