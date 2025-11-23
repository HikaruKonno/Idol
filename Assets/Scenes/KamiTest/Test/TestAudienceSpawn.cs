using System.Collections.Generic;
using UnityEngine;

public class TestAudienceSpawn : MonoBehaviour
{
    public List<Material> m_material = new List<Material>();
    public List<Renderer> m_renderer = new List<Renderer>();

    public List<Color> m_baseColorList;
    public List<Color> m_colorList;

    [Header("生成するオブジェクト")]
    [SerializeField]
    private GameObject m_audiencePrefab;

    void Start()
    {
        SpawnAudiences(30); // 10体生成
    }

    private void Update()
    {
        if (Time.time >= 3.0f)
        {
            ChangeColorMat();
        }
    }
    void SpawnAudiences(int count)
    {
        if (m_audiencePrefab == null || m_material.Count == 0)
        {
            Debug.LogWarning("Prefabかマテリアルリストが設定されていません");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            // 生成位置は少しずらす（例としてX座標をずらす）
            Vector3 spawnPos = new Vector3(i * 2.0f, 0, 0);

            GameObject audience = Instantiate(m_audiencePrefab, spawnPos, Quaternion.identity);

            Renderer rend = audience.GetComponent<Renderer>();

            if (rend != null)
            {
                Renderer psylliumRenderer = rend.GetComponent<Renderer>();
                m_renderer.Add(rend);

                if (psylliumRenderer != null)
                {
                    // リストの中からランダムに色を選ぶ
                    float randomColorIndex = Random.Range(0, m_material.Count);
                    // Random.Range は flaot で返されるので要素番号用に Int に変換
                    int colorIndex = Mathf.FloorToInt(randomColorIndex);

                    rend.material = m_material[colorIndex];
                }
                else
                {
                    Debug.LogWarning("Rendererが見つかりませんでした");
                }
            }
        }
    }
    private void ChangeColorMat()
    {
        m_material[0].SetColor("_BaseColor", m_colorList[0]);
    }

    public void ResetColor()
    {
        // リストの中からランダムに色を選ぶ
        float randomColorIndex = Random.Range(0, m_baseColorList.Count);
        // Random.Range は flaot で返されるので要素番号用に Int に変換
        int colorIndex = Mathf.FloorToInt(randomColorIndex);

        for (int i = 0; i < m_material.Count; ++i)
        {
            m_material[i].SetColor("_BaseColor", m_baseColorList[colorIndex]);
        }
    }
}