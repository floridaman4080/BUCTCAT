using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 校史馆移动 : MonoBehaviour
{
    [SerializeField] private Cinemachine.CinemachineVirtualCamera virtualCamera;

    [SerializeField] private Camera mainCamera; // 用于获取 aspect ratio
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool isFollowing = true;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        if (isFollowing && virtualCamera != null && mainCamera != null && spriteRenderer != null)
        {
            // 获取虚拟相机的正交大小
            float orthoSize = virtualCamera.m_Lens.OrthographicSize;

            // 计算相机右边界的世界坐标（使用虚拟相机的位置）
            float cameraRightEdge = virtualCamera.transform.position.x + orthoSize * mainCamera.aspect;

            // 获取sprite宽度的一半
            float halfWidth = spriteRenderer.bounds.size.x / 2f;

            // 设置sprite位置：相机右边界 + sprite宽度的一半(确保完全在屏幕外)
            Vector3 newPosition = transform.position;
            newPosition.x = cameraRightEdge + halfWidth;
            transform.position = newPosition;
        }
    }

    // 停止跟随，照片停在原地
    public void 停止跟随()
    {
        isFollowing = false;
    }

    // 恢复跟随（如果需要的话）
    public void 恢复跟随()
    {
        isFollowing = true;
    }
}
