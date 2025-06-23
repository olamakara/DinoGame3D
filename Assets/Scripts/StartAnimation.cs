using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartAnimation : MonoBehaviour
{
    [SerializeField] GameObject thePlayer;
    [SerializeField] GameObject playerAnim;

    void Start()
    {
        thePlayer.GetComponent<PlayerMovement>().enabled = false;
        playerAnim.GetComponent<Animator>().Play("dinosaur1_Start");
        StartCoroutine(EnableMovementAfterAnimation());
    }

    IEnumerator EnableMovementAfterAnimation()
    {
        // Czekaj na zakończenie animacji
        Animator animator = playerAnim.GetComponent<Animator>();
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);

        // Czekaj przez czas trwania animacji
        yield return new WaitForSeconds(animState.length);

        // Włącz PlayerMovement
        thePlayer.GetComponent<PlayerMovement>().enabled = true;
    }
}
