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
	int hintCount = 0;

    void Start() {
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

        hintTxtImg.enabled = true;

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
        hintTxtImg.enabled = false;
	}
}