using UnityEngine;
using System.Collections.Generic;

public class PrefabPlacer : MonoBehaviour
{
    public GameObject[] allPrefabs;        // a〜h (8個)
    public Transform[] places;             // 配置場所（毎回共通で6つ）

    public void PlaceNextRound()
    {
        // プレハブからランダムに選ぶ
        List<GameObject> shuffled = new List<GameObject>(allPrefabs);
        Shuffle(shuffled);
        List<GameObject> selectedPrefabs = shuffled.GetRange(0, 8);

        // シャッフルして配置順をランダムに
        Shuffle(selectedPrefabs);

        for (int i = 0; i < places.Length; i++)
        {
            Transform dishTransform = null;
            foreach (Transform child in places[i])
            {
                if (child.CompareTag("Dish"))
                {
                    dishTransform = child;
                    break;
                }
            }

            if (dishTransform == null)
            {
                dishTransform = places[i]; // fallback
            }

            // 子オブジェクト削除
            Transform[] children = new Transform[dishTransform.childCount];
            for (int j = 0; j < dishTransform.childCount; j++)
            {
                children[j] = dishTransform.GetChild(j);
            }
            foreach (var child in children)
            {
                Destroy(child.gameObject);
            }

            // プレハブ生成
            GameObject obj = Instantiate(selectedPrefabs[i], dishTransform);
            obj.transform.localPosition =  new Vector3(0, -0.23f, 0);
            obj.transform.localRotation = Quaternion.identity;
        }

    }


    // シャッフル処理
    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
