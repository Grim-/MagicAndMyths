using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class MapComp_PerceptionManager : MapComponent
    {

        protected HashSet<IPerceptible> _AllPerceptibles = new HashSet<IPerceptible>();


        public MapComp_PerceptionManager(Map map) : base(map)
        {

        }

        public List<IPerceptible> PerceptablesInRange(IntVec3 Position, float maxDistance = 3f)
        {
            return _AllPerceptibles.Where(x => x.Position.InHorDistOf(Position, maxDistance)).ToList();
        }


        public void Register(IPerceptible perceptible)
        {
            if (!_AllPerceptibles.Contains(perceptible))
            {
                _AllPerceptibles.Add(perceptible);
            }
        }

        public void UnRegister(IPerceptible perceptible)
        {
            if (_AllPerceptibles.Contains(perceptible))
            {
                _AllPerceptibles.Remove(perceptible);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
          
        }
    }


    public interface IPerceptible
    {
        int DC { get; }
        bool IsVisible { get; }

        Thing Thing { get; }

        IntVec3 Position { get; }

        void Show();
        void Hide();
    }
}

