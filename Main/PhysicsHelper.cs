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
    }
}
