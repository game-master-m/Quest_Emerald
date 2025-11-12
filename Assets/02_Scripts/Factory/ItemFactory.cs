using UnityEngine;
using System.Collections.Generic;

public class ItemFactory : MonoBehaviour, IItemFactory
{
    [System.Serializable]
    public class ItemPrefabEntry
    {
        public EItemType type;
        public GameObject prefab;
        public int initCount = 5;
    }

    [Header("Prefab Registration")]
    [SerializeField] private List<ItemPrefabEntry> m_itemPrefabList;

    private Transform m_poolRoot;

    private Dictionary<EItemType, IItem> m_prefabDic;

    void Awake()
    {
        InitializeFactory();
    }

    private void InitializeFactory()
    {
        if (Managers.Pool == null)
        {
            Debug.LogError("[ItemFactory] Managers.Pool이 초기화되지 않았습니다. 씬에 Managers 프리팹이 있는지 확인하세요.");
            this.enabled = false;
            return;
        }

        if (m_poolRoot == null)
        {
            m_poolRoot = Managers.Pool.transform;
        }

        m_prefabDic = new Dictionary<EItemType, IItem>();

        foreach (var entry in m_itemPrefabList)
        {
            IItem itemComponent = entry.prefab.GetComponent<IItem>();
            if (itemComponent == null)
            {
                Debug.LogError($"[Factory] {entry.type} 프리팹에 IItem이 없습니다!");
                continue;
            }

            MonoBehaviour itemMono = itemComponent as MonoBehaviour;
            if (itemMono == null)
            {
                Debug.LogError($"[Factory] {entry.type}의 IItem 컴포넌트가 MonoBehaviour가 아닙니다!");
                continue;
            }

            Managers.Pool.CreatePool(itemMono, entry.initCount, m_poolRoot);
            m_prefabDic[entry.type] = itemComponent;
        }
    }

    public IItem GetItem(EItemType type)
    {
        if (!m_prefabDic.TryGetValue(type, out IItem prefab))
        {
            Debug.LogError($"[Factory] {type}에 해당하는 프리팹이 등록되지 않았습니다.");
            return null;
        }

        MonoBehaviour prefabMono = prefab as MonoBehaviour;
        IItem newItem = Managers.Pool.GetFromPool(prefabMono) as IItem;

        if (newItem == null)
        {
            Debug.LogError($"[Factory] PoolManager에서 {type} 타입의 아이템을 가져오는데 실패했습니다.");
            return null;
        }

        newItem.SetReleaseAction((itemToReturn) =>
        {
            MonoBehaviour itemMono = itemToReturn as MonoBehaviour;
            if (itemMono != null)
            {
                Managers.Pool.ReturnToPool(itemMono);
            }
        });

        return newItem;
    }
}