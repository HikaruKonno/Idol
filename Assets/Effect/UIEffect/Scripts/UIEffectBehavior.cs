using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIEffectBehavior : MonoBehaviour
{
    [SerializeField] private float growTime = 0.3f;
    [SerializeField] private float shrinkTime = 0.3f;
    [SerializeField] private float fadeDelay = 0.2f;
    [SerializeField] private float fadeDuration = 0.5f;

    private CanvasGroup canvasGroup;
    private RectTransform rect;

    private void Awake()
    {
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;
    }

    private void OnEnable()
    {
        StartCoroutine(PlayEffect());
    }

    private IEnumerator PlayEffect()
    {
        // 拡大
        yield return ScaleTo(Vector3.one, growTime);

        // 縮小
        yield return ScaleTo(Vector3.zero, shrinkTime);

        // フェードアウト
        yield return new WaitForSeconds(fadeDelay);
        yield return FadeOut();

        Destroy(gameObject);
    }

    private IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        Vector3 startScale = rect.localScale;
        float time = 0f;

        while (time < duration)
        {
            rect.localScale = Vector3.Lerp(startScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        rect.localScale = targetScale;
    }

    private IEnumerator FadeOut()
    {
        float time = 0f;
        while (time < fadeDuration)
        {
            canvasGroup.alpha = 1 - (time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;
    }
}
