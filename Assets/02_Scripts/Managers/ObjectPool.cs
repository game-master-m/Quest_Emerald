using System.Collections.Generic;
using UnityEngine;

public interface IPoolTypeCheckable
{
    void EnqueueAfterTypeCheck(MonoBehaviour obj);
    int GetCurrentPoolSize();
}
public class ObjectPool<T> : IPoolTypeCheckable where T : MonoBehaviour
{
    private T prefab;
    private Queue<T> poolQueue = new Queue<T>();
    public Transform Root;

    public ObjectPool(T prefab, int initCount, Transform parent = null)
    {
        this.prefab = prefab;
        string name = prefab.name;
        Root = new GameObject($"{name}_pool").transform;
        if (parent != null) Root.SetParent(parent, false);

        for (int i = 0; i < initCount; i++)
        {
            T inst = GameObject.Instantiate(prefab, Root);
            inst.name = prefab.name;
            inst.gameObject.SetActive(false);
            poolQueue.Enqueue(inst);
        }
    }

    public T Dequeue()
    {
        if (poolQueue.Count == 0)
        {
            //T instance = GameObject.Instantiate(prefab, Root);
            //instance.name = prefab.name;
            //return instance;
            return null;
        }
        T inst = poolQueue.Dequeue();
        inst.gameObject.SetActive(true);
        return inst;
    }

    public void Enqueue(T prefab)
    {
        if (prefab == null) return;
        prefab.gameObject.SetActive(false);
        poolQueue.Enqueue(prefab);
    }

    public void EnqueueAfterTypeCheck(MonoBehaviour obj)
    {
        if (obj is T typeObj)
        {
            Enqueue(typeObj);
        }
    }

    public int GetCurrentPoolSize()
    {
        return poolQueue.Count;
    }
}
