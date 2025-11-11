using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class ItemFactory : MonoBehaviour, IItemFactory
{
    [System.Serializable]
    public class ItemPrefabEntry
    {
        public EItemType type;
        public GameObject prefab;
    }

    [Header("Prefab Registration")]
    [SerializeField] private List<ItemPrefabEntry> m_itemPrefabList;

    private Dictionary<EItemType, GameObject> m_prefabDic;
    private Dictionary<EItemType, IObjectPool<IItem>> m_poolDic;

    void Awake()
    {
        InitializeFactory();
    }

    private void InitializeFactory()
    {
        m_prefabDic = new Dictionary<EItemType, GameObject>();
        m_poolDic = new Dictionary<EItemType, IObjectPool<IItem>>();

        foreach (var entry in m_itemPrefabList)
        {
            if (entry.prefab.GetComponent<IItem>() == null)
            {
                Debug.LogError($"[Factory] {entry.type} 프리팹에 IEnemy가 없습니다!");
                continue;
            }
            m_prefabDic[entry.type] = entry.prefab;
        }

        foreach (EItemType type in m_prefabDic.Keys)
        {
            m_poolDic[type] = new ObjectPool<IItem>(
                createFunc: () => CreateNewItem(type),
                actionOnGet: (enemy) => OnGetFromPool(enemy),
                actionOnRelease: (enemy) => OnReleaseToPool(enemy),
                actionOnDestroy: (enemy) => OnDestroyFromPool(enemy),
                maxSize: 30
            );
        }
    }

    public IItem GetItem(EItemType type)
    {
        if (!m_poolDic.ContainsKey(type))
        {
            Debug.LogError($"[Factory] {type} 풀이 없습니다.");
            return null;
        }
        return m_poolDic[type].Get();
    }

    private IItem CreateNewItem(EItemType type)
    {
        GameObject newInstance = Instantiate(m_prefabDic[type]);
        IItem item = newInstance.GetComponent<IItem>();

        item.SetReleaseAction((e) => m_poolDic[type].Release(e));

        return item;
    }

    private void OnGetFromPool(IItem item)
    {
        // Spawner가 Initialize()에서 SetActive(true)를 할 것임
    }

    private void OnReleaseToPool(IItem item)
    {
        item.GetGameObject().SetActive(false);
    }

    private void OnDestroyFromPool(IItem item)
    {
        Destroy(item.GetGameObject());
    }
}