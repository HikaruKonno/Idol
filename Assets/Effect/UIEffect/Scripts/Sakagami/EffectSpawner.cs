/*
 * ファイル
 * UIEffectSpawner C#
 * 
 * システム
 * エフェクトを画面端にランダムで生成する
 * 
 * 作成
 * 2025/09/01 坂上　壱希
 * 
 * 最終変更
 * 2025/09/29　坂上　壱希
 */
using UnityEngine;
using System.Collections;

sealed public class EffectSpawner : MonoBehaviour
{
    [SerializeField] 
    private RectTransform m_spawnArea;   // Canvas内の基準領域
    [SerializeField] 
    private GameObject m_effectPrefab;   // 生成するプレハブ
    [SerializeField] 
    private float m_spawnInterval = 0.5f;    // 生成間隔
    [SerializeField] 
    private int m_spawnCountPerCycle = 1;    // 同時に生成する数
    [SerializeField] 
    private float m_margin = 50f;    // 画面端からの内側の余白

    private bool m_isSpawning = false;    //オブジェクトプールをしているか
    private SimpleObjectPool m_pool;  //

    private void OnEnable()
    {
        if (m_pool == null)
        {
            m_pool = new SimpleObjectPool(m_effectPrefab, m_spawnArea, _initialSize: 20);
        }

        m_isSpawning = true;
        StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        m_isSpawning = false;
    }

    /// <summary>
    /// 一定間隔でオブジェクトのスポーンエフェクトを繰り返し実行するコルーチン
    /// </summary>
    /// <returns>IEnumerator コルーチンの実行状態</returns>
    private IEnumerator SpawnLoop()
    {
        // isSpawning が true の間ループを続ける
        while (m_isSpawning)
        {
            // 1サイクルごとに spawnCountPerCycle 回エフェクトを発生させる
            for (int i = 0; i < m_spawnCountPerCycle; i++)
            {
                SpawnEffect();
            }

            // spawnInterval 秒待機してから次のサイクルへ
            yield return new WaitForSeconds(m_spawnInterval);
        }
    }

    /// <summary>
    /// エフェクトオブジェクトをプールから取得し、ランダムな画面端の位置に配置して初期化する
    /// </summary>
    private void SpawnEffect()
    {
        // ランダムに画面の端の位置を取得
        Vector2 anchoredPos = GetRandomEdgePosition();

        // オブジェクトプールからエフェクト用のオブジェクトを取得
        GameObject obj = m_pool.Get();

        // RectTransform コンポーネントを取得（UI用の位置調整に必要）
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect == null)
        {
            Debug.LogWarning("Effect prefabにRectTransformがありません。");
            return; // RectTransformがない場合は処理を中断
        }

        // 取得したオブジェクトの親を spawnArea に設定（falseはワールド座標を維持しない設定）
        rect.SetParent(m_spawnArea, false);

        // 取得したランダム位置にエフェクトを配置
        rect.anchoredPosition = anchoredPos;

        // UIScaleInOutDestroy コンポーネントを取得し、あればプールをセット
        var fx = obj.GetComponent<EffectScaleInOutDestroy>();
        if (fx != null)
        {
            fx.SetPool(m_pool);
        }
    }


    /// <summary>
    /// spawnAreaの四辺のいずれかからランダムな位置を取得する
    /// </summary>
    /// <returns>spawnAreaの端にあるランダムな2D座標</returns>
    private Vector2 GetRandomEdgePosition()
    {
        // spawnAreaの幅と高さを取得
        float width = m_spawnArea.rect.width;
        float height = m_spawnArea.rect.height;

        // 0=上, 1=下, 2=左, 3=右 のどれかの辺をランダムに選ぶ
        int side = Random.Range(0, 4);

        float x = 0, y = 0;

        // 偏りを防ぐためのマージン値をランダムに取得
        float offset = Random.Range(0f, m_margin);

        // 選ばれた辺に応じてx,y座標を決定
        switch (side)
        {
            case 0: // 上辺
                x = Random.Range(-width / 2f + offset, width / 2f - offset);
                y = height / 2f - offset;
                break;
            case 1: // 下辺
                x = Random.Range(-width / 2f + offset, width / 2f - offset);
                y = -height / 2f + offset;
                break;
            case 2: // 左辺
                x = -width / 2f + offset;
                y = Random.Range(-height / 2f + offset, height / 2f - offset);
                break;
            case 3: // 右辺
                x = width / 2f - offset;
                y = Random.Range(-height / 2f + offset, height / 2f - offset);
                break;
        }

        // 計算した位置を返す
        return new Vector2(x, y);
    }
}
