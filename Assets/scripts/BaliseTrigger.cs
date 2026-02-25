using UnityEngine;

public class BaliseTrigger : MonoBehaviour
{
    [SerializeField] private GuidageButton guidageButton;
    [SerializeField] private string playerTag = "PlayerHead";

    private bool dejaDeclenche = false;

    private void OnTriggerEnter(Collider other)
    {
        if (dejaDeclenche) return;
        if (!other.CompareTag(playerTag)) return;

        dejaDeclenche = true;
        guidageButton.PasserBaliseSuivante();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        dejaDeclenche = false;
    }
}