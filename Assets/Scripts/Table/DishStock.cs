using UnityEngine;

// Dish 在庫管理スクリプト
public class DishStock : MonoBehaviour
{
    [SerializeField] private int maxStock = 3;     // Max在庫

    public int stock = 3;

    // 在庫減らす
    public void DecreaseStock()
    {
        if (stock > 0)
        {
            stock--;
            Debug.Log($"{gameObject.name} の在庫が減りました。残り: {stock}");

            Animator animator = FindAnimator(transform);
            if (animator != null)
            {
                // ステート名のチェックは外して単純にトリガーを送る
                animator.SetTrigger("takeFood");
            }
            else
            {
                Debug.LogWarning("Animatorが見つかりませんでした");
            }
        }
        else
        {
            Debug.Log($"{gameObject.name} は在庫切れです！");
        }
    }


    private Animator FindAnimator(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null) return animator;

            animator = FindAnimator(child);
            if (animator != null) return animator;
        }
        return null;
    }


    // 在庫リセット
    public void ResetStock()
    {
        stock = maxStock;
        Debug.Log($"{gameObject.name} の在庫を最大値 {maxStock} にリセットしました。");
    }

    // ストックがあるかどうかを返す
    public bool getStock()
    {
        return stock != 0;
    }
}

