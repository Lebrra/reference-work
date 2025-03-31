using Unity.Collections;
using UnityEngine;
// ReSharper disable All

public class DrawingData : ScriptableObject
{
    [SerializeField]
    string drawingName = "none";
    [SerializeField]
    Color32 drawingColor = Color.white;
    [SerializeField]
    Drawing drawing = null;

    public Drawing Drawing => drawing;

    public void AddDrawing(Drawing newDrawing, string name)
    {
        drawing = newDrawing;
        drawingName = name;
    }
}
