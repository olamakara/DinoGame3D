using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit;

public class MoionRecorder : MonoBehaviour
{
    public TextMeshPro debugText;
    UnityEngine.XR.InputDevice targetDeviceRight;
    UnityEngine.XR.InputDevice targetDeviceLeft;
    UnityEngine.XR.InputDevice targetDeviceHead;

    ControllerActionEventHendler controllerActionEventHendler = null;
    // Start is called before the first frame update
    void Start()
    {
        DirectoryUtils.PathToDir = Application.persistentDataPath;
        List<UnityEngine.XR.InputDevice> devices
            = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDeviceCharacteristics rightControllerChar
            = UnityEngine.XR.InputDeviceCharacteristics.Right
            | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(rightControllerChar,
            devices);

        if (devices.Count > 0)
            targetDeviceRight = devices[0];

        UnityEngine.XR.InputDeviceCharacteristics leftControllerChar
            = UnityEngine.XR.InputDeviceCharacteristics.Left
            | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(leftControllerChar,
            devices);

        if (devices.Count > 0)
            targetDeviceLeft = devices[0];

        UnityEngine.XR.InputDeviceCharacteristics headControllerChar
            = UnityEngine.XR.InputDeviceCharacteristics.HeadMounted;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(headControllerChar,
            devices);

        if (devices.Count > 0)
            targetDeviceHead = devices[0];

        controllerActionEventHendler = new ControllerActionEventHendler();
        controllerActionEventHendler.targetDeviceHead = targetDeviceHead;
        controllerActionEventHendler.targetDeviceLeft = targetDeviceLeft;
        controllerActionEventHendler.targetDeviceRight = targetDeviceRight;
    }


    private int a = 0;
    // Update is called once per frame
    void Update()
    {
        controllerActionEventHendler.UpdateFrame();
        bool click = controllerActionEventHendler.IsClicked(
                ControllerActionEventHendler.ControllerEnum.right,
                ControllerActionEventHendler.ButtonEnum.trigger,
                true);
        if (click)
        {
            // Data will be saved to Download folder on device
            string myPath = "storage/emulated/0/Download/motions"
                + a + ".csv";
            debugText.text = myPath;
            controllerActionEventHendler.SaveAll(myPath);
            controllerActionEventHendler.ClearAll();
            a++;
        }
    }
}
