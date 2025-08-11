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

    public void hintClick() {
        newDialogue(true, hintCount);

        hintCount++;
        if (hintCount >= hints.Length) {
            hintCount = 0;
        }
    }

    public void talk(int n) {
		newDialogue(false, n);
	}

    public void justTalk(int n, float t) {
        StopAllCoroutines();
        hintTxt.gameObject.SetActive(true);
        hintTxtImg.gameObject.SetActive(true);
        hintTxt.text = talking[n];
        StartCoroutine(TextAppearanceTimer(t));
    }

    void newDialogue(bool h, int pos) {
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

        StartCoroutine(TextAppearanceTimer(audioSource.clip.length + 0.5f));
    }

	IEnumerator TextAppearanceTimer(float time) {
		yield return new WaitForSeconds(time);
		hintTxt.gameObject.SetActive(false);
		hintTxtImg.gameObject.SetActive(false);
	}
}