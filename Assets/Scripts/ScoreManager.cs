using UnityEngine;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance; // シングルトン風に

    private int score = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log("Sco: " + score);
        // UI更新処理をここで呼ぶことも多い
    }

    public int GetScore() => score;
}

