using UnityEngine;

/// <summary>
/// 控制摄像机的宽高比，确保在不同分辨率下保持一致的显示效果
/// </summary>
public class CameraAspectController : MonoBehaviour
{
    [Header("目标宽高比设置")]
    [Tooltip("目标宽度")]
    public float targetWidth = 16f;
    [Tooltip("目标高度")]
    public float targetHeight = 9f;

    [Header("显示设置")]
    [Tooltip("黑边颜色")]
    public Color letterboxColor = Color.black;

    private Camera mainCamera;
    private Camera letterboxCamera;
    private float targetAspect;

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        targetAspect = targetWidth / targetHeight;
        CreateLetterboxCamera();
    }

    void Start()
    {
        UpdateCameraAspect();
    }

    void Update()
    {
        // 检测分辨率变化并更新
        UpdateCameraAspect();
    }

    /// <summary>
    /// 创建用于绘制黑边的摄像机
    /// </summary>
    void CreateLetterboxCamera()
    {
        // 创建一个新的摄像机用于绘制黑边背景
        GameObject letterboxObj = new GameObject("LetterboxCamera");
        letterboxObj.transform.SetParent(transform);
        letterboxCamera = letterboxObj.AddComponent<Camera>();

        letterboxCamera.depth = mainCamera.depth - 1;
        letterboxCamera.clearFlags = CameraClearFlags.SolidColor;
        letterboxCamera.backgroundColor = letterboxColor;
        letterboxCamera.cullingMask = 0; // 不渲染任何物体
        letterboxCamera.orthographic = true;
    }

    /// <summary>
    /// 更新摄像机宽高比
    /// </summary>
    void UpdateCameraAspect()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = new Rect();

        if (scaleHeight < 1.0f)
        {
            // 添加上下黑边 (Letterbox)
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            // 添加左右黑边 (Pillarbox)
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        mainCamera.rect = rect;
    }

    /// <summary>
    /// 设置新的目标宽高比
    /// </summary>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    public void SetTargetAspect(float width, float height)
    {
        targetWidth = width;
        targetHeight = height;
        targetAspect = targetWidth / targetHeight;
        UpdateCameraAspect();
    }

    /// <summary>
    /// 设置黑边颜色
    /// </summary>
    /// <param name="color">颜色</param>
    public void SetLetterboxColor(Color color)
    {
        letterboxColor = color;
        if (letterboxCamera != null)
        {
            letterboxCamera.backgroundColor = color;
        }
    }

    /// <summary>
    /// 重置为全屏（无黑边）
    /// </summary>
    public void ResetToFullScreen()
    {
        mainCamera.rect = new Rect(0, 0, 1, 1);
    }

    void OnDestroy()
    {
        if (letterboxCamera != null)
        {
            Destroy(letterboxCamera.gameObject);
        }
    }
}
