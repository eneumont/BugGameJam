using UnityEngine;

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
	GameObject[] Buttons;

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
			Buttons[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = ButtonTexts[i];
			Buttons[i].GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
			if (i == correctButton)
			{
				Buttons[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(CorrectChoice);
			}
			else
			{
				Buttons[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(WrongChoice);
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

		foreach (GameObject button in Buttons)
		{
			button.SetActive(true);
		}
	}

	public void CorrectChoice()
	{
		Debug.Log("Correct Choice Made!");
	}

	public void WrongChoice()
	{
		Debug.Log("Wrong Choice Made!");
	}
}
