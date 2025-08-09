using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

[RequireComponent(typeof(TalkingController))]
public class DoneTalkingSoStartNextAnimation : MonoBehaviour
{
    [SerializeField]
    PlayableDirector anim;

    private TalkingController talk;

    void Start()
    {
        talk = GetComponent<TalkingController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (talk.done)
        {
            GetComponent<PlayerInput>().enabled = false;

            anim.gameObject.SetActive(true);
            anim.Play();

			gameObject.SetActive(false);
		}
    }
}
