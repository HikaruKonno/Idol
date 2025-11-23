#define DEBUG
#undef DEBUG

using UnityEngine;

public class AudienceAnimation : MonoBehaviour
{
    [Header("ジャンプの設定")]
    // ジャンプの間隔
    [SerializeField, Tooltip("ジャンプの間隔（秒）")]
    private float m_jumpInterval;
    // ジャンプの速度
    [SerializeField, Tooltip("ジャンプの速度")]
    private float m_jumpSpeed;
    // ジャンプの高さ
    [SerializeField, Tooltip("ジャンプの高さ")]
    private float m_jumpHeight;
    [SerializeField, Tooltip("最大のジャンプの高さのノイズ")]
    private float m_maxJumpHeightNoise;
    [SerializeField, Tooltip("最小のジャンプの高さのノイズ")]
    private float m_minJumpHeightNoise;
    [SerializeField, Tooltip("最大のジャンプの速さのノイズ")]
    private float m_maxJumpSpeedNoise;
    [SerializeField, Tooltip("最小のジャンプの速さのノイズ")]
    private float m_minJumpSpeedNoise;

    [Header("ジャンプ確率の設定")]
    [SerializeField, Range(0f, 1f), Tooltip("ジャンプを行う確率（0〜1）")]
    private float m_jumpProbability = 1.0f;

    // ジャンプ中かどうか
    private bool m_isJumping = false;
    // ジャンプの高さのノイズをランダムに決める
    private float m_randomHeightOffset = 0.0f;
    // ジャンプの高さのノイズをランダムに決める
    private float m_randomSpeedOffset = 0.0f;
    // 経過時間の管理
    private float m_jumpTimer = 0.0f;
    private float m_time = 0.0f;
    // 初期の位置
    private Vector3 m_initialPosition;

    [SerializeField]
    private bool IsJump = false;

    void Start()
    {
        // 初期の座標の保存
        m_initialPosition = this.transform.position;

        // 引数に渡した範囲の中でランダムにノイズを決める
        m_randomHeightOffset = Random.Range(m_minJumpHeightNoise, m_maxJumpHeightNoise);    // 高さ
        m_randomSpeedOffset = Random.Range(m_minJumpSpeedNoise, m_maxJumpSpeedNoise);       // 速さ

        // ジャンプの高さにノイズを加える
        m_jumpHeight += m_randomHeightOffset;
        m_jumpSpeed += m_randomSpeedOffset;

        enabled = IsJump;
    }
    void Update()
    {
        if (m_isJumping)
        {
            // ジャンプのアニメーション
            AnimateJump();
        }
        else
        {
            // ジャンプ経過時間の加算
            m_jumpTimer += Time.deltaTime;

            if (m_jumpTimer >= m_jumpInterval)
            {
                // ジャンプ可能にする
                m_isJumping = true;

                // 確率によってジャンプするか決定
                if (Random.value <= m_jumpProbability)
                {
                    m_isJumping = true;
                    m_time = 0.0f;
                }
            }
        }
    }

    /// <summary>
    /// サイン波に基づくジャンプのアニメーション
    /// </summary>
    private void AnimateJump()
    {
        m_time += Time.deltaTime;

        // Abs()は負の値を正にする関数。Abs(sin())で０～１の値を繰り返す
        float offsetY = Mathf.Abs(Mathf.Sin(m_time * m_jumpSpeed)) * m_jumpHeight;

        float newY = m_initialPosition.y + offsetY;

        // Y座標のみ変更
        this.transform.position = new Vector3(m_initialPosition.x, newY, m_initialPosition.z);

        // ジャンプ終了判定
        // Sin波は２πで一周期だが、Abs()で負の値を正の値にしているため、πで一周期になる
        if (m_time * m_jumpSpeed >= Mathf.PI)
        {
            // 最終位置を明示的に初期位置に戻す
            transform.position = m_initialPosition;
            // ジャンプ終了
            m_isJumping = false;
        }
    }
}
