using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = System.Object;

public class MinimapManager : MonoBehaviour
{
    [SerializeField] private GameObject npcIndicatorPrefab;
    [SerializeField] private GameObject playerIndicator;
    [SerializeField] private GameObject player;

    private Dictionary<GameObject, GameObject> _npcIndicators = new Dictionary<GameObject, GameObject>();
    private float _refreshTime = 1.0f;
    private Matrix4x4 _worldToMinimapMatrix = new Matrix4x4(
        new Vector4(3.05f, 0.0f, 0.0f, 0.0f),
        new Vector4(0.0f, 0.0f, 0.0f, 0.0f),
        new Vector4(0.0f, 3.05f, 0.0f, 0.0f),
        new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
    );

    void Start()
    {
        _worldToMinimapMatrix = Matrix4x4.Rotate(Quaternion.Euler(0.0f, 0.0f, 90.0f)) * _worldToMinimapMatrix;
    }

    // Update is called once per frame
    void Update()
    {
        _refreshTime -= Time.deltaTime;
        if (_refreshTime <= 0)
        {
            _refreshTime = 1.0f;
            foreach (var kvp in _npcIndicators)
            {
                GameObject npc = kvp.Key;
                GameObject indicator = kvp.Value;

                if (npc == null)
                {
                    Destroy(indicator);
                    _npcIndicators.Remove(npc);
                }
            }
            GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
            foreach (GameObject npc in npcs)
            {
                if (!_npcIndicators.ContainsKey(npc))
                {
                    GameObject indicator = Instantiate(npcIndicatorPrefab, this.transform);
                    _npcIndicators.Add(npc, indicator);
                }
                Vector4 npcPos = _worldToMinimapMatrix * new Vector4(
                    npc.transform.position.x,
                    npc.transform.position.y,
                    npc.transform.position.z, 
                    1.0f);
                _npcIndicators[npc].GetComponent<RectTransform>().localPosition = new Vector3(npcPos.x, npcPos.y, 0.0f);
            }
            
            Vector4 playerPos = _worldToMinimapMatrix * new Vector4(
                player.transform.position.x,
                player.transform.position.y,
                player.transform.position.z, 
                1.0f);
            playerIndicator.GetComponent<RectTransform>().localPosition = new Vector3(playerPos.x, playerPos.y, 0.0f);
        }
    }
}