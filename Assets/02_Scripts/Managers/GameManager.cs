using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private readonly string playSceneName = "PlayScene";
    //private readonly string lobbySceneName = "LobbyScene";

    [Header("데이터 참조")]
    //[SerializeField] private GameSettingsSO gameSettings;
    //[SerializeField] private LevelDataSO levelData;

    [Header("이벤트 구독")]
    //[SerializeField] private VoidEventChannelSO onPauseRequest;
    //[SerializeField] private VoidEventChannelSO onPlayerDie;    //PlayerStatsManager 가 발행

    //[Header("이벤트 발행")]
    //[SerializeField] private VoidEventChannelSO onGameStart;
    //[SerializeField] private VoidEventChannelSO onGamePause;
    //[SerializeField] private VoidEventChannelSO onGameResume;
    //[SerializeField] private VoidEventChannelSO onGameOver;

    //[SerializeField] private VoidEventChannelSO onReturnToLobby;        //PlayerStatManager 가 구독

    private bool isPause = false;
    private bool isGameOver = false;
    private void Start()
    {
        //LoadLobbyScene();
    }
    private void OnEnable()
    {
        //onPlayerDie.OnEvent += HandleGameOver;
        //onPauseRequest.OnEvent += TogglePause;

        //씬 전환관련
        SceneManager.sceneLoaded += HandleOnSceneLoad;
    }
    private void OnDisable()
    {
        //onPlayerDie.OnEvent -= HandleGameOver;
        //onPauseRequest.OnEvent -= TogglePause;
        SceneManager.sceneLoaded -= HandleOnSceneLoad;
    }
    public void HandleOnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        isPause = false;
        isGameOver = false;
        if (scene.name == playSceneName)
        {
            Time.timeScale = 1.0f;
            //if (onGameStart != null) onGameStart.Raised();
        }
    }
    public void LoadPlayScene()
    {
        //Managers.Sound.PlaySFX(ESfxName.SceneChange);
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(playSceneName);
    }
    //public void LoadLobbyScene()
    //{
    //    //Managers.Sound.PlaySFX(ESfxName.SceneChange);
    //    //onReturnToLobby.Raised();
    //    Time.timeScale = 1.0f;
    //    SceneManager.LoadScene(lobbySceneName);
    //}
    public void TogglePause()
    {
        if (SceneManager.GetActiveScene().name != playSceneName) return;
        if (isGameOver)
        {
            LoadPlayScene();
            return;
        }
        isPause = !isPause;
        if (isPause)
        {
            Time.timeScale = 0.0f;
            //onGamePause.Raised();
        }
        else
        {
            Time.timeScale = 1.0f;
            //onGameResume.Raised();
        }
    }
    public void HandleGameOver()
    {
        if (isGameOver) return;
        Time.timeScale = 0.8f;
        isGameOver = true;
        //onGameOver.Raised();
    }
}
