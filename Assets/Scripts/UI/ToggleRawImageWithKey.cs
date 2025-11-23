/*
 * ファイル
 * ToggleRawImageWithKey C#
 * 
 * システム
 * キー入力でイメージのオンオフを切り替える
 * 
 * 変更履歴
 * 2025/09/24　奥山　凜　作成
 * 2025/10/05　奥山　凜　イメージを最初から表示するかを切り替えられるように変更
 */

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// キー入力でイメージのオンオフを切り替えるクラス<br/>
/// </summary>
public class ToggleRawImageWithKey: MonoBehaviour
{
    [SerializeField] 
    private RawImage m_rawImage;       // オンオフを切り替えるUI
    [SerializeField]
    private KeyCode m_keyCode;         // 切り替えのキー
    [SerializeField]
    private bool m_isDisplayedByDefault = false;        // デフォルトでの表示、非表示の切り替え


    void Start()
    {
        if (m_rawImage == null || m_keyCode == KeyCode.None)
        {
#if UNITY_EDITOR
            Debug.LogError("UIの表示、非表示の切り替えに必要なImageまたはキーが設定されていません");
#endif
            enabled = false;
        }

        if ((m_isDisplayedByDefault) != (m_rawImage.gameObject.activeSelf))
        {
            ToggleUI();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(m_keyCode))
        {
            ToggleUI();
        }
    }

    /// <summary>
    /// UIのアクティブ、非アクティブを切り替える<br/>
    /// </summary>
    /// <returns>なし</returns>
    void ToggleUI()
    {
        m_rawImage.gameObject.SetActive(!m_rawImage.gameObject.activeSelf);
    }
}
