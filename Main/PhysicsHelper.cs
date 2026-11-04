using UnityEngine;

namespace HelicopterMod.Main
{
    public struct PhysicsHelper
    {
        public static void SetLayer(GameObject other, int layer)
        {
            if (other == null) return;
            if (other.name == "PlayerEnter") return;
            
            if(other.GetComponent<Collider>()) other.layer = layer;
            
            for (int i = 0; i < other.transform.childCount; i++)
            {
                SetLayer(other.transform.GetChild(i).gameObject, layer);
            }
        }
        
        public static void GetColliders(GameObject other, ref List<Collider> colliders)
        {
            if (other == null) return;
            if (other.name == "PlayerEnter") return;

            Collider collider = other.GetComponent<Collider>();
            if (other.GetComponent<Collider>())
            {
                colliders.Add(collider);
            }
            
            for (int i = 0; i < other.transform.childCount; i++)
            {
                GetColliders(other.transform.GetChild(i).gameObject, ref colliders);
            }
        }

        public static void IgnoreCollisions(GameObject lhs, GameObject rhs)
        {
            List<Collider> lhsColliders = new List<Collider>();
            
            GetColliders(lhs, ref lhsColliders);
            Collider[] rhsColliders = rhs.GetComponentsInChildren<Collider>();

            foreach (var lhsCol in lhsColliders)
            {
                foreach (var rhsCol in rhsColliders)
                {
                    if(lhsCol == rhsCol) continue;
                    
                    Physics.IgnoreCollision(lhsCol, rhsCol);
                }
            }
        } 
    }
}
