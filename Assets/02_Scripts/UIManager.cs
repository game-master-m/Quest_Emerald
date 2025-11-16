using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EffectContainer
{
    public EEffectName EffectName;
    public PooledEffect psPrefab;
}


public class UIManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 effectOffset;
    [SerializeField] private Transform rootGameOverPannel;
    [SerializeField] private Transform rootClearPannel;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Image[] redBallImages;
    [SerializeField] private Image[] greenBallImages;
    [SerializeField] private Image[] blueBallImages;
    [SerializeField] private Image blackBallImage;
    [SerializeField] private Sprite collectedImage;
    [SerializeField] private List<EffectContainer> effectContainers;
    private Dictionary<EEffectName, PooledEffect> psDic;
    [SerializeField] private float timeUpdateCool = 0.1f;
    [SerializeField] private float maxTime = 300.0f;

    [SerializeField] private Button reStartButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;

    [Header("구독 이벤트")]
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

    [Header("발행 이벤트")]
    [SerializeField] private VoidEventChannelSO onBlackBallRequest;
    [SerializeField] private VoidEventChannelSO onPlayerDie;
    [SerializeField] private FloatEventChannelSO onSuccess;

    private Color redColorOri;
    private Color blueColorOri;
    private Color greenColorOri;
    private Color blackColorOri;

    private uint redCount = 0;
    private uint blueCount = 0;
    private uint greenCount = 0;

    private bool isBlackStart = false;

    private WaitForSeconds timeCoolSec;
    private float timer = 0.0f;
    private Coroutine timerCo;
    private bool isPlaying = false;
    private void Awake()
    {
        redColorOri = redBallImages[0].color;
        blueColorOri = blueBallImages[0].color;
        greenColorOri = greenBallImages[0].color;
        blackColorOri = blackBallImage.color;

        psDic = new();

        timeCoolSec = new WaitForSeconds(timeUpdateCool);

        reStartButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();

    }
    void Start()
    {
        reStartButton.onClick.AddListener(() => Managers.Game.LoadPlayScene());
        quitButton.onClick.AddListener(() => Managers.Game.QuitGame());

        blackBallImage.enabled = false;

        foreach (EffectContainer effectContainer in effectContainers)
        {
            if (!psDic.ContainsKey(effectContainer.EffectName))
            {
                psDic.Add(effectContainer.EffectName, effectContainer.psPrefab);
                Managers.Pool.CreatePool<PooledEffect>(effectContainer.psPrefab, 2, Managers.Pool.transform);
            }
        }

    }
    private void OnEnable()
    {
        onInitGoldText.OnEvent += InitGoldText;
        onGoldTouch.OnEvent += HandleGoldTouch;
        onRedBallTouch.OnEvent += HandleRedBallTouch;
        onGreenBallTouch.OnEvent += HandleGreenBallTouch;
        onBlueBallTouch.OnEvent += HandleBlueBallTouch;
        onBlackBallTouch.OnEvent += HandleBlackBallTouch;
        onFakeBlackBallTouch.OnEvent += HandelFakeBlackBallTouch;
        onGameStart.OnEvent += InitUI;
        onGameOver.OnEvent += HandleGameOver;
        onGameSuccess.OnEvent += HandleGameSuccess;

        rootClearPannel.gameObject.SetActive(false);
        rootGameOverPannel.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        onInitGoldText.OnEvent -= InitGoldText;
        onGoldTouch.OnEvent -= HandleGoldTouch;
        onRedBallTouch.OnEvent -= HandleRedBallTouch;
        onGreenBallTouch.OnEvent -= HandleGreenBallTouch;
        onBlueBallTouch.OnEvent -= HandleBlueBallTouch;
        onBlackBallTouch.OnEvent -= HandleBlackBallTouch;
        onFakeBlackBallTouch.OnEvent -= HandelFakeBlackBallTouch;
        onGameStart.OnEvent -= InitUI;
        onGameOver.OnEvent -= HandleGameOver;
        onGameSuccess.OnEvent -= HandleGameSuccess;
    }
    private void HandleGameSuccess(float bestTime)
    {
        isPlaying = false;
        rootClearPannel.gameObject.SetActive(true);
        currentScoreText.text = timer.ToString("F1");
        bestScoreText.text = bestTime.ToString("F1");
    }
    private void HandleGameOver()
    {
        rootGameOverPannel.gameObject.SetActive(true);
        isPlaying = false;
    }
    private void InitUI()
    {
        isPlaying = true;
        timer = 0.0f;
        timerCo = StartCoroutine(TimeUpdateCo());
    }
    private void BlackStartCheck()
    {
        if (blueCount >= 4 && redCount >= 4 && greenCount >= 4)
        {
            isBlackStart = true;
            blackBallImage.enabled = true;
            //factory에 검은공 소환요청
            onBlackBallRequest.Raise();
        }
    }
    private void InitGoldText(int gold)
    {
        goldText.text = $"{gold}";
    }
    private void HandelFakeBlackBallTouch()
    {
        //폭발사운드 및 효과
        Managers.Sound.PlaySFX(ESfxName.BombTouch);
        PooledEffect ps = Managers.Pool.GetFromPool(psDic[EEffectName.FakeBlack]);
        ps.PlayEffect(player);
        //게임 오바~
        onPlayerDie.Raise();
    }
    private void HandleBlackBallTouch()
    {
        UpdateBlackBallUI();
        Managers.Sound.PlaySFX(ESfxName.BallTouch);
        PooledEffect ps = Managers.Pool.GetFromPool(psDic[EEffectName.Black]);
        ps.PlayEffect(player);
        //게임 석세스~
        onSuccess.Raise(timer);
    }
    private void HandleBlueBallTouch()
    {
        blueCount++;
        UpdateBlueBallUI();
        Managers.Sound.PlaySFX(ESfxName.BallTouch);
        PooledEffect ps = Managers.Pool.GetFromPool(psDic[EEffectName.Blue]);
        ps.PlayEffect(player);
    }
    private void HandleGreenBallTouch()
    {
        greenCount++;
        UpdateGreenBallUI();
        Managers.Sound.PlaySFX(ESfxName.BallTouch);
        PooledEffect ps = Managers.Pool.GetFromPool(psDic[EEffectName.Green]);
        ps.PlayEffect(player);
    }
    private void HandleRedBallTouch()
    {
        redCount++;
        UpdateRedBallUI();
        Managers.Sound.PlaySFX(ESfxName.BallTouch);
        PooledEffect ps = Managers.Pool.GetFromPool(psDic[EEffectName.Red]);
        ps.PlayEffect(player);
    }
    private void HandleGoldTouch(int gold)
    {
        goldText.text = $"{gold}";
        Managers.Sound.PlaySFX(ESfxName.GoldTouch);
        PooledEffect ps = Managers.Pool.GetFromPool(psDic[EEffectName.Gold]);
        ps.PlayEffect(player);
    }
    private void UpdateRedBallUI()
    {
        if (redCount > 4) return;
        for (int i = 0; i < redCount; i++)
        {
            redBallImages[i].sprite = collectedImage;
            Color collectedRedballColor = new Color(redColorOri.r, redColorOri.g, redColorOri.b, 1.0f);
            redBallImages[i].color = collectedRedballColor;
        }
        BlackStartCheck();
    }
    private void UpdateBlueBallUI()
    {
        if (blueCount > 4) return;
        for (int i = 0; i < blueCount; i++)
        {
            blueBallImages[i].sprite = collectedImage;
            Color collectedBlueballColor = new Color(blueColorOri.r, blueColorOri.g, blueColorOri.b, 1.0f);
            blueBallImages[i].color = collectedBlueballColor;
        }
        BlackStartCheck();
    }
    private void UpdateGreenBallUI()
    {
        if (greenCount > 4) return;
        for (int i = 0; i < greenCount; i++)
        {
            greenBallImages[i].sprite = collectedImage;
            Color collectedGreenballColor = new Color(greenColorOri.r, greenColorOri.g, greenColorOri.b, 1.0f);
            greenBallImages[i].color = collectedGreenballColor;
        }
        BlackStartCheck();
    }
    private void UpdateBlackBallUI()
    {
        if (!isBlackStart) return;
        blackBallImage.sprite = collectedImage;
        Color collectedBlackballColor = new Color(blackColorOri.r, blackColorOri.g, blackColorOri.b, 1.0f);
        blackBallImage.color = collectedBlackballColor;
    }
    private IEnumerator TimeUpdateCo()
    {
        while (isPlaying)
        {
            yield return timeCoolSec;
            timer += timeUpdateCool;
            if (timer > maxTime) onPlayerDie.Raise();
            timeText.text = timer.ToString("F1");
        }
    }
}
