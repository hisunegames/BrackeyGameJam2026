using System;
using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    [SerializeField] private Transform camera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        camera = GameObject.FindGameObjectWithTag("PlayerCamera").transform;
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 targetPos = camera.position;
        targetPos.y = transform.position.y; // Keep the billboard at the same height
        transform.LookAt(targetPos);
        transform.Rotate(0, 180, 0);

    }
}
