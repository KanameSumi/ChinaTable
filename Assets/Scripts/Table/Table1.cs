using UnityEngine;

public class Table1 : MonoBehaviour
{
    [SerializeField]    // privateでつかうため

    public bool IsEnabled { get; set; } = false;    // ゲームが開始しているかどうか
    private float rotateSpeed = 70f;    // 回転速度
    private float currentYaw = 0f;
    private float previousYaw = 0f;
    private float yawSpeedThreshold = 300f; //「速く回る」の基準

    [SerializeField] private PrefabPlacer placer; // 場所
    private bool isSpinningFast = false;

    void Update()
    {
        if (GameManager.Instance.currentState == GameState.Result) return;

        //UDP角度を使用（Yawに合わせてY軸を固定）
        currentYaw = -EncoderReceiver.angle; // 必要なら符号を反転
        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);
        // Yawの変化量を計算
        float deltaYaw = Mathf.DeltaAngle(previousYaw, currentYaw); // -180〜180の範囲に補正
        float yawSpeed = Mathf.Abs(deltaYaw) / Time.deltaTime;       // 角速度（度/秒）
        previousYaw = currentYaw;
        
        // // 左右キー入力
        // float horizontal = 0f;
        // if (Input.GetKey(KeyCode.LeftArrow)) horizontal = -3f;
        // else if (Input.GetKey(KeyCode.RightArrow)) horizontal = 3f;
        // // 回転量の計算（フレームごと）
        // float rotate = horizontal * rotateSpeed * Time.deltaTime;
        // transform.Rotate(0f, rotate, 0f);   // 回転軸(x,y,z)

        // // スペースキーで皿を初期化
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     if (Input.GetKeyDown(KeyCode.Space)) isSpinningFast = !isSpinningFast;
        // }
    }

    // 全部補充
    public void ResetAllDishes()
    {
        placer.PlaceNextRound();

        // 子孫の全てのAnimatorを取得
        Animator[] animators = GetComponentsInChildren<Animator>();

        // 子孫のすべてのDishStockを取得
        DishStock[] stocks = GetComponentsInChildren<DishStock>();

        foreach (DishStock stock in stocks)
        {
            stock.ResetStock(); // 在庫最大に戻す
        }

        if (IsEnabled) ScoreManager.Instance.AddScore(-5);
        Debug.Log("皿の状態をリセットしました。");
    }
}
