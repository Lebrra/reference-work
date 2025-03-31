using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public class Drawing
{
    // used to compare within Drawing
    struct InternalDrawingCompareData
    {
        public int baseDrawingIndex;
        public int compareDrawingIndex;
        public float distance;
    }

    // used to determine if drawings are within error bounds of matching externally
    [System.Serializable]
    public struct ExternalDrawingCompareData
    {
        public int pointCountDiff;
        public int largestModeCount;
        public float avgDistFromMatchingPoints;
        public int unmatchedPointsCount;
        public float unmatchedPointsCountPercent;
        
        public new string ToString => $"PointCountDiff = {pointCountDiff} | LargestModeCount = {largestModeCount} | " +
                                      $"AvgDist = {avgDistFromMatchingPoints} | unmatchedCount = {unmatchedPointsCount} - {Mathf.Round(unmatchedPointsCountPercent * 100F)}%";
    }

    // center will always be defined as (0, 0)
    //Vector2 center = Vector2.zero;
    [SerializeField]
    List<Vector2> normalPoints = new List<Vector2>();
    public List<Vector2> NormalPoints => normalPoints;

    Vector2 size = Vector2.zero;

    public Vector2 Size
    {
        get
        {
            if (size == Vector2.zero && normalPoints.Count > 1)
                GenerateBounds();
            return size;
        }
    }

    public Drawing(List<Vector3> worldPoints, bool generateDebugs = false)
    {
        if (worldPoints == null) return;

        // DEBUG
        Transform debugContainer = null;

        // generate regression line along x/z:
        Vector3 worldCenter = Vector3.zero;
        foreach (var point in worldPoints)
        {
            worldCenter += point;
        }
        // worldCenter complete: (average of each)
        worldCenter /= worldPoints.Count;
        
        float topSum = 0F, botSum = 0F;
        foreach (var point in worldPoints)
        {
            var x = point.x - worldCenter.x;
            topSum += (x * (point.z - worldCenter.z));
            botSum += (x * x);
        }
        float slope = topSum / botSum;
        Vector3 xzRegressionVector = new Vector3(1, 0, slope);
        xzRegressionVector.Normalize();
        Vector3 xzForwardVector = Vector3.Cross(xzRegressionVector, Vector3.up);
        
        // we need to see if the player is facing the same way, if not, rotate 180
        var camForward = new Vector3(Camera.main.transform.forward.x, 0F, Camera.main.transform.forward.z);
        camForward.Normalize();
        float camAngle = (Mathf.Atan2(camForward.x, camForward.z) + 2 * Mathf.PI) % (2 * Mathf.PI);
        float forwardAngle = (Mathf.Atan2(xzForwardVector.x, xzForwardVector.z) + 2 * Mathf.PI) % (2 * Mathf.PI);
        Debug.Log($"cam = {camAngle}, forward = {forwardAngle}, diff = {camAngle - forwardAngle}");

        if (Mathf.Abs(camAngle - forwardAngle) > Mathf.PI * 0.7F)
        {
            // flip the vectors
            xzRegressionVector = -xzRegressionVector;
            xzForwardVector = -xzForwardVector;
        }

        //DEBUG
        if (generateDebugs)
        {
            debugContainer = new GameObject("New Drawing (debug)").transform;

            GameObject debugCenter = new GameObject("Drawing-WorldCenter");
            debugCenter.transform.SetParent(debugContainer);
            debugCenter.transform.position = worldCenter;

            Debug.DrawRay(worldCenter, xzRegressionVector, Color.red, 500);
            Debug.DrawRay(worldCenter, xzForwardVector, Color.blue, 500);
            
            // todo: line renderer post-drawings
        }

        // now project all the points:      => if facing backwards, invert x value
        for (int i = 0; i < worldPoints.Count; i++)
        {
            var worldAtY = new Vector3(worldCenter.x, worldPoints[i].y, worldCenter.z);
            var v = worldPoints[i] - worldAtY;
            var dist = Vector3.Dot(v, xzRegressionVector);
            normalPoints.Add(new Vector2(dist, worldPoints[i].y - worldCenter.y));
        }

        GenerateBounds();

        if (generateDebugs && DebugDrawer.Instance)
        {
            //DebugDrawer.Instance.DrawDrawing(this, 8);
        } 
    }

    public Drawing(List<Vector2> scaledPoints)
    {
        normalPoints = scaledPoints;
        GenerateBounds();
    }

    /// <summary>
    /// returns a block of data that is the results of a comparision between two drawings
    /// </summary>
    public ExternalDrawingCompareData MatchDrawing(Drawing drawingToCompare)
    {
        float scaler = Size.x / drawingToCompare.Size.x;
        
        //Vector2 scaler = Size / drawingToCompare.Size;
        List<Vector2> scaledComparePoints = new List<Vector2>();
        foreach (var compareNorms in drawingToCompare.normalPoints)
        {
            scaledComparePoints.Add(new Vector2(compareNorms.x * scaler, compareNorms.y * scaler));
        }

        if (DebugDrawer.Instance)
        {
            Drawing debugDrawing = new Drawing(scaledComparePoints);
            //DebugDrawer.Instance.DrawDrawing(debugDrawing, 7);
        }
        
        if (normalPoints.Count > drawingToCompare.normalPoints.Count) 
            return MatchDrawing(scaledComparePoints, normalPoints);
        else
            return MatchDrawing(normalPoints, scaledComparePoints);
    }

    ExternalDrawingCompareData MatchDrawing(List<Vector2> smallerSet, List<Vector2> largerSet)
    {
        ExternalDrawingCompareData results = new ExternalDrawingCompareData
        {
            pointCountDiff = largerSet.Count - smallerSet.Count
        };

        List<InternalDrawingCompareData> compareData = new List<InternalDrawingCompareData>();

        // generate a list using the larger quantity of normalPoints that map each one to the closest point on the other drawing
        for (int i = 0; i < largerSet.Count; i++)
        {
            InternalDrawingCompareData newCompare = new InternalDrawingCompareData
            {
                baseDrawingIndex = i,
                compareDrawingIndex = 0,
                distance = Vector2.Distance(largerSet[i], smallerSet[0])
            };

            for (int j = 1; j < smallerSet.Count; j++)
            {
                var newDist = Vector2.Distance(largerSet[i], smallerSet[j]);
                if (newDist < newCompare.distance)
                {
                    newCompare.compareDrawingIndex = j;
                    newCompare.distance = newDist;
                }
            }

            compareData.Add(newCompare);
        }

        results.avgDistFromMatchingPoints = (compareData.Sum(c => c.distance) / compareData.Count) * 100F;  // making bigger for easier comparision
        results.largestModeCount = compareData.GroupBy(c => c.compareDrawingIndex).OrderByDescending(c => c.Count()).First().Count();
        
        // go through second set and see which points weren't matched at all
        int unmatchedCount = 0;
        for (int i = 0; i < smallerSet.Count; i++)
        {
            if (!compareData.Any(c => c.compareDrawingIndex == i))
                unmatchedCount++;
        }
        results.unmatchedPointsCount = unmatchedCount;
        results.unmatchedPointsCountPercent = (float)unmatchedCount / smallerSet.Count;
        
        return results;
    }

    void GenerateBounds()
    {
        float minX = normalPoints[0].x,
            minY = normalPoints[0].y,
            maxX = normalPoints[0].x,
            maxY = normalPoints[0].y;

        for (int i = 1; i < normalPoints.Count; i++)
        {
            if (minX > normalPoints[i].x)
            {
                minX = normalPoints[i].x;
            }
            if (maxX < normalPoints[i].x)
            {
                maxX = normalPoints[i].x;
            }

            if (minY > normalPoints[i].y)
            {
                minY = normalPoints[i].y;
            }
            if (maxY < normalPoints[i].y)
            {
                maxY = normalPoints[i].y;
            }
        }

        size = new Vector2(maxX - minX, maxY - minY);
    }
}
