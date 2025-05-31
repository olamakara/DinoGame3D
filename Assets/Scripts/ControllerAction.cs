using System.Globalization;
using UnityEngine;
class ControllerAction{
    public bool primaryButton = false;//A
    public bool primaryTouch = false;
    public bool secondaryButton = false;//B
    public bool secondaryTouch = false;//B
    public bool triggerButton = false;//trigger
    public float triggerButtonForce = 0;
    public Vector2 primary2DAxis;//joystick position
    public bool primary2DAxisTouch = false;
    public bool primary2DAxisClick = false;//joystick click

    public Vector2 secondary2DAxis;//unused
    public bool secondary2DAxisTouch = false;//unused
    public bool secondary2DAxisClick = false;//unused

    public bool gripButton = false;//trigger
    public float gripButtonForce = 0;
    public bool isTracked = false;
    public Vector3 devicePosition;
    public Quaternion deviceRotation;
    public Vector3 deviceDirection;

    public ControllerAction(UnityEngine.XR.InputDevice targetDevice)
    {
        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton,
            out primaryButton);
        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryTouch,
            out primaryTouch);

        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton,
            out secondaryButton);
        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryTouch,
            out secondaryTouch);

        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton,
            out triggerButton);
        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger,
            out triggerButtonForce);
        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis,
            out primary2DAxis);
        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick,
            out primary2DAxisClick);
        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisTouch,
            out primary2DAxisTouch);

        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondary2DAxis,
            out secondary2DAxis);
        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondary2DAxisClick,
            out secondary2DAxisClick);
        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondary2DAxisTouch,
            out secondary2DAxisTouch);

        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton,
            out gripButton);

        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip,
            out gripButtonForce);

        
        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition,
            out devicePosition);

        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation,
            out deviceRotation);

        targetDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked,
            out isTracked);
        
        deviceDirection = new Vector3(0,0,1);
        deviceDirection = deviceRotation * deviceDirection;
    }

    override public string ToString()
    {
        string separator = ";";
        CultureInfo culture = CultureInfo.InvariantCulture;
        return ToString(separator, culture);
    }

    public string ToString(string separator,
        CultureInfo culture)
    {
        string returnValue = "";
        returnValue += primaryButton.ToString(culture) + separator
                    + primaryTouch.ToString(culture) + separator
                    + secondaryButton.ToString(culture) + separator
                    + secondaryTouch.ToString(culture) + separator
                    + triggerButton.ToString(culture) + separator
                    + triggerButtonForce.ToString(culture) + separator
                    + primary2DAxis.x.ToString(culture) + separator
                    + primary2DAxis.y.ToString(culture) + separator
                    + primary2DAxisTouch.ToString(culture) + separator
                    + primary2DAxisClick.ToString(culture) + separator
                    + secondary2DAxis.x.ToString(culture) + separator
                    + secondary2DAxis.y.ToString(culture) + separator
                    + secondary2DAxisTouch.ToString(culture) + separator
                    + secondary2DAxisClick.ToString(culture) + separator
                    + gripButton.ToString(culture) + separator
                    + gripButtonForce.ToString(culture) + separator
                    + isTracked.ToString(culture) + separator
                    + devicePosition.x.ToString(culture) + separator
                    + devicePosition.y.ToString(culture) + separator
                    + devicePosition.z.ToString(culture) + separator
                    + deviceRotation.w.ToString(culture) + separator
                    + deviceRotation.x.ToString(culture) + separator
                    + deviceRotation.y.ToString(culture) + separator
                    + deviceRotation.z.ToString(culture) + separator
                    + deviceDirection.x.ToString(culture) + separator
                    + deviceDirection.y.ToString(culture) + separator
                    + deviceDirection.z.ToString(culture);
        return returnValue;
    }

    public string[]GetHeader(string prefix = "", string suffix = "")
    {
        string[]header = {"primaryButton", "primaryTouch", "secondaryButton",
            "secondaryTouch", "triggerButton", "triggerButtonForce",
            "primary2DAxis.x","primary2DAxis.y", 
            "primary2DAxisTouch", "primary2DAxisClick",
            "secondary2DAxis.x","secondary2DAxis.y", 
            "secondary2DAxisTouch", "secondary2DAxisClick",
            "gripButton", "gripButtonForce", "isTracked", 
            "devicePosition.x","devicePosition.y","devicePosition.z", 
            "deviceRotation.w","deviceRotation.x","deviceRotation.y","deviceRotation.z", 
            "deviceDirection.x","deviceDirection.y","deviceDirection.z"};
        for (int a = 0; a < header.Length; a++)
        {
            header[a] = prefix + header[a] + suffix;
        }
        return header;
    }
}
