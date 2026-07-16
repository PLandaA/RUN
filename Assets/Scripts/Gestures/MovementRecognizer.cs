using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using PDollarGestureRecognizer;
using Unity.XR.CoreUtils;

public class MovementRecognizer : MonoBehaviour
{
    public XRNode inputSource;
    public InputHelpers.Button inputButton;
    public float inputThreshold = 0.1f;
    public Transform movementSource;
    public Camera cam;

    public float newPositionThresholdDistance = 0.05f;
    public GameObject debugCubePrefab;
    public bool creationMode = false;
    public string newGestureName;

    public float recognitionThreshold = 0.5f;

    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string> { }
    public UnityStringEvent onRecognized;

    private List<Gesture> trainingSet = new List<Gesture>();
    private bool isMoving = false;
    private List<Vector3> positionList = new List<Vector3>();
    private int movingPoints = 0;
    [SerializeField]
    private int pointsToMove = 15;


    public float speed = 1;

    public float gravity = -9.81f;
    public LayerMask groundLayer;
    public float additionalHeight = 0.2f;
    private float fallingSpeed;
    private XROrigin rig;
    private CharacterController character;


    void Start()
    {
        // Bundled gestures (StreamingAssets) plus locally trained ones (persistent data path).
        string bundledGesturePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Gestures");
        if (Directory.Exists(bundledGesturePath))
        {
            foreach (var item in Directory.GetFiles(bundledGesturePath, "*.xml"))
            {
                trainingSet.Add(GestureIO.ReadGestureFromFile(item));
            }
        }

        string[] gestureFiles = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        foreach (var item in gestureFiles)
        {
            trainingSet.Add(GestureIO.ReadGestureFromFile(item));
        }

        if (trainingSet.Count == 0 && !creationMode)
        {
            Debug.LogWarning("MovementRecognizer: no gesture files found. Recognition will be skipped until gestures exist in StreamingAssets/Gestures or the persistent data path.");
        }

        character = GetComponent<CharacterController>();
        rig = GetComponent<XROrigin>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(inputSource), inputButton, out bool isPressed, inputThreshold);
        if (!isMoving && isPressed)
        {
            StartMovemenet();
        }
        else if (isMoving && isPressed && movingPoints < pointsToMove)
        {
            UpdateMovemnet();
        }
        else if (isMoving && (!isPressed || movingPoints >= pointsToMove))
        {
            EndMovement();
        }
    }

    void StartMovemenet()
    {
        isMoving = true;
        positionList.Clear();
        positionList.Add(movementSource.position);
        movingPoints++;
        if (debugCubePrefab)
        {
            Destroy(Instantiate(debugCubePrefab, movementSource.position, Quaternion.identity), 3);
        }
    }

    public void EndMovement()
    {
        isMoving = false;
        movingPoints = 0;

        Point[] pointArray = new Point[positionList.Count];
        for (int i = 0; i < positionList.Count; i++)
        {
            Vector2 screenPoints = cam.WorldToScreenPoint(positionList[i]);
            pointArray[i] = new Point(screenPoints.x, screenPoints.y, 0);
        }

        Gesture newGesture = new Gesture(pointArray);

        if (creationMode)
        {
            // Dev-only mode: record the stroke as a new named gesture.
            newGesture.Name = newGestureName;
            trainingSet.Add(newGesture);

            string fileName = Application.persistentDataPath + "/" + newGestureName + ".xml";
            GestureIO.WriteGesture(pointArray, newGestureName, fileName);
            return;
        }

        if (trainingSet.Count == 0)
        {
            return; // Nothing to classify against.
        }

        // $P's resampling breaks on nearly-empty or static strokes.
        if (positionList.Count < 10)
        {
            return;
        }

        Bounds strokeBounds = new Bounds(pointArray[0].X * Vector3.right + pointArray[0].Y * Vector3.up, Vector3.zero);
        for (int i = 1; i < pointArray.Length; i++)
        {
            strokeBounds.Encapsulate(new Vector3(pointArray[i].X, pointArray[i].Y, 0f));
        }
        if (strokeBounds.size.magnitude < 20f) // ~20 px on screen
        {
            return;
        }

        Result result = PointCloudRecognizer.Classify(newGesture, trainingSet.ToArray());

        if (result.Score < recognitionThreshold)
        {
            return;
        }

        if (result.GestureClass.ToLower() == "walk" && inputButton == InputHelpers.Button.PrimaryButton)
        {
            characterMovement();
        }
        else if (result.GestureClass.ToLower() == "lamp" && inputButton == InputHelpers.Button.SecondaryButton)
        {
            GameManager.timeFlashLight = Mathf.Min(GameManager.timeFlashLight + 5f, GameManager.maxFlashlightTime);
        }
    }

    void UpdateMovemnet()
    {
        Vector3 lastPosition = positionList[positionList.Count - 1];

        if (Vector3.Distance(movementSource.position, lastPosition) > newPositionThresholdDistance)
        {
            positionList.Add(movementSource.position);
            movingPoints++;
            if (debugCubePrefab)
            {
                Destroy(Instantiate(debugCubePrefab, movementSource.position, Quaternion.identity), 3);
            }
        }

    }
    public string getNameGesture()
    {
        return newGestureName;
    }

    private void characterMovement()
    {
        // The controller can be disabled during scene transitions.
        if (character == null || !character.enabled || !character.gameObject.activeInHierarchy)
        {
            return;
        }

        // One step per recognized gesture; gravity is handled by ContinuosMovement.
        CapsuleFollowHeadset();
        Vector3 direction = rig.Camera.transform.forward;
        direction.y = 0f;
        direction.Normalize();

        character.Move(direction * speed);
    }

    void CapsuleFollowHeadset()
    {
        character.height = rig.CameraInOriginSpaceHeight + additionalHeight;
        Vector3 capsuleCenter = transform.InverseTransformPoint(rig.Camera.transform.position);
        character.center = new Vector3(capsuleCenter.x, character.height / 2 + character.skinWidth, capsuleCenter.z);
    }

    bool CheckifGrounded()
    {
        Vector3 rayStart = transform.TransformPoint(character.center);
        float rayLenght = character.center.y + 0.1f;
        bool hasHit = Physics.SphereCast(rayStart, character.radius, Vector3.down, out RaycastHit hitInfo, rayLenght, groundLayer);
        return hasHit;
    }
}




