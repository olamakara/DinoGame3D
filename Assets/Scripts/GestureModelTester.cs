using UnityEngine;
using Unity.Barracuda;
using System.Collections.Generic;
using UnityEngine.XR;
using System.Globalization;
using System.IO;
using System.Linq;
using System;

public class GestureModelTester : MonoBehaviour
{
    public NNModel onnxModelAsset;
    public string[] classLabels = new string[] { "up", "down", "left", "right" };

    public XRNode trackedNode = XRNode.RightHand;
    public float triggerThreshold = 0.1f;
    public int targetFrames = 40;

    private bool isRecording = false;
    private bool wasTriggerPressed = false;
    private int frameId = 0;

    // Now we record full "frames" with extra data
    private List<GestureFrame> recordingFrames = new List<GestureFrame>();

    public GameObject dino;

    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(trackedNode);
        if (!device.isValid) return;

        bool triggerPressed = device.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue) && triggerValue > triggerThreshold;

        if (triggerPressed && !wasTriggerPressed)
        {
            StartRecording();
        }
        else if (!triggerPressed && wasTriggerPressed)
        {
            StopRecordingAndPredict();
        }

        if (isRecording)
        {
            if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position) &&
                device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
            {
                // Get button states
                bool primaryButton = device.TryGetFeatureValue(CommonUsages.primaryButton, out bool pb) && pb;
                bool secondaryButton = device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool sb) && sb;
                bool gripButton = device.TryGetFeatureValue(CommonUsages.gripButton, out bool gb) && gb;
                bool triggerButton = triggerPressed;

                GestureFrame frame = new GestureFrame
                {
                    Id = frameId++,
                    Time = Time.time,
                    Position = position,
                    Rotation = rotation,
                    PrimaryButton = primaryButton,
                    SecondaryButton = secondaryButton,
                    GripButton = gripButton,
                    TriggerButton = triggerButton
                };
                recordingFrames.Add(frame);
            }
        }

        wasTriggerPressed = triggerPressed;
    }

    void StartRecording()
    {
        recordingFrames.Clear();
        frameId = 0;
        isRecording = true;
        Debug.Log("[GESTURE] Start recording...");
    }

    void StopRecordingAndPredict()
    {
        isRecording = false;
        Debug.Log("[GESTURE] Stop recording. Predicting...");

        if (recordingFrames.Count < 5)
        {
            Debug.LogWarning("[GESTURE] Too few frames to predict.");
            return;
        }

        // Interpolujemy dane do targetFrames długości
        var interpFrames = InterpolateFrames(recordingFrames, targetFrames);

        // Przygotuj tensor do modelu: na przykład 7 floatów: pos(3), rot(4)
        float[,] data = new float[targetFrames, 7];
        for (int i = 0; i < targetFrames; i++)
        {
            data[i, 0] = interpFrames[i].Position.x;
            data[i, 1] = interpFrames[i].Position.y;
            data[i, 2] = interpFrames[i].Position.z;
            data[i, 3] = interpFrames[i].Rotation.x;
            data[i, 4] = interpFrames[i].Rotation.y;
            data[i, 5] = interpFrames[i].Rotation.z;
            data[i, 6] = interpFrames[i].Rotation.w;
        }

        var model = ModelLoader.Load(onnxModelAsset);
        using (var worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model))
        {
            Tensor inputTensor = new Tensor(1, targetFrames, 7, 1);
            for (int t = 0; t < targetFrames; t++)
                for (int f = 0; f < 7; f++)
                    inputTensor[0, t, f, 0] = data[t, f];

            worker.Execute(inputTensor);
            Tensor output = worker.PeekOutput();
            float[] outputArray = output.ToReadOnlyArray();
            int predicted = ArgMax(outputArray);
            string label = (predicted >= 0 && predicted < classLabels.Length) ? classLabels[predicted] : $"class_{predicted}";
            float confidence = Softmax(outputArray)[predicted];

            Debug.Log($"[PREDICTION] Gesture: {label}, Confidence: {confidence:F2}");

            inputTensor.Dispose();
            output.Dispose();

            OnGesturePredicted(label);
        }
    }

    List<GestureFrame> InterpolateFrames(List<GestureFrame> input, int targetLen)
    {
        List<GestureFrame> result = new List<GestureFrame>(new GestureFrame[targetLen]);

        // Interpolujemy pozycje
        List<Vector3> positions = input.Select(f => f.Position).ToList();
        var interpPos = InterpolatePositions(positions, targetLen);

        // Interpolujemy rotacje
        List<Quaternion> rotations = input.Select(f => f.Rotation).ToList();
        var interpRot = InterpolateQuaternions(rotations, targetLen);

        // Interpolujemy stany przycisków (binary) - np. bierzemy najbliższy lub majority vote
        bool[] primaries = input.Select(f => f.PrimaryButton).ToArray();
        bool[] secondaries = input.Select(f => f.SecondaryButton).ToArray();
        bool[] grips = input.Select(f => f.GripButton).ToArray();
        bool[] triggers = input.Select(f => f.TriggerButton).ToArray();

        // Najprostsze podejście: bierzemy stan z najbliższego klatki (bez interpolacji)
        float[] tOrig = Enumerable.Range(0, input.Count).Select(i => (float)i / (input.Count - 1)).ToArray();
        float[] tNew = Enumerable.Range(0, targetLen).Select(i => (float)i / (targetLen - 1)).ToArray();

        for (int i = 0; i < targetLen; i++)
        {
            int idx = Array.FindLastIndex(tOrig, x => x <= tNew[i]);
            if (idx >= primaries.Length) idx = primaries.Length - 1;

            result[i] = new GestureFrame
            {
                Position = interpPos[i],
                Rotation = interpRot[i],
                PrimaryButton = primaries[idx],
                SecondaryButton = secondaries[idx],
                GripButton = grips[idx],
                TriggerButton = triggers[idx]
            };
        }
        return result;
    }

    List<Vector3> InterpolatePositions(List<Vector3> input, int targetLen)
    {
        // Twoja implementacja interpolacji pozycji, możesz skopiować z oryginału
        List<Vector3> result = new List<Vector3>(new Vector3[targetLen]);
        float[] tOrig = Enumerable.Range(0, input.Count).Select(i => (float)i / (input.Count - 1)).ToArray();
        float[] tNew = Enumerable.Range(0, targetLen).Select(i => (float)i / (targetLen - 1)).ToArray();

        for (int d = 0; d < 3; d++)
        {
            float[] orig = input.Select(v => d == 0 ? v.x : d == 1 ? v.y : v.z).ToArray();
            float[] interp = new float[targetLen];

            for (int i = 0; i < targetLen; i++)
            {
                float t = tNew[i];
                int idx = Array.FindLastIndex(tOrig, x => x <= t);
                if (idx >= tOrig.Length - 1) idx = tOrig.Length - 2;

                float t0 = tOrig[idx], t1 = tOrig[idx + 1];
                float v0 = orig[idx], v1 = orig[idx + 1];
                interp[i] = Mathf.Lerp(v0, v1, (t - t0) / (t1 - t0));
            }

            for (int i = 0; i < targetLen; i++)
            {
                Vector3 temp = result[i];
                if (d == 0) temp.x = interp[i];
                else if (d == 1) temp.y = interp[i];
                else temp.z = interp[i];
                result[i] = temp;
            }
        }

        return result;
    }

    List<Quaternion> InterpolateQuaternions(List<Quaternion> input, int targetLen)
    {
        // Twoja implementacja interpolacji rotacji, możesz skopiować z oryginału
        List<Quaternion> result = new List<Quaternion>();
        float[] tOrig = Enumerable.Range(0, input.Count).Select(i => (float)i / (input.Count - 1)).ToArray();
        float[] tNew = Enumerable.Range(0, targetLen).Select(i => (float)i / (targetLen - 1)).ToArray();

        for (int i = 0; i < targetLen; i++)
        {
            float t = tNew[i];
            int idx = Array.FindLastIndex(tOrig, x => x <= t);
            if (idx >= tOrig.Length - 1) idx = tOrig.Length - 2;

            Quaternion q0 = input[idx], q1 = input[idx + 1];
            float t0 = tOrig[idx], t1 = tOrig[idx + 1];
            float lerpT = (t - t0) / (t1 - t0);
            result.Add(Quaternion.Slerp(q0, q1, lerpT));
        }

        return result;
    }

    int ArgMax(float[] array)
    {
        int bestIndex = 0;
        float bestValue = array[0];
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] > bestValue)
            {
                bestValue = array[i];
                bestIndex = i;
            }
        }
        return bestIndex;
    }

    float[] Softmax(float[] logits)
    {
        float max = logits.Max();
        float sum = logits.Select(v => Mathf.Exp(v - max)).Sum();
        return logits.Select(v => Mathf.Exp(v - max) / sum).ToArray();
    }

    void OnGesturePredicted(string label)
    {
        if (label == "left")
            dino.SendMessage("MoveLeft");
        else if (label == "right")
            dino.SendMessage("MoveRight");
        else if (label == "up")
            dino.SendMessage("Jump");
        else if (label == "down")
            dino.SendMessage("Bend");
        else
            Debug.LogWarning($"[UNKNOWN GESTURE] {label}");
    }

    // Prosta struktura na pojedynczą ramkę nagrania
    public class GestureFrame
    {
        public int Id;
        public float Time;
        public Vector3 Position;
        public Quaternion Rotation;
        public bool PrimaryButton;
        public bool SecondaryButton;
        public bool GripButton;
        public bool TriggerButton;
    }
}
