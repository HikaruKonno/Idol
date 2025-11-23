using System.Collections.Generic;
using UnityEngine;

public class AudienceSpawnerManager : MonoBehaviour
{
    // スポナーの配列
    [SerializeField]
    private List<AudienceSpawner> m_audienceSpawnerList = new List<AudienceSpawner>();

    /// <summary>
    /// 配列のスポナーから観客を生成させる関数
    /// </summary>
    public void SpawnAudienceForList()
    {
        foreach (var spawner in m_audienceSpawnerList)
        {
            if (spawner != null)
            {
                // 観客の生成
                spawner.SpwanAudienceGrid();
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("配列に入っているスポナーがありません");
#endif
            }
        }
#if UNITY_EDITOR
        Debug.Log($"配列に入っているスポナーで観客を生成します。合計スポナー数：{m_audienceSpawnerList.Count}");
#endif
    }

    /// <summary>
    /// 配列のスポナーの観客を削除する関数
    /// </summary>
    public void DeleteAudienceForList()
    {
        foreach (var spawner in m_audienceSpawnerList)
        {
            if (spawner != null)
            {
                // 観客の削除
                spawner.DeleteAudience();
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("配列に入っているスポナーがありません");
#endif
            }
        }
#if UNITY_EDITOR
        Debug.Log($"配列に入っているスポナーの観客を削除しました。合計スポナー数：{m_audienceSpawnerList.Count}");
#endif
    }

    /// <summary>
    /// スポナーを子オブジェクトから取得
    /// </summary>
    public void AddAllSpawnerForChild()
    {
        // 子オブジェクトがいなければ早期リターン
        if (transform.childCount == 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning("削除対象の観客が存在しません");
#endif
            return;
        }

        // 子オブジェクトの数を減少させる
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            // 子オブジェクトの取得
            Transform child = transform.GetChild(i);

            // 名前にSpawnerが含まれているか確認して、含まれていれば配列に追加
            if (child.name.IndexOf("Spawner") >= 0)
            {
                if (child.gameObject.GetComponent<AudienceSpawner>() != null)
                {
                    m_audienceSpawnerList.Add(child.gameObject.GetComponent<AudienceSpawner>());
#if UNITY_EDITOR
                    Debug.Log($"m_audienceSpawnerListの数が{m_audienceSpawnerList.Count}になりました。");
#endif
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning("配列に入っているスポナーがありません");
#endif
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("子オブジェクトにスポナーがありません");
#endif
            }
        }
    }

    /// <summary>
    /// スポナーのリストを0にしてリセットする関数
    /// </summary>
    public void ResetSpawnerList()
    {
        m_audienceSpawnerList.Clear();
    }
}