using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movePlayer : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("翻转设置")]
    [SerializeField] private bool flipByScale = true;  // true: 用Scale翻转, false: 用SpriteRenderer翻转
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
        transform.localScale = new Vector3(-1, 1, 1);
    }

    void Update()
    {
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

    }
}
