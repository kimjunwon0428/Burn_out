using UnityEngine;

/// <summary>
/// 훈련용 더미 UI - 우측 상단에 상태 표시
/// OnGUI 기반으로 빠른 구현
/// </summary>
public class TrainingDummyUI : MonoBehaviour
{
    [Header("UI 설정")]
    [SerializeField] private bool _showUI = true;
    [SerializeField] private int _fontSize = 18;
    [SerializeField] private Color _textColor = Color.white;
    [SerializeField] private Color _damageColor = Color.red;
    [SerializeField] private Color _durabilityColor = Color.yellow;

    private TrainingDummy _dummy;
    private GUIStyle _labelStyle;
    private GUIStyle _damageStyle;
    private GUIStyle _durabilityStyle;

    // 피해 표시 타이머
    private float _damageDisplayTimer;
    private float _durabilityDisplayTimer;
    private const float DISPLAY_DURATION = 2f;

    private void Awake()
    {
        _dummy = GetComponent<TrainingDummy>();
    }

    private void Start()
    {
        if (_dummy != null)
        {
            _dummy.OnDummyHit += OnDummyHit;
        }
    }

    private void OnDestroy()
    {
        if (_dummy != null)
        {
            _dummy.OnDummyHit -= OnDummyHit;
        }
    }

    private void OnDummyHit()
    {
        _damageDisplayTimer = DISPLAY_DURATION;
        _durabilityDisplayTimer = DISPLAY_DURATION;
    }

    private void Update()
    {
        if (_damageDisplayTimer > 0)
        {
            _damageDisplayTimer -= Time.deltaTime;
        }
        if (_durabilityDisplayTimer > 0)
        {
            _durabilityDisplayTimer -= Time.deltaTime;
        }
    }

    private void InitStyles()
    {
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = _fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperRight
            };
            _labelStyle.normal.textColor = _textColor;
        }

        if (_damageStyle == null)
        {
            _damageStyle = new GUIStyle(_labelStyle);
            _damageStyle.normal.textColor = _damageColor;
        }

        if (_durabilityStyle == null)
        {
            _durabilityStyle = new GUIStyle(_labelStyle);
            _durabilityStyle.normal.textColor = _durabilityColor;
        }
    }

    private void OnGUI()
    {
        if (!_showUI || _dummy == null) return;

        InitStyles();

        float padding = 10f;
        float lineHeight = _fontSize + 5f;
        float boxWidth = 300f;
        float boxHeight = lineHeight * 4 + padding * 2;

        // 우측 상단 위치
        Rect boxRect = new Rect(
            Screen.width - boxWidth - padding,
            padding,
            boxWidth,
            boxHeight
        );

        // 반투명 배경
        GUI.Box(boxRect, "");

        float y = boxRect.y + padding;
        float x = boxRect.x;

        // 제목
        GUI.Label(new Rect(x, y, boxWidth - padding, lineHeight),
            "<b>[ Training Dummy ]</b>", _labelStyle);
        y += lineHeight;

        // 체력
        string healthText = $"체력: {_dummy.CurrentHealth:F0} / {_dummy.MaxHealth:F0}";
        GUI.Label(new Rect(x, y, boxWidth - padding, lineHeight),
            healthText, _labelStyle);
        y += lineHeight;

        // 마지막 피해량
        if (_damageDisplayTimer > 0 && _dummy.LastDamageTaken > 0)
        {
            string damageText = $"피해량: {_dummy.LastDamageTaken:F1}";
            GUI.Label(new Rect(x, y, boxWidth - padding, lineHeight),
                damageText, _damageStyle);
        }
        else
        {
            GUI.Label(new Rect(x, y, boxWidth - padding, lineHeight),
                "피해량: -", _labelStyle);
        }
        y += lineHeight;

        // 마지막 강인도 피해량
        if (_durabilityDisplayTimer > 0 && _dummy.LastDurabilityDamage > 0)
        {
            string durabilityText = $"강인도 피해: {_dummy.LastDurabilityDamage:F1}";
            GUI.Label(new Rect(x, y, boxWidth - padding, lineHeight),
                durabilityText, _durabilityStyle);
        }
        else
        {
            string durabilityText = $"강인도: {_dummy.CurrentDurability:F0} / {_dummy.MaxDurability:F0}";
            GUI.Label(new Rect(x, y, boxWidth - padding, lineHeight),
                durabilityText, _labelStyle);
        }
    }
}
