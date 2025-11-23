/*
 * ファイル
 * DataBase C#
 * 
 * システム
 * 情報とその種類を分類するEnumを持つデータの基底となる抽象クラス
 * 
 * 変更履歴
 * 2025/07/16　奥山　凜　作成
 */

using UnityEngine;

// コメントアウト部分は継承先で書く
//[CreateAssetMenu(fileName = "○○Data", menuName = "ScriptableObjects/Create○○Data", order = ○○)]
/// <summary>
/// 情報とその種類を分類するEnumを持つデータの基底となる抽象クラス<br/>
/// TEDataName 情報の種類を分類するEnum<br/>
/// </summary>
public abstract class DataBase<TEDataName> : ScriptableObject where TEDataName : System.Enum
{
    public TEDataName DataName => _dataName;
    [SerializeField]
    private TEDataName _dataName;       // 情報の種類を分類するEnum
}
