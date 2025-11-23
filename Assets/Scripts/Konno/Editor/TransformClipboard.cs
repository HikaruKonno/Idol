// Assets/Editor/TransformClipboard.cs
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class TransformClipboard
{
    private static List<Vector3> _positions = new List<Vector3>();
    private static List<Quaternion> _rotations = new List<Quaternion>();

    // Windows: Ctrl+Shift+C / macOS: Cmd+Shift+C
    [MenuItem("Tools/Transform/Copy Position & Rotation %#c")]
    private static void CopyTransformData()
    {
        _positions.Clear();
        _rotations.Clear();

        foreach (var t in Selection.transforms)
        {
            _positions.Add(t.position);
            _rotations.Add(t.rotation);
        }

        Debug.Log($"Copied {_positions.Count} transforms (Position + Rotation).");
    }

    // Windows: Ctrl+Shift+V / macOS: Cmd+Shift+V
    [MenuItem("Tools/Transform/Paste Position & Rotation %#v")]
    private static void PasteTransformData()
    {
        var targets = Selection.transforms;
        if (targets.Length != _positions.Count)
        {
            Debug.LogWarning("Selection count must match the number of copied transforms.");
            return;
        }

        Undo.RecordObjects(targets, "Paste Position & Rotation");
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].position = _positions[i];
            targets[i].rotation = _rotations[i];
        }

        Debug.Log("Pasted Position + Rotation.");
    }
}