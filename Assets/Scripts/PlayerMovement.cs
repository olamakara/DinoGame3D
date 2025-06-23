using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 5;
    public float horizontalSpeed = 7;
    public float rightLimit = 2.5f;
    public float leftLimit = -2.5f;

    [SerializeField] private Animator animator;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        animator.SetTrigger("Walk"); // domyślnie idzie do przodu
    }

    void Update()
    {
        // Stały ruch do przodu
        transform.Translate(Vector3.forward * Time.deltaTime * playerSpeed, Space.World);

        // Lewo
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            if (transform.position.x > leftLimit)
            {
                transform.Translate(Vector3.left * Time.deltaTime * horizontalSpeed);
            }

            // Animacja tylko przy wciśnięciu
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                animator.SetTrigger("Left");
            }
        }

        // Prawo
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            if (transform.position.x < rightLimit)
            {
                transform.Translate(Vector3.right * Time.deltaTime * horizontalSpeed);
            }

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                animator.SetTrigger("Right");
            }
        }

        // Skok
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            animator.SetTrigger("Jump");
        }
    }
}
