using UnityEngine;
using System.Collections;

public class RandomAmbientPlayer : MonoBehaviour
{
    [Header("Resources folder")]
    public string folderName = "Jardin";

    [Header("Delay between sounds")]
    public float minDelay = 5f;
    public float maxDelay = 10f;

    private AudioSource audioSource;
    private AudioClip[] clips;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        clips = Resources.LoadAll<AudioClip>("son/"+folderName);

        if (clips.Length == 0)
        {
            Debug.LogWarning("No sounds found in Resources/" + folderName);
            return;
        }

        StartCoroutine(PlayLoop());
    }

    IEnumerator PlayLoop()
    {
        while (true)
        {
            // attendre si un son joue déjà
            while (audioSource.isPlaying)
                yield return null;

            // attendre entre min et max
            float wait = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(wait);

            // choisir un son aléatoire
            AudioClip clip = clips[Random.Range(0, clips.Length)];

            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}