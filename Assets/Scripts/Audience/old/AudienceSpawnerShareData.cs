using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudienceSpawnerShareData", menuName = "Create ScriptableObject/AudienceSpawnerShareData")]
public class AudienceSpawnerShareData : ScriptableObject
{
    [Header("サイリウム設定")]
    // サイリウムの色
    public List<Color> m_psylliumColorList;
    [Tooltip("サイリウムのエミッシブの強さ")]
    public float m_psylliumIntensity = 2.0f;

    [Header("生成時のノイズ")]
    [Tooltip("横の最大のノイズ")]
    public float m_maxNoiseLength;
    [Tooltip("横の最小のノイズ")]
    public float m_minNoiseLength;
    [Tooltip("縦の最大のノイズ")]
    public float m_maxNoiseWidth;
    [Tooltip("縦の最小のノイズ")]
    public float m_minNoiseWidth;
    [Tooltip("高さの最大のノイズ")]
    public float m_maxNoiseHight;
    [Tooltip("高さの最小のノイズ")]
    public float m_minNoiseHight;

    [Tooltip("ターゲット")]
    // ゲームオブジェクトの場合
    public GameObject m_targetGameObject;
    // Transformの場合
    public Transform m_targetTransform;
}
