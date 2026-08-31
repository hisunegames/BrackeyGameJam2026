using UnityEngine;

// Keeps this transform's world rotation fixed regardless of its parent's rotation. Used for the
// flat, top-down indicator sprites the minimap camera renders (see MinimapCameraFollow) - without
// this, an indicator parented under an NPC tilts/skews on the minimap every time that NPC turns
// to face a direction, since it otherwise just inherits the parent's rotation like any other child.
public class LockedWorldRotation : MonoBehaviour
{
    Quaternion _lockedRotation;

    void Awake()
    {
        _lockedRotation = transform.rotation;
    }

    void LateUpdate()
    {
        transform.rotation = _lockedRotation;
    }
}
