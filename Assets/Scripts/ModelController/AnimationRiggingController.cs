/*
 * ファイル
 * AnimationRiggingController C#
 * 
 * システム
 * Sourceとなるモデルから対応するボーンの位置と回転をコピーし、
 * もう一方のモデルに反映させる。
 * 
 * 変更履歴
 * 2025/07/20　奥山　凜　作成
 */

using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Sourceとなるモデルから対応するボーンの位置と回転をコピーし、<br/>
/// もう一方のモデルに反映させるクラス<br/>
/// </summary>
[DefaultExecutionOrder(101)]
public class AnimationRiggingController : MonoBehaviour
{
    /// <summary>
    /// モデルのアニメーションリギングオブジェクトのtransformとモデルのhipのトランスフォームをまとめたもの<br/>
    /// （hipはSorceの反映時の基準点として使う）<br/>
    /// </summary>
    [System.Serializable]
    private struct RigTransforms
    {
        public Transform SpineRotationTarget;       // TwistChainConstraintのsorceとなるオブジェクトが反映先のポジションに移動してしまうので別で必要
        public Transform LeftHandHint;
        public Transform LeftHandTarget;
        public Transform RightHandHint;
        public Transform RightHandTarget;

        public Transform Hip;
        public Transform UpperChest;
    }

    [SerializeField]
    private Animator m_sourceModelAnimator;     // アニメーションリギングの反映元のモデルのアニメーター
    [SerializeField]
    private GameObject m_rigPrefab;             // 上半身のアニメーションリギングの為のConstraintコンポーネントのアタッチされたオブジェクトやTargetのオブジェクトを持つPrefab
    [SerializeField]
    private GameObject m_modelObject;           // 反映先のモデルのオブジェクト
    [SerializeField]
    private Quaternion m_rightArmInitialRotation = Quaternion.identity;     // モデルの腕のボーン一部にのみ初期回転があるためその相殺用、Sorceとするモデルが同じモデルの場合は必要なし（leftはinverseで行う）
    [SerializeField]
    private float m_targetRigWeight = 0.0f;

    private Animator m_modelAnimator;           // アニメーションリギングの反映先のモデルのアニメーター。アタッチされているオブジェクトから検索
    private Rig m_rig;                          // m_rigPrefabより検索
    private RigBuilder m_rigBuilder;            // このコンポーネントでアタッチ


    [SerializeField]
    private RigTransforms m_rigTransforms;              // モデルのアニメーションリギングに使うオブジェクトのtransform
    [SerializeField] 
    private RigTransforms m_rigSourceTransforms;        // m_rigTransformsのtransformに反映させる元となるオブジェクト

    private Transform m_upperChestTransform;            // 上半身のアニメーションリギングの反映の制限に使う


    private const float HAND_HINT_WEIGHT = 0.0f;        // TwoBoneIKConstraintのヒントウェイトの値

    [SerializeField]
    private float m_weightChangeSpeed = 5.0f;       // ウェイトが変化する速さ
    private void Awake()
    {
        if (!m_modelObject.TryGetComponent<Animator>(out Animator modelAnimator))
        {
            return;
        }
        m_modelAnimator = modelAnimator;

        // AnimationRiging周りの準備
        // モデルの差し替えが多くある為スクリプト側で設定
        SettingRig();
    }

    void Update()
    {
        // 現在のrigウェイトを目標値にスムーズに近づける
        if (m_rig != null)
        {
            m_rig.weight = Mathf.Lerp(m_rig.weight, m_targetRigWeight, m_weightChangeSpeed * Time.deltaTime);
        }

        // Sorceのモデルの手や胸の回転や位置を、アニメーションリギングのSorceとするオブジェクトにコピーする
        CopyPositionAndRotation(m_rigTransforms.SpineRotationTarget, m_rigTransforms.Hip, m_rigSourceTransforms.SpineRotationTarget, m_rigSourceTransforms.Hip);
        CopyPositionAndRotation(m_rigTransforms.RightHandHint, m_rigTransforms.UpperChest, m_rigSourceTransforms.RightHandHint, m_rigSourceTransforms.UpperChest);
        CopyPositionAndRotation(m_rigTransforms.RightHandTarget, m_rigTransforms.UpperChest, m_rigSourceTransforms.RightHandTarget, m_rigSourceTransforms.UpperChest);
        CopyPositionAndRotation(m_rigTransforms.LeftHandHint, m_rigTransforms.UpperChest, m_rigSourceTransforms.LeftHandHint, m_rigSourceTransforms.UpperChest);
        CopyPositionAndRotation(m_rigTransforms.LeftHandTarget, m_rigTransforms.UpperChest, m_rigSourceTransforms.LeftHandTarget, m_rigSourceTransforms.UpperChest);
        
        m_rigTransforms.LeftHandTarget.rotation *= UnityEngine.Quaternion.Inverse(m_rightArmInitialRotation);       // 腕のボーンの初期回転を加算
        m_rigTransforms.RightHandTarget.rotation *= m_rightArmInitialRotation;                                      // 腕のボーンの初期回転を加算
    }

    /// <summary>
    /// rigWeightを徐々に反映<br/>
    /// 引数1：_targetRigWeight 目標のrigWeightの値（0.0f~1.0f）<br/>
    /// </summary>
    /// <param name="_targetRigWeight">目標のrigWeightの値（0.0f~1.0f）</param>
    /// <returns>なし</returns>
    public void SetTargetRigWeight(float _targetRigWeight)
    {
        m_targetRigWeight = Mathf.Clamp(_targetRigWeight, 0.0f, 1.0f);
    }

    /// <summary>
    /// rigWeightを即座に反映<br/>
    /// 引数1：_rigWeight rigWeightの値（0.0f~1.0f）<br/>
    /// </summary>
    /// <param name="_rigWeight">rigWeightの値（0.0f~1.0f）</param>
    /// <returns>なし</returns>
    public void SetTargetRigWeightInstantly(float _rigWeight)
    {
        float rigWeight = Mathf.Clamp(_rigWeight, 0.0f, 1.0f);

        m_targetRigWeight = rigWeight;

        if (m_rig != null)
        {
            m_rig.weight = rigWeight;
        }
    }

    /// <summary>
    /// 対象のオブジェクトからRootを基準としたtransformとrotationをコピーする<br/>
    /// 引数1：_targetTransform コピー先<br/>
    /// 引数2：_targetRootTransform コピー先のRoot（基準のオブジェクト）<br/>
    /// 引数3：_sourceTransform コピー元<br/>
    /// 引数4：_sourceRootTransform コピー元のRoot（基準のオブジェクト）<br/>
    /// </summary>
    /// <param name="_targetTransform">コピー先</param>
    /// <param name="_targetRootTransform">コピー先のRoot（基準のオブジェクト）</param>
    /// <param name="_sourceTransform">コピー元</param>
    /// <param name="_sourceRootTransform">コピー元のRoot（基準のオブジェクト）</param>
    /// <returns>なし</returns>
    private void CopyPositionAndRotation(Transform _targetTransform, Transform _targetRootTransform, Transform _sourceTransform, Transform _sourceRootTransform)
    {
        _targetTransform.position = _targetRootTransform.position + (_sourceTransform.position - _sourceRootTransform.position);
        _targetTransform.rotation = _sourceTransform.rotation;
    }

    /// <summary>
    /// AnimationRiging周りの準備<br/>
    /// </summary>
    /// <returns>なし</returns>
    private void SettingRig()
    {
        m_rigBuilder = m_modelObject.gameObject.AddComponent<RigBuilder>();

        GameObject rigRootObj = GameObject.Instantiate(m_rigPrefab, m_modelObject.gameObject.transform);

        // 反映先のモデルとアニメーションリギング用のSourceとなるオブジェクトから、必要なトランスフォームを取得
        m_rigTransforms.SpineRotationTarget = rigRootObj.transform.Find("Targets/SpineRotationTarget");
        m_rigTransforms.LeftHandHint = rigRootObj.transform.Find("Hints/LeftHandHint");
        m_rigTransforms.LeftHandTarget = rigRootObj.transform.Find("Targets/LeftHandTarget");
        m_rigTransforms.RightHandHint = rigRootObj.transform.Find("Hints/RightHandHint");
        m_rigTransforms.RightHandTarget = rigRootObj.transform.Find("Targets/RightHandTarget");
        m_rigTransforms.Hip = m_modelAnimator.GetBoneTransform(HumanBodyBones.Hips);
        m_rigTransforms.UpperChest = m_modelAnimator.GetBoneTransform(HumanBodyBones.UpperChest);

        // 反映元のオブジェクトから、必要なトランスフォームを取得
        m_rigSourceTransforms.SpineRotationTarget = m_sourceModelAnimator.GetBoneTransform(HumanBodyBones.UpperChest);
        m_rigSourceTransforms.LeftHandHint = m_sourceModelAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        m_rigSourceTransforms.LeftHandTarget = m_sourceModelAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
        m_rigSourceTransforms.RightHandHint = m_sourceModelAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        m_rigSourceTransforms.RightHandTarget = m_sourceModelAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        m_rigSourceTransforms.Hip = m_sourceModelAnimator.GetBoneTransform(HumanBodyBones.Hips);
        m_rigSourceTransforms.UpperChest = m_sourceModelAnimator.GetBoneTransform(HumanBodyBones.UpperChest);


        m_upperChestTransform = m_modelAnimator.GetBoneTransform(HumanBodyBones.UpperChest);

        {
            GameObject spineRotationObj = rigRootObj.transform.Find("Rig/SpineRotationIK").gameObject;
            GameObject leftHandObj = rigRootObj.transform.Find("Rig/LeftHandIK").gameObject;
            GameObject rightHandObj = rigRootObj.transform.Find("Rig/RightHandIK").gameObject;

            // 背骨のアニメーションリギング設定
            if (spineRotationObj.TryGetComponent<TwistChainConstraint>(out TwistChainConstraint spineRotationComponent))
            {
                TwistChainConstraintData data = spineRotationComponent.data;

                spineRotationComponent.weight = 0.0f;
                data.root = m_rigTransforms.Hip;
                data.tip = m_upperChestTransform;

                data.rootTarget = m_rigTransforms.Hip;
                data.tipTarget = m_rigTransforms.SpineRotationTarget;


                spineRotationComponent.data = data;
            }
            // 左腕のアニメーションリギング設定
            if (leftHandObj.TryGetComponent<TwoBoneIKConstraint>(out TwoBoneIKConstraint leftHandComponent))
            {
                TwoBoneIKConstraintData data = leftHandComponent.data;

                leftHandComponent.weight = 1.0f;
                data.root = m_modelAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                data.mid = m_modelAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                data.tip = m_modelAnimator.GetBoneTransform(HumanBodyBones.LeftHand);

                data.target = m_rigTransforms.LeftHandTarget;
                data.hint = m_rigTransforms.LeftHandHint;

                data.hintWeight = HAND_HINT_WEIGHT;

                leftHandComponent.data = data;
            }
            // 右腕のアニメーションリギング設定
            if (rightHandObj.TryGetComponent<TwoBoneIKConstraint>(out TwoBoneIKConstraint rightHandComponent))
            {
                TwoBoneIKConstraintData data = rightHandComponent.data;

                rightHandComponent.weight = 1.0f;
                data.root = m_modelAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                data.mid = m_modelAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                data.tip = m_modelAnimator.GetBoneTransform(HumanBodyBones.RightHand);

                data.target = m_rigTransforms.RightHandTarget;
                data.hint = m_rigTransforms.RightHandHint;

                data.hintWeight = HAND_HINT_WEIGHT;

                rightHandComponent.data = data;
            }
        }
        GameObject rigObj = rigRootObj.transform.Find("Rig").gameObject;
        if (rigObj.TryGetComponent<Rig>(out Rig rig))
        {
            m_rig = rigObj.GetComponent<Rig>();
        }

        m_rigBuilder.layers.Add(new RigLayer(m_rig, true));
        m_rigBuilder.Build();


        m_rig.weight = 0;
        SetTargetRigWeight(m_targetRigWeight);
    }
}