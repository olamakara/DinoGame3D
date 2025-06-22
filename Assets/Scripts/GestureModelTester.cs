using UnityEngine;
using Unity.Barracuda;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class GestureModelTester : MonoBehaviour
{
    public NNModel onnxModelAsset; // Assign your .onnx model in the Inspector
    public string csvFileName = "sample_gesture"; // Without .csv, must be in Resources/
    public string expectedLabel = "up"; // Set manually for your test
    public string[] classLabels = new string[] { "left", "right", "up", "down" }; // Set to your model's output classes

    private IWorker worker;

    void Start()
    {
        // Load model
        var model = ModelLoader.Load(onnxModelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model);

        // Load and preprocess CSV
        float[,] data = LoadAndPreprocessCSV(csvFileName);

        // Create tensor (1, 40, 7)
        Tensor inputTensor = new Tensor(1, 40, 7, 1); // Barracuda expects 4D: N,H,W,C
        for (int t = 0; t < 40; t++)
            for (int f = 0; f < 7; f++)
                inputTensor[0, t, f, 0] = data[t, f];

        // Run inference
        worker.Execute(inputTensor);
        Tensor output = worker.PeekOutput();

        // Get predicted class and probability
        float[] outputArray = output.ToReadOnlyArray();
        int predicted = ArgMax(outputArray);
        float confidence = Softmax(outputArray)[predicted];
        string predictedLabel = (predicted >= 0 && predicted < classLabels.Length) ? classLabels[predicted] : $"class_{predicted}";

        Debug.Log($"Expected label: {expectedLabel}, Predicted: {predictedLabel} (index: {predicted}), Confidence: {confidence:F2}");

        // Cleanup
        inputTensor.Dispose();
        output.Dispose();
        worker.Dispose();
    }

    float[,] LoadAndPreprocessCSV(string resourceName)
    {
        // Load CSV from Resources
        TextAsset csvAsset = Resources.Load<TextAsset>(resourceName);
        if (csvAsset == null)
        {
            Debug.LogError("CSV file not found in Resources!");
            return new float[40, 7];
        }

        // Parse CSV
        var lines = csvAsset.text.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
        List<Vector3> positions = new List<Vector3>();
        List<Quaternion> quaternions = new List<Quaternion>();
        foreach (var line in lines.Skip(1)) // skip header
        {
            var tokens = line.Split(';');
            if (tokens.Length < 8) continue;
            float px = float.Parse(tokens[1]);
            float py = float.Parse(tokens[2]);
            float pz = float.Parse(tokens[3]);
            float qx = float.Parse(tokens[4]);
            float qy = float.Parse(tokens[5]);
            float qz = float.Parse(tokens[6]);
            float qw = float.Parse(tokens[7]);
            positions.Add(new Vector3(px, py, pz));
            quaternions.Add(new Quaternion(qx, qy, qz, qw));
        }

        // Interpolate to 40 steps
        var interpPos = InterpolatePositions(positions, 40);
        var interpQuat = InterpolateQuaternions(quaternions, 40);

        // Combine to (40, 7)
        float[,] result = new float[40, 7];
        for (int i = 0; i < 40; i++)
        {
            result[i, 0] = interpPos[i].x;
            result[i, 1] = interpPos[i].y;
            result[i, 2] = interpPos[i].z;
            result[i, 3] = interpQuat[i].x;
            result[i, 4] = interpQuat[i].y;
            result[i, 5] = interpQuat[i].z;
            result[i, 6] = interpQuat[i].w;
        }
        return result;
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

    // Returns the predicted gesture label (e.g., "left", "right", "up", "down")
    public string GetPredictedGesture()
    {
        // Load model
        var model = ModelLoader.Load(onnxModelAsset);
        using (var worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model))
        {
            float[,] data = LoadAndPreprocessCSV(csvFileName);
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
}
