using UnityEngine;
using Unity.Barracuda;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.XR;

public class GestureModelTester : MonoBehaviour
{
    public NNModel onnxModelAsset; // Assign your .onnx model in the Inspector
    public string[] classLabels = new string[] { "left", "right", "up", "down" }; // Set to your model's output classes

    public XRNode trackedNode = XRNode.RightHand; // Możesz zmienić na LeftHand
    public bool useLiveVRInput = true; // Jeśli true, pobiera dane z kontrolera VR

    // Neutralny box (środek i rozmiar)
    public Vector3 neutralBoxCenter = new Vector3(0, 1, 0); // ustaw wg pozycji spoczynkowej
    public Vector3 neutralBoxSize = new Vector3(0.3f, 0.3f, 0.3f); // szerokość, wysokość, głębokość
    private bool isInNeutral = true;
    private bool isCollecting = true;
    private int targetFrames = 40;

    private List<Vector3> livePositionsRight = new List<Vector3>();
    private List<Quaternion> liveQuaternionsRight = new List<Quaternion>();
    private List<Vector3> livePositionsLeft = new List<Vector3>();
    private List<Quaternion> liveQuaternionsLeft = new List<Quaternion>();

    void Start()
    {
        // Niepotrzebna logika testowania offline została usunięta
    }

    void Update()
    {
        if (!useLiveVRInput)
            return;

        Vector3 posR = Vector3.zero;
        Vector3 posL = Vector3.zero;

        // Pobierz pozycje kontrolerów
        InputDevice deviceRight = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        InputDevice deviceLeft = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        bool rightValid = deviceRight.isValid && deviceRight.TryGetFeatureValue(CommonUsages.devicePosition, out posR);
        bool leftValid = deviceLeft.isValid && deviceLeft.TryGetFeatureValue(CommonUsages.devicePosition, out posL);
        bool rightOut = rightValid && !IsInNeutralBox(posR);
        bool leftOut = leftValid && !IsInNeutralBox(posL);

        // Start zbierania gdy którykolwiek kontroler wyjdzie z boxa
        if (isInNeutral && (rightOut || leftOut))
        {
            isInNeutral = false;
            isCollecting = true;
            livePositionsRight.Clear();
            liveQuaternionsRight.Clear();
            livePositionsLeft.Clear();
            liveQuaternionsLeft.Clear();
        }

        // Zbieraj próbki tylko jeśli jesteśmy poza boxem
        if (!isInNeutral && isCollecting)
        {
            if (rightValid && deviceRight.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotR))
            {
                livePositionsRight.Add(posR);
                liveQuaternionsRight.Add(rotR);
            }
            if (leftValid && deviceLeft.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotL))
            {
                livePositionsLeft.Add(posL);
                liveQuaternionsLeft.Add(rotL);
            }
            // Po zebraniu 40 próbek z obu rąk uruchom predykcję
            if (livePositionsRight.Count >= targetFrames && liveQuaternionsRight.Count >= targetFrames &&
                livePositionsLeft.Count >= targetFrames && liveQuaternionsLeft.Count >= targetFrames)
            {
                isCollecting = false;
                RunLivePredictionBothHands();
            }
        }
        // Reset do neutralnego po powrocie obu kontrolerów do boxa
        if (!isInNeutral && IsInNeutralBox(posR) && IsInNeutralBox(posL))
        {
            isInNeutral = true;
        }
    }

    bool IsInNeutralBox(Vector3 pos)
    {
        Vector3 min = neutralBoxCenter - neutralBoxSize * 0.5f;
        Vector3 max = neutralBoxCenter + neutralBoxSize * 0.5f;
        return (pos.x >= min.x && pos.x <= max.x &&
                pos.y >= min.y && pos.y <= max.y &&
                pos.z >= min.z && pos.z <= max.z);
    }

    void RunLivePredictionBothHands()
    {
        // Interpolacja do 40 próbek
        var interpPosR = InterpolatePositions(livePositionsRight, targetFrames);
        var interpQuatR = InterpolateQuaternions(liveQuaternionsRight, targetFrames);
        var interpPosL = InterpolatePositions(livePositionsLeft, targetFrames);
        var interpQuatL = InterpolateQuaternions(liveQuaternionsLeft, targetFrames);
        float[,] data = new float[targetFrames, 14];
        for (int i = 0; i < targetFrames; i++)
        {
            // Prawa ręka
            data[i, 0] = interpPosR[i].x;
            data[i, 1] = interpPosR[i].y;
            data[i, 2] = interpPosR[i].z;
            data[i, 3] = interpQuatR[i].x;
            data[i, 4] = interpQuatR[i].y;
            data[i, 5] = interpQuatR[i].z;
            data[i, 6] = interpQuatR[i].w;
            // Lewa ręka
            data[i, 7] = interpPosL[i].x;
            data[i, 8] = interpPosL[i].y;
            data[i, 9] = interpPosL[i].z;
            data[i, 10] = interpQuatL[i].x;
            data[i, 11] = interpQuatL[i].y;
            data[i, 12] = interpQuatL[i].z;
            data[i, 13] = interpQuatL[i].w;
        }
        var model = ModelLoader.Load(onnxModelAsset); // lub saved_model jeśli to NNModel
        using (var worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model))
        {
            Tensor inputTensor = new Tensor(1, targetFrames, 14, 1); // (N, H, W, C)
            for (int t = 0; t < targetFrames; t++)
                for (int f = 0; f < 14; f++)
                    inputTensor[0, t, f, 0] = data[t, f];
            worker.Execute(inputTensor);
            Tensor output = worker.PeekOutput();
            float[] outputArray = output.ToReadOnlyArray();
            int predicted = ArgMax(outputArray);
            float confidence = Softmax(outputArray)[predicted];
            string predictedLabel = (predicted >= 0 && predicted < classLabels.Length) ? classLabels[predicted] : $"class_{predicted}";
            Debug.Log($"[LIVE BOTH HANDS] Predicted: {predictedLabel} (index: {predicted}), Confidence: {confidence:F2}");
            inputTensor.Dispose();
            output.Dispose();
            OnGesturePredicted(predictedLabel);
        }
        // Reset do kolejnego gestu
        livePositionsRight.Clear();
        liveQuaternionsRight.Clear();
        livePositionsLeft.Clear();
        liveQuaternionsLeft.Clear();
    }

    List<Vector3> InterpolatePositions(List<Vector3> input, int targetLen)
    {
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
                int idx = System.Array.FindLastIndex(tOrig, x => x <= t);
                if (idx == tOrig.Length - 1) idx--;
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
        List<Quaternion> result = new List<Quaternion>();
        float[] tOrig = Enumerable.Range(0, input.Count).Select(i => (float)i / (input.Count - 1)).ToArray();
        float[] tNew = Enumerable.Range(0, targetLen).Select(i => (float)i / (targetLen - 1)).ToArray();
        for (int i = 0; i < targetLen; i++)
        {
            float t = tNew[i];
            int idx = System.Array.FindLastIndex(tOrig, x => x <= t);
            if (idx == tOrig.Length - 1) idx--;
            float t0 = tOrig[idx], t1 = tOrig[idx + 1];
            Quaternion q0 = input[idx], q1 = input[idx + 1];
            float lerpT = (t - t0) / (t1 - t0);
            result.Add(Quaternion.Slerp(q0, q1, lerpT));
        }
        return result;
    }

    int ArgMax(float[] arr)
    {
        int maxIdx = 0;
        float maxVal = arr[0];
        for (int i = 1; i < arr.Length; i++)
        {
            if (arr[i] > maxVal)
            {
                maxVal = arr[i];
                maxIdx = i;
            }
        }
        return maxIdx;
    }

    float[] Softmax(float[] logits)
    {
        float maxLogit = logits.Max();
        float sumExp = logits.Select(x => Mathf.Exp(x - maxLogit)).Sum();
        return logits.Select(x => Mathf.Exp(x - maxLogit) / sumExp).ToArray();
    }

    //PRÓBA ZMIAN 
    private float[,] LoadAndPreprocessCSV(string filePath)
    {
        float[,] result = new float[40, 7];

        if (!File.Exists(filePath))
        {
            Debug.LogError("Nie znaleziono pliku CSV: " + filePath);
            return result;
        }

        var lines = File.ReadAllLines(filePath).Take(40).ToList();

        for (int i = 0; i < lines.Count; i++)
        {
            var parts = lines[i].Split(',');
            if (parts.Length >= 7)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (!float.TryParse(parts[j], out result[i, j]))
                    {
                        Debug.LogWarning($"Błąd parsowania CSV [line {i}, col {j}]: '{parts[j]}'");
                        result[i, j] = 0f;
                    }
                }
            }
        }

        return result;
    }


    //PRÓBA ZMIAN +1


    // Returns the predicted gesture label (e.g., "left", "right", "up", "down")
    public string GetPredictedGesture()
    {
        // Load model
        var model = ModelLoader.Load(onnxModelAsset);
        using (var worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model))
        {
            //ZMIANA float[,] data = LoadAndPreprocessCSV("sample_gesture.csv");
            float[,] data = LoadAndPreprocessCSV(@"C:\Users\olivi\DinoGame3D\Assets\Resources\sample_gesture.csv");

            Tensor inputTensor = new Tensor(1, 40, 7, 1);
            for (int t = 0; t < 40; t++)
                for (int f = 0; f < 7; f++)
                    inputTensor[0, t, f, 0] = data[t, f];
            worker.Execute(inputTensor);
            Tensor output = worker.PeekOutput();
            float[] outputArray = output.ToReadOnlyArray();
            int predicted = ArgMax(outputArray);
            string predictedLabel = (predicted >= 0 && predicted < classLabels.Length) ? classLabels[predicted] : $"class_{predicted}";
            inputTensor.Dispose();
            output.Dispose();
            return predictedLabel;
        }
    }

    // Wywoływana po rozpoznaniu gestu, tu podłącz logikę ruchu
    protected virtual void OnGesturePredicted(string predictedLabel)
    {
        Debug.Log($"[ACTION] Wykonaj ruch: {predictedLabel}");
        // Zakładamy, że w projekcie są już metody do sterowania dinozaurem,
        // np. MoveLeft(), MoveRight(), MoveUp(), MoveDown() lub podobne.
        // Wywołujemy odpowiednią metodę na podstawie predictedLabel:
        switch (predictedLabel)
        {
            case "left":
                SendMessage("MoveLeft", SendMessageOptions.DontRequireReceiver);
                break;
            case "right":
                SendMessage("MoveRight", SendMessageOptions.DontRequireReceiver);
                break;
            case "up":
                SendMessage("MoveUp", SendMessageOptions.DontRequireReceiver);
                break;
            case "down":
                SendMessage("MoveDown", SendMessageOptions.DontRequireReceiver);
                break;
            default:
                Debug.LogWarning($"[DINO] Nieznany ruch: {predictedLabel}");
                break;
        }
    }
}
