using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudienceChangeColorData", menuName = "Create ScriptableObject/AudienceChangeColorData")]
public class AudienceChangeColorData : ScriptableObject
{
    [Header("サイリウム設定")]
    // サイリウムの色
    public List<Color> m_psylliumColorList;
    [Tooltip("サイリウムのエミッシブの強さ")]
    public float m_psylliumIntensity = 2.0f;
}
