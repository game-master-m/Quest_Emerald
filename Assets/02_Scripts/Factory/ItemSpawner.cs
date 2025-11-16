using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class ItemSpawner : MonoBehaviour
{
    [Header("Factory Dependency")]
    [SerializeField] private MonoBehaviour m_factoryProvider;
    private IItemFactory m_factory;

    [System.Serializable]
    public class ItemSpawnRule
    {
        public EItemType type;
        public int maxAliveCount;
        public float itemRadius = 2.0f;
    }

    [Header("Spawn Rules")]
    [SerializeField] private List<ItemSpawnRule> m_spawnRules;
    [SerializeField] private float m_spawnInterval = 1.5f;
    [SerializeField] private Vector2 m_spawnAreaRange = new Vector2(50f, 50f);
    [SerializeField] private float m_yOffset = 10.0f;
    [SerializeField] private int m_maxSpawnAttempts = 10;

    [Header("이벤트 구독")]
    [SerializeField] private IItemEventChannelSO onTouchItem;
    [SerializeField] private VoidEventChannelSO onBlackBallRequest;

    private Dictionary<EItemType, List<IItem>> m_activeItems = new Dictionary<EItemType, List<IItem>>();
    private Dictionary<EItemType, ItemSpawnRule> m_ruleMap = new Dictionary<EItemType, ItemSpawnRule>();

    private float m_timer;
    private List<ItemSpawnRule> m_availableRulesCache = new List<ItemSpawnRule>();

    private ItemSpawnRule[] blackBallSpawnRules = new ItemSpawnRule[2];

    private bool isBlackSpawn = false;

    public Vector2 SpawnAreaRange => m_spawnAreaRange;
    public Dictionary<EItemType, List<IItem>> ActiveItems => m_activeItems;
    public Action<IItem> OnItemSpawned;
    void Awake()
    {
        blackBallSpawnRules[0] = new ItemSpawnRule();
        blackBallSpawnRules[1] = new ItemSpawnRule();

        m_factory = m_factoryProvider.GetComponent<IItemFactory>();
        if (m_factory == null)
        {
            return;
        }

        m_activeItems.Clear();
        m_ruleMap.Clear();
        foreach (var rule in m_spawnRules)
        {
            m_activeItems[rule.type] = new List<IItem>();
            m_ruleMap[rule.type] = rule;
        }

        onTouchItem.OnEvent += HandleItemCollected;
        onBlackBallRequest.OnEvent += BlackBallSpawn;
    }

    void OnDestroy()
    {
        onTouchItem.OnEvent -= HandleItemCollected;
        onBlackBallRequest.OnEvent -= BlackBallSpawn;

        ReturnToPoolAll();
    }

    private void ReturnToPoolAll()
    {
        if (m_factory != null && m_activeItems != null)
        {
            foreach (var itemList in m_activeItems.Values)
            {
                foreach (var item in itemList.ToList())
                {
                    m_factory.ReturnItem(item);
                }
            }
            m_activeItems.Clear();
        }
    }

    private void BlackBallSpawn()
    {
        if (isBlackSpawn) return;
        isBlackSpawn = true;
        blackBallSpawnRules[0].type = EItemType.Black;
        blackBallSpawnRules[1].type = EItemType.FakeBlack;
        m_activeItems[EItemType.Black] = new List<IItem>();
        m_activeItems[EItemType.FakeBlack] = new List<IItem>();
        m_ruleMap[EItemType.Black] = blackBallSpawnRules[0];
        m_ruleMap[EItemType.FakeBlack] = blackBallSpawnRules[1];

        foreach (var rule in blackBallSpawnRules)
        {
            rule.itemRadius = 1;
            rule.maxAliveCount = 1;
        }
        for (int i = 0; i < 10; i++)
        {
            if (m_activeItems[EItemType.Black].Count != 0) break;
            FindSpotAndSpawn(blackBallSpawnRules[0]);
        }
        for (int i = 0; i < 10; i++)
        {
            if (m_activeItems[EItemType.FakeBlack].Count != 0) break;
            FindSpotAndSpawn(blackBallSpawnRules[1]);
        }
        m_spawnRules.Add(m_ruleMap[EItemType.Black]);
        m_spawnRules.Add(m_ruleMap[EItemType.FakeBlack]);
    }

    private void HandleItemCollected(IItem item)
    {
        if (item.Type == EItemType.Wall || item.Type == EItemType.Stepper) return;
        if (m_activeItems.TryGetValue(item.Type, out var itemList))
        {
            itemList.Remove(item);
        }

    }

    void Update()
    {
        m_timer += Time.deltaTime;
        if (m_timer >= m_spawnInterval)
        {
            m_timer = 0;
            TrySpawnItemsRandom();
        }
    }

    void TrySpawnItemsRandom()
    {
        m_availableRulesCache.Clear();
        foreach (var rule in m_spawnRules)
        {
            if (m_activeItems[rule.type].Count < rule.maxAliveCount)
            {
                m_availableRulesCache.Add(rule);
            }
        }

        if (m_availableRulesCache.Count > 0)
        {
            ItemSpawnRule ruleToSpawn = m_availableRulesCache[UnityEngine.Random.Range(0, m_availableRulesCache.Count)];
            FindSpotAndSpawn(ruleToSpawn);
        }
    }

    void FindSpotAndSpawn(ItemSpawnRule rule)
    {
        for (int i = 0; i < m_maxSpawnAttempts; i++)
        {
            Vector3 randomPos = GetRandomWorldPosition();

            if (!IsPositionOccupied(randomPos, rule.itemRadius))
            {
                IItem newItem = m_factory.GetItem(rule.type);
                if (newItem == null) return;

                newItem.Initialize(randomPos, Quaternion.identity);
                m_activeItems[rule.type].Add(newItem);
                OnItemSpawned?.Invoke(newItem);
                return;
            }
        }
    }

    bool IsPositionOccupied(Vector3 candidatePos, float candidateRadius)
    {
        foreach (var itemList in m_activeItems.Values)
        {
            foreach (var activeItem in itemList)
            {
                if (m_ruleMap.TryGetValue(activeItem.Type, out var activeItemRule))
                {
                    float requiredDistance = candidateRadius + activeItemRule.itemRadius;
                    float currentDistance = Vector2.Distance(
                        new Vector2(candidatePos.x, candidatePos.z),
                        new Vector2(activeItem.GetGameObject().transform.position.x, activeItem.GetGameObject().transform.position.z));

                    if (currentDistance < requiredDistance)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    Vector3 GetRandomWorldPosition()
    {
        float xPoint = UnityEngine.Random.Range(-m_spawnAreaRange.x, m_spawnAreaRange.x);
        float zPoint = UnityEngine.Random.Range(-m_spawnAreaRange.y, m_spawnAreaRange.y);
        Vector3 spawnPosition = transform.position + new Vector3(xPoint, 0, zPoint);
        spawnPosition.y = transform.position.y + m_yOffset;
        return spawnPosition;
    }
}