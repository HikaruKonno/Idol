using System.Collections.Generic;
using UnityEngine;

public class AudienceManager : MonoBehaviour
{
    [Header("共通設定")]
    // 共通設定を使用するかのフラグ
    [SerializeField]
    private bool m_isShareSettings = true;
    // スクリプタブルオブジェクトを格納する変数
    [SerializeField]
    private AudienceChangeColorData m_changeColorShareData;

    [SerializeField, Tooltip("サイリウムのエミッシブの強さ")]
    private float m_psylliumIntensity = 15.0f;
    // 色を変える間隔
    [SerializeField]
    private float m_changeColorInterval;

    // 色を変えるかのフラグ
    private bool m_isChangeColor = true;
    // 色を変えた数
    private int m_changeColorCount;
    // クールタイム
    private float m_coolTime;

    // 変える色のリスト
    private List<Color> m_changeColorList = new List<Color>();
    // ペンライトのRendererのリスト
    private List<Renderer> m_rendererList = new List<Renderer>();
    // ペンライトのアニメーションのリスト
    private List<PenlightAnimation> m_penlightList = new List<PenlightAnimation>();

    // 生成された時の色のリスト
    private List<Color> m_InitialColorList = new List<Color>();

    // ベースカラーのIDキャッシュ
    private int m_baseColorId;
    // エミッシブのIDキャッシュ
    private int m_emissionColorId;

    void Awake()
    {
        // IDをキャッシュ
        m_baseColorId = Shader.PropertyToID("_BaseColor");
        m_emissionColorId = Shader.PropertyToID("_EmissionColor");

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        PenlightAnimation[] penlightAnimations = GetComponentsInChildren<PenlightAnimation>(true);

        // 配列の初期化
        m_rendererList.Clear();
        m_InitialColorList.Clear();
        m_penlightList.Clear();

        // PenlightAnimationの取得
        foreach (PenlightAnimation penlight in penlightAnimations)
        {
            if (penlight == null) return;
            m_penlightList.Add(penlight);
        }

        // Rendererの取得
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) return;

            // サイリウムの検索
            if (renderer.gameObject.name == "Psyllium")
            {
                m_rendererList.Add(renderer);

                // sharedMaterial が無い可能性があるので安全に取り扱う
                if (renderer.sharedMaterial != null)
                {
                    // HDRP/URP の場合プロパティ名が _BaseColor かもしれないので両方試す
                    Color initColor;

                    if (renderer.sharedMaterial.HasProperty(m_baseColorId))
                    {
                        // ベースカラーの取得
                        initColor = renderer.sharedMaterial.GetColor(m_baseColorId);
                    }
                    else
                    {
                        // 互換性のため color プロパティも試す
                        initColor = renderer.sharedMaterial.color;
                    }

                    m_InitialColorList.Add(initColor);
                }
            }
        }
    }

    /// <summary>
    /// 色の変更を行う関数
    /// </summary>
    /// <param name="_renderer">色を変えたいrendererのリスト</param>
    public void ChangeColorList(List<Renderer> _renderer)
    {
        if (_renderer != null)
        {
            if (m_changeColorCount >= m_changeColorList.Count)
            {
                m_changeColorCount = 0;
            }

            for (int i = 0; i < m_rendererList.Count; i++)
            {
                // インスタンスを明示的に生成
                Material newMat = new Material(_renderer[i].sharedMaterial);

                // HDRPやURPのマテリアルの変更は、newMat.color = Color.red; ではなくSetColorを使う
                // ベースカラーの変更
                newMat.SetColor(m_baseColorId,
                    m_changeColorList[m_changeColorCount]);

                // エミッシブを有効化
                newMat.EnableKeyword("_EMISSION");

                // エミッシブの色も変更
                newMat.SetColor(m_emissionColorId,
                    m_changeColorList[m_changeColorCount] * m_psylliumIntensity);

                // 適用
                _renderer[i].sharedMaterial = newMat;
            }
            ++m_changeColorCount;
        }
        else
        {
            Debug.LogWarning($"{this.gameObject.name} の Renderer が見つかりません");
        }
    }

    /// <summary>
    /// ペンライトの振りをShakeShakeModeに変える関数
    /// </summary>
    public void ChangeShakeShake()
    {
        if (m_penlightList == null) return;

        for (int i = 0; i < m_penlightList.Count; i++)
        {
            // バラバラにする
            m_penlightList[i].ChangeUnsynchronized();
            // 振りを変える
            m_penlightList[i].ChangeShakeShake();
        }
    }

    /// <summary>
    /// 振りのモードを Play 時に選んだものにする
    /// </summary>
    public void ResetShakeMode()
    {
        if (m_penlightList == null) { return; }

        for (int i = 0; i < m_penlightList.Count; i++)
        {
            // 常に揃わせる
            m_penlightList[i].ChangeSynchronized();
            // 振りを変える
            m_penlightList[i].ResetShakeMode();
        }
    }

    /// <summary>
    /// 振りを徐々に止まらせて次の振り方に切り替える関数を呼ぶ関数
    /// </summary>
    public void StopAndChangeShakeMode()
    {
        if (m_penlightList == null) { return; }

        for (int i = 0; i < m_penlightList.Count; i++)
        {
            m_penlightList[i].StopAndChangeShakeMode(PenlightAnimation.ShakeMode.BasicY, PenlightAnimation.ShakeSyncMode.Synchronized, 0f);
        }
    }

    /// <summary>
    /// 左右に振るスピードを変える
    /// </summary>
    public void ChangeShakeShakeSpeed(float _speed)
    {
        if (m_penlightList == null) return;

        for (int i = 0; i < m_penlightList.Count; i++)
        {
            // 振りを変える
            m_penlightList[i].ChangeShakeShakeSpeed(_speed);
        }
    }

    /// <summary>
    /// マテリアルの色を変える関数
    /// </summary>
    /// <param name="_color">変えたい色</param>
    private void ChangeMaterialColor(Color _color)
    {
        if (m_rendererList == null || m_rendererList.Count == 0) return;

        if (m_isShareSettings && m_changeColorShareData != null)
        {
            // 共有設定を使う場合、ScriptableObject から強さを取得
            m_psylliumIntensity = m_changeColorShareData.m_psylliumIntensity;
            m_changeColorCount++;
        }

        foreach (Renderer r in m_rendererList)
        {
            // null 対策
            if (r == null || r.sharedMaterial == null) continue;

            Material newMat = new Material(r.sharedMaterial);

            // HDRPやURPのマテリアルの変更は、newMat.color = Color.red; ではなくSetColorを使う
            newMat.SetColor(m_baseColorId, _color);

            // 光の強さ
            newMat.EnableKeyword("_EMISSION");
            newMat.SetColor(m_emissionColorId, _color * m_psylliumIntensity);

            // マテリアルの適用
            r.sharedMaterial = newMat;
        }
    }


    /// <summary>
    /// マテリアルの色を最初の色にリセットする関数
    /// </summary>
    public void ResetColor()
    {
        int count = Mathf.Min(m_rendererList.Count, m_InitialColorList.Count);

        for (int i = 0; i < m_rendererList.Count; i++)
        {
            Renderer renderer = m_rendererList[i];
            if (renderer == null || renderer.sharedMaterial == null) continue;

            Material newMat = new Material(renderer.sharedMaterial);

            // 初期色の取得
            Color color = m_InitialColorList[i];

            // ベースカラーの変更
            newMat.SetColor(m_baseColorId, color);

            // エミッシブの有効化
            newMat.EnableKeyword("_EMISSION");

            // エミッシブの色も初期色に戻す
            newMat.SetColor(m_emissionColorId, color * m_psylliumIntensity);

            // 新しいマテリアルの適用
            renderer.sharedMaterial = newMat;
        }
    }

    /// <summary>
    /// 色を変えるかのフラグを反転
    /// </summary>
    public void ChangeColorFlag()
    {
        m_isChangeColor = !m_isChangeColor;
    }

    // ---------------------------------------------------------------
    // 以下、色を変える関数群　
    // m_psylliumColorList[]のインデックス番号と関数名の末尾の数字を合わせること
    // 例：m_psylliumColorList[0]ならChangeColorList0()を呼ぶ
    // ---------------------------------------------------------------
    public void ChangeColorList0()
    {
        if (m_isShareSettings && m_changeColorShareData != null)
        {
            ChangeMaterialColor(m_changeColorShareData.m_psylliumColorList[0]);
        }
        else
        {
            ChangeMaterialColor(m_changeColorList[0]);
        }
    }

    public void ChangeColorList1()
    {
        if (m_isShareSettings && m_changeColorShareData != null)
        {
            ChangeMaterialColor(m_changeColorShareData.m_psylliumColorList[1]);
        }
        else
        {
            ChangeMaterialColor(m_changeColorList[1]);
        }
    }
    public void ChangeColorList2()
    {
        if (m_isShareSettings && m_changeColorShareData != null)
        {
            ChangeMaterialColor(m_changeColorShareData.m_psylliumColorList[2]);
        }
        else
        {
            ChangeMaterialColor(m_changeColorList[2]);
        }
    }
    public void ChangeColorList3()
    {
        if (m_isShareSettings && m_changeColorShareData != null)
        {
            ChangeMaterialColor(m_changeColorShareData.m_psylliumColorList[3]);
        }
        else
        {
            ChangeMaterialColor(m_changeColorList[3]);
        }
    }
}