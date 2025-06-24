using UnityEngine;

public class IntoCameraRotate : MonoBehaviour
{
    public Transform target;                // Dinozaur
    public Animator dinoAnimator;           // Animator dinozaura

    [Header("Intro Rotation")]
    public float rotationSpeed = 35f;       // Szybkość pierwszego obrotu
    public float rotationDuration = 4f;     // Czas trwania pierwszego obrotu

    [Header("Collision Rotation")]
    public float collisionRotationSpeed = 50f;        // Szybkość drugiego obrotu
    public float collisionRotationDuration = 5f;      // Czas trwania drugiego obrotu

    private float introElapsed = 0f;
    private bool isIntroRotating = true;

    private float collisionElapsed = 0f;
    private bool isCollisionRotating = false;
    private bool hasCollisionRotationStarted = false;

    void Update()
    {
        // 🔄 Faza 1 – Intro
        if (isIntroRotating)
        {
            introElapsed += Time.deltaTime;
            transform.RotateAround(target.position, -Vector3.up, rotationSpeed * Time.deltaTime);

            if (introElapsed >= rotationDuration)
            {
                isIntroRotating = false;
            }
        }

        // ⏱️ Sprawdzenie czy animacja kolizji się zaczęła
        if (!hasCollisionRotationStarted && IsInCollisionAnimation())
        {
            hasCollisionRotationStarted = true;
            isCollisionRotating = true;
            collisionElapsed = 0f;
        }

        // 🔄 Faza 2 – Obrót przy kolizji
        if (isCollisionRotating)
        {
            collisionElapsed += Time.deltaTime;
            transform.RotateAround(target.position, -Vector3.up, collisionRotationSpeed * Time.deltaTime);

            if (collisionElapsed >= collisionRotationDuration)
            {
                isCollisionRotating = false;
            }
        }
    }

    private bool IsInCollisionAnimation()
    {
        AnimatorStateInfo info = dinoAnimator.GetCurrentAnimatorStateInfo(0);
        return info.IsName("dinosaur1_Collision");
    }
}
