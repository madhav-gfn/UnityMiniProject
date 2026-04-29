using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class SpellEvent : UnityEvent<int, Vector3, Quaternion> { }

[System.Serializable]
public class DebugTextEvent : UnityEvent<string> { }

public interface IGestureInput
{
    bool StartPressed();
    bool EndPressed();
    Vector3 Position();
}

public class MouseInput : IGestureInput
{
    public float planeDist;
    private Camera cam;

    public MouseInput(Camera camera, float distance)
    {
        cam = camera;
        planeDist = distance;
    }

    public bool StartPressed()
    {
        try { return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Z); } catch { return false; }
    }

    public bool EndPressed()
    {
        try { return Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Z); } catch { return false; }
    }

    public Vector3 Position()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return Vector3.zero;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(cam.transform.forward, cam.transform.position + cam.transform.forward * planeDist);
        
        if (plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return Vector3.zero;
    }
}

public class XRInput : IGestureInput
{
    private Transform controller;
    private bool lastState = false;
    private bool currentState = false;
    private int lastFrame = -1;

    public XRInput(Transform controllerTransform)
    {
        controller = controllerTransform;
    }

    private void RefreshState()
    {
        if (Time.frameCount == lastFrame) return;
        lastFrame = Time.frameCount;
        lastState = currentState;
        currentState = false;

        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
        if (devices.Count > 0)
        {
            if (devices[0].TryGetFeatureValue(CommonUsages.triggerButton, out bool btn) && btn)
                currentState = true;
            else if (devices[0].TryGetFeatureValue(CommonUsages.trigger, out float val) && val > 0.5f)
                currentState = true;
        }

        if (!currentState)
        {
            try { currentState = Input.GetButton("Fire1") || Input.GetKey(KeyCode.Z); } catch { }
        }
    }

    public bool StartPressed()
    {
        RefreshState();
        return currentState && !lastState;
    }

    public bool EndPressed()
    {
        RefreshState();
        return !currentState && lastState;
    }

    public Vector3 Position()
    {
        return controller != null ? controller.position : Vector3.zero;
    }
}

public class DirectionalGestureCaster : MonoBehaviour
{
    public enum InputMode { Mouse, XR }

    [Header("Input Settings")]
    public InputMode inputMode = InputMode.Mouse;
    public float planeDistance = 1.0f;
    public Transform xrController;
    [Tooltip("Leave empty to use Camera.main. Assign your XR Rig's CenterEyeAnchor or Main Camera here.")]
    public Camera headCamera;

    [Header("Gesture Parameters")]
    public float minDistance = 0.05f;
    public float confidenceThreshold = 0.7f;
    public float maxGestureTime = 0.3f;

    [Header("Testing")]
    public bool enableNoise = false;
    public float noiseAmount = 0.01f;
    [Tooltip("Drag the Text object from your DebugText canvas here")]
    public Text debugTextUI;

    [Header("Spell Prefabs")]
    public GameObject firePrefab;
    public GameObject icePrefab;
    public GameObject shieldPrefab;
    public GameObject lightningPrefab;

    [Header("Network Events")]
    public SpellEvent OnSpellCastEvent;
    public DebugTextEvent OnDebugTextChanged;

    private IGestureInput gestureInput;
    private MouseInput mouseInput;
    private XRInput xrInput;

    private bool isDragging = false;
    private Vector3 startPos;
    private Vector3 currentPos;
    private float gestureStartTime;
    private string lastDetectedDirection = "None";

    void Start()
    {
        Camera cam = headCamera != null ? headCamera : Camera.main;
        mouseInput = new MouseInput(cam, planeDistance);
        xrInput = new XRInput(xrController);
    }

    void Update()
    {
        mouseInput.planeDist = planeDistance;
        gestureInput = inputMode == InputMode.Mouse ? (IGestureInput)mouseInput : xrInput;

        if (gestureInput.StartPressed())
        {
            StartGesture();
        }
        else if (isDragging)
        {
            UpdateGesture();
            if (gestureInput.EndPressed())
            {
                EndGesture();
            }
        }

        if (isDragging)
        {
            Debug.DrawLine(startPos, currentPos, Color.yellow);
        }

        UpdateDebugUI();
    }

    private void UpdateDebugUI()
    {
        string debugString = $"Direction: {lastDetectedDirection}\n" +
                             $"Mode: {inputMode}\n" +
                             $"Dragging: {isDragging}\n" +
                             $"Start: {startPos}\n" +
                             $"Current: {currentPos}";

        if (debugTextUI != null)
        {
            debugTextUI.text = debugString;
        }

        // Broadcast to network/UI listeners
        OnDebugTextChanged?.Invoke(debugString);
    }

    private void StartGesture()
    {
        isDragging = true;
        startPos = GetPosition();
        currentPos = startPos;
        gestureStartTime = Time.time;
        lastDetectedDirection = "Casting...";
    }

    private void UpdateGesture()
    {
        currentPos = GetPosition();

        if (Time.time - gestureStartTime > maxGestureTime)
        {
            isDragging = false;
            lastDetectedDirection = "Timeout";
            return;
        }

        Vector3 diff = currentPos - startPos;
        if (diff.magnitude > minDistance)
        {
            Vector3 dir = diff.normalized;
            Transform camTrans = headCamera != null ? headCamera.transform : (Camera.main != null ? Camera.main.transform : transform);

            float dotUp = Vector3.Dot(dir, camTrans.up);
            float dotDown = Vector3.Dot(dir, -camTrans.up);
            float dotRight = Vector3.Dot(dir, camTrans.right);
            float dotLeft = Vector3.Dot(dir, -camTrans.right);

            if (dotUp > confidenceThreshold) TriggerCast("Up");
            else if (dotDown > confidenceThreshold) TriggerCast("Down");
            else if (dotRight > confidenceThreshold) TriggerCast("Right");
            else if (dotLeft > confidenceThreshold) TriggerCast("Left");
        }
    }

    private void EndGesture()
    {
        isDragging = false;
        if (lastDetectedDirection == "Casting...")
        {
            lastDetectedDirection = "Too small";
        }
    }

    private void TriggerCast(string direction)
    {
        lastDetectedDirection = direction;
        isDragging = false;

        switch (direction)
        {
            case "Up": Fire(); break;
            case "Down": Ice(); break;
            case "Left": Shield(); break;
            case "Right": Lightning(); break;
        }
    }

    private Vector3 GetPosition()
    {
        Vector3 pos = gestureInput.Position();
        if (enableNoise)
        {
            pos += UnityEngine.Random.insideUnitSphere * noiseAmount;
        }
        return pos;
    }

    private void SpawnSpell(GameObject prefab, string spellName, int spellId)
    {
        Debug.Log("Casting: " + spellName);
        if (prefab == null) return;
        
        // Shoot from the controller if in XR, otherwise from the camera
        Transform spawnPoint = inputMode == InputMode.XR && xrController != null ? xrController : 
                               (headCamera != null ? headCamera.transform : transform);
        
        // Spawn slightly in front of the hand/camera
        Vector3 spawnPos = spawnPoint.position + spawnPoint.forward * 0.2f;
        Instantiate(prefab, spawnPos, spawnPoint.rotation);

        // Broadcast to network listeners
        OnSpellCastEvent?.Invoke(spellId, spawnPos, spawnPoint.rotation);
    }

    private void Fire() => SpawnSpell(firePrefab, "Fire! (Up)", 0);
    private void Ice() => SpawnSpell(icePrefab, "Ice! (Down)", 1);
    private void Shield() => SpawnSpell(shieldPrefab, "Shield! (Left)", 2);
    private void Lightning() => SpawnSpell(lightningPrefab, "Lightning! (Right)", 3);

    void OnDrawGizmos()
    {
        if (isDragging)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPos, 0.02f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentPos, 0.02f);
        }
    }

    // OnGUI removed. Debug info is now routed to the debugTextUI canvas.
}
