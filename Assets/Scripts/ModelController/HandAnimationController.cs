/*
 * ファイル
 * HandAnimationController C#
 * 
 * システム
 * 手のアニメーションをm_mediapipeResultDataContainerのハンドシェイプの内容に合わせて切り替える
 * 
 * インゲーム中のキャラクターにこのスクリプトを適用させるとタイムラインのアニメーションとかち合ってしまうため、
 * 透明なボーン構造のみのモデルのAnimatorで手の形を変更し、
 * キャラクターにタイムライン反映後のLateUpdateで、アニメーションさせたモデルの手のボーンの回転をコピーしている
 * 
 * animator.Update(0f)は対象のオブジェクトへの影響が大きいため使えない？
 * （先にAnimationRiggingControllerを動かすとanimator.Update(0f)で、
 *   モデルの子オブジェクトのリギングターゲットのトランスフォームがリセットされる？など不具合が出たため）
 * 
 * 変更履歴
 * 2025/09/12　奥山　凜　作成
 */

using UnityEngine;

[DefaultExecutionOrder(100)]
public class HandAnimationController : MonoBehaviour
{
    [SerializeField]
    private Animator m_sourceAnimator;      // 反映元（ダミー）のアニメーター
    [SerializeField]
    private Animator m_modelAnimator;       // 反映先（キャラクター）のアニメーター
    [SerializeField]
    private MediapipeResultDataContainer m_mediapipeResultDataContainer;        // ハンドシェイプの結果が入っているスクリプタブルオブジェクト

    private HandShapeDetector.HandShape m_previousLeftHandShape;         // 前回の左手の形
    private HandShapeDetector.HandShape m_previousRightHandShape;        // 前回の右手の形

    private int m_handBoneCount;                                         // 手周りのボーンの数
    private void Awake()
    {
#if UNITY_EDITOR
        if ((m_sourceAnimator == null) || (m_modelAnimator == null))
        {
            Debug.Log("代用のベクトルを使用");
            Debug.Log("m_sourceAnimatorまたはm_modelAnimatorがありません");
        }
#endif
        // 手周りのボーンの数
        m_handBoneCount = ((int)HumanBodyBones.RightLittleDistal - (int)HumanBodyBones.LeftThumbProximal) + 1;

        if (m_mediapipeResultDataContainer == null)
        {
            m_mediapipeResultDataContainer = Resources.Load("Datas/Scene/MediapipeResultDataContainer") as MediapipeResultDataContainer;
        }

        m_previousLeftHandShape = HandShapeDetector.HandShape.Unknown;
        m_previousRightHandShape = HandShapeDetector.HandShape.Unknown;
        m_sourceAnimator.SetLayerWeight(1, 1.0f);
        m_sourceAnimator.SetLayerWeight(2, 1.0f);
        m_sourceAnimator.Play(HandShapeDetector.HandShape.Paper.ToString(), 1);      // アニメーターのステート名、レイヤー番号と合わせる必要があるので注意
        m_sourceAnimator.Play(HandShapeDetector.HandShape.Paper.ToString(), 2);      // アニメーターのステート名、レイヤー番号と合わせる必要があるので注意


    }

    private void Update()
    {
        ApplyHandShapeToModel();
    }

    // タイムラインで上書きされないようLateUpdateで反映
    private void LateUpdate()
    {
        // 右手、左手の回転を反映
        for (int i = 0; i < m_handBoneCount; ++i)
        {
            m_modelAnimator.GetBoneTransform((HumanBodyBones)((int)HumanBodyBones.LeftThumbProximal + i)).transform.localRotation 
                = m_sourceAnimator.GetBoneTransform((HumanBodyBones)((int)HumanBodyBones.LeftThumbProximal + i)).transform.localRotation;
        }
    }

    /// <summary>
    /// m_mediapipeResultDataContainerのハンドシェイプの内容に合わせてアニメーションを切り替える<br/>
    /// </summary>
    /// <returns>なし</returns>
    private void ApplyHandShapeToModel()
    {
        HandShapeDetector.HandShape currentLeftHandShape = m_mediapipeResultDataContainer.LeftHandShape;
        if ((currentLeftHandShape != m_previousLeftHandShape) && (currentLeftHandShape != HandShapeDetector.HandShape.Unknown))
        {
            m_sourceAnimator.CrossFade(currentLeftHandShape.ToString(), 1.0f, 1);      // アニメーターのステート名、レイヤー番号と合わせる必要があるので注意
            m_previousLeftHandShape = currentLeftHandShape;
        }

        HandShapeDetector.HandShape currentRightHandShape = m_mediapipeResultDataContainer.RightHandShape;
        if ((currentRightHandShape != m_previousRightHandShape) && (currentRightHandShape != HandShapeDetector.HandShape.Unknown))
        {
            m_sourceAnimator.CrossFade(currentRightHandShape.ToString(), 1.0f, 2);      // アニメーターのステート名、レイヤー番号と合わせる必要があるので注意
            m_previousRightHandShape = currentRightHandShape;
        }
    }
}
