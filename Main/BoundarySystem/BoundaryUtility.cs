using HelicopterMod.Main.Utilities;
using Il2CppTinyJSON;
using UnityEngine;

namespace HelicopterMod.Main.BoundarySystem
{
    public struct BoundaryUtility
    {
        public static Boundary Unpack(GameObject boundaries)
        {
            Boundary boundary = new Boundary();
            
            for (int i = 0; i < boundaries.transform.GetChildCount(); i++)
            {
                GameObject candidate = boundaries.transform.GetChild(i).gameObject;
                
                int index = candidate.name.IndexOf('_');
                if (index <= -1)
                {
                    Logger.Error($"[Boundaries] CreateLoadingZones warning; {candidate.name} is incorrectly formatted!");
                    continue;
                } 
                
                string type = candidate.name.Substring(index + 1, candidate.name.Length - (index + 1));
                string region = candidate.name.Substring(0, index);
                
                Logger.Msg($"[Boundaries] CreateLoadingZones, {candidate.name} (type={type}, scene={region})");
                if (type == "boundary")
                {
                    if (boundary.Transitions.ContainsKey(region))
                    {
                        boundary.Transitions[region].Add(candidate);
                    }
                    else
                    {
                        boundary.Transitions.Add(region, new List<GameObject>());
                        boundary.Transitions[region].Add(candidate);
                    }
                }
                else if (type == "exitPoint")
                {
                    boundary.ExitPoints.Add(region, candidate.transform);
                }
            }

            return boundary;
        }

        public static void Load(Boundary boundary)
        {
            foreach (string region in boundary.Transitions.Keys)
            {
                foreach (GameObject candidate in boundary.Transitions[region])
                {
                    Logger.Msg($"[Boundaries] CreateLoadingZones, RegionUtility.AttachLoadScene");
                    RegionUtility.AttachLoadScene(candidate, region);
                    Logger.Msg($"[Boundaries] CreateLoadingZones, RegionUtility.AttachLoadScene ... OK");
                }
            }
        }
    }
}
