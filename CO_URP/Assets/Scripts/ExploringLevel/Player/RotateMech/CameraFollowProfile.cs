using UnityEngine;

[CreateAssetMenu(fileName = "CameraProfile", menuName = "Camera/Camera Follow Profile")]
public class CameraFollowProfile : ScriptableObject
{
    [Header("Body - Transposer")]
    public Vector3 followOffset = new Vector3(0, 5, -10);

    [Header("Aim - Composer")]
    public Vector3 trackedObjectOffset = Vector3.zero;
    [Range(0f, 1f)] public float screenX = 0.5f;
    [Range(0f, 1f)] public float screenY = 0.5f;

    [Header("ÇÐ»»¹ý¶É")]
    public float blendDuration = 0.5f;
}
