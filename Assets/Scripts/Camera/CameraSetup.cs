using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Sets up the Cinemachine camera to follow the player and configures bounds.
/// Attach this to the CinemachineCamera GameObject.
/// </summary>
public class CameraSetup : MonoBehaviour
{
    [Header("Follow Target")]
    [Tooltip("Tag of the object to follow (default: Player)")]
    public string followTargetTag = "Player";

    [Header("Camera Bounds")]
    [Tooltip("Reference to the camera bounds collider")]
    public Collider2D boundsCollider;

    [Tooltip("If true, auto-configure bounds from Background sprite")]
    public bool autoConfigureBounds = true;

    [Tooltip("Tag of the background object to use for auto-bounds")]
    public string backgroundTag = "Untagged";

    [Tooltip("Name of the background object to use for auto-bounds")]
    public string backgroundName = "Background";

    [Header("Bounds Settings")]
    [Tooltip("Half-width of camera bounds (used if not auto-configured)")]
    public float boundsHalfWidth = 30f;

    [Tooltip("Half-height of camera bounds (used if not auto-configured)")]
    public float boundsHalfHeight = 10f;

    private CinemachineCamera _cinemachineCamera;
    private CinemachineConfiner2D _confiner;

    void Start()
    {
        _cinemachineCamera = GetComponent<CinemachineCamera>();
        _confiner = GetComponent<CinemachineConfiner2D>();

        SetupFollowTarget();
        SetupBounds();
    }

    void SetupFollowTarget()
    {
        if (_cinemachineCamera == null) return;

        // Find player by tag
        GameObject player = GameObject.FindGameObjectWithTag(followTargetTag);
        if (player != null)
        {
            _cinemachineCamera.Follow = player.transform;
            Debug.Log($"[CameraSetup] Camera now following: {player.name}");
        }
        else
        {
            Debug.LogWarning($"[CameraSetup] Could not find object with tag '{followTargetTag}'");
        }
    }

    void SetupBounds()
    {
        if (_confiner == null) return;

        if (boundsCollider != null)
        {
            _confiner.BoundingShape2D = boundsCollider;
            Debug.Log($"[CameraSetup] Camera bounds set to: {boundsCollider.name}");
            return;
        }

        if (autoConfigureBounds)
        {
            ConfigureBoundsFromBackground();
        }
    }

    void ConfigureBoundsFromBackground()
    {
        // Find background object
        GameObject background = GameObject.Find(backgroundName);
        if (background == null)
        {
            Debug.LogWarning($"[CameraSetup] Could not find background object '{backgroundName}'");
            return;
        }

        SpriteRenderer spriteRenderer = background.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogWarning("[CameraSetup] Background has no SpriteRenderer or sprite");
            return;
        }

        // Calculate bounds from sprite
        Bounds spriteBounds = spriteRenderer.bounds;
        float halfWidth = spriteBounds.extents.x;
        float halfHeight = spriteBounds.extents.y;

        // Find or create camera bounds object
        GameObject boundsObj = GameObject.Find("CameraBounds");
        if (boundsObj == null)
        {
            boundsObj = new GameObject("CameraBounds");
        }

        // Get or add PolygonCollider2D
        PolygonCollider2D polyCollider = boundsObj.GetComponent<PolygonCollider2D>();
        if (polyCollider == null)
        {
            polyCollider = boundsObj.AddComponent<PolygonCollider2D>();
        }

        // Configure as trigger
        polyCollider.isTrigger = true;

        // Set polygon points to match background bounds
        Vector2[] points = new Vector2[4]
        {
            new Vector2(-halfWidth, -halfHeight),
            new Vector2(-halfWidth, halfHeight),
            new Vector2(halfWidth, halfHeight),
            new Vector2(halfWidth, -halfHeight)
        };
        polyCollider.SetPath(0, points);

        // Assign to confiner
        _confiner.BoundingShape2D = polyCollider;
        Debug.Log($"[CameraSetup] Auto-configured bounds: {halfWidth * 2}x{halfHeight * 2}");
    }
}