using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum GameState
{
    Title,
    Countdown,
    Playing,
    Result
}

public class GameManager : MonoBehaviour
{
    public GameState currentState = GameState.Title;
    private bool isAnimatingResult = false; // リザルトアニメ中フラグ

    [Header("UI")]
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private Table1 table;
    [SerializeField] private GameObject resultScreen;
        [SerializeField] private Text[] highScoreTexts; // 上位3つ用のTextをInspectorでセット
    [SerializeField] private Text timerText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text resultScoreText;
    [SerializeField] private Text happinessText;
    [SerializeField] private Text Sto1Text;
    [SerializeField] private Text Sto2Text;
    [SerializeField] private Text Sto3Text;
    [SerializeField] private Image Sto1;
    [SerializeField] private Image Sto2;
    [SerializeField] private Image Sto3;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip happinessSound;
    [SerializeField] private Text countdownText;
    [SerializeField] private Image bonusTimeText; // ボーナスタイムUI

    [Header("Audio")]
    [SerializeField] private AudioSource bgmSource; // BGM専用AudioSource
    [SerializeField] private AudioClip normalBGM;   // 通常BGM
    [SerializeField] private AudioClip bonusBGM;    // ボーナスタイムBGM

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 10f;

    private float timer = 0f;
    private int score = 0;
    private Request[] allRequests;

    private bool bonusTimeActive = false;   // 皿がなくなったらボーナスタイム

    public static GameManager Instance { get; private set; }

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

    void Start()
    {
        allRequests = FindObjectsOfType<Request>();
        SetState(GameState.Title);

        if (bonusTimeText != null)
            bonusTimeText.gameObject.SetActive(false); // 初期非表示

        // 通常BGM再生
        if (bgmSource != null && normalBGM != null)
        {
            bgmSource.clip = normalBGM;
            bgmSource.Play();
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case GameState.Title:
                if (Input.GetKeyDown(KeyCode.Space))
                    StartCountdownFromInput();
                break;

            case GameState.Playing:
                timer -= Time.deltaTime;
                timerText.text = Mathf.CeilToInt(timer).ToString();

                // --- 残り10秒でBGM速くする ---
                if (timer <= 10f && bgmSource != null)
                    bgmSource.pitch = 1.2f;   // 通常1.0 → 少し速く

                // ボーナスタイム開始判定
                if (!bonusTimeActive && AreAllDishesEmpty())
                {
                    bonusTimeActive = true;
                    Debug.Log("ボーナスタイム開始！");
                    if (bonusTimeText != null)
                        bonusTimeText.gameObject.SetActive(true); // ゲーム終了まで表示

                    // ボーナスタイムBGM切り替え
                    if (bgmSource != null && bonusBGM != null)
                    {
                        bgmSource.clip = bonusBGM;
                        bgmSource.Play();
                    }

                    foreach (var ctrl in FindObjectsOfType<StoTenImageController>())
                        ctrl.SetFreezeFace(true);
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("スペースで強制終了");
                    EndGame();
                    break;
                }

                if (timer <= 0f)
                    EndGame();
                break;

            case GameState.Result:
                if (!isAnimatingResult && Input.GetKeyDown(KeyCode.Space))
                    ResetToTitle();
                break;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetHighScores();
        }
    }
    
    private void ResetHighScores()
{
    for (int i = 0; i < 3; i++)
    {
        PlayerPrefs.SetInt($"HighScore{i}", 0);
    }
    PlayerPrefs.Save();
    Debug.Log("HighScores reset!");

    // リザルト画面に反映
    DisplayHighScores();
}


    private void StartCountdownFromInput()
    {
        table.ResetAllDishes();
        bonusTimeActive = false;

        if (bonusTimeText != null)
            bonusTimeText.gameObject.SetActive(false);

        SetState(GameState.Countdown);
    }

    private void ResetToTitle()
    {

        foreach (var ctrl in FindObjectsOfType<StoTenImageController>())
            ctrl.ResetImages();

        foreach (Transform child in titleScreen.transform)
            child.gameObject.SetActive(true);

        table.ResetAllDishes();

        foreach (var req in allRequests)
            req.ClearRequest();

        if (bonusTimeText != null)
            bonusTimeText.gameObject.SetActive(false);

        // 通常BGMに戻す
        if (bgmSource != null && normalBGM != null)
        {
            bgmSource.clip = normalBGM;
            bgmSource.Play();
        }

        happinessText.gameObject.SetActive(true);
        happinessText.color = new Color(happinessText.color.r, happinessText.color.g, happinessText.color.b, 1f);

        SetState(GameState.Title);
    }

    public void OnRightTurn()
    {
        if (currentState == GameState.Title)
        {
            StartCountdownFromInput();
            return;
        }

        // ボーナスタイム中の回転で加点
        if (currentState == GameState.Playing && bonusTimeActive)
        {
            AddScore(5); // 1回転ごとに +5点
            Debug.Log("ボーナスタイム加点 +5");
        }
    }

    public void OnLeftTurn()
    {
        if (currentState == GameState.Result && !isAnimatingResult)
            ResetToTitle();

        // 左回転もボーナスタイム加点可能にする場合はここに追加
        if (currentState == GameState.Playing && bonusTimeActive)
        {
            AddScore(5);
            Debug.Log("ボーナスタイム加点 +5 (左回転)");
        }
    }

    void StartGame()
    {
        timer = gameDuration;
        score = 0;
        bonusTimeActive = false;

        if (bonusTimeText != null)
            bonusTimeText.gameObject.SetActive(false);

        UpdateScoreUI();
        SetState(GameState.Playing);
    }

    void EndGame()
    {
        SetState(GameState.Result);

        // リザルトでBGM戻す
        if (bgmSource != null && normalBGM != null)
        {
            bgmSource.clip = normalBGM;
            bgmSource.pitch = 1.0f;
            bgmSource.Play();
        }

        StoTenImageController[] controllers = FindObjectsOfType<StoTenImageController>();
        int happiness = 0;

        for (int i = 0; i < controllers.Length && i < 3; i++)
        {
            int stoValue = controllers[i].GetStoValue();
            happiness += (stoValue - 5) * 100;

            Sprite stoSprite = Resources.Load<Sprite>($"stomach/sto_{stoValue}");
            if (stoSprite != null)
            {
                switch (i)
                {
                    case 0: Sto1.sprite = stoSprite; Sto1Text.text = stoValue * 10 + "%"; break;
                    case 1: Sto2.sprite = stoSprite; Sto2Text.text = stoValue * 10 + "%"; break;
                    case 2: Sto3.sprite = stoSprite; Sto3Text.text = stoValue * 10 + "%"; break;
                }
            }
        }
        Debug.Log($"Saving final score: {score} + {happiness} = {score + happiness}");
        SaveScore(score + happiness);   // 最終スコアを保存
        DisplayHighScores();           // リザルトで表示

        happinessText.text = $"HappyBounus: {happiness}";
        resultScoreText.text = $"Score: {score}";

        isAnimatingResult = true;
        StartCoroutine(AnimateScoreWithDelay(score, score + happiness, 2f));

        if (bonusTimeText != null)
            bonusTimeText.gameObject.SetActive(false); // リザルト画面では非表示
    }

    private IEnumerator AnimateScoreWithDelay(int from, int to, float delay)
    {
        yield return new WaitForSeconds(delay);

        int durationFrames = 30;
        if (happinessSound != null)
            audioSource.PlayOneShot(happinessSound);

        for (int f = 0; f <= durationFrames; f++)
        {
            int displayedScore = Mathf.RoundToInt(Mathf.Lerp(from, to, f / (float)durationFrames));
            resultScoreText.text = $"Score: {displayedScore}";
            yield return null;
        }
        resultScoreText.text = $"Score: {to}";
        isAnimatingResult = false;
        happinessText.gameObject.SetActive(false);
    }

    private bool AreAllDishesEmpty()
    {
        DishStock[] allDishes = FindObjectsOfType<DishStock>();
        foreach (var dish in allDishes)
        {
            if (dish.getStock()) return false;
        }
        return true;
    }

    void SetState(GameState newState)
    {
        currentState = newState;

        titleScreen.SetActive(newState == GameState.Title || newState == GameState.Countdown);
        resultScreen.SetActive(newState == GameState.Result);
        timerText.gameObject.SetActive(newState == GameState.Playing);
        scoreText.gameObject.SetActive(newState == GameState.Playing);
        resultScoreText.gameObject.SetActive(newState == GameState.Result);
        countdownText.gameObject.SetActive(newState == GameState.Countdown);

        bool enableRequests = (newState == GameState.Playing);
        foreach (var req in allRequests)
            req.IsEnabled = enableRequests;

        if (newState == GameState.Countdown)
            StartCountdown();
    }

    private void StartCountdown()
    {
        foreach (Transform child in titleScreen.transform)
            child.gameObject.SetActive(false);

        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        int count = 3;
        while (count > 0)
        {
            countdownText.text = count.ToString();
            countdownText.gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            count--;
        }
        countdownText.text = "Start!!";
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
        StartGame();
    }

    public void AddScore(int amount)
    {
        if (amount > 0 && correctSound != null)
            audioSource.PlayOneShot(correctSound);

        score += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        scoreText.text = $"Score: {score}";
    }
    
    private void SaveScore(int finalScore)
    {
        // 既存スコアを取得
        List<int> scores = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            scores.Add(PlayerPrefs.GetInt($"HighScore{i}", 0));
        }

        // 新しいスコアを追加してソート（降順）
        scores.Add(finalScore);
        scores.Sort((a, b) => b.CompareTo(a)); // 大きい順
        scores = scores.GetRange(0, 3); // 上位3つだけ保持

        // 保存
        for (int i = 0; i < 3; i++)
        {
            PlayerPrefs.SetInt($"HighScore{i}", scores[i]);
        }
        PlayerPrefs.Save();
    }


    private void DisplayHighScores()
    {
        string[] rankStrings = { "1st", "2nd", "3rd" };
        for (int i = 0; i < highScoreTexts.Length && i < rankStrings.Length; i++)
        {
            int score = PlayerPrefs.GetInt($"HighScore{i}", 0);
            highScoreTexts[i].text = $"{rankStrings[i]}. {score}";
        }
    }

}
