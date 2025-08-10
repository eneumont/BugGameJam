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

	[SerializeField]
	int correctButton = 0; // Base 0

	public void SetUpTheChoices()
	{
		if (Buttons.Length != ButtonTexts.Length) { return; }

		for (int i = 0; i < Buttons.Length; i++)
		{
			Buttons[i].interactable = true;
			Buttons[i].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = ButtonTexts[i];
			Buttons[i].onClick.RemoveAllListeners();
			if (i == correctButton)
			{
				Buttons[i].onClick.AddListener(CorrectChoice);
			}
			else
			{
				Buttons[i].onClick.AddListener(WrongChoice);
			}
		}

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

	public void CorrectChoice()
	{
		Debug.Log("Correct Choice Made!");
		ChoicesGroup.SetActive(false);
	}

	public void WrongChoice()
	{
		Debug.Log("Wrong Choice Made!");
		//ChoicesGroup.SetActive(false);
	}
}
