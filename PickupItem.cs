using UnityEngine;

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

    private bool playerInRange = false;
    private bool isPickedUp = false;

    private void Start()
    {
        if (pickupHint != null)
            pickupHint.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && pickupOnKeyPress && Input.GetKeyDown(pickupKey))
        {
            Pickup();
        }
    }

    private void Pickup()
    {
        if (isPickedUp) return;
        isPickedUp = true;

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

        // 播放音效
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // 销毁物品
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
