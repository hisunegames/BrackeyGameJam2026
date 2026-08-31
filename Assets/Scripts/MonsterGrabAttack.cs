using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class MonsterGrabAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerController _playerController;
    [SerializeField] NavMeshAgent _agent;
    [SerializeField] Animator _animator;
    [SerializeField] string _grabTrigger = "Grab";
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _grabScreamSound;

    [Header("Placeholder Cutscene")]
    [SerializeField] float _cutsceneDuration = 1.5f;
    [SerializeField] CinemachineCamera _grabCamera;
    [SerializeField] int _grabCameraPriority = 20;

    [Header("Grab Timing")]
    // Player has to stay in the trigger for this long before actually getting grabbed - stops
    // an instant grab if the monster happens to spawn on top of (or right next to) the player.
    [SerializeField] float _grabDelay = 0.5f;

    bool _hasGrabbed;
    PlayerController _playerInZone;
    float _zoneTimer;

    void Awake()
    {
        if (_agent == null)
            _agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (_hasGrabbed || _playerInZone == null)
            return;

        if (_playerInZone.IsDead)
        {
            _playerInZone = null;
            return;
        }

        _zoneTimer += Time.deltaTime;
        if (_zoneTimer < _grabDelay)
            return;

        _hasGrabbed = true;
        StartCoroutine(GrabSequence(_playerInZone));
    }

    void OnTriggerEnter(Collider other)
    {
        if (_hasGrabbed || !other.CompareTag("Player"))
            return;

        // Prefer whatever actually touched us - the pre-assigned _playerController field can be
        // a stale cross-prefab reference rather than the live scene instance.
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
            player = _playerController;
        if (player == null || player.IsDead)
            return;

        _playerInZone = player;
        _zoneTimer = 0f;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _playerInZone = null;
        _zoneTimer = 0f;
    }

    IEnumerator GrabSequence(PlayerController player)
    {
        if (_agent != null)
            _agent.isStopped = true;

        player.Die();
        
        Renderer[] playerrenderer = player.GetComponentsInChildren<Renderer>();

        foreach(Renderer x in playerrenderer)
        {
            x.enabled = false;
        }

        // Locks the camera for the rest of the cutscene - no need to unlock it after, since
        // Die() freezes the player permanently anyway.
        PlayerLook playerLook = player.GetComponent<PlayerLook>();
        if (playerLook != null)
            playerLook.SetLookEnabled(false);

        // Starts disabled on the prefab - its priority ties with the player's vcam (both default
        // to 0), and Cinemachine breaks priority ties in favor of whichever vcam was activated
        // most recently, so leaving it enabled from spawn made it steal the camera the instant
        // the monster was instantiated. Only activate it here, for the actual grab.
        if (_grabCamera != null)
        {
            _grabCamera.gameObject.SetActive(true);
            _grabCamera.Priority = _grabCameraPriority;
        }

        if (_animator != null)
            _animator.SetTrigger(_grabTrigger);

        if (_audioSource != null && _grabScreamSound != null)
            _audioSource.PlayOneShot(_grabScreamSound);

        // Placeholder beat before the kill lands - swap this wait for a real grab/scream cutscene later.
        yield return new WaitForSeconds(_cutsceneDuration);

    }
}
