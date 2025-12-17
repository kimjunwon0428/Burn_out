using UnityEngine;
using System;

/// <summary>
/// 처형 시스템 - 그로기 상태의 적을 처형
/// </summary>
public class ExecutionSystem : MonoBehaviour
{
    public static ExecutionSystem Instance { get; private set; }

    [Header("처형 설정")]
    [SerializeField] private float _executionRange = 2f;      // 처형 가능 범위
    [SerializeField] private float _executionDuration = 1f;   // 처형 애니메이션 시간
    [SerializeField] private LayerMask _enemyLayer;

    // 플레이어 애니메이터 참조
    private Animator _playerAnimator;

    // 이벤트
    public event Action<EnemyController> OnExecutionStart;
    public event Action<EnemyController> OnExecutionComplete;

    // 상태
    private bool _isExecuting;
    private EnemyController _executionTarget;
    private float _executionTimer;

    // 플레이어 참조 (처형 중 이동 잠금용)
    private PlayerMovement _playerMovement;

    public bool IsExecuting => _isExecuting;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _enemyLayer = LayerMask.GetMask("Enemy");
    }

    private void Update()
    {
        if (_isExecuting)
        {
            UpdateExecution();
        }
    }

    /// <summary>
    /// 처형 시도 (플레이어에서 호출)
    /// </summary>
    /// <param name="playerPosition">플레이어 위치</param>
    /// <returns>처형 시작 성공 여부</returns>
    public bool TryExecute(Vector2 playerPosition)
    {
        if (_isExecuting) return false;

        // 범위 내 그로기 상태 적 찾기
        EnemyController groggyEnemy = FindGroggyEnemyInRange(playerPosition);

        if (groggyEnemy != null)
        {
            StartExecution(groggyEnemy);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 범위 내 그로기 상태 적 찾기
    /// </summary>
    private EnemyController FindGroggyEnemyInRange(Vector2 playerPosition)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(playerPosition, _executionRange, _enemyLayer);

        float closestDistance = float.MaxValue;
        EnemyController closestGroggy = null;

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<EnemyController>();
            if (enemy != null && enemy.Durability.CanBeExecuted())
            {
                float dist = Vector2.Distance(playerPosition, hit.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestGroggy = enemy;
                }
            }
        }

        return closestGroggy;
    }

    /// <summary>
    /// 플레이어 애니메이터 설정 (PlayerController에서 호출)
    /// </summary>
    public void SetPlayerAnimator(Animator animator)
    {
        _playerAnimator = animator;
    }

    /// <summary>
    /// 처형 시작
    /// </summary>
    private void StartExecution(EnemyController target)
    {
        _isExecuting = true;
        _executionTarget = target;
        _executionTimer = 0f;

        Debug.Log($"Execution started on {target.gameObject.name}!");
        OnExecutionStart?.Invoke(target);

        // 처형 애니메이션 트리거
        _playerAnimator?.SetTrigger("ExecutionTrigger");

        // 플레이어 이동 잠금
        if (_playerMovement == null)
        {
            _playerMovement = FindObjectOfType<PlayerMovement>();
        }
        _playerMovement?.LockMovement();

        // 적 고정 (처형 중 이동 불가)
        target.FreezeForExecution();
    }

    /// <summary>
    /// 처형 진행 업데이트
    /// </summary>
    private void UpdateExecution()
    {
        _executionTimer += Time.deltaTime;

        if (_executionTimer >= _executionDuration)
        {
            CompleteExecution();
        }
    }

    /// <summary>
    /// 처형 완료
    /// </summary>
    private void CompleteExecution()
    {
        if (_executionTarget != null)
        {
            // 그로기 상태에서 처형 처리
            var groggyState = _executionTarget.StateMachine.CurrentState as EnemyGroggyState;
            if (groggyState != null)
            {
                groggyState.OnExecuted();
            }
            else
            {
                // 그로기 상태가 아니면 직접 즉사 처리
                _executionTarget.Health.TakeDamage(float.MaxValue);
            }

            Debug.Log($"Execution completed on {_executionTarget.gameObject.name}!");
            OnExecutionComplete?.Invoke(_executionTarget);
        }

        // 플레이어 이동 잠금 해제
        _playerMovement?.UnlockMovement();

        _isExecuting = false;
        _executionTarget = null;
    }

    /// <summary>
    /// 처형 강제 취소
    /// </summary>
    public void CancelExecution()
    {
        if (_isExecuting)
        {
            Debug.Log("Execution cancelled!");

            // 플레이어 이동 잠금 해제
            _playerMovement?.UnlockMovement();

            // 적 고정 해제
            _executionTarget?.UnfreezeFromExecution();

            _isExecuting = false;
            _executionTarget = null;
        }
    }

    /// <summary>
    /// 처형 가능한 적이 범위 내에 있는지 확인
    /// </summary>
    public bool HasExecutableEnemyInRange(Vector2 playerPosition)
    {
        return FindGroggyEnemyInRange(playerPosition) != null;
    }

    /// <summary>
    /// 기즈모로 처형 범위 표시
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _executionRange);
    }
}
