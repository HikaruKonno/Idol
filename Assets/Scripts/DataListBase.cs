/*
 * ファイル
 * DataListBase C#
 * 
 * システム
 * Dataを配列として持つDataListの基底となる抽象クラス
 * 
 * 変更履歴
 * 2025/07/16　奥山　凜　作成
 */

using System.Collections.Generic;
using UnityEngine;

// CreateAssetMenuは継承先に書く
//[CreateAssetMenu(fileName = "○○DataList", menuName = "ScriptableObjects/Create○○DataList", order = ○○)]
/// <summary>
/// Dataを配列として持つDataListの基底となる抽象クラス<br/>
/// TEDataName Dataの情報の種類を分類するEnum<br/>
/// TDataBase  継承先のクラスが管理するDataのクラス<br/>
/// </summary>
public abstract class DataListBase<TEDataName, TData> : ScriptableObject
    where TEDataName : System.Enum
    where TData      : DataBase<TEDataName>
{
    public List<TData> Datas => _datas;
    [SerializeField]
    private List<TData> _datas;     // データを保持するリスト

    /// <summary>
    /// 指定したEnumの種類のDataを取得する<br/>
    /// 引数1：_landmark 探すDataの種類<br/>
    /// </summary>
    /// <param name="_tDataName">探すDataの種類</param>
    /// <returns>見つかったData<br/>（最初に見つかった物が返り、ない場合nullが変える）</returns>
    public TData GetData(TEDataName _tDataName)
    {
        foreach (TData data in _datas)
        {
            if (data.DataName.Equals(_tDataName))
            {
                return data;
            }
        }
        return null;
    }
}