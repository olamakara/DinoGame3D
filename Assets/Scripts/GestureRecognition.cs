using UnityEngine;
using TMPro;

public class GestureRecognition : MonoBehaviour
{
    [Header("Player Object")]
    public GameObject player;

    [Header("Debug UI")]
    public TextMeshPro debugText;

    void Update()
    {
        HandleControllerInput();
    }

    void HandleControllerInput()
    {
        // Lewo – A
        if (OVRInput.GetDown(OVRInput.Button.One)) // A
        {
            debugText.text = "Lewo (A)";
            player.SendMessage("MoveLeft");
        }

        // Prawo – B
        if (OVRInput.GetDown(OVRInput.Button.Two)) // B
        {
            debugText.text = "Prawo (B)";
            player.SendMessage("MoveRight");
        }

        // Góra (Skok) – X
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.5f) // X
        {
            debugText.text = "Skok (X)";
            player.SendMessage("Jump");
        }

        // Dół (Kucanie) – Y
        // if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.5f) // Y
        // {
        //     debugText.text = "Kucanie (Y)";
        //     player.SendMessage("Bend");
        // }
    }
}
