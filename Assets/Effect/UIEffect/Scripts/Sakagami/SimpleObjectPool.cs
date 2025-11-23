/*
 * ファイル
 * SimpleObjectPool C#
 * 
 * システム
 * ゲームオブジェクトを使いまわす
 * 
 * 作成
 * 2025/09/01 坂上　壱希
 * 
 * 最終変更
 * 2025/09/29　坂上　壱希
 */
using System.Collections.Generic;
using UnityEngine;

sealed public class SimpleObjectPool
{
    private readonly GameObject TARGET_PREFAB;    // プール元プレハブ
    private readonly Transform PARENT_TRANSFORM;    // プールの親のTransform
    private readonly Stack<GameObject> POOLSTACK = new Stack<GameObject>();    // プール内の非アクティブオブジェクトを管理するスタック

    // コンストラクタ
    public SimpleObjectPool(GameObject _prefab, Transform _parent = null, int _initialSize = 10)
    {
        TARGET_PREFAB = _prefab;
        PARENT_TRANSFORM = _parent;

        // 初期生成してプールに入れておく
        for (int i = 0; i < _initialSize; i++)
        {
            var obj = Object.Instantiate(_prefab, PARENT_TRANSFORM);
            obj.SetActive(false);
            POOLSTACK.Push(obj);
        }
    }

    /// <summary>
    /// プールから取得 
    /// </summary>
    /// <returns>取得するプールのオブジェクト</returns>
    public GameObject Get()
    {
        GameObject obj;
        if (POOLSTACK.Count > 0)
        {
            obj = POOLSTACK.Pop();
            obj.SetActive(true);
        }
        else
        {
            obj = Object.Instantiate(TARGET_PREFAB, PARENT_TRANSFORM);
        }
        return obj;
    }

    /// <summary>
    /// プールに返却 
    /// </summary>
    /// <param name="obj">プールを戻す</param>
    public void Release(GameObject _obj)
    {
        _obj.SetActive(false);
        POOLSTACK.Push(_obj);
    }
}
