using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class move : MonoBehaviour

{
    public static move instance;
    private float xInput;
    private Animator anim;
    private Rigidbody2D rb;
    [SerializeField] private float 固定速度 = 2f;
    [SerializeField] private float 延迟停下 = 4f;
    [SerializeField] private float 延迟继续 = 4f;
    [SerializeField] private float 跳跃力 = 8f;
    [SerializeField] private LayerMask 地面层;
    [SerializeField] private Transform 地面检测点;
    [SerializeField] private float 地面检测半径 = 0.2f;
    public bool canMove = true;
    private bool isGrounded;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        // FlipOnce();
    }

    void Update()
    {
        CheckGrounded();
        // CheckJumpInput();

        if (canMove)
        {
            AnimatorController();
            Movement();
        }
        else
        {
            AnimatorController();
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    private void CheckGrounded()
    {
        if (地面检测点 != null)
        {
            isGrounded = Physics2D.OverlapCircle(地面检测点.position, 地面检测半径, 地面层);
        }
        else
        {
            // 如果没有设置地面检测点，使用角色底部位置
            isGrounded = Physics2D.OverlapCircle(transform.position, 地面检测半径, 地面层);
        }
    }

    private void CheckJumpInput()
    {
        if (canMove && isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 跳跃力);
        anim.SetTrigger("Jump");
    }
    private void CheckInput()
    {
        xInput = Input.GetAxis("Horizontal");
    }
    private void AnimatorController()
    {
        bool IsMove = rb.velocity.x != 0;
        anim.SetBool("IsMove", IsMove);
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetFloat("VelocityY", rb.velocity.y);
    }
    private void Movement()
    {
        rb.velocity = new Vector2(固定速度, rb.velocity.y);
    }
    private void Flip()
    {
        if (xInput > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (xInput < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
    private void FlipOnce()
    {
        transform.localScale = new Vector3(-1, 1, 1);
    }

    public void 停止运动()
    {
        StartCoroutine(停止运动Coroutine());
    }

    private IEnumerator 停止运动Coroutine()
    {
        yield return new WaitForSeconds(延迟停下);
        canMove = false;
        Debug.Log("停止运动");
    }

    public void 继续运动()
    {
        StartCoroutine(继续运动Coroutine());
    }

    private IEnumerator 继续运动Coroutine()
    {
        yield return new WaitForSeconds(延迟继续);
        canMove = true;
        Debug.Log("继续运动");
    }

    // 在Scene视图中绘制地面检测范围（方便调试）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (地面检测点 != null)
        {
            Gizmos.DrawWireSphere(地面检测点.position, 地面检测半径);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 地面检测半径);
        }
    }
}