using UnityEngine;

public class ProgressMarker : MonoBehaviour
{
    public void MarkProgress()
    {
        GameProgress.hasProgressed += 1;
        Debug.Log("Progress marked!");
    }
}
