using UnityEngine;

public class Managers : MonoBehaviour
{
    public static Managers Instance { get; private set; }
    [Header("매니저 프리팹")]
    //[SerializeField] private GameObject dataManagerPrefab;
    [SerializeField] private GameObject soundManagerPrefab;
    [SerializeField] private GameObject poolManagerPrefab;
    [SerializeField] private GameObject inputManagerPrefab;
    [SerializeField] private GameObject gameManagerPrefab;
    //[SerializeField] private GameObject inventoryManagerPrefab;
    //[SerializeField] private GameObject playerStatsManagerPrefab;

    //public static DataManager Data { get; private set; }
    public static SoundManager Sound { get; private set; }
    public static PoolManager Pool { get; private set; }
    public static InputManager Input { get; private set; }
    public static GameManager Game { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        //매니저들 생성
        //if (dataManagerPrefab != null)
        //{
        //    GameObject dataGo = Instantiate(dataManagerPrefab, transform);
        //    Data = dataGo.GetComponent<DataManager>();
        //}
        if (soundManagerPrefab != null)
        {
            GameObject dataGo = Instantiate(soundManagerPrefab, transform);
            Sound = dataGo.GetComponent<SoundManager>();
        }
        if (poolManagerPrefab != null)
        {
            GameObject poolGo = Instantiate(poolManagerPrefab, transform);
            Pool = poolGo.GetComponent<PoolManager>();
        }
        if (inputManagerPrefab != null)
        {
            GameObject inputGo = Instantiate(inputManagerPrefab, transform);
            Input = inputGo.GetComponent<InputManager>();
        }

        if (gameManagerPrefab != null)
        {
            GameObject gameGo = Instantiate(gameManagerPrefab, transform);
            Game = gameGo.GetComponent<GameManager>();
        }
        //if (inventoryManagerPrefab != null)
        //{
        //    GameObject invenGo = Instantiate(inventoryManagerPrefab, transform);
        //}
        //if (playerStatsManagerPrefab != null)
        //{
        //    GameObject statsGo = Instantiate(playerStatsManagerPrefab, transform);
        //}
        //if (Data != null)
        //{
        //    Data.Init();
        //}

    }

}