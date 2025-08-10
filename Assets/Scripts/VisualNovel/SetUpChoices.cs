using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetUpChoices : MonoBehaviour
{
	[Header("UI")]
	[SerializeField]
	GameObject ChoicesGroup;
	[SerializeField]
	GameObject PersonPic;
	[SerializeField]
	CanvasGroup TalkingCanvas;

	[Header("Buttons")]
	[SerializeField]
	Button[] Buttons;

	[Header("Button Text")]
	[SerializeField]
	string[] ButtonTexts;

	[Header("Results")]
	[SerializeField]
	StartNewTalking[] Paths;
	[SerializeField]
	int[] correctButtons; // Base 0

	public void SetUpTheChoices()
	{
		if (Buttons.Length != ButtonTexts.Length) { return; }

		for (int i = 0; i < Buttons.Length; i++)
		{
			Buttons[i].interactable = true;
			Buttons[i].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = ButtonTexts[i];
			Buttons[i].onClick.RemoveAllListeners();

		}
		Buttons[0].onClick.AddListener(ChoiceZero);
		Buttons[1].onClick.AddListener(ChoiceOne);
		Buttons[2].onClick.AddListener(ChoiceTwo);
		Buttons[3].onClick.AddListener(ChoiceThree);

		ChoicesGroup.SetActive(true);
		if (PersonPic)
		{
			PersonPic.SetActive(false);
		}
		if (TalkingCanvas)
		{
			TalkingCanvas.alpha = 0;
			TalkingCanvas.blocksRaycasts = false;
		}

		foreach (Button button in Buttons)
		{
			button.gameObject.SetActive(true);
		}
	}

	public void ChoiceZero()
	{
		Paths[0].startNewTalking();
		EndChoice();
	}

	public void ChoiceOne()
	{
		Paths[1].startNewTalking();
		EndChoice();
	}

	public void ChoiceTwo()
	{
		Paths[2].startNewTalking();
		EndChoice();
	}

	public void ChoiceThree()
	{
		Paths[3].startNewTalking();
		EndChoice();
	}

	private void EndChoice()
	{
		ChoicesGroup.SetActive(false);
		this.gameObject.SetActive(false);
	}

}
