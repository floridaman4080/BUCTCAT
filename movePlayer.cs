using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
// using System.Threading.Tasks.Dataflow;
using UnityEngine;
using UnityEngine.Playables;  // ★ 添加 Timeline 支持

public class movePlayer : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("跳跃设置")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    private bool isGrounded = false;

    [Header("翻转设置")]
    [SerializeField] private bool flipByScale = true;  // true: 用Scale翻转, false: 用SpriteRenderer翻转

    [Header("Timeline 设置")]
    [Tooltip("拖入场景中的 PlayableDirector（Timeline播放器）")]
    [SerializeField] private PlayableDirector playableDirector;  // ★ Timeline 播放器引用

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool facingRight = true;
    private float horizontalInput = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        // transform.localScale = new Vector3(-1, 1, 1);
    }

    void Update()
    {
        // 始终检测地面（即使在对话中也需要）
        CheckGround();

        // ★ 对话进行时或 Timeline 播放时禁止移动
        if (IsMovementDisabled())
        {
            horizontalInput = 0f;
            // 强制设置为静止动画状态
            anim.SetBool("IsMove", false);
            anim.SetBool("IsJump", false);  // ★ 强制关闭跳跃动画
            return;  // 直接返回，不处理任何输入
        }

        horizontalInput = 0f;

        AnimatorController();

        // AD键控制
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
            
        }

        // 跳跃
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // 移动
        if (horizontalInput != 0)
        {
            transform.Translate(Vector3.right * horizontalInput * moveSpeed * Time.deltaTime);
            

            // 翻转方向
            if (horizontalInput > 0 && !facingRight)
            {
                Flip();
                
            }
            else if (horizontalInput < 0 && facingRight)
            {
                Flip();
                
            }
        }
    }

    /// <summary>
    /// 翻转角色朝向
    /// </summary>
    void Flip()
    {
        facingRight = !facingRight;

        if (flipByScale)
        {
            // 方式1: 通过Scale翻转（推荐，子物体也会一起翻转）
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        else
        {
            // 方式2: 通过SpriteRenderer翻转
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !spriteRenderer.flipX;
            }
        }
    }
    private void AnimatorController()
    {
        bool IsMove = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
        anim.SetBool("IsMove", IsMove);
        anim.SetBool("IsJump", !isGrounded);
    }

    /// <summary>
    /// 检测是否在地面上
    /// </summary>
    void CheckGround()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            // 如果没有设置groundCheck，使用角色位置向下检测
            isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, groundLayer);
        }
    }

    /// <summary>
    /// 跳跃
    /// </summary>
    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    /// <summary>
    /// 在Scene视图中绘制地面检测范围（方便调试）
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    /// <summary>
    /// 检查是否应该禁用移动
    /// </summary>
    private bool IsMovementDisabled()
    {
        // 对话进行时禁止移动
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive())
        {
            return true;
        }

        // Timeline 播放时禁止移动
        if (playableDirector != null && playableDirector.state == PlayState.Playing)
        {
            return true;

        }

        return false;
    }

    /// <summary>
    /// 设置 Timeline 播放器引用（可以通过代码设置）
    /// </summary>
    public void SetPlayableDirector(PlayableDirector director)
    {
        playableDirector = director;
    }
}
