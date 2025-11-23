/*
 * ファイル
 * LaserController C#
 * 
 * システム
 * レーザーの挙動の制御
 * 
 * 作成
 * 2025/07/23 寺門 冴羽
 * 
 * 最終変更
 * 2025/07/24 寺門 冴羽
 */

using UnityEngine;

public class LaserController : MonoBehaviour
{
    // --------------------------------------------------------------------------------
    // 変数宣言
    // --------------------------------------------------------------------------------
    
    enum RaserStatus    // レーザーのステータス
    {
        None,           // 制御しない
        AllOff,         // すべてのレーザーをオフにするフラグ
        AllOn,          // すべてのレーザーをオンにするフラグ
    }

    [SerializeField]
    RaserStatus raserStatus;

    enum RaserMode          // レーザー全体の制御に使用する値
    {
        IndividualMode,     // バラバラ照射モード
        HorizontalMode,     // 水平照射モード
        CornMode,           // コーン照射モード
    }

    [SerializeField]
    RaserMode raserMode;    // レーザーのモード

    [Range(-90f,90f)]
    float spreadAngle;      // 水平照射およびコーン照射の拡散角度

    [System.Serializable]
    struct Laser            // レーザー単体の制御に使用する値の構造体
    {
        bool active;        // アクティブ制御
        Vector3 rotation;   // 回転の制御。上のオブジェクトから制御できるように
    }

    [SerializeField]
    Laser[] Lasers;         // レーザーの構造体配列

    int laserCount;         // レーザーの個数

    /*
    // --------------------------------------------------------------------------------
    // メイン関数
    // --------------------------------------------------------------------------------

    // 最初のフレームの処理
    void Start()
    {
        // 初期化
        laserCount = 0;
    }

    // 毎フレームの処理
    void Update()
    {

    }

    // --------------------------------------------------------------------------------
    // サブ関数
    // --------------------------------------------------------------------------------

    // ステータスチェンジ
    void StatusChange(RaserStatus _raserStatus)
    {
        switch (_raserStatus) 
        {
            case RaserStatus.None:

                break;
            case RaserStatus.AllOff:

                break;

            case RaserStatus.AllOn:
                break;
        }
    }

    // モードチェンジ
    void ModeChange(RaserMode _raserMode)
    {
        switch (_raserMode) 
        {
            case RaserMode.IndividualMode:

                break;

            case RaserMode.HorizontalMode:
                // 弾一つ分の角度
                float oneSpreadAngle = spreadAngle / (nWay - 1);

                // 発射音を鳴らす
                _as.PlayOneShot(magicSound);

                // 設定した個数発射
                for (int i = 0; i < nWay; ++i)
                {
                    // 角度をずらす
                    float Angle = (oneSpreadAngle * i) - (spreadAngle / 2);

                    Quaternion rot = transform.rotation * Quaternion.Euler(0, 0, Angle);

                    // オブジェクトを生成
                    Instantiate(obj, transform.position, rot);

                    break;

            case RaserMode.CornMode:

                break;
        }
    }
    */
}