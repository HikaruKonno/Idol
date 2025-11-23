/*
 * ファイル
 * RotationFilter C#
 * 
 * システム
 * ボーンの回転にかけるフィルター
 * 
 * 変更履歴
 * 2025/07/23　奥山　凜　作成
 */

using UnityEngine;

/// <summary>
/// ボーンの回転にかけるフィルターのクラス<br/>
/// 非MonoBehaviour
/// </summary>
public class RotationFilter
{
    private OneEuroFilterFloat m_filterX;        // X回転用フィルター
    private OneEuroFilterFloat m_filterY;        // Y回転用フィルター
    private OneEuroFilterFloat m_filterZ;        // Z回転用フィルター

    private RotationLimit m_rotationLimit;       // 回転の制限


    private float m_prevX = 0f;                  // 前回のX回転
    private float m_prevY = 0f;                  // 前回のY回転
    private float m_prevZ = 0f;                  // 前回のZ回転

    /// <summary>
    /// ボーンの回転にかけるフィルターのクラスのコンストラクタ<br/>
    /// 引数1：_freq サンプリング周波数（fpsを指定）<br/>
    /// 引数2：_minCutoff 最低限の平滑化、回転（小さいほどなめらか）<br/>
    /// 引数3：_beta 動きの速さへの反応性（大きい程応答性高）<br/>
    /// 引数4：_dCutoff 速度（微分）のフィルタ強度<br/>
    /// 引数4：_rotationLimit 回転の制限
    /// </summary>
    /// <param name="_freq">サンプリング周波数（fpsを指定）</param>
    /// <param name="_minCutoff">最低限の平滑化、回転（小さいほどなめらか）</param>
    /// <param name="_beta">動きの速さへの反応性（大きい程応答性高）</param>
    /// <param name="_dCutoff">速度（微分）のフィルタ強度</param>
    /// <param name="_rotationLimit">回転の制限</param>
    /// <returns>なし（このクラスのインスタンス）</returns>
    public RotationFilter(float _freq, float _minCutoff, float _beta,float _dCutoff, RotationLimit _rotationLimit = null)
    {
        m_filterX = new OneEuroFilterFloat(_freq, _minCutoff, _beta, _dCutoff);
        m_filterY = new OneEuroFilterFloat(_freq, _minCutoff, _beta, _dCutoff);
        m_filterZ = new OneEuroFilterFloat(_freq, _minCutoff, _beta, _dCutoff);
        m_rotationLimit = _rotationLimit;
    }

    /// <summary>
    /// Wrapを回避して連続角度に変換する<br/>
    /// 前回の角度から今回の角度になるのに必要な最短の角度を加算する事で+179°から-179°になる際、差が358°となる（本来は2°）不具合を避ける<br/>
    /// 引数1：_current 現在の値<br/>
    /// 引数2：_previous 前回の値（変換後の値）
    /// </summary>
    /// <param name="_current">現在の値</param>
    /// <param name="_previous">前回の値（変換後の値）</param>
    /// <returns>変換した値</returns>
    private float UnwrapAngle(float _current, ref float _previous)
    {
        // Mathf.DeltaAngleは、-180から180の範囲で角度の差を計算する
        float delta = Mathf.DeltaAngle(_previous, _current);
        // その差を前回の角度に加算して連続的な角度に変換
        _previous += delta;
        return _previous;
    }

    /// <summary>
    /// 回転にフィルターをかける<br/>
    /// 引数1：_rotation フィルターをかけた回転
    /// </summary>
    /// <param name="_rotation">フィルターをかける回転</param>
    /// <returns>フィルターした回転</returns>
    public Quaternion Filter(Quaternion _rotation)
    {
        // QuaternionをEuler角に変換
        // 0から360度（この後-180から180の範囲に変換）
        Vector3 euler = _rotation.eulerAngles;

        // -180から180の範囲に変換
        euler.x = Mathf.DeltaAngle(0, euler.x);
        euler.y = Mathf.DeltaAngle(0, euler.y);
        euler.z = Mathf.DeltaAngle(0, euler.z);

        // Unwrap角度（連続性を保つため）
        // 前回の角度から今回の角度になるのに必要な最短の角度を加算する事で+179°から-179°になる際、差が358°となる（本来は2°）不具合を避ける
        // 返ってくる値は-180から180の範囲外になることもある
        float x = UnwrapAngle(euler.x, ref m_prevX);
        float y = UnwrapAngle(euler.y, ref m_prevY);
        float z = UnwrapAngle(euler.z, ref m_prevZ);


        // OneEuroFilterで滑らかに
        x = m_filterX.Filter(x);
        y = m_filterY.Filter(y);
        z = m_filterZ.Filter(z);

        if (m_rotationLimit != null)
        {
            // x = 180n + a 
            // aを消したいから -(x % 180.0f)これで180nが残る
            // Clampでaを出す
            // -180 ~ 180 の間
            x += -(x % 180.0f) + Mathf.Clamp(x % 180.0f, m_rotationLimit.LimitX.x, m_rotationLimit.LimitX.y);
            y += -(y % 180.0f) + Mathf.Clamp(y % 180.0f, m_rotationLimit.LimitY.x, m_rotationLimit.LimitY.y);
            z += -(z % 180.0f) + Mathf.Clamp(z % 180.0f, m_rotationLimit.LimitZ.x, m_rotationLimit.LimitZ.y);
        }
        return Quaternion.Euler(x, y, z);
    }
}

/// <summary>
/// 回転の制限の情報を纏めたクラス<br/>
/// 非MonoBehaviour
/// </summary>
public class RotationLimit
{
    // Vector2のxをmin、yをmaxとする
    // -179→0（）
    // 0→180（）
    public Vector2 LimitX { get; private set; }
    public Vector2 LimitY { get; private set; }
    public Vector2 LimitZ { get; private set; }

    /// <summary>
    /// 回転の制限の情報を纏めたクラスのコンストラクタ<br/>
    /// 引数1：_limitX 制限（Vector2のxをmin、yをmaxとする）<br/>
    /// 引数2：_limitY 制限（Vector2のxをmin、yをmaxとする）<br/>
    /// 引数3：_limitZ 制限（Vector2のxをmin、yをmaxとする）<br/>
    /// </summary>
    /// <param name="_limitX">制限（Vector2のxをmin、yをmaxとする）</param>
    /// <param name="_limitY">制限（Vector2のxをmin、yをmaxとする）</param>
    /// <param name="_limitZ">制限（Vector2のxをmin、yをmaxとする）</param>
    /// <returns>なし（このクラスのインスタンス）</returns>
    public RotationLimit(Vector2 _limitX, Vector2 _limitY, Vector2 _limitZ)
    {
        LimitX = new Vector2(Mathf.DeltaAngle(0, _limitX.x), Mathf.DeltaAngle(0, _limitX.y));
        LimitY = new Vector2(Mathf.DeltaAngle(0, _limitY.x), Mathf.DeltaAngle(0, _limitY.y));
        LimitZ = new Vector2(Mathf.DeltaAngle(0, _limitZ.x), Mathf.DeltaAngle(0, _limitZ.y));
    }
}

/// <summary>
/// floatの値にフィルターをかけスムージングするクラス<br/>
/// 非MonoBehaviour
/// </summary>
public class OneEuroFilterFloat
{
    private float m_freq;             // サンプリング周波数（fpsを指定）
    private float m_minCutoff;        // 最低限の平滑化、回転（小さいほどなめらか）
    private float m_beta;             // 動きの速さへの反応性（大きい程応答性高）
    private float m_dCutoff;          // 速度（微分）のフィルタ強度

    private float m_prevValue;        // 前回の値
    private float m_prevDx;           // 前回の微分値

    private bool _isfirst = true;       // 最初に呼び出された時か（始めは_prevValue等に値が無いため）

    /// <summary>
    /// floatの値にフィルターをかけスムージングするクラスのコンストラクタ<br/>
    /// 引数1：_freq サンプリング周波数（fpsを指定）<br/>
    /// 引数2：_minCutoff 最低限の平滑化、回転（小さいほどなめらか）<br/>
    /// 引数3：_beta 動きの速さへの反応性（大きい程応答性高）<br/>
    /// 引数4：_dCutoff 速度（微分）のフィルタ強度
    /// </summary>
    /// <param name="_freq">サンプリング周波数（fpsを指定）</param>
    /// <param name="_minCutoff">最低限の平滑化、回転（小さいほどなめらか）</param>
    /// <param name="_beta">動きの速さへの反応性（大きい程応答性高）</param>
    /// <param name="_dCutoff">速度（微分）のフィルタ強度</param>
    /// <returns>なし（このクラスのインスタンス）</returns>
    public OneEuroFilterFloat(float _freq, float _minCutoff = 1.0f, float _beta = 0.0f, float _dCutoff = 1.0f)
    {
        m_freq = _freq;
        m_minCutoff = _minCutoff;
        m_beta = _beta;
        m_dCutoff = _dCutoff;
    }

    /// <summary>
    /// カットオフ周波数に基づいて平滑化係数を計算する<br/>
    /// 一次ローパスフィルタのα係数（どの程度新しい値に従い、過去の値を残すかを決める値）を求める<br/>
    /// 引数1：_cutoff カットオフ周波数
    /// </summary>
    /// <param name="_cutoff">カットオフ周波数</param>
    /// <returns>α係数</returns>
    private float SmoothingFactor(float _cutoff)
    {
        // 時間定数、信号がどのくらいで減衰するか表すもの
        float timeConstant = 1f / (2 * Mathf.PI * _cutoff);
        // 一フレームの時間
        float SamplingPeriod = 1f / m_freq;

        return 1f / (1f + timeConstant / SamplingPeriod);
    }

    /// <summary>
    /// 入力値の変化（微分）を計算し、ノイズの大きさを測る<br/>
    /// サイズが大きい時はフィルタのカットオフ周波数を上げて追従性を高め、小さいときは滑らかさを重視<br/>
    /// 引数1：_value フィルターをかける値
    /// </summary>
    /// <param name="_value">フィルターをかける値</param>
    /// <returns>フィルターをかけた値</returns>
    public float Filter(float _value)
    {
        // 最初のみ前回の値が無いためここで設定
        if (_isfirst)
        {
            m_prevValue = _value;
            m_prevDx = 0f;
            _isfirst = false;
        }

        // 微分により求まる、変化（回転）の速さ
        // 一秒間の間にどの程度変化するか
        // (現在の値-前回の値)*fps
        float dx = (_value - m_prevValue) * m_freq;

        float alphaD = SmoothingFactor(m_dCutoff);
        m_prevDx = Mathf.Lerp(m_prevDx, dx, alphaD);

        // 最低限の滑らかさ
        float cutoff = m_minCutoff + m_beta * Mathf.Abs(m_prevDx);
        float alpha = SmoothingFactor(cutoff);

        m_prevValue = Mathf.Lerp(m_prevValue, _value, alpha);
        return m_prevValue;
    }
}