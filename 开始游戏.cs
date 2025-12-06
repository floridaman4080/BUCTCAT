using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class 开始游戏 : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI 标题;
    [SerializeField] private TextMeshProUGUI 开始游戏文本;
    [SerializeField] private float fadeDuration = 2f;




    public Cinemachine.CinemachineVirtualCamera virtualCamera;

    void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(uninteractable);
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    public void OnStartButtonClicked()
    {
        if (标题 != null)
        {
            StartCoroutine(标题淡出());
        }

        // 延迟2秒后停止虚拟相机跟随
        StartCoroutine(延迟停止相机跟随());


    }

    public IEnumerator 延迟停止相机跟随()
    {
        yield return new WaitForSeconds(4f);

        if (virtualCamera != null)
        {
            virtualCamera.Follow = null;
            virtualCamera.LookAt = null;
        }
    }

    public IEnumerator 标题淡出()
    {
        float elapsedTime = 0f;
        Color originalColor = 标题.color;


        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            标题.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            开始游戏文本.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            startButton.image.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        标题.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }
    private void uninteractable()
    {
        startButton.interactable = false;
    }

}
