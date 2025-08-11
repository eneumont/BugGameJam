using UnityEngine;

[RequireComponent(typeof(TalkingController))]
public class StartTalkingAtTheStartOfTheScene : MonoBehaviour
{
    TalkingController talk;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
	    talk = GetComponent<TalkingController>();
        talk.StartText();
	}
}
