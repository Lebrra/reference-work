using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draws all current Drawings in DrawingManager in a Canvas with linerenderers
/// </summary>
public class DebugDrawer : MonoBehaviour
{
    public static DebugDrawer Instance;
    
    [SerializeField]
    Material debugMaterial = null;
    [SerializeField]
    float debugLineWidth = 0.01F;
    [SerializeField]
    List<Color32> debugColors = new();

    Transform parent;
    
    private void Awake()
    {
        if (Instance)
        {
            Destroy(Instance);
        }
        Instance = this;
    }

    public void DrawAllSavedDrawings(List<DrawingData> drawings)
    {
        if (!parent)
        {
            parent = new GameObject("Drawings_Debug").transform;
        }

        for (int i = 0; i < drawings.Count; i++)
        {
            DrawDrawing(drawings[i].Drawing, debugColors.Count > i ? debugColors[i] : default);
        }
    }

    public void DrawDrawing(Drawing drawing, Vector3 worldPos, Color32 color = default)
    {
        var go = new GameObject("DebugDrawing");
        go.transform.SetParent(parent);
        var newLine = go.AddComponent<LineRenderer>();
        go.transform.position = worldPos;
        
        go.transform.LookAt(Camera.main.transform);
        var lookAtRot = go.transform.rotation;
        lookAtRot.eulerAngles = new Vector3(0F, lookAtRot.eulerAngles.y, 0F);
        go.transform.rotation = lookAtRot;
        
        newLine.startColor = newLine.endColor = color;
        newLine.startWidth = newLine.endWidth = debugLineWidth;
        newLine.material = debugMaterial;
        newLine.positionCount = drawing.NormalPoints.Count;
        
        for (int i = 0; i < newLine.positionCount; i++)
        {
            var transformedPos = lookAtRot * (Vector3)drawing.NormalPoints[i];
            newLine.SetPosition(i, transformedPos + worldPos);
        }
    }
    
    public void DrawDrawing(Drawing drawing, Color32 color = default)
    {
        DrawDrawing(drawing, Vector3.zero, color);
    }
    
    public void DrawDrawing(Drawing drawing, Vector3 worldPos, int debugColorIndex = 0)
    {
        DrawDrawing(drawing, worldPos, debugColors.Count > debugColorIndex ? debugColors[debugColorIndex] : default);
    }
    
    public void DrawDrawing(Drawing drawing, int debugColorIndex = 0)
    {
        DrawDrawing(drawing, Vector3.zero, debugColors.Count > debugColorIndex ? debugColors[debugColorIndex] : default);
    }
}
