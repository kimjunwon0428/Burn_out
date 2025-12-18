using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 전투 시스템 디버그 UI - 현재 상태 및 입력 표시
/// </summary>
public class CombatDebugUI : MonoBehaviour
{
    [SerializeField] private PlayerController _player;
    [SerializeField] private bool _showDebug = true;

    private GUIStyle _labelStyle;
    private GUIStyle _healthStyle;
    private GUIStyle _warningStyle;

    private void Start()
    {
        // GUI 스타일 설정
        _labelStyle = new GUIStyle();
        _labelStyle.fontSize = 16;
        _labelStyle.normal.textColor = Color.white;
        _labelStyle.padding = new RectOffset(10, 10, 5, 5);

        _healthStyle = new GUIStyle(_labelStyle);
        _healthStyle.normal.textColor = Color.green;

        _warningStyle = new GUIStyle(_labelStyle);
        _warningStyle.normal.textColor = Color.red;

        // 플레이어 자동 찾기
        if (_player == null)
            _player = FindFirstObjectByType<PlayerController>();
    }

    private void OnGUI()
    {
        if (!_showDebug || _player == null)
            return;

        DrawPlayerInfo();
        DrawEnemyInfo();
    }

    private void DrawPlayerInfo()
    {
        // 좌상단에 플레이어 정보 표시
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        GUILayout.BeginVertical("box");

        GUILayout.Label("=== Player Debug ===", _labelStyle);
        GUILayout.Space(5);

        // 현재 상태
        string stateName = _player.StateMachine.CurrentState != null
            ? _player.StateMachine.CurrentState.GetType().Name
            : "None";
        GUILayout.Label($"State: {stateName}", _labelStyle);

        // 체력 정보
        if (_player.Health != null)
        {
            float healthPercent = _player.Health.HealthPercent * 100f;
            var style = healthPercent > 25 ? _healthStyle : _warningStyle;
            GUILayout.Label($"Health: {_player.Health.CurrentHealth:F0}/{_player.Health.MaxHealth:F0} ({healthPercent:F0}%)", style);
        }

        // 특수 자원 표시
        if (PlayerStats.Instance != null)
        {
            float current = PlayerStats.Instance.CurrentSpecialResource;
            float max = PlayerStats.Instance.MaxSpecialResource;
            float percent = PlayerStats.Instance.SpecialResourcePercent * 100f;
            bool canUseSpecial = current >= 50f;

            var style = canUseSpecial ? _healthStyle : _labelStyle;
            string canUseText = canUseSpecial ? " [READY]" : "";
            GUILayout.Label($"Special: {current:F0}/{max:F0} ({percent:F0}%){canUseText}", style);
        }

        GUILayout.Space(5);

        // 입력 정보
        if (InputManager.Instance != null)
        {
            Vector2 move = InputManager.Instance.MoveInput;
            GUILayout.Label($"Move: ({move.x:F2}, {move.y:F2})", _labelStyle);
            GUILayout.Label($"Attack: {InputManager.Instance.AttackPressed}", _labelStyle);
            GUILayout.Label($"Guard: {InputManager.Instance.GuardHeld}", _labelStyle);
            GUILayout.Label($"Dodge: {InputManager.Instance.DodgePressed}", _labelStyle);
        }

        // 가드/닷지 상태 특수 정보
        var currentState = _player.StateMachine.CurrentState;
        if (currentState is GuardState guardState)
        {
            string guardType = guardState.IsInPerfectGuardWindow ? "PERFECT WINDOW" : "Normal";
            GUILayout.Label($"Guard: {guardType}", _warningStyle);
        }
        else if (currentState is DodgeState dodgeState)
        {
            string invincible = dodgeState.IsInvincible ? "INVINCIBLE" : "Vulnerable";
            GUILayout.Label($"Dodge: {invincible}", _warningStyle);
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawEnemyInfo()
    {
        // 좌하단에 적 정보 표시
        var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        if (enemies.Length == 0) return;

        GUILayout.BeginArea(new Rect(10, Screen.height - 250, 350, 240));
        GUILayout.BeginVertical("box");

        GUILayout.Label($"=== Enemies ({enemies.Length}) ===", _labelStyle);
        GUILayout.Space(5);

        int displayCount = Mathf.Min(enemies.Length, 5);  // 최대 5마리까지 표시
        for (int i = 0; i < displayCount; i++)
        {
            var enemy = enemies[i];
            if (enemy == null) continue;

            string stateName = enemy.StateMachine.CurrentState?.GetType().Name ?? "None";
            float health = enemy.Health.CurrentHealth;
            float durability = enemy.Durability.CurrentDurability;
            bool isGroggy = enemy.Durability.IsGroggy;

            var style = isGroggy ? _warningStyle : _labelStyle;
            string groggyText = isGroggy ? " [GROGGY!]" : "";

            GUILayout.Label($"{enemy.gameObject.name}: {stateName}{groggyText}", style);
            GUILayout.Label($"  HP: {health:F0} | Durability: {durability:F0}", _labelStyle);
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void Update()
    {
        // F1 키로 디버그 표시 토글 (New Input System 사용)
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            _showDebug = !_showDebug;
        }
    }
}
