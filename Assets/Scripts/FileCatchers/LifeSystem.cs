using UnityEngine;

public class LifeSystem : MonoBehaviour
{
    [SerializeField] private int startingLives = 3;
    private int currentLives;

    private void Awake()
    {
        currentLives = startingLives;
    }

    public void LoseLife()
    {
        currentLives--;
        Debug.Log($"Life lost! Lives remaining: {currentLives}");

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
        // You can add your game over logic here (reload scene, show menu, etc.)
    }
}
