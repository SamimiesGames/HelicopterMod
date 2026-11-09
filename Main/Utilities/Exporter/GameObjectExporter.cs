using System.Text;
using UnityEngine;

namespace HelicopterMod.Main.Utilities.Exporter
{
    public struct GameObjectExporter
    {
        private class NodeData
        {
            public GameObject GameObject;
            public long GameObjectId;
            public long TransformId;
            public long Parent;
            public List<long> Children = new();
        }
        
        public static void Export(GameObject root, string directory)
        {
            if (root == null)
            {
                Logger.Msg($"[GameObjectExporter] GameObject is null!");
                return;
            }
            Logger.Msg($"[GameObjectExporter] Export {root.name}");

            List<NodeData> nodes = new List<NodeData>();
            
            Logger.Msg($"[GameObjectExporter] Traversing hierarchy");
            _TraverseDFS(root.transform, 0L, nodes);
            Logger.Msg($"[GameObjectExporter] Traversing hierarchy ... OK");
            
            Logger.Msg($"[GameObjectExporter] Generate YAML");
            
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("%YAML 1.1");
            stringBuilder.AppendLine("%TAG !u! tag:unity3d.com,2011:");

            foreach (var node in nodes)
            {
                AppendGameObject(stringBuilder, node);
                AppendTransform(stringBuilder, node);
            }
            
            Logger.Msg($"[GameObjectExporter] Generate YAML ... OK");
            
            Logger.Msg($"[GameObjectExporter] Write");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Logger.Msg($"[GameObjectExporter] Created export directory \"{directory}\"");
            }
            
            string filePath = Path.Combine(directory, $"{root.name}.prefab");
            
            Logger.Msg($"[GameObjectExporter] Saving prefab to \"{filePath}\"");
            File.WriteAllText(filePath, stringBuilder.ToString(), Encoding.UTF8);
            Logger.Msg($"[GameObjectExporter] Write ... OK");
            
            Logger.Msg($"[GameObjectExporter] Export ... COMPLETE");
        }

        private static void _TraverseDFS(Transform transform, long transformId, List<NodeData> nodes)
        {
            NodeData node = new NodeData
            {
                GameObject = transform.gameObject,
                GameObjectId = GenerateId(),
                TransformId = GenerateId(),
                Parent = transformId
            };

            nodes.Add(node);

            // Is child
            if (transformId != 0)
            {
                nodes.Find(x => x.TransformId == transformId)
                    ?.Children
                    .Add(node.TransformId);
            }
            
            for (int i = 0; i < transform.GetChildCount(); i++)
            {
                _TraverseDFS(transform.GetChild(i), node.TransformId, nodes);
            }
        }

        private static void AppendGameObject(StringBuilder sb, NodeData node)
        {
            GameObject go = node.GameObject;
            sb.AppendLine($"--- !u!1 &{node.GameObjectId}");
            sb.AppendLine("GameObject:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_CorrespondingSourceObject: {fileID: 0}");
            sb.AppendLine("  m_PrefabInstance: {fileID: 0}");
            sb.AppendLine("  m_PrefabAsset: {fileID: 0}");
            sb.AppendLine("  serializedVersion: 6");
            sb.AppendLine("  m_Component:");
            sb.AppendLine($"  - component: {{fileID: {node.TransformId}}}");
            sb.AppendLine($"  m_Layer: {go.layer}");
            sb.AppendLine($"  m_Name: \"{go.name}\"");
            sb.AppendLine("  m_TagString: Untagged");
            sb.AppendLine("  m_Icon: {fileID: 0}");
            sb.AppendLine("  m_NavMeshLayer: 0");
            sb.AppendLine("  m_StaticEditorFlags: 0");
            sb.AppendLine($"  m_IsActive: {(go.activeSelf ? 1 : 0)}");
        }

        private static void AppendTransform(StringBuilder sb, NodeData node)
        {
            Transform t = node.GameObject.transform;
            Vector3 pos = t.localPosition;
            Quaternion rot = t.localRotation;
            Vector3 scale = t.localScale;
            Vector3 euler = t.localEulerAngles;

            sb.AppendLine($"--- !u!4 &{node.TransformId}");
            sb.AppendLine("Transform:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_CorrespondingSourceObject: {fileID: 0}");
            sb.AppendLine("  m_PrefabInstance: {fileID: 0}");
            sb.AppendLine("  m_PrefabAsset: {fileID: 0}");
            sb.AppendLine($"  m_GameObject: {{fileID: {node.GameObjectId}}}");
            sb.AppendLine($"  m_LocalRotation: {{x: {rot.x:R}, y: {rot.y:R}, z: {rot.z:R}, w: {rot.w:R}}}");
            sb.AppendLine($"  m_LocalPosition: {{x: {pos.x:R}, y: {pos.y:R}, z: {pos.z:R}}}");
            sb.AppendLine($"  m_LocalScale: {{x: {scale.x:R}, y: {scale.y:R}, z: {scale.z:R}}}");
            sb.AppendLine("  m_ConstrainProportionsScale: 0");

            if (node.Children.Count <= 0)
            {
                sb.AppendLine("  m_Children: []");
            }
            else
            {
                sb.AppendLine("  m_Children:");
                foreach (int childId in node.Children)
                {
                    sb.AppendLine($"  - {{fileID: {childId}}}");
                }   
            }
            
            sb.AppendLine($"  m_Father: {{fileID: {node.Parent}}}");
            sb.AppendLine($"  m_LocalEulerAnglesHint: {{x: {euler.x:R}, y: {euler.y:R}, z: {euler.z:R}}}");
        }
        
        private static long GenerateId()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            long id = BitConverter.ToInt64(buffer, 0);
            
            // Mask out the sign bit to guarantee a positive 64-bit integer (0 to 9.2 quintillion)
            return id & 0x7FFFFFFFFFFFFFFF;
        }
    } 
}
