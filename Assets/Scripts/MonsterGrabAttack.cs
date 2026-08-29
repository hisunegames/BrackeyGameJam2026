using System.Collections;
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

        PlayerController player = _playerController != null ? _playerController : other.GetComponentInParent<PlayerController>();
        if (player == null || player.IsDead)
            return;

        _hasGrabbed = true;
        StartCoroutine(GrabSequence(player));
    }

    IEnumerator GrabSequence(PlayerController player)
    {
        if (_agent != null)
            _agent.isStopped = true;

        if (_animator != null)
            _animator.SetTrigger(_grabTrigger);

        if (_audioSource != null && _grabScreamSound != null)
            _audioSource.PlayOneShot(_grabScreamSound);

        // Placeholder beat before the kill lands - swap this wait for a real grab/scream cutscene later.
        yield return new WaitForSeconds(_cutsceneDuration);

        player.Die();
    }
}
