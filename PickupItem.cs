using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 可拾取物品
/// 挂载到场景中的道具物体上，玩家碰到或按E拾取
/// </summary>
public class PickupItem : MonoBehaviour
{
    [Header("物品设置")]
    public string itemName = "钥匙";      // 物品名称（要和对话条件中的一致）
    public int itemCount = 1;              // 获得数量

    [Header("拾取方式")]
    public bool pickupOnTouch = true;      // 碰到就拾取
    public bool pickupOnKeyPress = false;  // 按键拾取
    public KeyCode pickupKey = KeyCode.E;

    [Header("提示")]
    public GameObject pickupHint;          // "按E拾取" 提示

    [Header("音效")]
    public AudioClip pickupSound;

    [Header("UI 面板")]
    public GameObject itemPanel;           // 拾取时显示的物品介绍面板
    public Button closeButton;             // 面板的关闭按钮

    [Header("面板动画")]
    public float fadeInDuration = 0.3f;    // 显示面板的渐变时间
    public float fadeOutDuration = 0.3f;   // 关闭面板的渐变时间
    [SerializeField] private Animator anim;

    private bool playerInRange = false;
    private bool isPickedUp = false;
    private CanvasGroup panelCanvasGroup;

    private void Start()
    {
        if (pickupHint != null)
            pickupHint.SetActive(false);

        // 初始化关闭按钮
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePanel);
        }

        // 初始化隐藏面板
        if (itemPanel != null)
        {
            itemPanel.SetActive(false);

            // 获取或添加 CanvasGroup 组件
            panelCanvasGroup = itemPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = itemPanel.AddComponent<CanvasGroup>();
            }
        }
    }

    private void Update()
    {
        if (playerInRange && pickupOnKeyPress && Input.GetKeyDown(pickupKey))
        {
            Pickup();
        }
        if (DialogueSystem.GetBool("haspaperfalse"))
        {
            anim.SetBool("haspaper", false);
            Debug.Log("haspaperfalse");
        }
    }
    

    private void Pickup()
    {
        if (isPickedUp) return;
        isPickedUp = true;

        anim.SetBool("haspaper", true);
        Debug.Log("haspaper");
        // 找到 DialogueSystemExample 并添加道具
        DialogueSystemExample example = FindObjectOfType<DialogueSystemExample>();
        if (example != null)
        {
            example.AddItem(itemName, itemCount);
            Debug.Log("获得了 " + itemName + " x" + itemCount);
        }
        else
        {
            Debug.LogWarning("找不到 DialogueSystemExample，无法添加道具！");
        }

        // 显示物品介绍面板（带渐变效果）
        if (itemPanel != null)
        {
            StartCoroutine(ShowPanelWithFade());

            // 如果还没有绑定关闭按钮，现在绑定
            if (closeButton != null && closeButton.onClick.GetPersistentEventCount() == 0)
            {
                closeButton.onClick.AddListener(HidePanel);
            }
        }

        // 播放音效
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // 隐藏物品模型（而不是立即销毁，等面板关闭后再销毁）
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // 禁用碰撞器
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        Collider2D col2D = GetComponent<Collider2D>();
        if (col2D != null)
        {
            col2D.enabled = false;
        }

        // 隐藏提示
        if (pickupHint != null)
        {
            pickupHint.SetActive(false);
        }
    }

    public void HidePanel()
    {
        if (itemPanel != null)
        {
            StartCoroutine(HidePanelWithFade());
        }
        else
        {
            // 如果没有面板，直接销毁
            Destroy(gameObject);
        }
    }

    private IEnumerator ShowPanelWithFade()
    {
        itemPanel.SetActive(true);

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                panelCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
                yield return null;
            }

            panelCanvasGroup.alpha = 1f;
        }
    }

    private IEnumerator HidePanelWithFade()
    {
        if (panelCanvasGroup != null)
        {
            float elapsedTime = 0f;
            float startAlpha = panelCanvasGroup.alpha;

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutDuration);
                yield return null;
            }

            panelCanvasGroup.alpha = 0f;
        }

        itemPanel.SetActive(false);

        // 面板关闭后再销毁物品
        Destroy(gameObject);
    }

    // 3D 碰撞
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (pickupOnTouch)
            {
                Pickup();
            }
            else if (pickupHint != null)
            {
                pickupHint.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (pickupHint != null)
                pickupHint.SetActive(false);
        }
    }

    // 2D 碰撞
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (pickupOnTouch)
            {
                Pickup();
            }
            else if (pickupHint != null)
            {
                pickupHint.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (pickupHint != null)
                pickupHint.SetActive(false);
        }
    }
   
}
