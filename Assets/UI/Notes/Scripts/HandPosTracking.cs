/*
 * ファイル
 * HandPosTracking C#
 * 
 * システム
 * アバターの手のワールド座標をスクリーン座標に変換して位置更新する
 * 
 * 作成
 * 2025/08/23 寺門 冴羽
 * 
 * 最終変更
 * 2025/09/01 寺門 冴羽
 */

using UnityEngine;
using UnityEngine.UI;

public class HandPosTracking : MonoBehaviour
{
    // --------------------------------------------------------------------------------
    // 変数宣言
    // --------------------------------------------------------------------------------
    
    [SerializeField] private GameObject m_leftHand;
    [SerializeField] private GameObject m_rightHand;
    [SerializeField] private RectTransform m_leftHandUIPos;
    [SerializeField] private RectTransform m_rightHandUIPos;

    [SerializeField] private Camera m_UICamera;

    // --------------------------------------------------------------------------------
    // メイン関数
    // --------------------------------------------------------------------------------

    // 最初のフレームの処理
    void Start()
    {
        // モデルの手の位置をスクリーン座標に変換して更新する処理
        ConvertHandPosToScreenPos();
    }

    // 毎フレームの処理
    void Update()
    {
        // モデルの手の位置をスクリーン座標に変換して更新する処理
        ConvertHandPosToScreenPos();
    }

    // --------------------------------------------------------------------------------
    // サブ関数
    // --------------------------------------------------------------------------------

    // モデルの手の位置をスクリーン座標に変換して更新する処理
    private void ConvertHandPosToScreenPos()
    {
        // 左手が存在しているかの判定
        if (m_leftHand != null)
        {
            // 手の座標をスクリーン座標に変換
            // 手の座標に合わせて位置を更新
            m_leftHandUIPos.position = m_UICamera.WorldToScreenPoint(m_leftHand.transform.position);
            m_leftHandUIPos.position = new Vector3(m_leftHandUIPos.position.x, m_leftHandUIPos.position.y,0);
        }

        // 右手が存在しているかの判定
        if (m_rightHand != null)
        {
            // 手の座標をスクリーン座標に変換
            // 手の座標に合わせて位置を更新
            m_rightHandUIPos.position = m_UICamera.WorldToScreenPoint(m_rightHand.transform.position);
            m_rightHandUIPos.position = new Vector3(m_rightHandUIPos.position.x, m_rightHandUIPos.position.y, 0);
        }
    }
}
