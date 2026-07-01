using UnityEngine;
using UnityEngine.UI;

public class SpeechBubble : MonoBehaviour
{
    [SerializeField] private Image dishImage;

    [SerializeField] private Image seasoningImage;

    public void SetDishImage(string dishName)
    {
        // Resources/Foods/ にある画像ファイル名と一致させて読み込む
        Sprite sprite = Resources.Load<Sprite>($"Foods/{dishName}");
        dishImage.sprite = sprite;
    }
}
