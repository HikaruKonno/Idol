#define DEBUG
#undef DEBUG
using UnityEngine;

public class PenlightAnimation : MonoBehaviour
{
    /// <summary>
    /// 振るモード<br/>
    /// BasicX    ：横振り<br/>
    /// BasicY    ：縦振り<br/>
    /// BasicYZ   ：縦上振り<br/>
    /// ShakeShake：左右に早く揺れる<br/>
    /// Stop      ：待機<br/>
    /// </summary>
    public enum ShakeMode : byte
    {
        /// <summary>横振り</summary>
        BasicX = 1,
        /// <summary>縦振り</summary>
        BasicY = 2,
        /// <summary>縦上振り</summary>
        BasicYZ = 3,
        /// <summary>左右に早く揺れる</summary>
        ShakeShake = 4,
        /// <summary>待機</summary>
        Stop = 0
    }
    [SerializeField]
    private ShakeMode m_shakeMode;
    // 最初のペンライトの振り方
    private ShakeMode m_initialShakeMode;
    /// <summary>
    /// ペンライトの振りの同期モード<br/>
    /// Synchronized    ：常にそろっている<br/>
    /// Unsynchronized  ：常にそろっていない<br/>
    /// </summary>
    [SerializeField]
    public enum ShakeSyncMode : byte
    {
        /// <summary>常にそろっている</summary>
        Synchronized = 0,
        /// <summary>常にそろわない</summary>
        Unsynchronized = 1,
    }
    [SerializeField]
    private ShakeSyncMode m_shakeSyncMode;

    // ペンライトを振る速さ
    [SerializeField]
    private float m_shakeSpeed;
    // ペンライトを振る角度の最大値
    [SerializeField]
    private float m_maxShakeAngle;
    // ランダムのオフセット
    private float m_randomOffset;
    // 初期の位置
    private Vector3 m_initialPosition;

    [Header("BasicY Settings")]
    [SerializeField, Tooltip("BasicY 振りの最小角度（度）")]
    private float m_minShakeAngleY = -30f;
    [SerializeField, Tooltip("BasicY 振りの最大角度（度）")]
    private float m_maxShakeAngleY = 30f;

    [Header("BasicYZ Settings")]
    [SerializeField, Tooltip("Y軸の振幅")]
    private float m_basicYZAmplitudeY;
    [SerializeField, Tooltip("Z軸の振幅")]
    private float m_basicYZAmplitudeZ;
    [SerializeField, Tooltip("揺れの大きさを小さくする調整値")]
    private float m_amplitudeScale = 0.1f;
    [SerializeField, Tooltip("一往復する周期（秒）")]
    private float m_basicYZPeriod;

    [Header("ShakeShake Settings")]
    [SerializeField, Tooltip("振る早さ")]
    private float m_shakeshakeSpeed = 20f;
    [SerializeField, Tooltip("振る最大の角度")]
    private float m_maxShakeshakeAngle = 30f;

    // 徐々に止まっているか
    private bool m_isStopping = false;
    // 停止までの時間（秒）
    private float m_stopDuration = 3f;
    // 停止開始からの経過時間
    private float m_stopElapsed = 0f;
    // 次に切り替えるモード
    private ShakeMode m_nextShakeMode = 0;
    // 切り替えた後の同期モード
    private ShakeSyncMode m_nextShakeSyncMode = 0;

    void Start()
    {
        m_initialPosition = this.transform.position;
        m_initialShakeMode = m_shakeMode;

        // ±πの範囲でランダムにオフセット
        m_randomOffset = Random.Range(-Mathf.PI, Mathf.PI);

#if DEBUG && UNITY_EDITOR
        Debug.Log($"m_randomOffset{m_randomOffset}");
#endif
    }

    void FixedUpdate()
    {
        // 経過時間
        float time = Time.time;

        // ランダムで振る場合、個々のペンライトで時間をずらす
        if (m_shakeSyncMode == ShakeSyncMode.Unsynchronized)
        {
            time += m_randomOffset;
        }

        // 徐々に停止させる処理
        if (m_isStopping)
        {
            // 経過時間の加算
            m_stopElapsed += Time.fixedDeltaTime;

            // 停止処理の進行度
            float rogress = m_stopElapsed / m_stopDuration;

            // 停止処理の進行度を0～１の間で補正
            rogress = Mathf.Clamp01(rogress);

            // 徐々に振り角度を減衰させる
            float attenuationValue = 1f - rogress;

            // 振り毎の動き
            switch (m_shakeMode)
            {
                // 横振り
                case ShakeMode.BasicX:
                    {
                        float angle = Mathf.Sin(time * m_shakeSpeed) * m_maxShakeAngle * attenuationValue;
                        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                        break;
                    }
                // 縦振り
                case ShakeMode.BasicY:
                    {
                        // -1～1 のサインを 0～1 に正規化
                        float s = (Mathf.Sin(time * m_shakeSpeed) + 1f) * 0.5f;
                        // 最小から最大角度の間に補正
                        float angle = Mathf.Lerp(m_minShakeAngleY, m_maxShakeAngleY, s) * attenuationValue;

                        transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
                        break;
                    }
                // 縦上振り
                case ShakeMode.BasicYZ:
                    {
                        float sinValue = Mathf.Sin(Mathf.PI * time / m_basicYZPeriod);
                        // 周期的な直線運動
                        float offsetY = m_basicYZAmplitudeY * m_amplitudeScale * sinValue * attenuationValue;
                        float offsetZ = m_basicYZAmplitudeZ * m_amplitudeScale * sinValue * attenuationValue;

                        // 初期位置を基に上下前後に移動
                        Vector3 newPos = new Vector3(
                            m_initialPosition.x,
                            m_initialPosition.y + offsetY,
                            m_initialPosition.z + offsetZ
                        );
                        transform.position = newPos;

                        // 角度を決める（Yのピークで最大角度）
                        float normalizedY = offsetY / (m_basicYZAmplitudeY * m_amplitudeScale);
                        float angle = normalizedY * m_maxShakeAngle * attenuationValue;
                        transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
                        break;
                    }
                // 左右に早く揺れる
                case ShakeMode.ShakeShake:
                    {
                        float angle = Mathf.Sin(time * m_shakeshakeSpeed) * m_maxShakeshakeAngle * attenuationValue;    // -1〜1
                        transform.localRotation = Quaternion.Euler(0f, 0f, angle);                                      // -maxAngle〜maxAngle
                        break;
                    }
                // 待機
                case ShakeMode.Stop:
                    {
                        // 少し揺れているように動かす為に0.1f倍する
                        float angle = Mathf.Sin(time * 0.5f) * (m_maxShakeAngle * 0.1f) * attenuationValue;
                        transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
                        break;
                    }
            }

            // 停止完了後の処理
            if (rogress >= 1f)
            {
                //次のモードに切り替え
                m_isStopping = false;
                m_shakeMode = m_nextShakeMode;
                m_shakeSyncMode = m_nextShakeSyncMode;

                // 初期位置や回転をリセット
                transform.position = m_initialPosition;
                transform.localRotation = Quaternion.identity;
            }
        }
        else
        {
            // 振り毎の動き
            switch (m_shakeMode)
            {
                // 横振り
                case ShakeMode.BasicX:
                    {
                        float angle = Mathf.Sin(time * m_shakeSpeed) * m_maxShakeAngle;
                        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                        break;
                    }

                // 縦振り
                case ShakeMode.BasicY:
                    {
                        // -1～1 のサインを 0～1 に正規化
                        float s = (Mathf.Sin(time * m_shakeSpeed) + 1f) * 0.5f;

                        // 最小から最大角度の間に補正
                        float angle = Mathf.Lerp(m_minShakeAngleY, m_maxShakeAngleY, s);

                        transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
                        break;
                    }

                // 縦上振り
                case ShakeMode.BasicYZ:
                    {
                        // sinの作成
                        float sinValue = Mathf.Sin(Mathf.PI * time / m_basicYZPeriod);

                        // 周期的な直線運動
                        float offsetY = m_basicYZAmplitudeY * m_amplitudeScale * sinValue;
                        float offsetZ = m_basicYZAmplitudeZ * m_amplitudeScale * sinValue;

#if DEBUG && UNITY_EDITOR
                        Debug.Log($"offsetY{offsetY}、offsetZ{offsetZ}");
#endif

                        // 初期位置に対してオフセットを加える
                        Vector3 newPos = new Vector3(
                            m_initialPosition.x,
                            m_initialPosition.y + offsetY,
                            m_initialPosition.z + offsetZ
                        );

                        // 移動を適用
                        if (transform.position != Vector3.zero)
                        {
                            transform.position = newPos;
                        }
                        else
                        {
#if DEBUG && UNITY_EDITOR
                            Debug.LogError($"ペンライトのtransform.position == Vector3.zero");
#endif
                        }

                        // 角度を決める（Yのピークで最大角度）
                        float normalizedY = offsetY / (m_basicYZAmplitudeY * m_amplitudeScale);    // -1〜1                    
                        float angle = normalizedY * m_maxShakeAngle;                               // -maxAngle〜maxAngle

#if DEBUG && UNITY_EDITOR
                        Debug.Log($"normalizedY{normalizedY}、angle{angle}");
#endif

                        // 回転を適用
                        transform.localRotation = Quaternion.Euler(angle, 0f, 0f);

                        break;
                    }

                // 左右に早く揺れる
                case ShakeMode.ShakeShake:
                    {
                        float angle = Mathf.Sin(time * m_shakeshakeSpeed) * m_maxShakeshakeAngle;
                        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                        break;
                    }

                // 待機（少し揺れているように）
                case ShakeMode.Stop:
                    {
                        // 少し揺れているように動かす為に0.1f倍する
                        float angle = Mathf.Sin(time * 0.5f) * (m_maxShakeAngle * 0.1f);
                        transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
                        break;
                    }
            }
        }
    }

    /// <summary>
    /// 振るモードを変える関数
    /// </summary>
    /// <param name="_shakeMode">振るモード</param>
    public void ChangeShakeMode(ShakeMode _shakeMode)
    {
        // 引数の振るモードに変更
        m_shakeMode = _shakeMode;
    }

    /// <summary>
    /// 徐々に振りを止まらせて次の振り方に切り替えるようにする関数
    /// </summary>
    /// <param name="_nextMode">次の振り方</param>
    /// <param name="_stopDuration">切り替えるまでの期間</param>
    public void StopAndChangeShakeMode(ShakeMode _nextMode, ShakeSyncMode _shakeSyncMode, float _stopDuration = 1f)
    {
        if (m_isStopping) return; // すでに停止処理を行っている場合は戻る

        m_isStopping = true;
        m_stopElapsed = 0f;
        m_stopDuration = _stopDuration;
        m_nextShakeMode = _nextMode;
        m_nextShakeSyncMode = _shakeSyncMode;
    }

    /// <summary>
    /// 左右に振るモードに変える関数
    /// </summary>
    public void ChangeShakeShake()
    {
        // 振りをバラバラにする
        ChangeUnsynchronized();

        ChangeShakeMode(ShakeMode.ShakeShake);
    }

    /// <summary>
    /// 振るモードのリセットする関数
    /// </summary>
    public void ResetShakeMode()
    {
        ChangeShakeMode(m_initialShakeMode);
    }

    /// <summary>
    /// 振りをバラバラにする関数
    /// </summary>
    public void ChangeUnsynchronized()
    {
        m_shakeSyncMode = ShakeSyncMode.Unsynchronized;
    }

    /// <summary>
    /// 振りを常に揃わせる関数
    /// </summary>
    public void ChangeSynchronized()
    {
        m_shakeSyncMode = ShakeSyncMode.Synchronized;
    }

    /// <summary>
    /// 振る速さを変える関数
    /// </summary>
    /// <param name="_speed">振る速さ</param>
    public void ChangeShakeShakeSpeed(float _speed)
    {
        m_shakeshakeSpeed = _speed;
    }
}