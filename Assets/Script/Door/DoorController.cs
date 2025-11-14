using UnityEngine;

public class DoorController : MonoBehaviour
{
    private int playerCount = 0;
    [SerializeField] private Animator anim;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if (playerCount == 0) anim.SetBool("PlayerClose", true);
            playerCount++;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCount--;
            if(playerCount<=0)
            {
                anim.SetBool("PlayerClose", false);
                playerCount = 0;
            }
        }
    }
}
