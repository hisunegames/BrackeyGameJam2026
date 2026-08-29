using System.Collections;
using UnityEngine;

public class EntityDamage : MonoBehaviour
{

    [SerializeField] PlayerController _playerController;
    [SerializeField] float _damage = 10f;
    [SerializeField] float _debounceDuration = 1f;

    bool debounce = false;
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player") && debounce == false)
        {

            debounce = true;
            StartCoroutine(DamageCooldown());
            _playerController.TakeDamage(_damage);

        }
    }

    private IEnumerator DamageCooldown()
    {
        yield return new WaitForSeconds(_debounceDuration);
        debounce = false ;
    }

}
    