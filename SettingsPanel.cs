using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 设置界面控制器
/// 功能：打开/关闭设置界面，暂停/恢复游戏，重新开始关卡
/// 所有界面显示和淡出都带有渐变效果
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    [Header("UI 引用")]
    [Tooltip("设置按钮（点击后打开设置界面）")]
    [SerializeField] private Button settingsButton;

    [Tooltip("设置界面面板")]
    [SerializeField] private GameObject settingsPanel;

    [Tooltip("关闭设置界面按钮")]
    [SerializeField] private Button closeButton;

    [Header("关卡按钮")]
    [Tooltip("重新开始关卡1的按钮")]
    [SerializeField] private Button level1Button;

    [Tooltip("重新开始关卡2的按钮")]
    [SerializeField] private Button level2Button;

    [Tooltip("重新开始关卡3的按钮")]
    [SerializeField] private Button level3Button;

    [Header("制作人员名单")]
    [Tooltip("打开制作人员名单的按钮")]
    [SerializeField] private Button creditsButton;

    [Tooltip("制作人员名单面板")]
    [SerializeField] private GameObject creditsPanel;

    [Tooltip("关闭制作人员名单的按钮")]
    [SerializeField] private Button creditsCloseButton;

    [Header("场景设置")]
    [Tooltip("关卡1的场景名称")]
    [SerializeField] private string level1SceneName = "副本0";

    [Tooltip("关卡2的场景名称")]
    [SerializeField] private string level2SceneName = "副本1";

    [Tooltip("关卡3的场景名称")]
    [SerializeField] private string level3SceneName = "副本2";

    [Header("渐变设置")]
    [Tooltip("渐变动画持续时间")]
    [SerializeField] private float fadeDuration = 0.3f;

    // 记录暂停前的时间缩放
    private float previousTimeScale = 1f;

    // 设置界面是否打开
    private bool isSettingsOpen = false;

    // 制作人员名单是否打开
    private bool isCreditsOpen = false;

    // 渐变动画是否正在进行
    private bool isFading = false;

    // 面板的 CanvasGroup 组件
    private CanvasGroup settingsPanelCanvasGroup;
    private CanvasGroup creditsPanelCanvasGroup;

    void Start()
    {
        // 获取或添加 CanvasGroup 组件
        if (settingsPanel != null)
        {
            settingsPanelCanvasGroup = settingsPanel.GetComponent<CanvasGroup>();
            if (settingsPanelCanvasGroup == null)
            {
                settingsPanelCanvasGroup = settingsPanel.AddComponent<CanvasGroup>();
            }
            settingsPanelCanvasGroup.alpha = 0f;
            settingsPanel.SetActive(false);
        }

        if (creditsPanel != null)
        {
            creditsPanelCanvasGroup = creditsPanel.GetComponent<CanvasGroup>();
            if (creditsPanelCanvasGroup == null)
            {
                creditsPanelCanvasGroup = creditsPanel.AddComponent<CanvasGroup>();
            }
            creditsPanelCanvasGroup.alpha = 0f;
            creditsPanel.SetActive(false);
        }

        // 绑定设置按钮点击事件
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OpenSettings);
        }

        // 绑定关闭按钮点击事件
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }

        // 绑定关卡按钮点击事件
        if (level1Button != null)
        {
            level1Button.onClick.AddListener(RestartLevel1);
        }

        if (level2Button != null)
        {
            level2Button.onClick.AddListener(RestartLevel2);
        }

        if (level3Button != null)
        {
            level3Button.onClick.AddListener(RestartLevel3);
        }

        // 绑定制作人员名单按钮点击事件
        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(OpenCredits);
        }

        // 绑定关闭制作人员名单按钮点击事件
        if (creditsCloseButton != null)
        {
            creditsCloseButton.onClick.AddListener(CloseCredits);
        }
    }

    void Update()
    {
        // ESC键也可以打开/关闭设置界面（渐变动画进行时不响应）
        if (Input.GetKeyDown(KeyCode.Escape) && !isFading)
        {
            if (isCreditsOpen)
            {
                // 如果制作人员名单打开，先关闭它
                CloseCredits();
            }
            else if (isSettingsOpen)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }
    }

    /// <summary>
    /// 打开设置界面并暂停游戏（带渐变）
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPanel != null && !isFading && !isSettingsOpen)
        {
            // 记录当前时间缩放
            previousTimeScale = Time.timeScale;

            // 暂停游戏
            Time.timeScale = 0f;

            // 显示设置界面（带渐变）
            StartCoroutine(FadeIn(settingsPanel, settingsPanelCanvasGroup, () =>
            {
                isSettingsOpen = true;
            }));
        }
    }

    /// <summary>
    /// 关闭设置界面并恢复游戏（带渐变）
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null && !isFading && isSettingsOpen)
        {
            // 先关闭制作人员名单（如果打开）
            if (isCreditsOpen)
            {
                StartCoroutine(FadeOut(creditsPanel, creditsPanelCanvasGroup, () =>
                {
                    isCreditsOpen = false;
                    // 然后关闭设置界面
                    StartCoroutine(FadeOut(settingsPanel, settingsPanelCanvasGroup, () =>
                    {
                        isSettingsOpen = false;
                        // 恢复时间缩放
                        Time.timeScale = previousTimeScale;
                    }));
                }));
            }
            else
            {
                StartCoroutine(FadeOut(settingsPanel, settingsPanelCanvasGroup, () =>
                {
                    isSettingsOpen = false;
                    // 恢复时间缩放
                    Time.timeScale = previousTimeScale;
                }));
            }
        }
    }

    /// <summary>
    /// 打开制作人员名单面板（带渐变）
    /// </summary>
    public void OpenCredits()
    {
        if (creditsPanel != null && !isFading && !isCreditsOpen)
        {
            StartCoroutine(FadeIn(creditsPanel, creditsPanelCanvasGroup, () =>
            {
                isCreditsOpen = true;
            }));
        }
    }

    /// <summary>
    /// 关闭制作人员名单面板（带渐变）
    /// </summary>
    public void CloseCredits()
    {
        if (creditsPanel != null && !isFading && isCreditsOpen)
        {
            StartCoroutine(FadeOut(creditsPanel, creditsPanelCanvasGroup, () =>
            {
                isCreditsOpen = false;
            }));
        }
    }

    /// <summary>
    /// 淡入动画协程
    /// </summary>
    private IEnumerator FadeIn(GameObject panel, CanvasGroup canvasGroup, System.Action onComplete = null)
    {
        isFading = true;
        panel.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            // 使用 unscaledDeltaTime 因为游戏可能已暂停
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        isFading = false;

        onComplete?.Invoke();
    }

    /// <summary>
    /// 淡出动画协程
    /// </summary>
    private IEnumerator FadeOut(GameObject panel, CanvasGroup canvasGroup, System.Action onComplete = null)
    {
        isFading = true;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            // 使用 unscaledDeltaTime 因为游戏可能已暂停
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            yield return null;
        }

        canvasGroup.alpha = 0f;
        panel.SetActive(false);
        isFading = false;

        onComplete?.Invoke();
    }

    /// <summary>
    /// 重新开始关卡1
    /// </summary>
    public void RestartLevel1()
    {
        // 恢复时间缩放（否则场景加载后游戏仍然暂停）
        Time.timeScale = 1f;
        SceneManager.LoadScene(level1SceneName);
    }

    /// <summary>
    /// 重新开始关卡2
    /// </summary>
    public void RestartLevel2()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(level2SceneName);
    }

    /// <summary>
    /// 重新开始关卡3
    /// </summary>
    public void RestartLevel3()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(level3SceneName);
    }

    /// <summary>
    /// 返回主菜单（可选功能）
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("开始界面");
    }

    /// <summary>
    /// 检查设置界面是否打开（供其他脚本查询）
    /// </summary>
    public bool IsSettingsOpen()
    {
        return isSettingsOpen;
    }

    /// <summary>
    /// 检查制作人员名单是否打开
    /// </summary>
    public bool IsCreditsOpen()
    {
        return isCreditsOpen;
    }

    /// <summary>
    /// 检查是否正在进行渐变动画
    /// </summary>
    public bool IsFading()
    {
        return isFading;
    }

    void OnDestroy()
    {
        // 确保在销毁时恢复时间缩放
        if (isSettingsOpen)
        {
            Time.timeScale = 1f;
        }
    }
}
