/*
 * ファイル
 * JudgeUI C#
 * 
 * システム
 * UIの手の判定処理
 * 
 * 作成
 * 2025/09/01 寺門 冴羽
 * 
 * 最終変更
 * 2025/09/01 寺門 冴羽
 */

using UnityEngine;

public class JudgeUI : MonoBehaviour
{
    // --------------------------------------------------------------------------------
    // 変数宣言
    // --------------------------------------------------------------------------------

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // --------------------------------------------------------------------------------
    // イベント関数
    // --------------------------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Hit " +  other.gameObject.name);
    }
}
