using UnityEngine;

public class ProgressMarker : MonoBehaviour
{
    public void MarkProgress()
    {
        GameProgress.hasProgressed = true;
        Debug.Log("Progress marked!");
    }
}
