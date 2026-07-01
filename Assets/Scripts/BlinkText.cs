using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class BlinkText : MonoBehaviour
{
    private Text targetText;

    [SerializeField] private float interval = 0.5f; // 点滅間隔

    private void Awake()
    {
        targetText = GetComponent<Text>();
    }

    private void OnEnable()
    {
        StartCoroutine(BlinkRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        targetText.enabled = true; // 停止時は表示状態に戻す
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            targetText.enabled = !targetText.enabled;
            yield return new WaitForSeconds(interval);
        }
    }
}
