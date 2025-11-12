using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }
    private Dictionary<string, IPoolTypeCheckable> pools = new Dictionary<string, IPoolTypeCheckable>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CreatePool<T>(T prefab, int initCount, Transform parent = null) where T : MonoBehaviour
    {
        if (prefab == null) return;
        string key = prefab.name;
        if (pools.ContainsKey(key)) return;
        pools.Add(key, new ObjectPool<T>(prefab, initCount, parent));
    }

    public T GetFromPool<T>(T prefab) where T : MonoBehaviour
    {
        if (prefab == null) return null;
        string key = prefab.name;
        if (!pools.TryGetValue(key, out var box)) return null;
        var pool = box as ObjectPool<T>;
        if (pool == null) return null;
        return pool.Dequeue();
    }

    public void ReturnToPool<T>(T instance) where T : MonoBehaviour
    {
        if (instance == null) return;
        if (!pools.ContainsKey(instance.name))
        {
            Destroy(instance.gameObject);
            return;
        }
        pools[instance.name].EnqueueAfterTypeCheck(instance);
    }
}
