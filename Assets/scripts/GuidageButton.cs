using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GuidageButton : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private string playerTag = "PlayerHead";

    [Header("Input (XR)")]
    [SerializeField] private InputActionReference leftTriggerAction;

    [Header("Balises (ordre = chemin)")]
    [SerializeField] private List<Transform> balises = new List<Transform>();

    [Header("Bip")]
    [SerializeField] private float intervalMin = 0.12f;
    [SerializeField] private float intervalMax = 1.10f;
    [SerializeField] private float distancePourBipRapide = 2.0f;
    [SerializeField] private float distancePourBipLent = 25.0f;

    [Header("Volume")]
    [SerializeField] private float volumeMin = 0.10f;
    [SerializeField] private float volumeMax = 0.90f;

    private Transform playerHead;
    private int indexActif = 0;
    private bool guidageActif = false;
    private bool triggerHeld = false;
    private Coroutine routine;

    void Awake()
    {
        if (leftTriggerAction != null) leftTriggerAction.action.Enable();
    }

    void Start()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go != null) playerHead = go.transform;

        StopAllBalises();
    }

    void Update()
    {
        if (leftTriggerAction == null) return;

        float v = leftTriggerAction.action.ReadValue<float>();
        bool pressed = v > 0.75f;

        if (pressed && !triggerHeld)
        {
            triggerHeld = true;
            ToggleGuidage();
        }
        else if (!pressed)
        {
            triggerHeld = false;
        }
    }

    public void ToggleGuidage()
    {
        if (playerHead == null)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go != null) playerHead = go.transform;
        }
        if (playerHead == null || balises.Count == 0) return;

        guidageActif = !guidageActif;

        if (guidageActif)
        {
            indexActif = Mathf.Clamp(indexActif, 0, balises.Count - 1);
            ActiverBalise(indexActif);
            routine = StartCoroutine(BipLoop());
        }
        else
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
            StopAllBalises();
        }
    }

    public void PasserBaliseSuivante()
    {
        if (!guidageActif) return;
        if (balises.Count == 0) return;

        StopBalise(indexActif);

        indexActif = Mathf.Clamp(indexActif + 1, 0, balises.Count - 1);
        ActiverBalise(indexActif);
    }

    private IEnumerator BipLoop()
    {
        while (guidageActif)
        {
            if (playerHead == null || balises.Count == 0)
            {
                yield return null;
                continue;
            }

            float dTotale = DistanceTotale(playerHead.position, indexActif);
            float t = Mathf.InverseLerp(distancePourBipRapide, distancePourBipLent, dTotale);
            float interval = Mathf.Lerp(intervalMin, intervalMax, Mathf.Clamp01(t));

            float vT = Mathf.InverseLerp(distancePourBipLent, distancePourBipRapide, dTotale);
            float volume = Mathf.Lerp(volumeMin, volumeMax, Mathf.Clamp01(vT));

            JouerBipBalise(indexActif, volume);

            yield return new WaitForSeconds(interval);
        }
    }

    private float DistanceTotale(Vector3 playerPos, int idx)
    {
        idx = Mathf.Clamp(idx, 0, balises.Count - 1);
        float sum = Vector3.Distance(playerPos, balises[idx].position);

        for (int i = idx; i > 0; i--)
            sum += Vector3.Distance(balises[i].position, balises[i - 1].position);

        return sum;
    }

    private void JouerBipBalise(int idx, float volume)
    {
        if (idx < 0 || idx >= balises.Count) return;

        var a = balises[idx].GetComponent<AudioSource>();
        if (a == null) return;

        a.volume = volume;

        if (a.clip != null) a.PlayOneShot(a.clip, a.volume);
        else a.Play();
    }

    private void ActiverBalise(int idx)
    {
        if (idx < 0 || idx >= balises.Count) return;

        var a = balises[idx].GetComponent<AudioSource>();
        if (a == null) return;

        a.spatialBlend = 1f;
        a.playOnAwake = false;
        a.Stop();
    }

    private void StopBalise(int idx)
    {
        if (idx < 0 || idx >= balises.Count) return;
        var a = balises[idx].GetComponent<AudioSource>();
        if (a != null) a.Stop();
    }

    private void StopAllBalises()
    {
        for (int i = 0; i < balises.Count; i++)
        {
            var a = balises[i].GetComponent<AudioSource>();
            if (a != null) a.Stop();
        }
    }
}