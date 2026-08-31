using System.Collections.Generic;
using UnityEngine;

// Shared helpers for the various per-day spawners (NPCManager, ConfiscationNpcSpawner,
// NotepadPodiumSpawner, QuestItemSpawner) - they all destroy and re-spawn a tracked list of
// instances, and some also need a shuffled spawn-point order.
public static class SpawnerUtility
{
    public static void DestroyAndClear(List<GameObject> spawned)
    {
        foreach (GameObject instance in spawned)
        {
            if (instance != null)
                Object.Destroy(instance);
        }

        spawned.Clear();
    }

    public static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // Shared by NPCManager and ConfiscationNpcSpawner - samples a random point within
    // NPCBounds and instantiates the prefab there. Registration/identity setup stays with each
    // caller since that part genuinely differs between them.
    public static bool TrySpawnInBounds(GameObject prefab, out GameObject spawned)
    {
        spawned = null;
        if (prefab == null || NPCBounds.Instance == null)
            return false;

        if (!NavMeshSpawner.TryGetRandomPointInPolygon(NPCBounds.Instance.Center, NPCBounds.Instance.Polygon, out Vector3 point))
            return false;

        spawned = Object.Instantiate(prefab, point, Quaternion.identity);
        return true;
    }
}
