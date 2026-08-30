using System.Collections.Generic;
using UnityEngine;

public class HidingSpotManager : MonoBehaviour
{
    [SerializeField] HidingSpot[] _hidingSpots;
    [SerializeField] int _openCount = 2;
    [SerializeField, Range(0f, 1f)] float _semiOpenFraction = 0.67f;

    public static HidingSpotManager Instance { get; private set; }

    bool _spotsOpenedThisEncounter;
    HidingSpotState _debugState = HidingSpotState.Closed;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // GameManager's CurrentSequence already defaults to Normal, so SetSequence(Normal) at
        // StartGame() is a no-op and never fires OnSequenceChanged - open explicitly instead.
        OpenAllSpots();
    }

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
            OpenAllSpots();
            _spotsOpenedThisEncounter = false;
        }
    }

    void OpenRandomSpots()
    {
        List<HidingSpot> pool = new List<HidingSpot>(_hidingSpots);
        int count = Mathf.Min(_openCount, pool.Count);

        HashSet<HidingSpot> spotsToOpen = new();
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            spotsToOpen.Add(pool[index]);
            pool.RemoveAt(index);
        }

        // Explicitly close everything not chosen - stores are all left open during Normal,
        // so Penalty can't just open its subset on top of that and call it done.
        foreach (HidingSpot spot in _hidingSpots)
        {
            if (spot == null)
                continue;

            if (spotsToOpen.Contains(spot))
                spot.SemiOpen(_semiOpenFraction);
            else
                spot.Close();
        }
    }

    void OpenAllSpots()
    {
        foreach (HidingSpot spot in _hidingSpots)
        {
            if (spot != null)
                spot.Open();
        }
    }

    void SemiOpenAllSpots()
    {
        foreach (HidingSpot spot in _hidingSpots)
        {
            if (spot != null)
                spot.SemiOpen(_semiOpenFraction);
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

    // Cycles Closed -> SemiOpen (crouch/crawl gap) -> Open (fully walkable) -> Closed, so all
    // three door states can be previewed without waiting for the real Normal/Penalty flow.
    public void DebugToggleAllSpots()
    {
        _debugState = (HidingSpotState)(((int)_debugState + 1) % 3);

        switch (_debugState)
        {
            case HidingSpotState.Closed:
                CloseAllSpots();
                break;
            case HidingSpotState.SemiOpen:
                SemiOpenAllSpots();
                break;
            case HidingSpotState.Open:
                OpenAllSpots();
                break;
        }

        Debug.Log($"[HidingSpotManager] Debug state -> {_debugState}");
    }
}
