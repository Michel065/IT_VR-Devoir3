using UnityEngine;

public class PlayerRelaySensor : MonoBehaviour
{
    [SerializeField] private GuidageChainController[] managers;

    private void OnTriggerEnter(Collider other)
    {
        RelayIdentifier id = other.GetComponentInParent<RelayIdentifier>();
        if (id == null) return;

        foreach (var m in managers)
        {
            m.OnEnterRelay(other.transform);
        }
    }
}