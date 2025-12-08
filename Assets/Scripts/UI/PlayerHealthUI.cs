using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 체력 UI
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PlayerController _player;
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Image _healthFill;

    [Header("색상 설정")]
    [SerializeField] private Color _healthyColor = Color.green;
    [SerializeField] private Color _damagedColor = Color.yellow;
    [SerializeField] private Color _criticalColor = Color.red;
    [SerializeField] private float _criticalThreshold = 0.25f;
    [SerializeField] private float _damagedThreshold = 0.5f;

    [Header("애니메이션")]
    [SerializeField] private float _smoothSpeed = 5f;

    private float _targetHealth;
    private Health _health;

    private void Start()
    {
        // 플레이어 자동 찾기
        if (_player == null)
        {
            _player = FindFirstObjectByType<PlayerController>();
        }

        if (_player != null)
        {
            _health = _player.Health;
            _health.OnHealthChanged += OnHealthChanged;

            // 초기화
            _targetHealth = _health.HealthPercent;
            if (_healthSlider != null)
            {
                _healthSlider.value = _targetHealth;
            }
            UpdateColor(_targetHealth);
        }
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnHealthChanged -= OnHealthChanged;
        }
    }

    private void Update()
    {
        if (_healthSlider == null) return;

        // 부드러운 체력바 애니메이션
        _healthSlider.value = Mathf.Lerp(_healthSlider.value, _targetHealth, Time.deltaTime * _smoothSpeed);
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        _targetHealth = currentHealth / maxHealth;
        UpdateColor(_targetHealth);
    }

    private void UpdateColor(float healthPercent)
    {
        if (_healthFill == null) return;

        if (healthPercent <= _criticalThreshold)
        {
            _healthFill.color = _criticalColor;
        }
        else if (healthPercent <= _damagedThreshold)
        {
            _healthFill.color = _damagedColor;
        }
        else
        {
            _healthFill.color = _healthyColor;
        }
    }
}
