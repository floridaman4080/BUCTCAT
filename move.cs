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
    [SerializeField] private float 固定速度 = 5f;
    [SerializeField] private float 延迟停下 = 4f;
    [SerializeField] private float 延迟继续 = 4f;
    public bool canMove = true;
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
        FlipOnce();
    }

    void Update()
    {
        //CheckInput();
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
    private void CheckInput()
    {
        xInput = Input.GetAxis("Horizontal");
    }
    private void AnimatorController()
    {
        bool IsMove = rb.velocity.x != 0;
        anim.SetBool("IsMove", IsMove);
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
}