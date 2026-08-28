using System.Collections.Generic;
using UnityEngine;

public class HidingSpotManager : MonoBehaviour
{
    [SerializeField] HidingSpot[] _hidingSpots;
    [SerializeField] int _openCount = 2;

    bool _spotsOpenedThisEncounter;

    void OnEnable()
    {
        GameManager.OnSequenceChanged += HandleSequenceChanged;
    }

    void OnDisable()
    {
        GameManager.OnSequenceChanged -= HandleSequenceChanged;
    }

    void HandleSequenceChanged(GameplaySequence sequence)
    {
        if (sequence == GameplaySequence.Penalty)
        {
            if (_spotsOpenedThisEncounter)
                return;

            OpenRandomSpots();
            _spotsOpenedThisEncounter = true;
        }
        else if (sequence == GameplaySequence.Normal)
        {
            CloseAllSpots();
            _spotsOpenedThisEncounter = false;
        }
    }

    void OpenRandomSpots()
    {
        List<HidingSpot> pool = new List<HidingSpot>(_hidingSpots);
        int count = Mathf.Min(_openCount, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            pool[index].Open();
            pool.RemoveAt(index);
        }
    }

    void CloseAllSpots()
    {
        foreach (HidingSpot spot in _hidingSpots)
        {
            if (spot != null)
                spot.Close();
        }
    }
}
