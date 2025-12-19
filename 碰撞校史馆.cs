using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class 碰撞校史馆 : MonoBehaviour
{
    [SerializeField] private string nextSceneName;  // 下一个场景的名字
    [SerializeField] private GameObject player;  // 直接拖入玩家物体
    public bool isTouchMuseum = false;
    public static 碰撞校史馆 instance;
    private void Awake()
    {
        // 每个场景的触发器都是独立的，不需要跨场景保留
        instance = this;
    }

    private void OnDestroy()
    {
        // 当对象被销毁时，清除静态引用
        if (instance == this)
        {
            instance = null;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        StartCoroutine(touchMuseum(other));
    }
    public IEnumerator touchMuseum(Collider2D other)
    {
        if (other.gameObject == player)
        {
            isTouchMuseum = true;
            yield return new WaitForSeconds(0f);
            //move.instance.canMove = false;
        }
    }

    // 如果使用3D碰撞器，用这个方法
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag(playerTag))
    //     {
    //         SceneManager.LoadScene(nextSceneName);
    //     }
    // }
}
