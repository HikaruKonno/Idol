/*
 * ファイル
 * BigFieldAttribute C#
 * 
 * システム
 * フォントサイズを変える属性クラスの定義
 * 
 * 作成
 * 2025/09/12 坂上　壱希
 * 
 * 最終変更
 * 2025/09/29 坂上　壱希
 */
using UnityEngine;

sealed public class BigFieldAttribute : PropertyAttribute
{
    //フォントの大きさ
    public int FontSize { get; }

    public BigFieldAttribute(int _fontSize = 20)
    {
        FontSize = _fontSize;
    }
}
