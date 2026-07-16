using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

/// <summary>
/// Device-based snap turn, replacing the deprecated XRI DeviceBasedSnapTurnProvider.
/// </summary>
public class SnapTurn : MonoBehaviour
{
    public XRNode inputSource = XRNode.RightHand;
    public float turnAmount = 45f;
    [Range(0.1f, 0.95f)] public float deadZone = 0.75f;
    public float debounceTime = 0.5f;

    private XROrigin origin;
    private float lastTurnTime;
    private bool stickCentered = true;

    void Start()
    {
        origin = GetComponent<XROrigin>();
    }

    void Update()
    {
        InputDevices.GetDeviceAtXRNode(inputSource).TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis);

        float x = axis.x;

        if (Mathf.Abs(x) < deadZone * 0.5f)
        {
            stickCentered = true;
            return;
        }

        if (Mathf.Abs(x) >= deadZone && stickCentered && Time.time - lastTurnTime >= debounceTime)
        {
            float angle = Mathf.Sign(x) * turnAmount;

            // Rotate around the camera so the player doesn't swing sideways.
            if (origin != null)
            {
                origin.RotateAroundCameraUsingOriginUp(angle);
            }
            else
            {
                transform.Rotate(0f, angle, 0f);
            }

            lastTurnTime = Time.time;
            stickCentered = false;
        }
    }
}
