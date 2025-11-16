using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private readonly string playSceneName = "PlayScene";

    [Header("데이터 참조")]
    [SerializeField] private GameData gameData;

    [Header("이벤트 구독")]
    [SerializeField] private VoidEventChannelSO onPauseRequest;
    [SerializeField] private IItemEventChannelSO onTouchItem;
    [SerializeField] private VoidEventChannelSO onPlayerDie;
    [SerializeField] private FloatEventChannelSO onSuccess;

    [Header("이벤트 발행")]
    [SerializeField] private IntEventChannelSO onGoldTouch;
    [SerializeField] private VoidEventChannelSO onRedBallTouch;
    [SerializeField] private VoidEventChannelSO onGreenBallTouch;
    [SerializeField] private VoidEventChannelSO onBlueBallTouch;
    [SerializeField] private VoidEventChannelSO onBlackBallTouch;
    [SerializeField] private VoidEventChannelSO onFakeBlackBallTouch;
    [SerializeField] private VoidEventChannelSO onWallTouch;
    [SerializeField] private IntEventChannelSO onInitGoldText;
    [SerializeField] private VoidEventChannelSO onGameStart;
    [SerializeField] private VoidEventChannelSO onGameOver;
    [SerializeField] private FloatEventChannelSO onGameSuccess;

    private bool isPause = false;
    private bool isGameOver = false;

    private readonly int getGoldAmount = 5;
    private void Start()
    {
        float bestTime = PlayerPrefs.GetFloat("BestTime", 300.0f);
        int gold = PlayerPrefs.GetInt("Gold", gameData.Gold);
        gameData.SetGold(gold);
        gameData.SetBestTime(bestTime);
        onInitGoldText.Raise(gameData.Gold);
        Managers.Sound.PlayBGM(EBgmName.Bgm, true);

    }
    private void OnEnable()
    {
        onPlayerDie.OnEvent += HandleGameOver;
        onPauseRequest.OnEvent += TogglePause;
        onTouchItem.OnEvent += HandleItemTouch;
        onSuccess.OnEvent += HandleSuccess;
        //씬 전환관련
        SceneManager.sceneLoaded += HandleOnSceneLoad;
    }
    private void OnDisable()
    {
        onPlayerDie.OnEvent -= HandleGameOver;
        onPauseRequest.OnEvent -= TogglePause;
        onTouchItem.OnEvent -= HandleItemTouch;
        onSuccess.OnEvent -= HandleSuccess;
        SceneManager.sceneLoaded -= HandleOnSceneLoad;
    }
    private void HandleSuccess(float timeResult)
    {
        Time.timeScale = 0.2f;
        if (timeResult < gameData.BestTime)
        {
            gameData.SetBestTime(timeResult);
        }
        onGameSuccess.Raise(gameData.BestTime);
    }
    private void HandleItemTouch(IItem touchedItem)
    {
        var itemType = touchedItem.Type;
        switch (itemType)
        {
            case EItemType.Gold:
                gameData.AddGold(getGoldAmount);
                onGoldTouch.Raise(gameData.Gold);
                break;
            case EItemType.Red:
                onRedBallTouch.Raise();
                break;
            case EItemType.Blue:
                onBlueBallTouch.Raise();
                break;
            case EItemType.Black:
                onBlackBallTouch.Raise();
                break;
            case EItemType.FakeBlack:
                onFakeBlackBallTouch.Raise();
                break;
            case EItemType.Green:
                onGreenBallTouch.Raise();
                break;
            case EItemType.Wall:
                Managers.Sound.PlaySFX(ESfxName.WallTouch);
                break;
            case EItemType.Stepper:
                Managers.Sound.PlaySFX(ESfxName.StepperTouch);
                break;
        }
    }
    public void HandleOnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        isPause = false;
        isGameOver = false;
        if (scene.name == playSceneName)
        {
            Time.timeScale = 1.0f;
            if (onGameStart != null) onGameStart.Raise();
            onInitGoldText.Raise(gameData.Gold);
        }
    }
    public void LoadPlayScene()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(playSceneName);
    }

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
        Time.timeScale = 0.2f;
        isGameOver = true;
        onGameOver.Raise();
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    private void OnApplicationQuit()
    {
        PlayerPrefs.SetFloat("BestTime", gameData.BestTime);
        PlayerPrefs.SetInt("Gold", gameData.Gold);
    }
}
