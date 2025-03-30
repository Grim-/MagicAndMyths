using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class GateSymbolDef : Def
    {
        public int symbolIndex;
        public string texPath;


        private Texture2D _Texture;
        public Texture2D Texture
        {
            get
            {
                if (_Texture == null && !string.IsNullOrEmpty(texPath))
                {
                    _Texture = ContentFinder<Texture2D>.Get(texPath);
                }

                return _Texture;
            }
        }

    }
}
