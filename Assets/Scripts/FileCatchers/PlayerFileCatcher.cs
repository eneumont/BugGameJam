using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerFileCatcher : MonoBehaviour
{
    [SerializeField] private AudioClip dingSound;
    [SerializeField] private AudioSource audioSource; // Optional, assign in Inspector

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("GoodFile"))
        {
            Debug.Log("Player caught a good file!");
            if (dingSound != null)
            {
                audioSource.PlayOneShot(dingSound);
            }
            Destroy(other.gameObject);
        }
    }
}
