using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class CollectStapler : MonoBehaviour
{
    private bool playerInRange = false;
    private PlayerInput playerInput;

    void Reset()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("BoxCollider2D is not set to Trigger. Setting isTrigger = true.");
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Replace this with your actual check for paper game completion
            if (GameProgress.hasCompletedPaperGame)
            {
                SceneManager.LoadScene("SampleScene");
            }
        }
    }
}
