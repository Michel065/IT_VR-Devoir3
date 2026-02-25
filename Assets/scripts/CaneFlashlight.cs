using UnityEngine;
using UnityEngine.InputSystem;

public class CaneFlashlight : MonoBehaviour
{
    [Header("XR input")]
    public InputActionProperty toggleAction; // bind: <XRController>{RightHand}/primaryButton ou gripPressed

    [Header("Aim axis (from -> to)")]
    public Transform from; // manche
    public Transform to;   // cube

    [Header("Light")]
    public Light flashlight;
    public bool startOn = false;

    void Awake()
    {
        if (flashlight != null)
            flashlight.enabled = startOn;
    }

    void OnEnable()
    {
        if (toggleAction.action != null)
            toggleAction.action.Enable();
    }

    void OnDisable()
    {
        if (toggleAction.action != null)
            toggleAction.action.Disable();
    }

    void Update()
    {
        if (from != null && to != null && flashlight != null)
        {
            Vector3 dir = (to.position - from.position);
            if (dir.sqrMagnitude > 0.0001f)
                flashlight.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }

        if (toggleAction.action != null && toggleAction.action.WasPressedThisFrame())
        {
            if (flashlight != null)
                flashlight.enabled = !flashlight.enabled;
        }
    }
}