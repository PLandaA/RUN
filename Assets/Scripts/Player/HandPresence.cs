using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandPresence : MonoBehaviour
{
    public bool showController = false;

    [Tooltip("Compensates the grip-pose rotation difference between the legacy Oculus plugin and OpenXR. Tune live in Play mode if hands feel tilted.")]
    public Vector3 openXRRotationOffset = new Vector3(60f, 0f, 0f);

    [Tooltip("Fine position adjustment so the hand sits where the controller is actually held. Tune live in Play mode.")]
    public Vector3 openXRPositionOffset = Vector3.zero;
    public InputDeviceCharacteristics controllerChracteristics;
    public List<GameObject> controllerPrefabs;
    public GameObject handModelPrefab;

    private InputDevice targetDevice;
    private GameObject spawnedController;
    private GameObject spawnedHandModel;
    private Animator handAnimator;

    // Start is called before the first frame update
    void Start()
    {
        TryInitialize();
    }

    void TryInitialize()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(controllerChracteristics, devices);

        if (devices.Count > 0)
        {
            targetDevice = devices[0];
            GameObject prefab = controllerPrefabs.Find(controller => controller != null && controller.name == targetDevice.name);

            if (prefab)
            {
                spawnedController = Instantiate(prefab, transform);
                spawnedController.transform.localRotation = Quaternion.Euler(openXRRotationOffset);
                spawnedController.transform.localPosition = openXRPositionOffset;
            }
            else
            {
                GameObject fallback = controllerPrefabs.Find(controller => controller != null);
                if (fallback != null)
                {
                    Debug.LogWarning("HandPresence: no controller model matches '" + targetDevice.name + "'. Using default model.");
                    spawnedController = Instantiate(fallback, transform);
                    spawnedController.transform.localRotation = Quaternion.Euler(openXRRotationOffset);
                    spawnedController.transform.localPosition = openXRPositionOffset;
                }
            }

            if (handModelPrefab)
            {
                spawnedHandModel = Instantiate(handModelPrefab, transform);
                spawnedHandModel.transform.localRotation = Quaternion.Euler(openXRRotationOffset);
                spawnedHandModel.transform.localPosition = openXRPositionOffset;
                handAnimator = spawnedHandModel.GetComponent<Animator>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Keep retrying until the device is available (late connection or XR still initializing).
        if (!targetDevice.isValid)
        {
            TryInitialize();
            return;
        }

        if (showController)
        {
            if (spawnedHandModel) spawnedHandModel.SetActive(false);
            if (spawnedController) spawnedController.SetActive(true);
        }
        else
        {
            if (spawnedHandModel) spawnedHandModel.SetActive(true);
            if (spawnedController) spawnedController.SetActive(false);
            if (handAnimator) UpdateHandAnimator();
        }
    }

    void UpdateHandAnimator()
    {
        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Trigger", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Trigger", 0);
        }
        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Grip", gripValue);
        }
        else
        {
            handAnimator.SetFloat("Grip", 0);
        }
    }
}
