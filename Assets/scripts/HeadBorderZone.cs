using UnityEngine;

public class HeadBorderZone : MonoBehaviour
{
    public AudioClip loopSound;
    public string headTag = "PlayerHead";

    private AudioSource headAudio;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(headTag)) return;

        Debug.Log("ENTER BORDER with: " + other.name);

        headAudio = other.GetComponent<AudioSource>();
        if (headAudio == null) headAudio = other.GetComponentInParent<AudioSource>();
        if (headAudio == null) return;

        if (loopSound == null) return;

        headAudio.Stop();
        headAudio.clip = loopSound;
        headAudio.time = 0f;
        headAudio.loop = true;
        headAudio.Play();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(headTag)) return;

        Debug.Log("EXIT BORDER with: " + other.name);

        if (headAudio == null) return;

        headAudio.Stop();
        headAudio.time = 0f;
        headAudio.loop = false;
    }
}