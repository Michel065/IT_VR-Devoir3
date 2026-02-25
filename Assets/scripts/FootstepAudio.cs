using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    [Header("XR refs")]
    public Transform head; // XR Camera (Main Camera)
    public float groundCheckDistance = 2.0f;

    [Header("Move detection")]
    public float moveThreshold = 0.01f;  // distance (m) par frame pour considérer "en mouvement"
    public float stopGraceTime = 0.12f;  // évite les micro-coupures

    [Header("Tag -> Loop clip")]
    public AudioClip defaultLoop;
    public AudioClip woodLoop;       // tag: "Wood"
    public AudioClip waterLoop;      // tag: "Water"
    public AudioClip gravelLoop;     // tag: "Gravel"
    public AudioClip concreteLoop;   // tag: "Concrete"
    public AudioClip grassLoop;      // tag: "Grass"

    private AudioSource _audio;
    private Vector3 _lastHeadPos;
    private float _stopTimer;

    private string _currentTag;
    private AudioClip _currentClip;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.spatialBlend = 1f; // 3D si tu veux; mets 0 si tu veux 2D
        _audio.loop = true;
    }

    void Start()
    {
        if (head == null) return;
        _lastHeadPos = head.position;
    }

    void Update()
    {
        if (head == null) return;

        bool isMoving = Vector3.Distance(head.position, _lastHeadPos) > moveThreshold;
        _lastHeadPos = head.position;

        bool isGrounded = TryGetGroundTag(out string groundTag);

        if (isMoving && isGrounded)
        {
            _stopTimer = stopGraceTime;

            AudioClip clip = ClipForTag(groundTag);
            if (clip == null)
            {
                StopLoop();
                return;
            }

            if (_currentClip != clip)
                StartLoop(clip, groundTag);
            else if (!_audio.isPlaying)
                StartLoop(clip, groundTag);
        }
        else
        {
            _stopTimer -= Time.deltaTime;
            if (_stopTimer <= 0f)
                StopLoop();
        }
    }

    bool TryGetGroundTag(out string tag)
    {
        tag = null;

        if (Physics.Raycast(head.position, Vector3.down, out RaycastHit hit, groundCheckDistance))
        {
            tag = hit.collider.tag;
            return true;
        }

        return false;
    }

    AudioClip ClipForTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return defaultLoop;

        if (tag == "Wood") return woodLoop != null ? woodLoop : defaultLoop;
        if (tag == "Water") return waterLoop != null ? waterLoop : defaultLoop;
        if (tag == "Gravel") return gravelLoop != null ? gravelLoop : defaultLoop;
        if (tag == "Concrete") return concreteLoop != null ? concreteLoop : defaultLoop;
        if (tag == "Grass") return grassLoop != null ? grassLoop : defaultLoop;

        return defaultLoop;
    }

    void StartLoop(AudioClip clip, string tag)
    {
        _currentTag = tag;
        _currentClip = clip;

        _audio.Stop();
        _audio.clip = clip;
        _audio.time = 0f;     // repart du début à chaque reprise / changement
        _audio.Play();
    }

    void StopLoop()
    {
        _currentTag = null;
        _currentClip = null;

        if (_audio.isPlaying)
            _audio.Stop();

        if (_audio.clip != null)
            _audio.time = 0f;
    }
}