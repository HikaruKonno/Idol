#define DEBUG
#undef DEBUG

using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    // 色を変えるフラグ
    private bool m_isChangeColor = false;
    // 色を変えた関数
    private int m_changColorCount;
    // 色を変える間隔
    [SerializeField]
    private float m_changeColorInterval;
    // クールタイム
    private float m_coolTime;
    // 変える色の配列
    [SerializeField]
    private List<Color> m_changeColorList = new List<Color>();

    void Update()
    {
        m_coolTime += Time.deltaTime;

        // マテリアルの色を変更
        ChangeMaterialColor(GetComponent<Renderer>(), Color.red);

        // 色の変更
        if (m_isChangeColor && m_changeColorInterval <= m_coolTime)
        {
            //ChangeColorForList(GetComponent<Renderer>());
            m_coolTime = 0f;
        }
    }

    /// <summary>
    /// 色の変更を行う関数
    /// </summary>
    public void ChangeColorForList(Renderer _renderer)
    {
        if (_renderer != null)
        {
            if (m_changColorCount >= m_changeColorList.Count)
            {
                m_changColorCount = 0;
            }

            // インスタンスを明示的に生成
            Material newMat = new Material(_renderer.sharedMaterial);

            // HDRPやURPのマテリアルの変更は、newMat.color = Color.red; ではなくSetColorを使う
            newMat.SetColor("_BaseColor",
                m_changeColorList[m_changColorCount]);

            // Emissionを有効化
            newMat.EnableKeyword("_EMISSION");
            // エミッシブの色も変更
            newMat.SetColor("_EmissionColor",
                m_changeColorList[m_changColorCount]);

            // 適用
            _renderer.sharedMaterial = newMat;

            ++m_changColorCount;
        }
        else
        {
#if DEBUG && UNITY_EDITOR
            Debug.LogWarning($"{this.gameObject.name} の Renderer が見つかりません");
#endif
        }
    }

    public void ChangeMaterialColor(Renderer _renderer, Color _color)
    {
        if (_renderer != null)
        {
            // インスタンスを明示的に生成
            Material newMat = new Material(_renderer.sharedMaterial);

            // HDRPやURPのマテリアルの変更は、newMat.color = Color.red; ではなくSetColorを使う
            newMat.SetColor("_BaseColor", _color);

            // Emissionを有効化
            newMat.EnableKeyword("_EMISSION");
            // エミッシブの色も変更
            newMat.SetColor("_EmissionColor", _color);

            // 適用
            _renderer.sharedMaterial = newMat;
        }
        else
        {
#if DEBUG && UNITY_EDITOR
            Debug.LogWarning($"{this.gameObject.name} の Renderer が見つかりません");
#endif
        }
    }

    /// <summary>
    /// フラグを反転
    /// </summary>
    public void ChangeColorFlag()
    {
        m_isChangeColor = !m_isChangeColor;
    }
}
