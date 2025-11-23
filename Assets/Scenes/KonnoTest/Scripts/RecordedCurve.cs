using System;
using UnityEngine;

[Serializable]
public class RecordedCurve
{
    public string path;             // バインディングのパス
    public string propertyName;     // プロパティ名
    public float[] times;           // キーフレーム時刻配列
    public float[] values;          // キーフレーム値配列
}

[CreateAssetMenu(
    menuName = "Timeline/RecordedKeyData",
    fileName = "NewRecordedKeyData")]
public class RecordedKeyData : ScriptableObject
{
    public RecordedCurve[] curves;  // 複数プロパティ分まとめて持てる
}
