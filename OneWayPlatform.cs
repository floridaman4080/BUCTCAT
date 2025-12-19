using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单向平台脚本 - 挂载在单向平台物体上
/// 功能：玩家从下方可以跳上平台，从上方需要特定操作才能穿透下落
/// 
/// 使用方法：
/// 1. 在平台物体上添加此脚本
/// 2. 平台需要有 Collider2D 组件
/// 3. 添加 PlatformEffector2D 组件（脚本会自动添加）
/// 4. 确保平台的 Layer 在玩家的 groundLayer 中
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PlatformEffector2D))]
public class OneWayPlatform : MonoBehaviour
{
    [Header("平台设置")]
    [Tooltip("玩家穿透平台后，重新启用碰撞的时间")]
    [SerializeField] private float disableCollisionTime = 0.3f;

    private PlatformEffector2D platformEffector;
    private Collider2D platformCollider;

    // 存储正在穿透的玩家碰撞体
    private HashSet<Collider2D> ignoredColliders = new HashSet<Collider2D>();

    void Awake()
    {
        platformCollider = GetComponent<Collider2D>();
        platformEffector = GetComponent<PlatformEffector2D>();

        // 设置 PlatformEffector2D
        if (platformEffector != null)
        {
            platformEffector.useOneWay = true;
            platformEffector.surfaceArc = 1f;  // 只有正上方极小角度有碰撞（几乎只有上边）
            platformEffector.useOneWayGrouping = true;
            platformEffector.useSideFriction = false;  // 禁用侧面摩擦
            platformEffector.useSideBounce = false;    // 禁用侧面弹力
        }

        // 确保碰撞体使用 Effector
        if (platformCollider != null)
        {
            platformCollider.usedByEffector = true;
        }
    }

    /// <summary>
    /// 允许玩家穿透此平台（供玩家脚本调用）
    /// </summary>
    /// <param name="playerCollider">玩家的碰撞体</param>
    public void AllowPlayerToFallThrough(Collider2D playerCollider)
    {
        if (playerCollider != null && !ignoredColliders.Contains(playerCollider))
        {
            StartCoroutine(DisableCollisionTemporarily(playerCollider));
        }
    }

    /// <summary>
    /// 临时禁用与玩家的碰撞
    /// </summary>
    private IEnumerator DisableCollisionTemporarily(Collider2D playerCollider)
    {
        ignoredColliders.Add(playerCollider);

        // 忽略玩家和平台之间的碰撞
        Physics2D.IgnoreCollision(platformCollider, playerCollider, true);

        // 等待指定时间
        yield return new WaitForSeconds(disableCollisionTime);

        // 重新启用碰撞
        Physics2D.IgnoreCollision(platformCollider, playerCollider, false);

        ignoredColliders.Remove(playerCollider);
    }

    /// <summary>
    /// 检查某个碰撞体是否当前被忽略
    /// </summary>
    public bool IsColliderIgnored(Collider2D collider)
    {
        return ignoredColliders.Contains(collider);
    }
}
