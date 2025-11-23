/*
 * ファイル
 * ImageFadeBlinker C#
 * 
 * システム
 * UIを点滅させる
 * 
 * 変更履歴
 * 2025/09/22　奥山　凜　作成
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIを点滅させるクラス<br/>
/// </summary>
public class ImageFadeBlinker : MonoBehaviour
{
    [SerializeField]
    private float blinkSpeed = 2.0f; // 1秒間に何回フェードイン/アウトするか

    private Image image;        // 点滅させるUI

    void Awake()
    {
        image = GetComponent<Image>();
    }

    // このGameObjectが有効になった時にコルーチンを開始
    void OnEnable() 
    {
        StartCoroutine(FadeBlinkRoutine());
    }

    // このGameObjectが無効になった時にコルーチンを停止
    void OnDisable() 
    {
        StopAllCoroutines();
        // 必要に応じて、完全に不透明な状態に戻しておく
        Color currentColor = image.color;
        currentColor.a = 1f;
        image.color = currentColor;
    }

    /// <summary>
    /// Imageのアルファ値を滑らかに変化させるコルーチン<br/>
    /// </summary>
    /// <returns>なし</returns>
    private IEnumerator FadeBlinkRoutine()
    {
        while (true)
        {
            // 時間と共にアルファ値を計算（Sin波の往復運動）
            // Math.Sinは-1から1の範囲なので、0から1に変換する ( (sin(t) + 1) / 2 )
            float alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;

            // Imageの色を取得し、アルファ値を更新して設定
            Color currentColor = image.color;
            currentColor.a = alpha;
            image.color = currentColor;

            yield return null; // 1フレーム待機してループ
        }
    }
}