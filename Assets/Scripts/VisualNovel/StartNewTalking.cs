using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TalkingController))]
public class StartNewTalking : MonoBehaviour
{
    TalkingController talk;

	[Header("Person In Front of you")]
	[SerializeField]
	bool PersonPic = false;
	[SerializeField]
    Sprite PersonSprite;
	[SerializeField]
	Image PersonImage;

	[Header("Person Talking")]
	[SerializeField]
    Sprite PersonTalkingSprite;
    [SerializeField]
	string PersonTalkingName = "Person";
	[SerializeField]
	Image PersonTalkingImage;
	[SerializeField]
	TextMeshProUGUI PersonTalkingNameText;


	void Awake()
    {
		talk = GetComponent<TalkingController>();
	}

    public void startNewTalking()
    {
		if (PersonPic && PersonSprite)
		{
			PersonImage.sprite = PersonSprite;
			PersonImage.gameObject.SetActive(true);
		} else
		{
			if (PersonImage)
			{
				PersonImage.gameObject.SetActive(false);
			}
		}

		if (PersonTalkingSprite)
		{
			PersonTalkingImage.sprite = PersonTalkingSprite;
		}

		if (PersonTalkingNameText)
		{
			PersonTalkingNameText.text = PersonTalkingName;
		}

		talk.StartText();
	}
}
