using UnityEngine;
using UnityEngine.XR;

public class ControllerTracker : MonoBehaviour
{
    public XRNode controllerNode = XRNode.RightHand; // or XRNode.LeftHand

    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);
        if (device.isValid)
        {
            if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position) &&
                device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
            {
                // Update this GameObject's transform to match the controller
                transform.localPosition = position;
                transform.localRotation = rotation;
            }
        }
    }
}
