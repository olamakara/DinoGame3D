using System.Collections;
using UnityEngine;

public class CollisionDetect : MonoBehaviour
{
    [SerializeField] GameObject thePlayer;
    [SerializeField] GameObject playerAnim;

    private PlayerMovement movementScript;
    private Animator anim;

    void Start()
    {
        movementScript = thePlayer.GetComponent<PlayerMovement>();
        anim = playerAnim.GetComponent<Animator>();

        movementScript.enabled = false;
        anim.Play("dinosaur1_Start");
        StartCoroutine(WaitForStartAnimationThenMove());
    }

    IEnumerator WaitForStartAnimationThenMove()
    {
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("dinosaur1_Start"))
        {
            yield return null;

        }

        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);
        movementScript.enabled = true;
        anim.Play("dinosaur1_WalkForward");
    }

    void OnTriggerEnter(Collider other)
    {
        movementScript.enabled = false;
        anim.Play("dinosaur1_Up");
    }
}
