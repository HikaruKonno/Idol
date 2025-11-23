using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SineTest : MonoBehaviour
{
    public float freq = 440f;
    public float duration = 2f;
    public float amplitude = 1f; // 1 = max
    void Start()
    {
        var src = GetComponent<AudioSource>();

        int sampleRate = 48000;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        var clip = AudioClip.Create("sine", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            data[i] = Mathf.Clamp(Mathf.Sin(2 * Mathf.PI * freq * i / sampleRate) * amplitude, -1f, 1f);
        }
        clip.SetData(data, 0);
        src.clip = clip;
        src.Play();

        Debug.Log($"SineTest started: amplitude={amplitude}, sampleRate={sampleRate}");
        Debug.Log($"AudioListener.volume = {AudioListener.volume}");
        if (src.outputAudioMixerGroup != null) Debug.Log("AudioSource is routed to Mixer: " + src.outputAudioMixerGroup.name);
        else Debug.Log("AudioSource not routed to Mixer (outputAudioMixerGroup == null)");
    }
}
