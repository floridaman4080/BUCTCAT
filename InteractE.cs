using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractE : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private bool isOn;
    [SerializeField] private float fadeDuration = 0.5f; // 渐变持续时间

    private Renderer targetRenderer;
    private Material material;
    private bool isFading = false;

    // Start is called before the first frame update
    void Start()
    {
        if (targetObject != null)
        {
            targetRenderer = targetObject.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                // 获取材质实例
                material = targetRenderer.material;

                // 初始化透明度
                SetAlpha(isOn ? 1f : 0f);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (targetObject == null || material == null)
            {
                Debug.LogWarning("targetObject 或材质未设置!");
                return;
            }

            if (isFading)
            {
                return;
            }

            isOn = !isOn;
            StartCoroutine(FadeCoroutine(isOn));
        }
    }

    private IEnumerator FadeCoroutine(bool fadeIn)
    {
        isFading = true;

        float startAlpha = material.GetFloat("_Alpha");
        float targetAlpha = fadeIn ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);
        isFading = false;
    }

    private void SetAlpha(float alpha)
    {
        if (material != null)
        {
            material.SetFloat("_Alpha", alpha);
        }
    }

    private void OnDestroy()
    {
        // 清理材质实例，防止内存泄漏
        if (material != null)
        {
            Destroy(material);
        }
    }
}
