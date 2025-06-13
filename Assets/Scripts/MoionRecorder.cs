using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using System.Globalization;

//using UnityEngine.XR.Interaction.Toolkit;

public class MoionRecorder : MonoBehaviour
{
    public TextMeshPro debugText;
    UnityEngine.XR.InputDevice targetDeviceRight;
    UnityEngine.XR.InputDevice targetDeviceLeft;
    UnityEngine.XR.InputDevice targetDeviceHead;

    ControllerActionEventHendler controllerActionEventHendler = null;
    List<ControllerActionFrame> recordingFrames = new List<ControllerActionFrame>();
    private bool wasTriggerPressed = false;
    private int fileIndex = 0;

    void RequestPermissions()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
    }

    void Start()
    {
        RequestPermissions();
        DirectoryUtils.PathToDir = Application.persistentDataPath;

        var devices = new List<UnityEngine.XR.InputDevice>();

        var rightControllerChar = UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(rightControllerChar, devices);
        if (devices.Count > 0) targetDeviceRight = devices[0];

        var leftControllerChar = UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(leftControllerChar, devices);
        if (devices.Count > 0) targetDeviceLeft = devices[0];

        var headControllerChar = UnityEngine.XR.InputDeviceCharacteristics.HeadMounted;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(headControllerChar, devices);
        if (devices.Count > 0) targetDeviceHead = devices[0];

        controllerActionEventHendler = new ControllerActionEventHendler
        {
            targetDeviceHead = targetDeviceHead,
            targetDeviceLeft = targetDeviceLeft,
            targetDeviceRight = targetDeviceRight
        };
    }

    void Update()
    {
        try
        {
            controllerActionEventHendler.UpdateFrame();

            // Check current trigger button state
            var currentFrame = controllerActionEventHendler.GetCurrent();
            bool isTriggerPressed = currentFrame?.caR.triggerButton ?? false;

            if (isTriggerPressed)
            {
                // While holding the trigger, accumulate frames
                var clone = new ControllerActionFrame
                {
                    Id = currentFrame.Id,
                    Time = currentFrame.Time,
                    caL = currentFrame.caL,
                    caR = currentFrame.caR,
                    caH = currentFrame.caH
                };
                recordingFrames.Add(clone);
            }

            // Detect release of trigger (was pressed, now not)
            if (wasTriggerPressed && !isTriggerPressed)
            {
                if (recordingFrames.Count > 0)
                {
                    string myPath = $"storage/emulated/0/Download/down/motions{fileIndex}.csv";
                    debugText.text = $"Saved: {myPath}";
                    SaveRecording(myPath);
                    fileIndex++;
                }

                recordingFrames.Clear();
            }

            wasTriggerPressed = isTriggerPressed;
        }
        catch (Exception ex)
        {
            debugText.text = "Error in Update(): " + ex.Message;
            Debug.LogError("Error in Update(): " + ex.ToString());
        }
    }

    void SaveRecording(string path)
    {
        string separator = ";";
        CultureInfo culture = CultureInfo.InvariantCulture;

        try
        {
            using (StreamWriter file = new StreamWriter(path, false))
            {
                // Header
                string resultText = "Id" + separator + "Time" + separator;
                string[] header = recordingFrames[0].caL.GetHeader("L", "");
                foreach (var h in header) resultText += h + separator;
                header = recordingFrames[0].caR.GetHeader("R", "");
                foreach (var h in header) resultText += h + separator;
                header = recordingFrames[0].caH.GetHeader("H", "");
                foreach (var h in header) resultText += h + separator;
                resultText = resultText.Substring(0, resultText.Length - separator.Length);
                file.WriteLine(resultText);

                // Data
                foreach (var frame in recordingFrames)
                {
                    resultText = frame.Id + separator + frame.Time + separator
                               + frame.caL.ToString(separator, culture) + separator
                               + frame.caR.ToString(separator, culture) + separator
                               + frame.caH.ToString(separator, culture);
                    file.WriteLine(resultText);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("SaveRecording() error: " + ex.Message);
        }
    }
}
