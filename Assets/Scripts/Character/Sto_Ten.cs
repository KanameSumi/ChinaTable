using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StoTenImageController : MonoBehaviour
{
    [SerializeField] private GameObject stoTenPane;
    [SerializeField] private TMP_Text feedbackText; // アサインしてね

    private Image stoImage;
    private Image tenImage;

    private int stoIndex = 1;       // 次に表示する数字
    private int currentSto = 0;     // 現在表示中の数字
    private int tenIndex = 3;       // 初期Ten値

    private bool freezeFace = false;

    void Start()
    {
        stoImage = stoTenPane.transform.Find("StoImage").GetComponent<Image>();
        tenImage = stoTenPane.transform.Find("TenImage").GetComponent<Image>();

        ResetImages();

        feedbackText.text = "";
        feedbackText.alpha = 0f; // 最初は透明
    }

    public void UpdateStoImage()
    {
        Sprite newStoSprite = Resources.Load<Sprite>($"stomach/sto_{stoIndex}");
        if (newStoSprite != null) stoImage.sprite = newStoSprite;

        currentSto = stoIndex;

        if (stoIndex < 10) stoIndex++;
    }

    // Ten画像減少
    public void DecreaseTenImage()
    {
        int currentIndex = GetTenImageIndex();
        if (currentIndex > 1)
        {
            currentIndex--;
            UpdateTenImage(currentIndex);
        }

        // Ten最小なら大きく減点、そうでなければ普通
        int scorePopup = (currentIndex == 1) ? 40 : 30;
        ShowFeedback("-" + scorePopup, new Color(1f, 0.4f, 0.4f, 0.8f)); // ←淡い透明感ある赤
    }

    // Ten画像増加
    public void IncreaseTenImage()
    {
        int currentIndex = GetTenImageIndex();
        if (currentIndex < 5)
        {
            currentIndex++;
            UpdateTenImage(currentIndex);
        }

        // Ten最大なら大きく加点、そうでなければ普通
        int scorePopup = (currentIndex == 5) ? 60 : 50;
        ShowFeedback("+" + scorePopup, new Color(0.5f, 1f, 0.5f, 0.8f)); // ←淡い透明感ある緑
    }

    // --- Tenの現在値を取得 ---
    public int GetTenValue()
    {
        return GetTenImageIndex();
    }

    // --- 内部関数 ---
    private int GetTenImageIndex()
    {
        string spriteName = tenImage.sprite.name;
        string[] parts = spriteName.Split('_');
        if (parts.Length == 2 && int.TryParse(parts[1], out int index))
            return index;
        return 1;
    }

    private void UpdateTenImage(int index)
    {
        if (freezeFace) return;
        Sprite newSprite = Resources.Load<Sprite>($"tension/ten_{index}");
        if (newSprite != null) tenImage.sprite = newSprite;
    }

    // --- フィードバック表示 ---
    private void ShowFeedback(string message, Color color)
    {
        StopAllCoroutines(); // 前のフェード処理を止める
        StartCoroutine(FeedbackRoutine(message, color));
    }

    private IEnumerator FeedbackRoutine(string message, Color color)
    {
        feedbackText.text = message;
        feedbackText.color = color;

        // フェードイン
        for (float t = 0; t < 0.3f; t += Time.deltaTime)
        {
            feedbackText.alpha = Mathf.Lerp(0f, 1f, t / 0.3f);
            yield return null;
        }
        feedbackText.alpha = 1f;

        // 表示キープ
        yield return new WaitForSeconds(0.5f);

        // フェードアウト
        for (float t = 0; t < 0.3f; t += Time.deltaTime)
        {
            feedbackText.alpha = Mathf.Lerp(1f, 0f, t / 0.3f);
            yield return null;
        }
        feedbackText.alpha = 0f;
        feedbackText.text = "";
    }

    // --- リセット ---
    public void ResetImages()
    {
        freezeFace = false;
        stoIndex = 1;
        currentSto = 0;

        Sprite stoSprite = Resources.Load<Sprite>("stomach/sto_0");
        if (stoSprite != null) stoImage.sprite = stoSprite;

        tenIndex = 3;
        Sprite tenSprite = Resources.Load<Sprite>("tension/ten_3");
        if (tenSprite != null) tenImage.sprite = tenSprite;

        feedbackText.text = "";
        feedbackText.alpha = 0f;
    }

    // ボーナスタイムで顔を固定するための関数
    public void SetFreezeFace(bool freeze)
    {
        freezeFace = freeze;
    }

    // --- EndGame用に現在のSto値を返す ---
    public int GetStoValue()
    {
        return currentSto;
    }
}
