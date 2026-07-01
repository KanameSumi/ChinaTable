using System.Collections;
using UnityEngine;
using System.Collections.Generic;


public class Request : MonoBehaviour
{
    [SerializeField] private StoTenImageController imageController;
    public bool IsEnabled { get; set; } = false;    // ゲームが開始しているかどうか

    // Dishの配列
    public string[] foods = { "Dish_1", "Dish_2", "Dish_3", "Dish_4", "Dish_5", "Dish_6", "Dish_7" };

    private string request = "";    // リクエストを入れる
    private string nowRequest = ""; // 前のリクエストを入れる
    private bool waiting = false;      // 待ち状態かどうか

    [SerializeField] private SpeechBubble speechBubble; // 吹き出し

    private Coroutine timeoutCoroutine = null;

    void Update()
    {
        if (!IsEnabled) return;   // ゲーム中以外は何もしない

        speechBubble.SetDishImage(request);
        // リクエストが空で、かつまだ待機していなければ新しいリクエストを待機開始
        if (string.IsNullOrEmpty(request) && !waiting)
        {
            StartCoroutine(GenerateRequest());
        }
    }

    // 当たったDish名と一致したらリクエストをクリア（外部呼び出し）
    public void CheckHit(string dishName)
    {
        if (dishName == request)
        {
            // リクエスト先のDishを取得
            GameObject dishObject = GameObject.Find(dishName + "(Clone)");
            DishStock stock = dishObject.GetComponent<DishStock>();

            if (stock == null) return;

            // ストックがあれば
            if (stock.getStock())
            {
                stock.DecreaseStock();  // 該当Dishの在庫を１減らす
                Debug.Log($"リクエスト {request} と一致！");
                request = "";
                int tenIndex = imageController.GetTenValue();

                // スコアを加算（例：50点）
                if (tenIndex >= 5)
                {
                    ScoreManager.Instance.AddScore(60);
                    FindObjectOfType<GameManager>().AddScore(60);
                }
                else
                {
                    ScoreManager.Instance.AddScore(50);
                    FindObjectOfType<GameManager>().AddScore(50);
                }
                imageController.UpdateStoImage();
                imageController.IncreaseTenImage();

                if (timeoutCoroutine != null)
                {
                    StopCoroutine(timeoutCoroutine);
                    timeoutCoroutine = null;
                }
            }
        }
    }



    // x秒後にランダムでリクエスト生成
    private IEnumerator GenerateRequest()
    {
        waiting = true;
        float waitTime = Random.Range(0.5f, 2f);
        yield return new WaitForSeconds(waitTime);

        // 在庫があるDishだけをリストに集める
        List<string> availableFoods = new List<string>();

        foreach (string foodName in foods)
        {
            GameObject dishObject = GameObject.Find(foodName + "(Clone)");
            if (dishObject == null) continue;

            DishStock stock = dishObject.GetComponent<DishStock>();
            if (stock == null) continue;
            if (!stock.getStock()) continue;  // 在庫なしなら除外

            // 他のRequestコンポーネント全て取得して、自分以外でそのDishリクエストしてるかチェック
            bool isOrderedByOthers = false;

            Request[] allRequests = FindObjectsOfType<Request>();
            foreach (var r in allRequests)
            {
                if (r == this) continue;  // 自分自身はスルー
                if (r.request == foodName)
                {
                    isOrderedByOthers = true;
                    break;
                }
            }

            if (!isOrderedByOthers)
            {
                availableFoods.Add(foodName);
            }
        }

        // 在庫が1つもない場合は処理終了（リクエストなし）
        if (availableFoods.Count == 0)
        {
            request = "";
            speechBubble.SetDishImage(request);
            waiting = false;
            yield break;
        }

        // 安全にランダム選択（前回リクエストと被らないように）
        int index = Random.Range(0, availableFoods.Count);
        if (availableFoods.Count > 1)
        {
            int safetyCounter = 0; // 無限ループ回避
            while (availableFoods[index] == nowRequest && safetyCounter < 10)
            {
                index = Random.Range(0, availableFoods.Count);
                safetyCounter++;
            }
        }

        request = availableFoods[index];
        nowRequest = request;

        speechBubble.SetDishImage(request);
        Debug.Log($"新しいリクエスト: {request}");

        if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = StartCoroutine(RequestTimeout());

        waiting = false;
    }

    public string CurrentRequest => request;


    private IEnumerator RequestTimeout()
    {
        while (!string.IsNullOrEmpty(request))
        {
            yield return new WaitForSeconds(5f);

            if (!string.IsNullOrEmpty(request))
            {
                int tenIndex = imageController.GetTenValue();

                // スコアを減算
                if (tenIndex == 1)
                {
                    ScoreManager.Instance.AddScore(-40);
                    FindObjectOfType<GameManager>().AddScore(-40);
                }
                else
                {
                    ScoreManager.Instance.AddScore(-30);
                    FindObjectOfType<GameManager>().AddScore(-30);
                }
                Debug.Log($"リクエスト {request} のタイムアウト！-30点");

                imageController.DecreaseTenImage();
            }
        }
        timeoutCoroutine = null;
    }

    public void ClearRequest()
    {
        request = "";
        nowRequest = "";
        waiting = false;

        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }

        speechBubble.SetDishImage("");  // 吹き出しも空に
    }

    // まだリクエストが残っているか
    public bool HasRequest()
    {
        // request が空じゃなければ残っている
        return !string.IsNullOrEmpty(request);
    }

}