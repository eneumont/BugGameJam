using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HintSystem : MonoBehaviour {
    [SerializeField] TextMeshProUGUI hintTxt;
    [SerializeField] Image hintTxtImg;
    [SerializeField] string[] hints;
    [SerializeField] string[] talking;

    int hintCount = 0;

    void Start() {
        
    }

    void Update() {

    }

    public void hintClick() {
        hintTxt.text = hints[hintCount];

    }

    public void talk() {
    
    }
}