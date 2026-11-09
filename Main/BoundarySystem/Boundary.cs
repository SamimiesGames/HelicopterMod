using UnityEngine;

namespace HelicopterMod.Main.BoundarySystem
{
    public class Boundary
    {
        public Dictionary<string, Transform> ExitPoints;
        public Dictionary<string, List<GameObject>> Transitions;

        public Boundary()
        {
            ExitPoints = new Dictionary<string, Transform>();
            Transitions = new Dictionary<string, List<GameObject>>();
        }
    }   
}
