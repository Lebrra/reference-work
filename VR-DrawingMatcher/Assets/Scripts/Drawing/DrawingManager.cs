using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class DrawingManager : MonoBehaviour
{
    public static DrawingManager Instance = null;


    [SerializeField, Tooltip("Value to be under for a drawing to be considered 'matching' to another")] // TODO: reword this when the number makes sense
    float matchError = 0.01F;
    [SerializeField]
    List<DrawingData> drawingsToMatch = new List<DrawingData>();
    
    [SerializeField, ReadOnly]
    List<Drawing.ExternalDrawingCompareData> latestCompareData = new();

    private void Awake()
    {
        if (Instance)
        {
            Destroy(Instance);
        }
        Instance = this;
    }

    private void Start()
    {
        //DebugDrawer.Instance?.DrawAllSavedDrawings(drawingsToMatch);
    }

    public DrawingData TestForDrawingMatch(Drawing drawing, Vector3 position)
    {
        // iterate through every drawingsToMatch to test for a match
        // return best-matched drawing data (or none if bad)
        // ideally this disregards size

        latestCompareData = new List<Drawing.ExternalDrawingCompareData>();
        foreach (var savedDrawing in drawingsToMatch)
        {
            latestCompareData.Add(savedDrawing.Drawing.MatchDrawing(drawing));
        }
        
        int match = 0;
        for (int i = 1; i < latestCompareData.Count; i++)
        {
            if (latestCompareData[i].avgDistFromMatchingPoints < latestCompareData[match].avgDistFromMatchingPoints)
            {
                match = i;
            }
        }

        if (latestCompareData[match].avgDistFromMatchingPoints > 3F)
        {
            Debug.Log("I don't think any match");
            return null;
        }
        else
        {
            Debug.Log($"I think the closest match is the {drawingsToMatch[match].name}");
            Debug.Log(latestCompareData[match].ToString);
            if (DebugDrawer.Instance)
            {
                DebugDrawer.Instance.DrawDrawing(drawingsToMatch[match].Drawing, position, match);
            }
            return drawingsToMatch[match];
        }
    }
}
