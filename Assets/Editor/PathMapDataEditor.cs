using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathManager))]
public class PathManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Straighten All Lines"))
        {
            PathManager pathManager = (PathManager)target;
            var pathMap = pathManager.pathMapData;
            if (pathMap == null || pathMap.nodes == null)
                return;

            Undo.RecordObject(pathMap, "Straighten All Lines");

            for (int i = 0; i < pathMap.nodes.Length; i++)
            {
                var node = pathMap.nodes[i];
                if (node.nextNodeIndices == null) continue;

                foreach (var nextIndex in node.nextNodeIndices)
                {
                    if (nextIndex < 0 || nextIndex >= pathMap.nodes.Length) continue;
                    var nextNode = pathMap.nodes[nextIndex];

                    Vector3 from = node.position;
                    Vector3 to = nextNode.position;
                    Vector3 newTo = to;

                    // Determine which axis has the greater distance and align accordingly
                    if (Mathf.Abs(to.x - from.x) > Mathf.Abs(to.y - from.y))
                    {
                        newTo.y = from.y; // Align horizontally
                    }
                    else
                    {
                        newTo.x = from.x; // Align vertically
                    }

                    nextNode.position = newTo;
                }
            }

            EditorUtility.SetDirty(pathMap);
        }
    }

    void OnSceneGUI()
    {
        PathManager pathManager = (PathManager)target;
        var pathMap = pathManager.pathMapData;
        if (pathMap == null || pathMap.nodes == null)
            return;

        for (int i = 0; i < pathMap.nodes.Length; i++)
        {
            var node = pathMap.nodes[i];
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(node.position, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(pathMap, "Move Path Node");
                node.position = newPos;
                EditorUtility.SetDirty(pathMap);
            }
            Handles.Label(newPos + Vector3.up * 0.5f, $"Node {i}");
        }
    }
}