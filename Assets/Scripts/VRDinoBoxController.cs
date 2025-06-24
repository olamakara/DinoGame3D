using UnityEngine;

public class VRDinoBoxController : MonoBehaviour
{
    public PlayerMovement dino;
    public Transform leftController;
    public Transform rightController;
    public Collider neutralBox; // The single neutral zone
    public GestureModelTester gestureModel; // Assign your real model in Inspector

    // To avoid repeated triggers, store last state
    private bool wasInNeutralZoneLeft = true;
    private bool wasInNeutralZoneRight = true;
    private bool actionTriggeredLeft = false;
    private bool actionTriggeredRight = false;

    void Update()
    {
        // Check left controller
        CheckControllerInNeutralZone(leftController, ref wasInNeutralZoneLeft, ref actionTriggeredLeft);
        // Check right controller
        CheckControllerInNeutralZone(rightController, ref wasInNeutralZoneRight, ref actionTriggeredRight);
    }

    void CheckControllerInNeutralZone(Transform controller, ref bool wasInNeutralZone, ref bool actionTriggered)
    {
        if (controller == null || dino == null || neutralBox == null) return;
        bool inNeutralZone = neutralBox.bounds.Contains(controller.position);

        // If controller just left the neutral zone and no action has been triggered
        if (!inNeutralZone && wasInNeutralZone && !actionTriggered)
        {
            if (gestureModel != null)
            {
                string gesture = gestureModel.GetPredictedGesture();
                if (gesture == "up")
                {
                    dino.Jump();
                    actionTriggered = true;
                }
                else if (gesture == "down")
                {
                    dino.Bend();
                    actionTriggered = true;
                }
                else if (gesture == "left")
                {
                    dino.MoveLeft();
                    actionTriggered = true;
                }
                else if (gesture == "right")
                {
                    dino.MoveRight();
                    actionTriggered = true;
                }
            }
        }
        // Reset action trigger when controller returns to neutral zone
        if (inNeutralZone && !wasInNeutralZone)
        {
            actionTriggered = false;
        }
        wasInNeutralZone = inNeutralZone;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f); // Cyan for neutral zone
        if (neutralBox != null)
            Gizmos.DrawCube(neutralBox.bounds.center, neutralBox.bounds.size);
    }
#endif
}
