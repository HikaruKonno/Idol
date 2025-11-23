using UnityEngine;
using System.Collections.Generic;

public class PenlightManager : MonoBehaviour
{
    public List<PenlightData> m_penlightList = new List<PenlightData>();

    void Update()
    {
        float time = Time.time;
        foreach (var penlight in m_penlightList)
        {
            penlight.Update(time);
        }
    }
}

/// <summary>
/// ペンライトのデータを管理するクラス
/// </summary>
public class PenlightData
{
    public Transform m_transform;
    public float m_shakeSpeed;
    public float m_maxShakeAngle;
    public float m_offset;
    public PenlightAnimation.ShakeMode m_mode;

    /// <summary>
    /// ペンライトのデータを初期化する関数
    /// </summary>
    /// <param name="_time">経過時間</param>
    public void Update(float _time)
    {
        // 時間
        float time = _time + m_offset;
        // ふり幅の動き
        float angle = Mathf.Sin(time * m_shakeSpeed) * m_maxShakeAngle;
        // 角度を適用
        m_transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
