using UnityEngine;

public class SoundTest : MonoBehaviour
{

    [SerializeField]
    AudioSource m_touch;
    [SerializeField]
    AudioSource m_loop;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_touch.Play();
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            m_touch.Stop();
        }

        if (Input.GetKeyUp(KeyCode.X))
        {
            m_loop.Play();
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            m_loop.Stop();
        }
    }
}
