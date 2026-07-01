
using UnityEngine;

public class AutoDestroyOnAnimationEnd : MonoBehaviour
{
    // Animationイベントから呼び出される
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
