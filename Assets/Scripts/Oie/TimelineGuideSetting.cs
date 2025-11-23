using UnityEngine;

[CreateAssetMenu(fileName = "GuideDeata", menuName = "Project/GuideData")]
public class TimelineGuideSetting : ScriptableObject
{
    [System.Serializable]
    public struct GuideData
    {
        public GameObject GuideObject;

        public string GuideName;
    }

    public GuideData[] guideData;
}
