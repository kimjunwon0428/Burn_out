using UnityEngine;

/// <summary>
/// 플레이어 2D 이동 및 스프라이트 플리핑 처리
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정 (기본값 - PlayerStats로 오버라이드)")]
    [SerializeField] private float _baseMoveSpeed = 5f;

    private Rigidbody2D _rb;
    private Animator _animator;
    private Vector2 _moveInput;
    private int _facingDirection = 1; // 1 = 오른쪽, -1 = 왼쪽
    private bool _isMovementLocked = false;

    public int FacingDirection => _facingDirection;

    /// <summary>
    /// 현재 이동 속도 (스탯 적용)
    /// </summary>
    private float MoveSpeed => PlayerStats.Instance != null
        ? PlayerStats.Instance.GetStat(StatType.MoveSpeed)
        : _baseMoveSpeed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    public void SetMoveInput(Vector2 input)
    {
        _moveInput = input;

        // 스프라이트 방향 업데이트
        UpdateFacing();
    }

    public void FixedUpdateMovement()
    {
        // 이동 잠금 시 velocity 설정 건너뛰기 (Dodge 등에서 직접 velocity 제어)
        if (_isMovementLocked) return;

        // 이동 속도 계산 (스탯 기반)
        float currentSpeed = MoveSpeed;

        // 수평 이동만 설정 (Y축은 물리 엔진에 맡김)
        float targetVelocityX = _moveInput.x * currentSpeed;
        _rb.linearVelocityX = targetVelocityX;

        // 이동 애니메이션 파라미터 업데이트
        if (_animator != null)
        {
            _animator.SetFloat("Speed", Mathf.Abs(_rb.linearVelocityX));
        }
    }

    private void UpdateFacing()
    {
        // 이동 입력 기반 스프라이트 플리핑
        if (_moveInput.x > 0.1f)
        {
            _facingDirection = 1;
            transform.localScale = new Vector3(-1, 1, 1);  // 오른쪽으로 이동 시 스프라이트 플립
        }
        else if (_moveInput.x < -0.1f)
        {
            _facingDirection = -1;
            transform.localScale = new Vector3(1, 1, 1);  // 왼쪽으로 이동 시 스프라이트 원상태
        }
    }

    // 외부에서 이동 잠금 (공격/가드/회피 중)
    public void LockMovement()
    {
        _isMovementLocked = true;
        _moveInput = Vector2.zero;
    }

    // 이동 잠금 해제
    public void UnlockMovement()
    {
        _isMovementLocked = false;
    }
}
