using UnityEngine;

public class BottomFileCatcher : MonoBehaviour
{
    [SerializeField] private WarningUIManager warningUI; // Drag your LifeSystem object in the Inspector

    private void OnTriggerEnter2D(Collider2D collision)
    {
        FileData fileData = collision.GetComponent<FileData>(); // Assume FileData has info if it's good or bad

        if (fileData != null)
        {
            if (fileData.isGoodFile)
            {
                Debug.Log("Warning");
                warningUI.AddWarning();
            }

            Destroy(collision.gameObject); // Destroy the file in all cases
        }
    }
}
