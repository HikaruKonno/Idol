/*
 * ファイル
 * ArmTrackingController C#
 * 
 * システム
 * Mediapipeから受け取った結果のランドマークを使用し、
 * モデルの腕のボーンを回転させ動かす
 * 
 * インゲーム中のキャラクターにこのスクリプトを適用させるとアニメーションとかち合ってしまうため、
 * 透明なボーン構造のみのモデルの腕を回転させ、その情報を元に、
 * キャラクターにアニメーションとブレンド可能なAnimationRigingで反映させている。（AnimationRigingは別スクリプト）
 * 
 * 二者の向きが合わないと腕の方向がおかしくなる為、
 * アニメーションしているキャラクターの向きに、このスクリプトをアタッチしているモデルを合わせている。
 * 
 * 変更履歴
 * 2025/09/18　奥山　凜　作成
 */

using System.Collections;
using UnityEngine;

/// <summary>
/// Mediapipeから受け取った結果のランドマークを使用し、<br/>
/// モデルの腕のボーンを回転させ動かすクラス<br/>
/// </summary>
[DefaultExecutionOrder(99)]
public class ArmTrackingController : MonoBehaviour
{
    [SerializeField]
    private MediapipeResultDataContainer m_mediapipeResultDataContainer;        // Mediapipeから受け取った結果が入っているスクリプタブルオブジェクト
    [SerializeField]
    private Animator m_modelAnimator;                                           // モデルのアニメーター
    [SerializeField]
    private GameObject m_modelObject;                                           // モデルのゲームオブジェクト


    private SettingBoneMapping.BoneLandmarkMapping[] m_poseBoneMappings;        // ポーズのボーンとランドマークを対応させる情報を纏めて持つ配列
    private int m_imageHeight = 1;                                              // 接続したwebカメラの縦比
    private int m_imageWidth = 1;                                               // 接続したwebカメラの横比


    [SerializeField]
    private Animator m_sourceAnimator;      // アニメーションリギング先のアニメーター
    private Transform m_sourceUpperChestTransform;
    private Transform m_sourceChestTransform;
    private Transform m_sourceSpineTransform;
    private Transform m_sourceHipTransform;

    // WebカメラがセットされMediapipeに画像が流れてくるのを待つためIEnumerator
    private IEnumerator Start()
    {
        if (m_mediapipeResultDataContainer == null)
        {
            m_mediapipeResultDataContainer = Resources.Load("Datas/Scene/MediapipeResultDataContainer") as MediapipeResultDataContainer;
        }

        SettingBoneMapping.SettingPoseBoneMappings(ref m_poseBoneMappings);

        m_sourceUpperChestTransform = m_sourceAnimator.GetBoneTransform(HumanBodyBones.UpperChest);
        m_sourceChestTransform = m_sourceAnimator.GetBoneTransform(HumanBodyBones.Chest);
        m_sourceSpineTransform = m_sourceAnimator.GetBoneTransform(HumanBodyBones.Spine);
        m_sourceHipTransform = m_sourceAnimator.GetBoneTransform(HumanBodyBones.Hips);


        yield return StartCoroutine(MediaPipeUtils.WaitImageSourceReady());
        SetImageWidthAndHeight();
    }

    private void Update()
    {
        if (m_modelObject == null)
        {
            return;
        }

        if (m_mediapipeResultDataContainer.CopyPoseNormalizedLandmarks == null)
        {
            return;
        }
        // 回転適用前に親オブジェクトの回転を0に戻す（ApplyLandmarksToModel()後の、sourceの回転を適用させる処理の為に必要）
        m_modelObject.transform.rotation = Quaternion.identity;
        
        // 回転を適用
        ApplyLandmarksToModel();

        if (m_sourceAnimator == null)
        {
            return;
        }

        // 二者の向きが合わないと腕の方向がおかしくなる為、
        // アニメーションしているキャラクターの向きに、このスクリプトをアタッチしているモデルを合わせる
        Vector3 axis = Vector3.up;      // ワールドY軸周りの回転成分だけ抽出
        Quaternion yOnlyRotations = ExtractRotationAroundAxis(m_sourceHipTransform.localRotation, axis);
        yOnlyRotations *= ExtractRotationAroundAxis(m_sourceSpineTransform.localRotation, axis);
        yOnlyRotations *= ExtractRotationAroundAxis(m_sourceChestTransform.localRotation, axis);
        yOnlyRotations *= ExtractRotationAroundAxis(m_sourceUpperChestTransform.localRotation, axis);

        m_modelObject.transform.rotation = yOnlyRotations;
    }

    /// <summary>
    /// ランドマークをモデルに適用する<br/>
    /// </summary>
    /// <returns>なし</returns>
    private void ApplyLandmarksToModel()
    {
        if (m_poseBoneMappings == null)
        {
            return;
        }
        else if (m_mediapipeResultDataContainer.CopyPoseNormalizedLandmarks == null)
        {
            return;
        }

        // マッピングに設定されているボーンにランドマークを適用させる
        foreach (SettingBoneMapping.BoneLandmarkMapping mapping in m_poseBoneMappings)
        {
            Mediapipe.NormalizedLandmark startLandmark = m_mediapipeResultDataContainer.CopyPoseNormalizedLandmarks[mapping.StartLandmarkIndex];
            Mediapipe.NormalizedLandmark endLandmark = m_mediapipeResultDataContainer.CopyPoseNormalizedLandmarks[mapping.EndLandmarkIndex];

            Mediapipe.NormalizedLandmark startLandmarkSecond = m_mediapipeResultDataContainer.CopyPoseNormalizedLandmarks[mapping.StartOtherLandmarkIndex];
            Mediapipe.NormalizedLandmark endLandmarkSecond = m_mediapipeResultDataContainer.CopyPoseNormalizedLandmarks[mapping.EndOtherLandmarkIndex];

            // ランドマークからボーンの回転を求めるのに使う二つのベクトルを求める
            Vector3 landmarkDirection1 = CalculateLandmarkDirection(startLandmark, endLandmark);
            Vector3 landmarkDirection2 = CalculateLandmarkDirection(startLandmarkSecond, endLandmarkSecond);

            // 最終的に、求めたベクトル1と外積2つをこの三方向のベクトルに格納する
            Vector3 DirectionUp = Vector3.zero;
            Vector3 DirectionRight = Vector3.zero;
            Vector3 DirectionBackward = Vector3.zero;


            // 1つ目のベクトルを、デフォルトの状態（Tポーズ時）に向いている方向の変数に格納
            // 上下なら DirectionUp
            // 左右なら DirectionRight
            // 前後なら DirectionBackward
            switch (mapping.VectorDirection)
            {
                case SettingBoneMapping.DirectionCheckResult.Backward:
                    DirectionBackward = landmarkDirection1;
                    break;
                case SettingBoneMapping.DirectionCheckResult.Down:
                    DirectionUp = -landmarkDirection1;
                    break;
                case SettingBoneMapping.DirectionCheckResult.Forward:
                    DirectionBackward = -landmarkDirection1;
                    break;
                case SettingBoneMapping.DirectionCheckResult.Left:
                    DirectionRight = -landmarkDirection1;
                    break;
                case SettingBoneMapping.DirectionCheckResult.Right:
                    DirectionRight = landmarkDirection1;
                    break;
                case SettingBoneMapping.DirectionCheckResult.Up:
                    DirectionUp = landmarkDirection1;
                    break;
            }

            // 外積が潰れているならlandmarkDirection2ではない予備のベクトルを使って外積を出す
            if (IsCrossProductCollapsed(landmarkDirection1, landmarkDirection2))
            {
#if UNITY_EDITOR
                Debug.Log("代用のベクトルを使用");
#endif
                // 予備のベクトルを作成
                Mediapipe.NormalizedLandmark fallbackStartLandmarkSecond = m_mediapipeResultDataContainer.CopyPoseNormalizedLandmarks[mapping.FallbackStartOtherLandmarkIndex];
                Mediapipe.NormalizedLandmark fallbackEndLandmarkSecond = m_mediapipeResultDataContainer.CopyPoseNormalizedLandmarks[mapping.FallbackEndOtherLandmarkIndex];
                landmarkDirection2 = CalculateLandmarkDirection(fallbackStartLandmarkSecond, fallbackEndLandmarkSecond);
                // 外積を求める
                Vector3 crossProductDirection = Vector3.Cross(landmarkDirection1, landmarkDirection2).normalized;


                if (IsCrossProductCollapsed(landmarkDirection1, landmarkDirection2))
                {
#if UNITY_EDITOR
                    Debug.Log("代用のベクトルで外積の取得に失敗");
#endif
                }

                float dot = Vector3.Dot(landmarkDirection1, landmarkDirection2);

                if (dot <= -1f) // ほぼ180°
                {
                    crossProductDirection *= -1;
                }

                // 外積を、デフォルトの状態（Tポーズ時）に向いている方向の変数に格納
                // 上下なら DirectionUp
                // 左右なら DirectionRight
                // 前後なら DirectionBackward
                switch (mapping.FallbackCrossProductDirection)
                {
                    case SettingBoneMapping.DirectionCheckResult.Backward:
                        DirectionBackward = crossProductDirection;
                        break;
                    case SettingBoneMapping.DirectionCheckResult.Down:
                        DirectionUp = -crossProductDirection;
                        break;
                    case SettingBoneMapping.DirectionCheckResult.Forward:
                        DirectionBackward = -crossProductDirection;
                        break;
                    case SettingBoneMapping.DirectionCheckResult.Left:
                        DirectionRight = -crossProductDirection;
                        break;
                    case SettingBoneMapping.DirectionCheckResult.Right:
                        DirectionRight = crossProductDirection;
                        break;
                    case SettingBoneMapping.DirectionCheckResult.Up:
                        DirectionUp = crossProductDirection;
                        break;
                }
            }
            else
            {
                // 外積を求める
                Vector3 crossProductDirection = Vector3.Cross(landmarkDirection1, landmarkDirection2).normalized;
                float dot = Vector3.Dot(landmarkDirection1, landmarkDirection2);

                if (dot <= -1f) // ほぼ180°
                {
                    crossProductDirection *= -1;
                }

                // 外積を、デフォルトの状態（Tポーズ時）に向いている方向の変数に格納
                // 上下なら DirectionUp
                // 左右なら DirectionRight
                // 前後なら DirectionBackward
                switch (mapping.CrossProductDirection)
                {
                    case SettingBoneMapping.DirectionCheckResult.Backward:
                        DirectionBackward = crossProductDirection;
                        break;
                    case SettingBoneMapping.DirectionCheckResult.Down:
                        DirectionUp = -crossProductDirection;
                        break;
                    case SettingBoneMapping.DirectionCheckResult.Forward:
                        DirectionBackward = -crossProductDirection;
                        break;
                    case SettingBoneMapping.DirectionCheckResult.Left:
                        DirectionRight = -crossProductDirection;
                        break;
                    case SettingBoneMapping.DirectionCheckResult.Right:
                        DirectionRight = crossProductDirection;
                        break;
                    case SettingBoneMapping.DirectionCheckResult.Up:
                        DirectionUp = crossProductDirection;
                        break;
                }
            }

            // 値が入っていない三つ目の方向に、他二つの方向で作成した外積を代入
            if (DirectionUp == Vector3.zero)
            {
                DirectionUp = Vector3.Cross(DirectionRight, DirectionBackward);

                IsCrossProductCollapsed(DirectionRight, DirectionBackward);
            }
            else if (DirectionRight == Vector3.zero)
            {
                DirectionRight = Vector3.Cross(DirectionBackward, DirectionUp);

                IsCrossProductCollapsed(DirectionBackward, DirectionUp);
            }
            else if (DirectionBackward == Vector3.zero)
            {
                DirectionBackward = Vector3.Cross(DirectionUp, DirectionRight);

                IsCrossProductCollapsed(DirectionUp, DirectionRight);
            }

            // 求めた二ベクトルを使用してボーンの回転を作成
            Quaternion rotationValue = Quaternion.LookRotation(DirectionBackward, DirectionUp);

            // 親の回転を考慮してローカル回転を設定
            Transform boneTransform = m_modelAnimator.GetBoneTransform(mapping.Bone);
            if (boneTransform == null)
            {
                return;
            }

            Transform parentTransform = boneTransform.parent;

            // 親の回転がない場合は単位クォータニオンを使用
            Quaternion parentRotation = (parentTransform != null) ? parentTransform.rotation : Quaternion.identity;

            // 親の回転を考慮してローカル回転を計算
            Quaternion localRotation = Quaternion.Inverse(parentRotation) * rotationValue;
            boneTransform.localRotation = mapping.RotationFilter.Filter(localRotation);
        }
    }

    /// <summary>
    /// 二つのランドマークからベクトルを生成する<br/>
    /// 引数1：_startLandmark 始点とするランドマーク<br/>
    /// 引数2：_endLandmark 終点とするランドマーク<br/>
    /// </summary>
    /// <param name="_startLandmark">始点とするランドマーク</param>
    /// <param name="_endLandmark">終点とするランドマーク</param>
    /// <returns>生成したベクトル</returns>
    private Vector3 CalculateLandmarkDirection(Mediapipe.NormalizedLandmark _startLandmark, Mediapipe.NormalizedLandmark _endLandmark)
    {
        // ランドマークのワールド座標を取得
        Vector3 startPos = MediaPipeUtils.ConvertToUnityPos(_startLandmark, m_imageHeight, m_imageWidth, true, 1.0f);
        Vector3 endPos = MediaPipeUtils.ConvertToUnityPos(_endLandmark, m_imageHeight, m_imageWidth, true, 1.0f);
        return (endPos - startPos).normalized;
    }

    /// <summary>
    /// 指定した軸に対するQuaternionの回転成分を抽出して、新しいQuaternionを返す<br/>
    /// 引数1：_quaternion 元の回転<br/>
    /// 引数2：_axis 抽出したい軸（正規化されている必要あり）<br/>
    /// </summary>
    /// <param name="_quaternion">元の回転</param>
    /// <param name="_axis">抽出したい軸（正規化されている必要あり）</param>
    /// <returns>指定軸周りの回転だけを持つQuaternion</returns>
    private Quaternion ExtractRotationAroundAxis(Quaternion _quaternion, Vector3 _axis)
    {
        // 軸に対する回転角を求める
        float angle = GetRotationAroundAxis(_quaternion, _axis);

        // その軸と角度から新たな Quaternion を作成
        return Quaternion.AngleAxis(angle, _axis);
    }

    /// <summary>
    /// 軸周りの回転角を求める<br/>
    /// 引数1：_quaternion 回転<br/>
    /// 引数2：_axis 軸<br/>
    /// </summary>
    /// <param name="_quaternion">回転</param>
    /// <param name="_axis">軸</param>
    /// <returns>回転の内、軸周りのみの回転角</returns>
    private float GetRotationAroundAxis(Quaternion _quaternion, Vector3 _axis)
    {
        // 軸に垂直なベクトルを用意
        Vector3 orthoVector = Vector3.Cross(_axis, Vector3.right);
        if (orthoVector == Vector3.zero)
            orthoVector = Vector3.Cross(_axis, Vector3.up);
        orthoVector.Normalize();

        Vector3 before = orthoVector;
        Vector3 after = _quaternion * orthoVector;

        Vector3 projectedBefore = Vector3.ProjectOnPlane(before, _axis).normalized;
        Vector3 projectedAfter = Vector3.ProjectOnPlane(after, _axis).normalized;

        float angle = Vector3.SignedAngle(projectedBefore, projectedAfter, _axis);
        return angle;
    }

    /// <summary>
    /// 接続したwebカメラの縦横比を取得する<br/>
    /// </summary>
    /// <returns>なし</returns>
    private void SetImageWidthAndHeight()
    {
        m_imageWidth = MediaPipeUtils.GetImageWidth();
        m_imageHeight = MediaPipeUtils.GetImageHeight();
    }

    /// <summary>
    /// 2ベクトルから外積を求め、その外積が潰れているか調べる<br/>
    /// 引数1：_vector1 外積を作るベクトル1<br/>
    /// 引数2：_vector2 外積を作るベクトル2<br/>
    /// 引数3：_threshold 外積が潰れているかを調べる閾値<br/>
    /// </summary>
    /// <param name="_vector1">外積を作るベクトル1</param>
    /// <param name="_vector2">外積を作るベクトル2</param>
    /// <param name="_threshold">外積が潰れているかを調べる閾値</param>
    /// <returns>外積が潰れているか</returns>
    private bool IsCrossProductCollapsed(Vector3 _vector1, Vector3 _vector2, float _threshold = 0.02f)
    {
        Vector3 cross = Vector3.Cross(_vector1, _vector2);
        float crossMag = cross.magnitude;
        float dot = Mathf.Abs(Vector3.Dot(_vector1.normalized, _vector2.normalized));

        if (crossMag < _threshold || dot > 0.99f)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[IsCrossProductCollapsed] 潰れてるかも crossMag={crossMag}, dot={dot}");
#endif
            return true;
        }

        return false;
    }
}
