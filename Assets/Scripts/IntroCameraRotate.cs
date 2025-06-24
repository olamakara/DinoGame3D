using UnityEngine;
using UnityEngine.SceneManagement;

public class IntoCameraRotate : MonoBehaviour
{
    public Transform target;               
    public Animator dinoAnimator;           

    [Header("Intro Rotation")]
    public float rotationSpeed = 35f;      
    public float rotationDuration = 4f;    

    [Header("Collision Rotation")]
    public float collisionRotationSpeed = 50f;      
    public float collisionRotationDuration = 5f;    

    private float introElapsed = 0f;
    private bool isIntroRotating = true;

    private float collisionElapsed = 0f;
    private bool isCollisionRotating = false;
    private bool hasCollisionRotationStarted = false;

    void Update()
    {
        if (isIntroRotating)
        {
            introElapsed += Time.deltaTime;
            transform.RotateAround(target.position, -Vector3.up, rotationSpeed * Time.deltaTime);

            if (introElapsed >= rotationDuration)
            {
                isIntroRotating = false;
            }
        }

        if (!hasCollisionRotationStarted && IsInCollisionAnimation())
        {
            hasCollisionRotationStarted = true;
            isCollisionRotating = true;
            collisionElapsed = 0f;
        }


        if (isCollisionRotating)
        {
            collisionElapsed += Time.deltaTime;
            transform.RotateAround(target.position, -Vector3.up, collisionRotationSpeed * Time.deltaTime);

            if (collisionElapsed >= collisionRotationDuration)
            {
                isCollisionRotating = false;
                SceneManager.LoadScene(0);
            }

    
            
        }
    }

    private bool IsInCollisionAnimation()
    {
        AnimatorStateInfo info = dinoAnimator.GetCurrentAnimatorStateInfo(0);
        return info.IsName("dinosaur1_Collision");
    }
}
