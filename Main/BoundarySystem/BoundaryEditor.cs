using HelicopterMod.Main.Utilities;
using HelicopterMod.Main.Utilities.Exporter;
using Il2Cpp;
using UnityEngine;

namespace HelicopterMod.Main.BoundarySystem
{
    public static class BoundaryEditor
    {
        private static GameObject Boundaries;
        private static GameObject Selected;
        private static string ExportDirectory;

        public static void Export()
        {
            if (!Boundaries)
            {
                Logger.Warning($"[BoundaryEditor] Export failed; no Boundaries!");
                return;
            }
            if (string.IsNullOrEmpty(ExportDirectory))
            {
                Logger.Warning($"[BoundaryEditor] Export failed; no ExportDirectory!");
                return;
            }
            
            GameObjectExporter.Export(Boundaries, ExportDirectory);
        }

        public static void Start()
        {
            if (!ManagerCache.PlayerManager)
            {
                Logger.Warning("[BoundaryEditor] Start failed; not in a region!");
                return;
            }
            
            string boundariesName = $"Boundaries_{GameManager.m_ActiveScene}";
            Boundaries = new GameObject(boundariesName);
            Boundaries.layer = LayerMask.NameToLayer("Trigger");
            
            Logger.Msg($"[BoundaryEditor] Start success; created \"{boundariesName}\"");
        }
        
        public static void SetExport()
        {
            if (uConsole.GetNumParameters() <= 0)
            {
                Logger.Error($"[BoundaryEditor] SetExport failed; invalid usage. Usage: BE.SetExport <string: directory>");
                return;
            }

            ExportDirectory = uConsole.GetString();
            if (!Directory.Exists(ExportDirectory))
            {
                Logger.Error($"[BoundaryEditor] SetExport failed; directory \"{ExportDirectory}\" does not exist.");
                ExportDirectory = string.Empty;
                return;
            }
            Logger.Msg($"[BoundaryEditor] SetExport success; ExportDirectory set to \"{ExportDirectory}\"");
        }

        public static void Show()
        {
            if (!BoundaryCache.Boundaries)
            {
                Logger.Error($"[BoundaryEditor] Show failed; there are no BoundaryCache.Boundaries!");
                return;
            }

            Logger.Msg($"[BoundaryEditor] Show");
            Boundaries = BoundaryCache.Boundaries;

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            MeshFilter cubeMeshFilter = cube.GetComponent<MeshFilter>();
            MeshRenderer cubeMeshRenderer = cube.GetComponent<MeshRenderer>();
            int visualized = 0;
            bool hide = false;
            
            foreach (List<GameObject> transitions in BoundaryCache.ActiveBoundary.Transitions.Values)
            {
                foreach (GameObject transition in transitions)
                {
                    MeshFilter meshFilter = transition.GetComponent<MeshFilter>();
                    if (meshFilter)
                    {
                        hide = true;
                        GameObject.Destroy(meshFilter);
                        GameObject.Destroy(transition.GetComponent<MeshRenderer>());
                        continue;
                    }
                    meshFilter = transition.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = cubeMeshFilter.sharedMesh;
                    
                    MeshRenderer meshRenderer = transition.AddComponent<MeshRenderer>();
                    meshRenderer.materials = cubeMeshRenderer.materials;
                    meshRenderer.material = cubeMeshRenderer.material;
                }
            }
            
            GameObject.Destroy(cube);
            if (hide)
            {
                Boundaries = null;
                Logger.Msg($"[BoundaryEditor] Hid transitions. (Editing cancelled.)");
            }
            else
            {
                Logger.Msg($"[BoundaryEditor] Showing transitions. (You can now edit.)");
            }
        }
        
        public static void Create()
        {
            if (uConsole.GetNumParameters() < 2)
            {
                Logger.Error($"[BoundaryEditor] Create failed; invalid usage. Usage: BE.Create <string: regionName>, <float: width>");
                return;
            }
            if (!Boundaries)
            {
                Logger.Warning("[BoundaryEditor] Create failed; no boundaries!");
                return;
            }
            if (!GameManager.m_vpFPSPlayer.FPSCamera)
            {
                Logger.Warning("[BoundaryEditor] Create failed; no m_vpFPSPlayer!");
                return;
            }

            string regionName = uConsole.GetString();
            float width = uConsole.GetFloat();

            GameObject boundary = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boundary.transform.SetParent(Boundaries.transform);
            boundary.layer = Boundaries.layer;
            
            Vector3 position = GameManager.m_vpFPSPlayer.FPSCamera.transform.position;
            
            Vector3 forward = GameManager.m_vpFPSPlayer.FPSCamera.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Quaternion rotation = forward.magnitude > 0 ? Quaternion.LookRotation(forward) : Quaternion.identity;

            boundary.transform.position = position + forward * 100f;
            boundary.transform.rotation = rotation;
            boundary.transform.localScale = new Vector3(width, 1000f, 50f);
            
            boundary.name = $"{regionName}_boundary";
            
            Selected = boundary;
            Logger.Msg($"[BoundaryEditor] Created boundary; {boundary.name}");
        }
        
        public static void ExitPoint()
        {            
            if (uConsole.GetNumParameters() < 1)
            {
                Logger.Error($"[BoundaryEditor] ExitPoint failed; invalid usage. Usage: BE.SetExit <string: regionName>");
                return;
            }
            if (!Boundaries)
            {
                Logger.Warning("[BoundaryEditor] ExitPoint failed; no boundaries!");
                return;
            }
            if (!GameManager.m_vpFPSPlayer.FPSCamera)
            {
                Logger.Warning("[BoundaryEditor] ExitPoint failed; no m_vpFPSPlayer!");
                return;
            }

            string regionName = uConsole.GetString();
            
            Vector3 position = GameManager.m_vpFPSPlayer.FPSCamera.transform.position;
            
            Vector3 forward = GameManager.m_vpFPSPlayer.FPSCamera.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Quaternion rotation = forward.magnitude > 0 ? Quaternion.LookRotation(forward) : Quaternion.identity;

            GameObject exitPoint = new GameObject($"{regionName}_exitPoint");
            exitPoint.transform.SetParent(Boundaries.transform);
            exitPoint.transform.position = position;
            exitPoint.transform.rotation = rotation;
            
            Logger.Msg($"[BoundaryEditor] ExitPoint success; created exit point.");
        }
        
        public static void Place()
        {
            if (!Selected)
            {
                Logger.Warning("[BoundaryEditor] Place failed; nothing selected!");
                return;
            }
            if (!Boundaries)
            {
                Logger.Warning("[BoundaryEditor] Place failed; no boundaries!");
                return;
            }
            if (!GameManager.m_vpFPSPlayer.FPSCamera)
            {
                Logger.Warning("[BoundaryEditor] Place failed; no m_vpFPSPlayer!");
                return;
            }
            
            Vector3 position = GameManager.m_vpFPSPlayer.FPSCamera.transform.position;
            
            Vector3 forward = GameManager.m_vpFPSPlayer.FPSCamera.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Quaternion rotation = forward.magnitude > 0 ? Quaternion.LookRotation(forward) : Quaternion.identity;

            Selected.transform.position = position + forward * 100f;
            Selected.transform.rotation = rotation;
            
            Logger.Msg($"[BoundaryEditor] Moved success; moved {Selected.name}.");
        }
    }    
}
