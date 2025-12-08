using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 적 체력 및 내구력 UI (월드 스페이스 캔버스)
/// </summary>
public class EnemyHealthUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private EnemyController _enemy;
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _durabilitySlider;
    [SerializeField] private Image _healthFill;
    [SerializeField] private Image _durabilityFill;
    [SerializeField] private GameObject _executionIndicator;

    [Header("색상 설정")]
    [SerializeField] private Color _healthColor = Color.red;
    [SerializeField] private Color _durabilityColor = Color.yellow;
    [SerializeField] private Color _groggyColor = Color.magenta;

    [Header("위치 설정")]
    [SerializeField] private Vector3 _offset = new Vector3(0, 1.5f, 0);

    [Header("애니메이션")]
    [SerializeField] private float _smoothSpeed = 5f;

    private float _targetHealth;
    private float _targetDurability;
    private Health _health;
    private Durability _durability;
    private Transform _followTarget;

    private void Start()
    {
        // 적 자동 찾기 (자식 컴포넌트가 아닐 경우)
        if (_enemy == null)
        {
            _enemy = GetComponentInParent<EnemyController>();
        }

        if (_enemy != null)
        {
            _health = _enemy.Health;
            _durability = _enemy.Durability;
            _followTarget = _enemy.transform;

            // 이벤트 구독
            _health.OnHealthChanged += OnHealthChanged;
            _durability.OnDurabilityChanged += OnDurabilityChanged;
            _durability.OnGroggyStart += OnGroggyStart;
            _durability.OnGroggyEnd += OnGroggyEnd;

            // 초기화
            _targetHealth = _health.HealthPercent;
            _targetDurability = _durability.DurabilityPercent;

            if (_healthSlider != null) _healthSlider.value = _targetHealth;
            if (_durabilitySlider != null) _durabilitySlider.value = _targetDurability;
            if (_executionIndicator != null) _executionIndicator.SetActive(false);

            SetupColors();
        }
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnHealthChanged -= OnHealthChanged;
        }
        if (_durability != null)
        {
            _durability.OnDurabilityChanged -= OnDurabilityChanged;
            _durability.OnGroggyStart -= OnGroggyStart;
            _durability.OnGroggyEnd -= OnGroggyEnd;
        }
    }

    private void LateUpdate()
    {
        // 적 위치 따라가기
        if (_followTarget != null)
        {
            transform.position = _followTarget.position + _offset;
        }

        // 부드러운 슬라이더 애니메이션
        if (_healthSlider != null)
        {
            _healthSlider.value = Mathf.Lerp(_healthSlider.value, _targetHealth, Time.deltaTime * _smoothSpeed);
        }
        if (_durabilitySlider != null)
        {
            _durabilitySlider.value = Mathf.Lerp(_durabilitySlider.value, _targetDurability, Time.deltaTime * _smoothSpeed);
        }
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        _targetHealth = currentHealth / maxHealth;
    }

    private void OnDurabilityChanged(float currentDurability, float maxDurability)
    {
        _targetDurability = currentDurability / maxDurability;
    }

    private void OnGroggyStart()
    {
        // 그로기 상태 표시
        if (_durabilityFill != null)
        {
            _durabilityFill.color = _groggyColor;
        }
        if (_executionIndicator != null)
        {
            _executionIndicator.SetActive(true);
        }
    }

    private void OnGroggyEnd()
    {
        // 그로기 상태 해제
        if (_durabilityFill != null)
        {
            _durabilityFill.color = _durabilityColor;
        }
        if (_executionIndicator != null)
        {
            _executionIndicator.SetActive(false);
        }
    }

    private void SetupColors()
    {
        if (_healthFill != null) _healthFill.color = _healthColor;
        if (_durabilityFill != null) _durabilityFill.color = _durabilityColor;
    }
}
