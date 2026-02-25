using UnityEngine;

public class PlayerRelaySensor : MonoBehaviour
{
    [SerializeField] private GuidageChainController manager;
    [SerializeField] private string relayTag = "Relay";

    private void OnTriggerEnter(Collider other)
    {
        if (manager == null) return;

        Transform t = other.transform;
        while (t != null && !t.CompareTag(relayTag))
            t = t.parent;

        if (t == null) return;

        manager.OnEnterRelay(t);
    }
}