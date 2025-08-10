using UnityEngine;

[RequireComponent(typeof(TalkingController))]
public class GoToChoice : MonoBehaviour
{
    TalkingController talk;

    [SerializeField]
    SetUpChoices ChoiceObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		talk = GetComponent<TalkingController>();
	}

    // Update is called once per frame
    void Update()
    {
        if(talk.done && ChoiceObject)
        {
            ChoiceObject.SetUpTheChoices();
            this.gameObject.SetActive(false);
		}
    }
}
