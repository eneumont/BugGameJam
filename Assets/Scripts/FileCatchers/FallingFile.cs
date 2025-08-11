using UnityEngine;

public class FallingFile : MonoBehaviour
{
    private float fallSpeed = 3f;

    public void SetFallSpeed(float speed)
    {
        fallSpeed = speed;
    }

    private void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // Destroy if off-screen
        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }
}
