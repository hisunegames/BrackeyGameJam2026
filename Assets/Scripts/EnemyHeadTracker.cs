using UnityEngine;

public class EnemyHeadTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform _headBone;
    [SerializeField] EnemyLineOfSight _lineOfSight;
    [SerializeField] Transform _target;

    [Header("Tracking Settings")]
    [SerializeField] float _turnSpeed = 180f;

    [Header("Front/Back Sprites")]
    [SerializeField] SpriteRenderer _frontSprite;
    [SerializeField] SpriteRenderer _backSprite;

    Quaternion _neutralLocalRotation;
    Transform _cameraTransform;

    void Awake()
    {
        if (_lineOfSight == null)
            _lineOfSight = GetComponent<EnemyLineOfSight>();

        if (_target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _target = player.transform;
        }

        GameObject playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera");
        if (playerCamera != null)
            _cameraTransform = playerCamera.transform;

        if (_headBone != null)
            _neutralLocalRotation = _headBone.localRotation;
    }

    void LateUpdate()
    {
        if (_headBone == null)
            return;

        Quaternion targetLocalRotation = _neutralLocalRotation;

        if (_lineOfSight.HasDetectedPlayer && _target != null)
        {
            Vector3 direction = _target.position - _headBone.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion worldLookRotation = Quaternion.LookRotation(direction);
                targetLocalRotation = _headBone.parent != null
                    ? Quaternion.Inverse(_headBone.parent.rotation) * worldLookRotation
                    : worldLookRotation;
            }
        }

        _headBone.localRotation = Quaternion.RotateTowards(_headBone.localRotation, targetLocalRotation, _turnSpeed * Time.deltaTime);

        UpdateSpriteFacing();
    }

    void UpdateSpriteFacing()
    {
        if (_frontSprite == null || _backSprite == null || _cameraTransform == null)
            return;

        Vector3 toCamera = _cameraTransform.position - _headBone.position;
        bool showFront = Vector3.Dot(_headBone.forward, toCamera) > 0f;

        _frontSprite.enabled = showFront;
        _backSprite.enabled = !showFront;
    }
}
