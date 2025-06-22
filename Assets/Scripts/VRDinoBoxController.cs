using UnityEngine;

public class VRDinoBoxController : MonoBehaviour
{
    public PlayerMovement dino;
    public Transform leftController;
    public Transform rightController;
    public Collider leftBox;
    public Collider rightBox;
    public Collider upBox;
    public Collider downBox;
    public GestureModelTester gestureModelTester; // Assign in Inspector

    // To avoid repeated triggers, store last state
    private bool wasLeftIn = false;
    private bool wasRightIn = false;
    private bool wasUpIn = false;
    private bool wasDownIn = false;

    void Update()
    {
        // Check left controller
        CheckControllerInBox(leftController);
        // Check right controller
        CheckControllerInBox(rightController);
    }

    void CheckControllerInBox(Transform controller)
    {
        if (controller == null || dino == null) return;
        bool leftIn = leftBox != null && leftBox.bounds.Contains(controller.position);
        bool rightIn = rightBox != null && rightBox.bounds.Contains(controller.position);
        bool upIn = upBox != null && upBox.bounds.Contains(controller.position);
        bool downIn = downBox != null && downBox.bounds.Contains(controller.position);

        // Only allow one action at a time (priority: jump > bend > left > right)
        if (gestureModelTester != null)
        {
            string gesture = gestureModelTester.GetPredictedGesture();
            if (gesture == "up" && !wasUpIn && !downIn && !leftIn && !rightIn) dino.Jump();
            else if (gesture == "down" && !wasDownIn && !upIn && !leftIn && !rightIn) dino.Bend();
            else if (gesture == "left" && !wasLeftIn && !rightIn && !upIn && !downIn) dino.MoveLeft();
            else if (gesture == "right" && !wasRightIn && !leftIn && !upIn && !downIn) dino.MoveRight();
            // Update state tracking for exclusivity
            wasLeftIn = (gesture == "left");
            wasRightIn = (gesture == "right");
            wasUpIn = (gesture == "up");
            wasDownIn = (gesture == "down");
            return;
        }
        // Fallback to box logic if no gesture is detected
        if (upIn && !wasUpIn && !downIn && !leftIn && !rightIn) {
            dino.Jump();
        } else if (downIn && !wasDownIn && !upIn && !leftIn && !rightIn) {
            dino.Bend();
        } else if (leftIn && !wasLeftIn && !rightIn && !upIn && !downIn) {
            dino.MoveLeft();
        } else if (rightIn && !wasRightIn && !leftIn && !upIn && !downIn) {
            dino.MoveRight();
        }
        wasLeftIn = leftIn;
        wasRightIn = rightIn;
        wasUpIn = upIn;
        wasDownIn = downIn;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f); // Red for left
        if (leftBox != null)
            Gizmos.DrawCube(leftBox.bounds.center, leftBox.bounds.size);
        Gizmos.color = new Color(0, 1, 0, 0.3f); // Green for right
        if (rightBox != null)
            Gizmos.DrawCube(rightBox.bounds.center, rightBox.bounds.size);
        Gizmos.color = new Color(0, 0, 1, 0.3f); // Blue for up
        if (upBox != null)
            Gizmos.DrawCube(upBox.bounds.center, upBox.bounds.size);
        Gizmos.color = new Color(1, 1, 0, 0.3f); // Yellow for down
        if (downBox != null)
            Gizmos.DrawCube(downBox.bounds.center, downBox.bounds.size);
    }
#endif
}
