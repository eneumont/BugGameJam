using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HintSystem : MonoBehaviour {
    [SerializeField] TextMeshProUGUI hintTxt;
    [SerializeField] Image hintTxtImg;
    [SerializeField] string[] hints;
    [SerializeField] string[] talking;
	[SerializeField] AudioClip[] hintClips;
	[SerializeField] AudioClip[] talkClips;

    AudioSource audioSource;
	int hintCount;

    void Start() {
		hintTxt.gameObject.SetActive(false);
		hintTxtImg.gameObject.SetActive(false);
		hintCount = 0;
        audioSource = GetComponent<AudioSource>();    
    }

    public void hintClick(int time) {
        newDialogue(true, hintCount, time);

        hintCount++;
        if (hintCount >= hints.Length) {
            hintCount = 0;
        }
    }

    public void talk(int n, float time) {
		newDialogue(false, n, time);
	}

    void newDialogue(bool h, int pos, float time) {
        StopAllCoroutines();

		hintTxt.gameObject.SetActive(true);
		hintTxtImg.gameObject.SetActive(true);

		if (h) {
            hintTxt.text = hints[pos];
            audioSource.clip = hintClips[pos];
            audioSource.Play();
        } else { 
            hintTxt.text = talking[pos];
            audioSource.clip = talkClips[pos];
            audioSource.Play();
        }

        StartCoroutine(TextAppearanceTimer(time));
    }

	IEnumerator TextAppearanceTimer(float time) {
		yield return new WaitForSeconds(time);
		hintTxt.gameObject.SetActive(false);
		hintTxtImg.gameObject.SetActive(false);
	}
}