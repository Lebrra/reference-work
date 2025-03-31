using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

// Original script taken from https://dineshpunni.notion.site/Drawing-in-XR-2a2b46869e6f46c589092045a86e8a0a
// Video followed: https://www.youtube.com/watch?v=JZFfKTYSt7k
public class XRHandDraw : MonoBehaviour
{
    [SerializeField] private InputDevice trackingHand;

    [SerializeField] private float minDistanceBeforeNewPoint = 0.008f;

    [SerializeField] private float tubeDefaultWidth = 0.010f;
    [SerializeField] private int tubeSides = 8;

    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Material defaultLineMaterial;

    private Vector3 prevPointDistance = Vector3.zero;

    private List<Vector3> points = new List<Vector3>();
    private List<TubeRenderer> tubeRenderers = new List<TubeRenderer>();

    private TubeRenderer currentTubeRenderer;

    [SerializeField]    // todo: assuming a wand will be the draw point, so not adding one for left and right hands
    private Transform drawPoint = null;

    [SerializeField, Header("TESTING")]
    private bool isDrawing = false;
    bool wasDrawing = false;

    private void Start()
    {
        // todo: don't hardcode to right hand
        trackingHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        
        // todo: figure out why the first drawing doesn't work well
        AddNewTubeRenderer();
        Destroy(currentTubeRenderer.gameObject);
    }

    private void Update()
    {
        CheckDrawState();
    }

    private void AddNewTubeRenderer()
    {
        points.Clear();
        GameObject go = new GameObject($"TubeRenderer__{tubeRenderers.Count}");
        go.transform.position = Vector3.zero;

        TubeRenderer goTubeRenderer = go.AddComponent<TubeRenderer>();
        tubeRenderers.Add(goTubeRenderer);

        var renderer = go.GetComponent<MeshRenderer>();
        renderer.material = defaultLineMaterial;

        goTubeRenderer.SetPositions(points.ToArray());
        goTubeRenderer.radiusOne = tubeDefaultWidth;
        goTubeRenderer.radiusTwo = tubeDefaultWidth;
        goTubeRenderer.sides = tubeSides;

        currentTubeRenderer = goTubeRenderer;
    }

    private void CheckDrawState()
    {
        if (drawPoint == null)
            return;

        // if assignment failed try again (todo: don't hardcode right hand)
        if (!trackingHand.isValid) trackingHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        
        if (trackingHand.isValid) trackingHand.TryGetFeatureValue(CommonUsages.triggerButton, out isDrawing);
        else
        {
#if UNITY_EDITOR
            
            isDrawing = UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
            if (isDrawing)
            {
                var pos = UnityEngine.InputSystem.Mouse.current.position;
                drawPoint.transform.position = Camera.main.ScreenToWorldPoint((Vector3)pos.value + Vector3.forward);
            }
            else
            {
                drawPoint.transform.localPosition = Vector3.zero;
            }
#else
            return;
#endif
        }

        if (isDrawing)
        {
            if (!wasDrawing)    // drawing started!
            {
                AddNewTubeRenderer();
            }

            UpdateTube();
        }
        if (!isDrawing && wasDrawing)   // drawing ended!
        {
            currentTubeRenderer.OnDrawingFinished();
        }

        // update previous drawing state:
        wasDrawing = isDrawing;
    }

    private void UpdateTube()
    {
        if (prevPointDistance == Vector3.zero)
        {
            prevPointDistance = drawPoint.transform.position;
        }

        if (Vector3.Distance(prevPointDistance, drawPoint.transform.position) >= minDistanceBeforeNewPoint)
        {
            prevPointDistance = drawPoint.transform.position;
            AddPoint(prevPointDistance);
        }
    }

    private void AddPoint(Vector3 position)
    {
        points.Add(position);
        currentTubeRenderer.SetPositions(points.ToArray());
        currentTubeRenderer.GenerateMesh();
    }
}