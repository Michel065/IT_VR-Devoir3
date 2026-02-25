using UnityEngine;
using UnityEngine.InputSystem;

public class CaneTipTriggerTap : MonoBehaviour
{
    public InputActionProperty tapAction; // bind: triggerPressed ou gripPressed

    public string defaultFolder = "son/Default";
    public string metalFolder = "son/Metal";
    public string woodFolder = "son/Wood";
    public string stoneFolder = "son/Stone";

    public float cooldown = 0.08f;
    public float volume = 1f;
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    private AudioSource _audio;
    private float _nextTime;

    private AudioClip[] _defaultClips, _metalClips, _woodClips, _stoneClips;

    private SurfaceAudio _currentSurface; // surface touchée en ce moment

    void Awake()
    {
        _audio = GetComponent<AudioSource>();

        _defaultClips = Resources.LoadAll<AudioClip>(defaultFolder);
        _metalClips = Resources.LoadAll<AudioClip>(metalFolder);
        _woodClips = Resources.LoadAll<AudioClip>(woodFolder);
        _stoneClips = Resources.LoadAll<AudioClip>(stoneFolder);
    }

    void OnEnable()
    {
        if (tapAction.action != null) tapAction.action.Enable();
    }

    void OnDisable()
    {
        if (tapAction.action != null) tapAction.action.Disable();
    }

    void Update()
    {
        if (tapAction.action == null) return;

        if (tapAction.action.WasPressedThisFrame())
            Tap();
    }

    void Tap()
    {
        if (Time.time < _nextTime) return;
        _nextTime = Time.time + cooldown;

        AudioClip clip = PickClip(_currentSurface);
        if (clip == null) return;

        _audio.pitch = Random.Range(pitchRange.x, pitchRange.y);
        _audio.PlayOneShot(clip, volume);
    }

    AudioClip PickClip(SurfaceAudio surface)
    {
        if (surface == null) return PickRandom(_defaultClips);

        switch (surface.surfaceType)
        {
            case SurfaceType.Metal: return PickRandom(_metalClips);
            case SurfaceType.Wood: return PickRandom(_woodClips);
            case SurfaceType.Stone: return PickRandom(_stoneClips);
            default: return PickRandom(_defaultClips);
        }
    }

    AudioClip PickRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    void OnTriggerEnter(Collider other)
    {
        var s = other.GetComponent<SurfaceAudio>();
        if (s != null) _currentSurface = s;
    }

    void OnTriggerStay(Collider other)
    {
        var s = other.GetComponent<SurfaceAudio>();
        if (s != null) _currentSurface = s;
    }

    void OnTriggerExit(Collider other)
    {
        var s = other.GetComponent<SurfaceAudio>();
        if (s != null && _currentSurface == s) _currentSurface = null;
    }
}