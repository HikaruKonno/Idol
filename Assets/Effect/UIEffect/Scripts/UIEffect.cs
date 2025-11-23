using UnityEngine;
public class UIEffect : MonoBehaviour
{
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private AnimationCurve scaleCurve;
    [SerializeField] private AnimationCurve alphaCurve;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private float timer;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        timer = 0f;
        transform.localScale = Vector3.zero;
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);
        float scale = scaleCurve.Evaluate(t);
        float alpha = alphaCurve.Evaluate(t);

        transform.localScale = Vector3.one * scale;
        if (canvasGroup != null)
            canvasGroup.alpha = alpha;

        if (t >= 1f)
            Destroy(gameObject);

        Debug.Log(alpha);
    }
}
