using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BottomFileCatcher : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BadFile"))
        {
            Debug.Log("Bad file destroyed at bottom.");
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("GoodFile"))
        {
            Debug.Log("You lose a life! Missed a good file.");
            Destroy(other.gameObject);
        }
    }
}
