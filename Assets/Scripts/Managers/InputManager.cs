using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Unity Input System 래퍼 - 입력 관리 싱글톤
/// Player Actions: Move, Attack, Dodge(Shift), Guard(E)
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputSystem_Actions _inputActions;

    // 캐시된 입력 값
    public Vector2 MoveInput { get; private set; }
    public bool AttackPressed { get; private set; }        // 좌클릭 약공격
    public bool HeavyAttackPressed { get; private set; }   // 우클릭 강공격
    public bool DodgePressed { get; private set; }  // Shift로 Dodge
    public bool GuardHeld { get; private set; }     // E키로 Guard
    public bool JumpPressed { get; private set; }   // Space로 Jump

    private void Awake()
    {
        // Singleton 패턴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Input Actions 초기화
        _inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();

        // 버튼 입력 이벤트 구독
        _inputActions.Player.Attack.performed += OnAttackPerformed;
        // HeavyAttack 액션은 Unity가 InputSystem_Actions.cs 재생성 후 사용 가능
        // 현재는 수동으로 바인딩 (우클릭 직접 체크)
        _inputActions.Player.Jump.performed += OnJumpPerformed;
        // Dodge 액션 (inputactions에서 Dodge로 이름 변경됨, 재생성 전까지 Sprint 사용)
        _inputActions.Player.Sprint.performed += OnDodgePerformed;
    }

    private void OnDisable()
    {
        _inputActions.Player.Attack.performed -= OnAttackPerformed;
        _inputActions.Player.Jump.performed -= OnJumpPerformed;
        _inputActions.Player.Sprint.performed -= OnDodgePerformed;

        _inputActions.Disable();
    }

    private void Update()
    {
        // 매 프레임 입력 값 업데이트
        MoveInput = _inputActions.Player.Move.ReadValue<Vector2>();
        GuardHeld = _inputActions.Player.Interact.IsPressed();  // E키로 Guard

        // HeavyAttack: Unity가 InputSystem_Actions.cs 재생성 전까지 마우스 직접 체크
        if (UnityEngine.InputSystem.Mouse.current != null &&
            UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame)
        {
            HeavyAttackPressed = true;
        }
    }

    private void LateUpdate()
    {
        // 상태 머신이 입력을 읽은 후 프레임 끝에 리셋
        if (AttackPressed)
            AttackPressed = false;
        if (HeavyAttackPressed)
            HeavyAttackPressed = false;
        if (JumpPressed)
            JumpPressed = false;
        if (DodgePressed)
            DodgePressed = false;
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        AttackPressed = true;
    }

    private void OnHeavyAttackPerformed(InputAction.CallbackContext context)
    {
        HeavyAttackPressed = true;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        JumpPressed = true;
    }

    private void OnDodgePerformed(InputAction.CallbackContext context)
    {
        DodgePressed = true;
    }

    // 외부에서 버튼 입력 소비 (사용됨을 표시)
    public void ConsumeAttackInput()
    {
        AttackPressed = false;
    }

    public void ConsumeHeavyAttackInput()
    {
        HeavyAttackPressed = false;
    }

    public void ConsumeJumpInput()
    {
        JumpPressed = false;
    }

    public void ConsumeDodgeInput()
    {
        DodgePressed = false;
    }
}
