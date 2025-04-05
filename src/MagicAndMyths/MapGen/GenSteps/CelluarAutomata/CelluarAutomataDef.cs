using System;
using Verse;

namespace MagicAndMyths
{
    public class CelluarAutomataDef : Def
    {
        private CellularAutomataWorker _Worker = null;
        public CellularAutomataWorker Worker
        {
            get
            {
                if (_Worker == null)
                {
                    _Worker = (CellularAutomataWorker)Activator.CreateInstance(workerClass);
                }

                return _Worker;
            }
        }

        public Type workerClass;

        public void Apply(Map map, BoolGrid dungeonGrid, BoolGrid currentState)
        {
            if (workerClass == null)
            {
                return;
            }

            Log.Message($"Applying Cellular Automata {this.defName}");
            Worker.Apply(map, dungeonGrid, currentState);
        }
    }
}
