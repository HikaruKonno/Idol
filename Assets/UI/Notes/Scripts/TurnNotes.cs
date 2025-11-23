using UnityEngine;

public class TurnNotes : MonoBehaviour
{
    // メディアパイプのランドマーク取得する為の変数
    [SerializeField]
    private MediapipeResultDataContainer m_mediapipeContainer;

    private void Update()
    {
        if(m_mediapipeContainer.RightHandNormalizedLandmarkList != null && m_mediapipeContainer.LeftHandNormalizedLandmarkList != null)
        {
            float rightX = m_mediapipeContainer.RightHandNormalizedLandmarkList[(int)EHandLandmarks.MiddleTip].X;
            float leftX = m_mediapipeContainer.LeftHandNormalizedLandmarkList[(int)EHandLandmarks.MiddleTip].X;

            if(rightX < 0.5f && leftX > 0.5f )
            {
                gameObject.SetActive(false);
            }
        }
    }
}
