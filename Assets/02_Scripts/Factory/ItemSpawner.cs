using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Factory Dependency")]
    [SerializeField] private IItemFactory m_factory;


    [SerializeField] private float m_spawnInterval = 1.5f;
    private float m_timer;

    void Awake()
    {
        if (m_factory == null)
        {
            Debug.LogError("IEnemyFactory가 주입되지 않았습니다!", this);
            this.enabled = false;
        }
    }

    void Update()
    {
        m_timer += Time.deltaTime;
        if (m_timer >= m_spawnInterval)
        {
            m_timer = 0;
            SpawnRandomItem();
        }
    }

    private void SpawnRandomItem()
    {
        // 1. 타입 결정
        var types = System.Enum.GetValues(typeof(EItemType));
        EItemType randomType = (EItemType)types.GetValue(Random.Range(0, types.Length));

        // 2. 위치 결정
        Vector3 spawnPosition = transform.position + (Random.insideUnitSphere * 5.0f);
        spawnPosition.y = transform.position.y;

        // 3. [클린 코드] 팩토리에 '요청'
        IItem newItem = m_factory.GetItem(randomType);

        // 4. [클린 코드] 받은 '인터페이스'의 행위 호출
        newItem.Initialize(spawnPosition, Quaternion.identity);
    }
}