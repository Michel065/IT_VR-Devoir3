using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidageChainController : MonoBehaviour
{
    [System.Serializable]
    public class Relay
    {
        public Transform point;
        public AudioSource audio;
    }

    [Header("Player")]
    [SerializeField] private string playerTag = "PlayerHead";

    [Header("Source (bouton)")]
    [SerializeField] private AudioSource sourceEmitter;

    [Header("Relais (ordre = chemin du joueur -> vers la source)")]
    [SerializeField] private List<Relay> relays = new List<Relay>();

    [Header("Bip - intervalle (selon distance totale)")]
    [SerializeField] private float intervalMin = 0.12f;
    [SerializeField] private float intervalMax = 1.10f;
    [SerializeField] private float distancePourBipRapide = 2.0f;
    [SerializeField] private float distancePourBipLent = 25.0f;

    [Header("Bip - volume (selon distance totale)")]
    [SerializeField] private float volumeMin = 0.10f;
    [SerializeField] private float volumeMax = 0.90f;

    [Header("Auto")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool resetToStartOnEnable = true;

    private Transform playerHead;
    private bool guidageActif;

    private int indexBalise;         // 0..relays.Count-1 ; relays.Count => mode bouton
    private int indexAudioActif;     // -1 => bouton, sinon index relay qui porte l'audio
    private Coroutine loop;

    private void OnEnable()
    {
        if (resetToStartOnEnable)
        {
            indexBalise = 0;
            indexAudioActif = -1;
        }
    }

    private void Start()
    {
        RefreshPlayer();
        StopAllOutputAudio();

        if (autoStart) StartGuidage();
    }

    public void ToggleGuidage()
    {
        if (guidageActif) StopGuidage();
        else StartGuidage();
    }

    public void StartGuidage()
    {
        RefreshPlayer();
        if (!EnsureReadyForStart()) return;

        guidageActif = true;

        if (indexBalise < 0) indexBalise = 0;
        if (indexBalise > relays.Count) indexBalise = relays.Count;

        if (indexBalise >= relays.Count)
        {
            indexAudioActif = -1; // bouton
        }
        else
        {
            indexAudioActif = FindNextRelayWithAudio(indexBalise);
        }

        StopAllOutputAudio();

        if (loop != null) StopCoroutine(loop);
        loop = StartCoroutine(BipLoop());
    }

    public void StopGuidage()
    {
        guidageActif = false;

        if (loop != null) StopCoroutine(loop);
        loop = null;

        StopAllOutputAudio();
    }

    // Appelé depuis PlayerRelaySensor (sur le joueur)
    public void OnEnterRelay(Transform hit)
    {
        if (!guidageActif) return;
        if (!EnsureReadyRuntime()) return;

        // Si on est déjà en mode bouton, ignorer les balises
        if (indexBalise >= relays.Count) return;

        Transform expected = relays[indexBalise].point;
        if (expected == null) return;

        // Résolution : si le collider est un enfant de la balise attendue, on “remonte” au point attendu
        Transform resolved = hit;
        if (hit != null && hit.IsChildOf(expected))
            resolved = expected;

        if (resolved == expected)
        {
            PasserBaliseSuivante();
        }
    }

    private void PasserBaliseSuivante()
    {
        indexBalise++;

        // Fin de chaîne => on bascule sur le bouton
        if (indexBalise >= relays.Count)
        {
            StopAllOutputAudio();
            indexAudioActif = -1;
            return;
        }

        int nextAudio = FindNextRelayWithAudio(indexBalise);

        if (nextAudio != indexAudioActif)
        {
            StopAllOutputAudio();
            indexAudioActif = nextAudio; // peut rester -1 si aucune balise n'a d'audio -> bouton
        }
    }

    private IEnumerator BipLoop()
    {
        while (guidageActif)
        {
            if (!EnsureReadyRuntime())
            {
                yield return null;
                continue;
            }

            float dTotale = GetDistanceCouloir();

            // t = 0 (loin) -> t = 1 (proche)
            float t = Mathf.InverseLerp(distancePourBipLent, distancePourBipRapide, dTotale);
            t = Mathf.Clamp01(t);

            float interval = Mathf.Lerp(intervalMax, intervalMin, t);
            float volume = Mathf.Lerp(volumeMin, volumeMax, t);

            PlayDuplicatedBip(volume);

            yield return new WaitForSeconds(interval);
        }
    }

    private float GetDistanceCouloir()
    {
        // Mode bouton (plus de balise) => distance directe joueur -> bouton
        if (indexBalise >= relays.Count)
        {
            return Vector3.Distance(playerHead.position, sourceEmitter.transform.position);
        }

        // Sinon : distance totale “couloir” joueur -> balise courante -> ... -> bouton
        return DistanceTotaleVersSource(indexBalise);
    }

    private void PlayDuplicatedBip(float volume)
    {
        if (sourceEmitter == null) return;
        AudioClip clip = sourceEmitter.clip;
        if (clip == null) return;

        AudioSource outAudio = GetCurrentOutputAudioSource();
        if (outAudio == null) return;

        outAudio.spatialBlend = 1f;
        outAudio.playOnAwake = false;

        outAudio.volume = volume;
        outAudio.PlayOneShot(clip, volume);
    }

    private AudioSource GetCurrentOutputAudioSource()
    {
        if (indexAudioActif >= 0 && indexAudioActif < relays.Count)
        {
            var a = relays[indexAudioActif].audio;
            if (a != null) return a;
        }
        return sourceEmitter;
    }

    private float DistanceTotaleVersSource(int idx)
    {
        if (relays == null || relays.Count == 0) return 0f;
        if (playerHead == null || sourceEmitter == null) return 0f;

        idx = Mathf.Clamp(idx, 0, relays.Count - 1);

        Transform pIdx = relays[idx].point;
        Transform pLast = relays[relays.Count - 1].point;

        if (pIdx == null || pLast == null) return 0f;

        float sum = Vector3.Distance(playerHead.position, pIdx.position);

        for (int i = idx; i < relays.Count - 1; i++)
        {
            Transform a = relays[i].point;
            Transform b = relays[i + 1].point;
            if (a == null || b == null) continue;
            sum += Vector3.Distance(a.position, b.position);
        }

        sum += Vector3.Distance(pLast.position, sourceEmitter.transform.position);
        return sum;
    }

    private int FindNextRelayWithAudio(int startIdx)
    {
        if (relays == null || relays.Count == 0) return -1;

        startIdx = Mathf.Clamp(startIdx, 0, relays.Count - 1);

        for (int i = startIdx; i < relays.Count; i++)
        {
            if (relays[i].audio != null) return i;
        }
        return -1; // aucune balise n'a d'audio => sortie sur bouton
    }

    private void StopAllOutputAudio()
    {
        if (relays != null)
        {
            for (int i = 0; i < relays.Count; i++)
            {
                var a = relays[i].audio;
                if (a != null) a.Stop();
            }
        }

        if (sourceEmitter != null) sourceEmitter.Stop();
    }

    private void RefreshPlayer()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        playerHead = go != null ? go.transform : null;
    }

    private bool EnsureReadyForStart()
    {
        if (playerHead == null) RefreshPlayer();
        if (playerHead == null) return false;

        if (sourceEmitter == null) return false;
        if (relays == null || relays.Count == 0) return false;

        return true;
    }

    private bool EnsureReadyRuntime()
    {
        if (playerHead == null) RefreshPlayer();
        if (playerHead == null) return false;

        if (sourceEmitter == null) return false;
        if (relays == null || relays.Count == 0) return false;

        if (indexBalise < 0) indexBalise = 0;
        if (indexBalise > relays.Count) indexBalise = relays.Count;

        if (indexBalise < relays.Count && relays[indexBalise].point == null) return false;

        return true;
    }
}