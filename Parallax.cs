using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单层视差背景配置
/// </summary>
[System.Serializable]
public class ParallaxLayer
{
    [Tooltip("该层的背景精灵（至少需要3个以确保无缝循环）")]
    public Transform[] backgrounds;
    [Range(0f, 1f)]
    [Tooltip("视差系数：0=完全不动(最远), 1=与相机同速(最近)")]
    public float parallaxFactor = 0.5f;
}

/// <summary>
/// 三层视差背景系统 - 支持无限左右循环，跟随 Virtual Camera
/// </summary>
public class Parallax : MonoBehaviour
{
    [Header("相机设置")]
    [Tooltip("拖入你的 Cinemachine Virtual Camera；留空则自动用 Camera.main")]
    public Cinemachine.CinemachineVirtualCamera virtualCamera;

    [Header("背景层设置")]
    [Tooltip("背景层数组：建议设置3层（远景、中景、近景）")]
    public ParallaxLayer[] layers;

    private Camera mainCamera;
    private Vector3 lastCameraPos;
    private float[] layerWidths;        // 每层单个背景的宽度
    private Vector3[][] startPositions; // 每层每个背景的初始位置

    void Start()
    {
        // 获取相机引用
        mainCamera = Camera.main;

        // 初始化相机位置记录
        lastCameraPos = GetCameraPosition();

        // 验证并初始化各层
        if (layers == null || layers.Length == 0)
        {
            Debug.LogError("Parallax: 请至少设置一个背景层！");
            enabled = false;

            return;
        }

        layerWidths = new float[layers.Length];
        startPositions = new Vector3[layers.Length][];


        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].backgrounds == null || layers[i].backgrounds.Length < 3)
            {
                Debug.LogError($"Parallax: 第{i}层背景至少需要3个精灵以确保无缝循环！");
                enabled = false;
                return;
            }

            // 获取该层背景宽度（假设同层所有背景宽度相同）
            SpriteRenderer sr = layers[i].backgrounds[0].GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                Debug.LogError($"Parallax: 第{i}层背景缺少 SpriteRenderer 组件！");
                enabled = false;
                return;
            }
            layerWidths[i] = sr.bounds.size.x;

            // 记录初始位置
            startPositions[i] = new Vector3[layers[i].backgrounds.Length];
            for (int j = 0; j < layers[i].backgrounds.Length; j++)
            {
                startPositions[i][j] = layers[i].backgrounds[j].position;
            }
        }
    }

    void LateUpdate()
    {
        Vector3 currentCamPos = GetCameraPosition();
        Vector3 deltaMovement = currentCamPos - lastCameraPos;

        // 遍历每一层
        for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
        {
            ParallaxLayer layer = layers[layerIndex];
            float parallax = layer.parallaxFactor;
            float width = layerWidths[layerIndex];

            // 移动该层所有背景（视差效果）
            // parallaxFactor 越小，移动越少，看起来越远
            float moveAmount = deltaMovement.x * (1 - parallax);

            foreach (Transform bg in layer.backgrounds)
            {
                bg.position += new Vector3(moveAmount, 0, 0);
            }

            // 无限循环检测 - 左右双向
            CheckAndRepositionBackgrounds(layer, width, currentCamPos.x);
        }

        lastCameraPos = currentCamPos;
    }

    /// <summary>
    /// 检测并重新定位背景，实现无限循环
    /// </summary>
    public void CheckAndRepositionBackgrounds(ParallaxLayer layer, float width, float cameraX)
    {
        // 计算总宽度（所有背景加起来）
        float totalWidth = width * layer.backgrounds.Length;
        float halfTotal = totalWidth / 2f;

        foreach (Transform bg in layer.backgrounds)
        {
            float distanceFromCamera = bg.position.x - cameraX;

            // 如果背景太靠左（超出左边界），移到最右边
            if (distanceFromCamera < -halfTotal)
            {
                float rightmostX = GetRightmostX(layer.backgrounds);
                bg.position = new Vector3(rightmostX + width, bg.position.y, bg.position.z);
                bg.localScale = new Vector3(-bg.localScale.x, bg.localScale.y, bg.localScale.z); // 翻转背景以避免视觉重复
                Debug.Log($"背景 {bg.name} 从左边移到右边: {bg.position.x}");
            }
            // 如果背景太靠右（超出右边界），移到最左边
            else if (distanceFromCamera > halfTotal)
            {
                float leftmostX = GetLeftmostX(layer.backgrounds);
                bg.position = new Vector3(leftmostX - width, bg.position.y, bg.position.z);
                bg.localScale = new Vector3(-bg.localScale.x, bg.localScale.y, bg.localScale.z); // 翻转背景以避免视觉重复
                Debug.Log($"背景 {bg.name} 从右边移到左边: {bg.position.x}");
            }
        }
    }

    /// <summary>
    /// 获取该层最右边背景的X坐标
    /// </summary>
    private float GetRightmostX(Transform[] backgrounds)
    {
        float max = float.MinValue;
        foreach (Transform bg in backgrounds)
        {
            if (bg.position.x > max) max = bg.position.x;
        }
        return max;
    }

    /// <summary>
    /// 获取该层最左边背景的X坐标
    /// </summary>
    private float GetLeftmostX(Transform[] backgrounds)
    {
        float min = float.MaxValue;
        foreach (Transform bg in backgrounds)
        {
            if (bg.position.x < min) min = bg.position.x;
        }
        return min;
    }

    /// <summary>
    /// 获取当前相机位置（优先 Virtual Camera）
    /// </summary>
    private Vector3 GetCameraPosition()
    {
        if (virtualCamera != null)
        {
            return virtualCamera.transform.position;
        }
        else if (mainCamera != null)
        {
            return mainCamera.transform.position;
        }
        return Vector3.zero;
    }
}
