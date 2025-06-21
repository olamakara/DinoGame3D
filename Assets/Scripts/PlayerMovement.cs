using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 2;
    public float horizontalSpeed = 7;
    public float rightLimit = 2.5f;
    public float leftLimit = -2.5f;
    public Animator animator; // Assign in Inspector

    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * playerSpeed, Space.World);
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            if (this.gameObject.transform.position.x > leftLimit)
            {
                transform.Translate(Vector3.left * Time.deltaTime * horizontalSpeed);
            }
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            if (this.gameObject.transform.position.x < rightLimit)
            {
                transform.Translate(Vector3.left * Time.deltaTime * horizontalSpeed * -1);
            }
        }
    }

    public void MoveLeft()
    {
        if (transform.position.x > leftLimit)
            transform.Translate(Vector3.left * Time.deltaTime * horizontalSpeed);
    }

    public void MoveRight()
    {
        if (transform.position.x < rightLimit)
            transform.Translate(Vector3.right * Time.deltaTime * horizontalSpeed);
    }

    public void Jump()
    {
        if (animator != null)
            animator.SetTrigger("Jump");
        Debug.Log("Jump!");
    }

    public void Bend()
    {
        if (animator != null)
            animator.SetTrigger("Bend");
        Debug.Log("Bend!");
    }
}
