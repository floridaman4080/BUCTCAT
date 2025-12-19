using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ObjectShow : MonoBehaviour
{
    [Header("要显示的GameObject")]
    public GameObject targetObject;

    [Header("触发显示的Button")]
    public Button showButton;

    [Header("渐变设置")]
    [Tooltip("渐变持续时间(秒)")]
    public float fadeDuration = 0.3f;

    [Header("缩放设置")]
    [Tooltip("最小缩放比例")]
    public float minScale = 0.5f;
    [Tooltip("最大缩放比例")]
    public float maxScale = 2.0f;

    private CanvasGroup canvasGroup;
    private bool isAnimating = false;
    private Vector3 initialScale;
    private float previousTouchDistance = 0f;

    void Start()
    {
        // 获取或添加CanvasGroup组件
        if (targetObject != null)
        {
            canvasGroup = targetObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = targetObject.AddComponent<CanvasGroup>();
            }

            // 保存初始缩放值
            initialScale = targetObject.transform.localScale;

            // 初始时隐藏目标对象
            targetObject.SetActive(false);
        }

        // 为按钮添加点击事件
        if (showButton != null)
        {
            showButton.onClick.AddListener(ShowObject);
        }
    }

    void Update()
    {
        // 双指缩放检测
        if (Input.touchCount == 2 && targetObject != null && targetObject.activeSelf)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            // 计算当前两指距离
            float currentTouchDistance = Vector2.Distance(touch0.position, touch1.position);

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                // 初始化距离
                previousTouchDistance = currentTouchDistance;
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                // 计算缩放增量
                float deltaDistance = currentTouchDistance - previousTouchDistance;
                float scaleFactor = 1 + (deltaDistance / Screen.dpi * 0.5f);

                // 应用缩放
                Vector3 newScale = targetObject.transform.localScale * scaleFactor;

                // 限制缩放范围
                float scaleRatio = newScale.x / initialScale.x;
                if (scaleRatio >= minScale && scaleRatio <= maxScale)
                {
                    targetObject.transform.localScale = newScale;
                }

                previousTouchDistance = currentTouchDistance;
            }

            // 双指缩放时不处理单击关闭
            return;
        }

        // 检测输入(支持鼠标和触控)
        bool inputDetected = false;
        Vector2 inputPosition = Vector2.zero;

        // 鼠标输入
        if (Input.GetMouseButtonDown(0))
        {
            inputDetected = true;
            inputPosition = Input.mousePosition;
        }
        // 触控输入(单指)
        else if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            inputDetected = true;
            inputPosition = Input.GetTouch(0).position;
        }

        if (inputDetected)
        {
            // 如果目标对象是激活状态且没有正在播放动画
            if (targetObject != null && targetObject.activeSelf && !isAnimating)
            {
                // 检查是否点击在目标对象上
                if (!IsPointerOverUIObject(targetObject, inputPosition))
                {
                    // 点击在目标对象外部,关闭它
                    StartCoroutine(HideObject());
                }
            }
        }
    }

    // 显示目标对象(带渐变效果)
    void ShowObject()
    {
        if (targetObject != null && !isAnimating)
        {
            StartCoroutine(FadeIn());
        }
    }

    // 渐变显示
    IEnumerator FadeIn()
    {
        isAnimating = true;
        // 重置缩放为初始值
        targetObject.transform.localScale = initialScale;
        targetObject.SetActive(true);

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        isAnimating = false;
    }

    // 渐变隐藏
    IEnumerator HideObject()
    {
        isAnimating = true;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        targetObject.SetActive(false);
        isAnimating = false;
    }

    // 检查输入位置是否在指定的UI对象上(支持鼠标和触控)
    bool IsPointerOverUIObject(GameObject uiObject, Vector2 inputPosition)
    {
        // 创建PointerEventData
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = inputPosition;

        // 射线检测
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        // 检查是否点击在目标对象或其子对象上
        foreach (RaycastResult result in results)
        {
            if (result.gameObject == uiObject || result.gameObject.transform.IsChildOf(uiObject.transform))
            {
                return true;
            }
        }

        return false;
    }
}
