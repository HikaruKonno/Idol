/*
 * ファイル
 * HandTrailManager C#
 * 
 * システム
 * Media Pipeから受け取った手の位置を元に手の位置を示すトレイルを動かす
 * 
 * 変更履歴
 * 2025/09/22　奥山　凜　作成
 * 2025/10/16　奥山　凜
 *             神野　琉生　　トレイルが消える速度が加速度に応じて変わるように修正
 */

using UnityEngine;

/// <summary>
/// Media Pipeから受け取った手の位置を元に手の位置を示すトレイルを動かすクラス
/// </summary>
public class HandTrailManager : MonoBehaviour
{
    [SerializeField]
    private TrailRenderer m_leftTrailRenderer;          // 左手のトレイル
    [SerializeField]
    private RectTransform m_leftHandTrailUIPos;         // 左手のトレイルの位置
    [SerializeField]
    private TrailRenderer m_rightTrailRenderer;         // 右手のトレイル
    [SerializeField]
    private RectTransform m_rightHandTrailUIPos;        // 右手のトレイルの位置
    [SerializeField]
    private float m_smoothFactor = 10f;                 // トレイルが手の位置に移動する速度
    [SerializeField]
    private float m_endSmoothFactor = 10f;              // トレイルが消える時間の変化の速度に影響
    [SerializeField]
    private float m_accelerationFactor = 40f;           // トレイルが消えるまでの速さに影響
    [SerializeField]
    private Camera m_uiCamera;                          // トレイルを表示しているキャンバスを写すカメラ
    [SerializeField]
    private MediapipeResultDataContainer m_mediapipeResultDataContainer;        // Mediapipeの結果が入ったスクリプタブルオブジェクト

    private float m_leftBasetime;                       // TrailRendererのラインが描画されてから消えるまでの時間の初期設定値
    private float m_rightBasetime;                      // TrailRendererのラインが描画されてから消えるまでの時間の初期設定値
    private Vector3 m_leftPreviousVelocity = Vector3.zero;           // 前回の速度
    private Vector3 m_rightPreviousVelocity = Vector3.zero;          // 前回の速度

    private Vector2 SCREEN_SIZE = new Vector2(1920, 1080);           // 画面サイズ(Screen.width、Screen.heightを使用するとUIの表示の補正と混ざるのか4K等で描画した時にズレる)
    private void Awake()
    {
        if (m_mediapipeResultDataContainer == null)
        {
            m_mediapipeResultDataContainer = Resources.Load("Datas/Scene/MediapipeResultDataContainer") as MediapipeResultDataContainer;
        }
        m_leftBasetime = m_leftTrailRenderer.time;
        m_rightBasetime = m_rightTrailRenderer.time;
    }

    void Update()
    {
        // 左手
        if (m_mediapipeResultDataContainer.LeftHandNormalizedLandmarkList != null)
        {
            Vector3 previousPos = m_leftHandTrailUIPos.position;

            m_leftHandTrailUIPos.position = UpdateHandTrailPosition(m_mediapipeResultDataContainer.LeftHandNormalizedLandmarkList[(int)EHandLandmarks.IndexMCP], m_leftHandTrailUIPos);

            Vector3 velocity = m_leftHandTrailUIPos.position - previousPos;     // 現在の速度
            Vector3 currentAcceleration = (velocity - m_leftPreviousVelocity) / Time.deltaTime;       // 現在の加速度
            m_leftPreviousVelocity = velocity;      // 現在の速度を前回の速度に保存

            float accelerationMagnitude = currentAcceleration.magnitude;      // 加速度のベクトルの長さを取得することでfloatに変換

            // トレイルの終端が消えるまでの生存期間を移動速度（移動距離）に応じて変更
            float trailRendererTime = Mathf.Max(0.1f, m_leftBasetime - accelerationMagnitude * m_accelerationFactor);       // 加速度のベクトルの長さに応じて値を変化させる
            
            m_leftTrailRenderer.time = Mathf.Lerp(m_leftTrailRenderer.time, trailRendererTime, Time.deltaTime * m_endSmoothFactor);
        }
        // 右手
        if (m_mediapipeResultDataContainer.RightHandNormalizedLandmarkList != null)
        {
            Vector3 previousPos = m_rightHandTrailUIPos.position;

            m_rightHandTrailUIPos.position = UpdateHandTrailPosition(m_mediapipeResultDataContainer.RightHandNormalizedLandmarkList[(int)EHandLandmarks.IndexMCP], m_rightHandTrailUIPos);

            Vector3 velocity = m_rightHandTrailUIPos.position - previousPos;     // 現在の速度
            Vector3 currentAcceleration = (velocity - m_rightPreviousVelocity) / Time.deltaTime;       // 現在の加速度
            m_rightPreviousVelocity = velocity;      // 現在の速度を前回の速度に保存

            float accelerationMagnitude = currentAcceleration.magnitude;      // 加速度のベクトルの長さを取得することでfloatに変換

            // トレイルの終端が消えるまでの生存期間を移動速度（移動距離）に応じて変更
            float trailRendererTime = Mathf.Max(0.1f, m_rightBasetime - accelerationMagnitude * m_accelerationFactor);       // 加速度のベクトルの長さに応じて値を変化させる

            m_rightTrailRenderer.time = Mathf.Lerp(m_rightTrailRenderer.time, trailRendererTime, Time.deltaTime * m_endSmoothFactor);
        }
    }

    /// <summary>
    /// ランドマークを元に新しいトレイルの位置を求める<br/>
    /// 引数1： _handLandmark 手の位置のランドマーク<br/>
    /// 引数2： _handTrailUIPos トレイルの位置<br/>
    /// </summary>
    /// <param name="_handLandmark">手の位置のランドマーク</param>
    /// <param name="_handTrailUIPos">トレイルの位置</param>
    /// <returns>新しいトレイルの位置</returns>
    private Vector3 UpdateHandTrailPosition(Mediapipe.NormalizedLandmark _handLandmark, RectTransform _handTrailUIPos)
    {
        Vector3 worldPos;
        Vector2 handViewportPos = MediaPipeUtils.ConvertToViewportPos(_handLandmark);
        Vector2 screenPos = handViewportPos * SCREEN_SIZE;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_handTrailUIPos.parent as RectTransform, screenPos, m_uiCamera, out worldPos))
        {
            return Vector3.Lerp(_handTrailUIPos.position, worldPos, Time.deltaTime * m_smoothFactor);
        }

        return _handTrailUIPos.position;       // 変換に失敗した場合現在の位置を返す
    }
}
