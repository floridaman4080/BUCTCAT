using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
// using System.Threading.Tasks.Dataflow;
using UnityEngine;
using UnityEngine.Playables;  // 添加Timeline支持

public class movePlayer : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("跳跃设置")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [Tooltip("地面检测的宽度（猫底盘的一半宽度）")]
    [SerializeField] private float groundCheckWidth = 0.3f;  // 左右两侧检测点的偏移距离
    private bool isGrounded = false;

    [Header("翻转设置")]
    [SerializeField] private bool flipByScale = true;  // true: 用Scale翻转, false: 用SpriteRenderer翻转

    [Header("Timeline 设置")]
    [Tooltip("拖入场景中的 PlayableDirector（Timeline播放器）")]
    [SerializeField] private PlayableDirector playableDirector;  // ★ Timeline 播放器引用

    [Header("手机端控制")]
    [Tooltip("拖入场景中的 Joystick（摇杆控制器）")]
    [SerializeField] private Joystick joystick;  // 摇杆引用
    [Tooltip("是否启用手机端控制")]
    [SerializeField] private bool useMobileControls = false;

    [Header("单向平台设置")]
    [Tooltip("摇杆向下的阈值（0-1之间，超过此值认为是向下）")]
    [SerializeField] private float downInputThreshold = 0.5f;
    [Tooltip("检测单向平台的范围")]
    [SerializeField] private float platformCheckRadius = 0.5f;
    [Tooltip("单向平台的Layer")]
    [SerializeField] private LayerMask oneWayPlatformLayer;

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;  // 玩家碰撞体
    private bool facingRight = true;
    private float horizontalInput = 0f;
    private bool jumpButtonPressed = false;  // 跳跃按钮状态
    private bool dropDownButtonPressed = false;  // 下落按钮状态

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        playerCollider = GetComponent<Collider2D>();  // 获取玩家碰撞体
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
            anim.SetBool("Isjumppaper", false);  // ★ 强制关闭跳跃动画
            anim.SetBool("IsJumpwithoutpaper", false);
            return;  // 直接返回，不处理任何输入
        }

        horizontalInput = 0f;

        AnimatorController();

        // 获取移动输入（键盘 + 摇杆）
        if (useMobileControls && joystick != null)
        {
            // 使用摇杆输入
            horizontalInput = joystick.Horizontal;
        }
        else
        {
            // AD键控制
            if (Input.GetKey(KeyCode.A))
            {
                horizontalInput = -1f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                horizontalInput = 1f;
            }
        }

        // 检测下落穿透单向平台（优先检测，因为 S+Space 应该下落而不是跳跃）
        bool didDropThrough = CheckDropThroughPlatform();

        // 跳跃（键盘空格键 或 跳跃按钮）- 如果没有执行下落才跳跃
        if (!didDropThrough && (Input.GetKeyDown(KeyCode.Space) || jumpButtonPressed) && isGrounded)
        {
            Jump();
            jumpButtonPressed = false;  // 重置跳跃按钮状态
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
        bool IsMove = false;

        if (useMobileControls && joystick != null)
        {
            // 摇杆控制时，根据摇杆输入判断是否移动
            IsMove = Mathf.Abs(joystick.Horizontal) > 0.1f;
        }
        else
        {
            // 键盘控制
            IsMove = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
        }

        anim.SetBool("IsMove", IsMove);
        if (!anim.GetBool("haspaper"))
        {
            anim.SetBool("IsJumpwithoutpaper", !isGrounded);
        }
        if (anim.GetBool("haspaper"))
        {
            anim.SetBool("Isjumppaper", !isGrounded);
        }

        // 设置 Y 轴速度到 Animator 的 Yvelocity 参数（用于 BlendTree）
        anim.SetFloat("Yvelocity", rb.velocity.y);
    }

    /// <summary>
    /// 检测是否在地面上（使用多点检测，适配猫的宽底盘）
    /// </summary>
    void CheckGround()
    {
        // 合并地面Layer和单向平台Layer，两者都算作地面
        LayerMask combinedGroundLayer = groundLayer | oneWayPlatformLayer;

        if (groundCheck != null)
        {
            // 中心点检测
            bool centerGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, combinedGroundLayer);

            // 左侧检测点
            Vector3 leftCheckPos = groundCheck.position + Vector3.left * groundCheckWidth;
            bool leftGrounded = Physics2D.OverlapCircle(leftCheckPos, groundCheckRadius, combinedGroundLayer);

            // 右侧检测点
            Vector3 rightCheckPos = groundCheck.position + Vector3.right * groundCheckWidth;
            bool rightGrounded = Physics2D.OverlapCircle(rightCheckPos, groundCheckRadius, combinedGroundLayer);

            // 只要有一个点在地面上，就认为在地面
            isGrounded = centerGrounded || leftGrounded || rightGrounded;
        }
        else
        {
            // 如果没有设置groundCheck，使用角色位置向下检测（同样使用多点）
            bool centerGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, combinedGroundLayer);
            bool leftGrounded = Physics2D.Raycast(transform.position + Vector3.left * groundCheckWidth, Vector2.down, 0.5f, combinedGroundLayer);
            bool rightGrounded = Physics2D.Raycast(transform.position + Vector3.right * groundCheckWidth, Vector2.down, 0.5f, combinedGroundLayer);

            isGrounded = centerGrounded || leftGrounded || rightGrounded;
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
            // 中心点（绿色）
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

            // 左侧检测点（黄色）
            Gizmos.color = Color.yellow;
            Vector3 leftCheckPos = groundCheck.position + Vector3.left * groundCheckWidth;
            Gizmos.DrawWireSphere(leftCheckPos, groundCheckRadius);

            // 右侧检测点（黄色）
            Vector3 rightCheckPos = groundCheck.position + Vector3.right * groundCheckWidth;
            Gizmos.DrawWireSphere(rightCheckPos, groundCheckRadius);
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

    /// <summary>
    /// 跳跃按钮按下（供UI按钮调用）
    /// </summary>
    public void OnJumpButtonDown()
    {
        if (isGrounded && !IsMovementDisabled())
        {
            jumpButtonPressed = true;
        }
    }

    /// <summary>
    /// 设置摇杆引用（可以通过代码设置）
    /// </summary>
    public void SetJoystick(Joystick newJoystick)
    {
        joystick = newJoystick;
    }

    /// <summary>
    /// 设置是否使用手机端控制
    /// </summary>
    public void SetUseMobileControls(bool useMobile)
    {
        useMobileControls = useMobile;
    }

    /// <summary>
    /// 获取当前是否在地面上（供外部查询）
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded;
    }

    /// <summary>
    /// 检测并执行下落穿透单向平台
    /// </summary>
    /// <returns>是否成功执行了下落穿透</returns>
    private bool CheckDropThroughPlatform()
    {
        // 检查是否满足下落条件
        bool isJoystickDown = false;
        bool isKeyboardDown = false;

        if (useMobileControls && joystick != null)
        {
            // 摇杆向下判断
            isJoystickDown = joystick.Vertical < -downInputThreshold;
        }
        else
        {
            // 键盘 S 键向下
            isKeyboardDown = Input.GetKey(KeyCode.S);
        }

        // 判断是否应该下落穿透
        bool shouldDropDown = false;

        if (useMobileControls)
        {
            // 手机端：摇杆向下 + 跳跃按钮（复用跳跃按钮）
            shouldDropDown = isJoystickDown && jumpButtonPressed;
        }
        else
        {
            // PC端：S键 + 空格键
            shouldDropDown = isKeyboardDown && Input.GetKeyDown(KeyCode.Space);
        }

        if (shouldDropDown && isGrounded)
        {
            TryDropThroughPlatform();
            jumpButtonPressed = false;  // 重置跳跃按钮状态（手机端复用）
            dropDownButtonPressed = false;
            return true;  // 成功执行下落
        }
        return false;  // 没有执行下落
    }

    /// <summary>
    /// 尝试穿透脚下的单向平台
    /// </summary>
    private void TryDropThroughPlatform()
    {
        if (playerCollider == null) return;

        // 检测脚下的单向平台
        Vector2 checkPosition = groundCheck != null ? groundCheck.position : (Vector2)transform.position;

        // 使用 OverlapCircleAll 检测周围的单向平台
        Collider2D[] platforms = Physics2D.OverlapCircleAll(checkPosition, platformCheckRadius, oneWayPlatformLayer);

        foreach (Collider2D platformCollider in platforms)
        {
            // 获取单向平台脚本
            OneWayPlatform oneWayPlatform = platformCollider.GetComponent<OneWayPlatform>();
            if (oneWayPlatform != null)
            {
                // 调用平台的穿透方法
                oneWayPlatform.AllowPlayerToFallThrough(playerCollider);
            }
            else
            {
                // 如果平台没有 OneWayPlatform 脚本，直接忽略碰撞
                StartCoroutine(TemporarilyIgnoreCollision(platformCollider));
            }
        }
    }

    /// <summary>
    /// 临时忽略与平台的碰撞（备用方法）
    /// </summary>
    private IEnumerator TemporarilyIgnoreCollision(Collider2D platformCollider)
    {
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        yield return new WaitForSeconds(0.3f);
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }

    /// <summary>
    /// 下落按钮按下（供UI按钮调用 - 用于穿透单向平台）
    /// </summary>
    public void OnDropDownButtonDown()
    {
        if (isGrounded && !IsMovementDisabled())
        {
            dropDownButtonPressed = true;
        }
    }

    /// <summary>
    /// 下落按钮松开（供UI按钮调用）
    /// </summary>
    public void OnDropDownButtonUp()
    {
        dropDownButtonPressed = false;
    }

    /// <summary>
    /// 获取摇杆垂直输入（供外部查询）
    /// </summary>
    public float GetVerticalInput()
    {
        if (useMobileControls && joystick != null)
        {
            return joystick.Vertical;
        }
        else
        {
            if (Input.GetKey(KeyCode.W)) return 1f;
            if (Input.GetKey(KeyCode.S)) return -1f;
            return 0f;
        }
    }
}
