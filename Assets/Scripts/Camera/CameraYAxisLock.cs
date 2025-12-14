using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Locks the camera's Y position and clamps X position within background bounds.
/// Attach this to the Main Camera (with CinemachineBrain).
/// </summary>
public class CameraYAxisLock : MonoBehaviour
{
    [Header("Y Axis Lock")]
    [Tooltip("The fixed Y position for the camera")]
    public float fixedYPosition = 0f;

    [Header("X Axis Bounds")]
    [Tooltip("Enable X axis clamping to background bounds")]
    public bool clampXAxis = true;

    [Tooltip("Background object to use for bounds (optional - auto-finds 'Background' if empty)")]
    public SpriteRenderer backgroundSprite;

    [Tooltip("Minimum X position (auto-calculated from background if not set)")]
    public float minX = -30f;

    [Tooltip("Maximum X position (auto-calculated from background if not set)")]
    public float maxX = 30f;

    private CinemachineBrain _brain;
    private Camera _camera;

    void Awake()
    {
        _brain = GetComponent<CinemachineBrain>();
        _camera = GetComponent<Camera>();
    }

    void Start()
    {
        CalculateBoundsFromBackground();
    }

    void CalculateBoundsFromBackground()
    {
        if (backgroundSprite == null)
        {
            GameObject bg = GameObject.Find("Background");
            if (bg != null)
            {
                backgroundSprite = bg.GetComponent<SpriteRenderer>();
            }
        }

        if (backgroundSprite != null && _camera != null)
        {
            Bounds bgBounds = backgroundSprite.bounds;
            float cameraHalfWidth = _camera.orthographicSize * _camera.aspect;

            minX = bgBounds.min.x + cameraHalfWidth;
            maxX = bgBounds.max.x - cameraHalfWidth;

            Debug.Log($"[CameraYAxisLock] Bounds set: X({minX:F2} to {maxX:F2})");
        }
    }

    void OnEnable()
    {
        if (_brain != null)
        {
            CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
        }
    }

    void OnDisable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
    }

    void OnCameraUpdated(CinemachineBrain brain)
    {
        if (brain == _brain)
        {
            Vector3 pos = transform.position;

            // Lock Y axis
            pos.y = fixedYPosition;

            // Clamp X axis to background bounds
            if (clampXAxis)
            {
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
            }

            transform.position = pos;
        }
    }
}