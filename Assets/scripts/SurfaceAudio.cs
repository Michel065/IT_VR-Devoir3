using UnityEngine;

public enum SurfaceType
{
    Default,
    Metal,
    Wood,
    Stone,
    Gravel,
    Water
}

public class SurfaceAudio : MonoBehaviour
{
    public SurfaceType surfaceType;
}