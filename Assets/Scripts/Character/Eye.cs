using UnityEngine;
using UnityEngine.UI;

public class Eye : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Request requestScript;
    [SerializeField] private Slider gazeSlider;
    [SerializeField] private Animator gazeAnimator; // 同階層の Animator をアサイン

    [Header("設定")]
    [SerializeField] private float gazeDistance = 10f;
    [SerializeField] private LayerMask dishLayers;
    [SerializeField] private float gazeThreshold = 0.8f;

    private float gazeTimer = 0f;
    private Transform lastHitTransform = null;

    void Update()
    {
        // --- レイキャストで注視対象を判定 ---
        Vector3 direction = transform.forward;
        Ray ray = new Ray(transform.position, direction);
        Debug.DrawRay(transform.position, direction * gazeDistance, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, gazeDistance, dishLayers))
        {
            Transform currentHit = hit.collider.transform;

            if (currentHit == lastHitTransform) // 同じオブジェクトを見続けている
            {
                if (currentHit.childCount > 0)
                {
                    Transform child = currentHit.GetChild(0);
                    string cleanName = child.name.Replace("(Clone)", "").Trim();

                    // リクエスト中 & 注視対象が一致
                    if (requestScript.HasRequest() && requestScript.CurrentRequest == cleanName)
                    {
                        gazeTimer += Time.deltaTime;

                        if (gazeSlider != null)
                            gazeSlider.value = Mathf.Clamp01(gazeTimer / gazeThreshold);

                        if (gazeTimer >= gazeThreshold)
                        {
                            requestScript.CheckHit(cleanName);
                            ResetGaze();
                        }
                    }
                    else
                    {
                        ResetGaze();
                    }
                }
            }
            else // 新しい対象を見始めたとき
            {
                gazeTimer = 0f;
                lastHitTransform = currentHit;
                ResetGaze();
            }
        }
        else // 何も見ていない
        {
            gazeTimer = 0f;
            lastHitTransform = null;
            ResetGaze();
        }

        UpdateAnimator();
    }

    /// <summary>
    /// ゲージとタイマーをリセット
    /// </summary>
    private void ResetGaze()
    {
        gazeTimer = 0f;
        if (gazeSlider != null)
            gazeSlider.value = 0f;
    }

    /// <summary>
    /// アニメーターにゲージ進捗を渡す
    /// </summary>
    private void UpdateAnimator()
    {
        if (gazeAnimator == null) return;

        float progress = (gazeSlider != null) ? gazeSlider.value : 0f;

        // Blend Tree 用のパラメータ
        gazeAnimator.SetFloat("GazeProgress", progress);

        // ゲージが動いている間だけ GazeLayer を有効にする
        int layerIndex = gazeAnimator.GetLayerIndex("GazeLayer");
        if (layerIndex >= 0)
        {
            gazeAnimator.SetLayerWeight(layerIndex, progress > 0f ? 1f : 0f);
        }
    }
}
