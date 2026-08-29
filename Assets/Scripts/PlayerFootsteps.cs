using System.Collections;
using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private AudioSource footstepSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(PlayFootsteps());
    }

    IEnumerator PlayFootsteps()
    {
        while (true)
        {
            float footstepPeriod = 0.35f;
            if (!playerController.IsCrouching && playerController.Motion.magnitude > 4.5f)
            {
                footstepPeriod *= 5.0f / playerController.Motion.magnitude;
                footstepSource.Play();
            }

            // float footstepDuration = 0.50f * (2.5f / playerController.Motion.magnitude);
            yield return new WaitForSeconds(footstepPeriod);
        }
    }
}
