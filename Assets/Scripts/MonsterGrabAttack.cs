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

    bool _hasGrabbed;

    void Awake()
    {
        if (_agent == null)
            _agent = GetComponent<NavMeshAgent>();
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

        _hasGrabbed = true;
        StartCoroutine(GrabSequence(player));
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

        // Raising priority above the player's vcam (which sits at the default/disabled 0) is
        // enough for CinemachineBrain to blend over automatically - no manual camera swap needed.
        if (_grabCamera != null)
            _grabCamera.Priority = _grabCameraPriority;

        if (_animator != null)
            _animator.SetTrigger(_grabTrigger);

        if (_audioSource != null && _grabScreamSound != null)
            _audioSource.PlayOneShot(_grabScreamSound);

        // Placeholder beat before the kill lands - swap this wait for a real grab/scream cutscene later.
        yield return new WaitForSeconds(_cutsceneDuration);

    }
}
