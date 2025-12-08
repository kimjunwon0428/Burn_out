using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Linq;

public class PlayerAnimatorSetup
{
    [MenuItem("Tools/Setup Player Animator Transitions")]
    public static void SetupTransitions()
    {
        string controllerPath = "Assets/Animations/Player.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        
        if (controller == null)
        {
            Debug.LogError($"Player.controller not found at {controllerPath}");
            return;
        }

        var rootStateMachine = controller.layers[0].stateMachine;
        
        // 상태들 찾기
        var states = rootStateMachine.states;
        AnimatorState idle = FindState(states, "Idle");
        AnimatorState run = FindState(states, "Run");
        AnimatorState jump = FindState(states, "Jump");
        AnimatorState dash = FindState(states, "Dash");
        AnimatorState attack = FindState(states, "Attack");
        AnimatorState guard = FindState(states, "Guard");
        AnimatorState damage = FindState(states, "Damage");

        if (idle == null || run == null)
        {
            Debug.LogError("Required states (Idle, Run) not found!");
            return;
        }

        // 기존 전환 제거 (중복 방지)
        ClearTransitions(idle);
        ClearTransitions(run);
        ClearTransitions(jump);
        ClearTransitions(dash);
        ClearTransitions(attack);
        ClearTransitions(guard);
        ClearTransitions(damage);
        rootStateMachine.anyStateTransitions = new AnimatorStateTransition[0];

        // === 이동 전환 ===
        // Idle -> Run (Speed > 0.1)
        var idleToRun = idle.AddTransition(run);
        idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToRun.hasExitTime = false;
        idleToRun.duration = 0.1f;

        // Run -> Idle (Speed < 0.1)
        var runToIdle = run.AddTransition(idle);
        runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        runToIdle.hasExitTime = false;
        runToIdle.duration = 0.1f;

        // === 점프 전환 ===
        if (jump != null)
        {
            // Idle -> Jump (IsGrounded = false)
            var idleToJump = idle.AddTransition(jump);
            idleToJump.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");
            idleToJump.hasExitTime = false;
            idleToJump.duration = 0.05f;

            // Run -> Jump (IsGrounded = false)
            var runToJump = run.AddTransition(jump);
            runToJump.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");
            runToJump.hasExitTime = false;
            runToJump.duration = 0.05f;

            // Jump -> Idle (IsGrounded = true)
            var jumpToIdle = jump.AddTransition(idle);
            jumpToIdle.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
            jumpToIdle.hasExitTime = false;
            jumpToIdle.duration = 0.1f;
        }

        // === Any State 전환 ===
        // Any -> Attack (AttackTrigger)
        if (attack != null)
        {
            var anyToAttack = rootStateMachine.AddAnyStateTransition(attack);
            anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "AttackTrigger");
            anyToAttack.hasExitTime = false;
            anyToAttack.duration = 0.05f;

            // Attack -> Idle (Exit Time)
            var attackToIdle = attack.AddTransition(idle);
            attackToIdle.hasExitTime = true;
            attackToIdle.exitTime = 0.9f;
            attackToIdle.duration = 0.1f;
        }

        // Any -> Dash (DodgeTrigger)
        if (dash != null)
        {
            var anyToDash = rootStateMachine.AddAnyStateTransition(dash);
            anyToDash.AddCondition(AnimatorConditionMode.If, 0, "DodgeTrigger");
            anyToDash.hasExitTime = false;
            anyToDash.duration = 0.05f;

            // Dash -> Idle (Exit Time)
            var dashToIdle = dash.AddTransition(idle);
            dashToIdle.hasExitTime = true;
            dashToIdle.exitTime = 0.9f;
            dashToIdle.duration = 0.1f;
        }

        // Any -> Guard (IsGuarding = true)
        if (guard != null)
        {
            var anyToGuard = rootStateMachine.AddAnyStateTransition(guard);
            anyToGuard.AddCondition(AnimatorConditionMode.If, 0, "IsGuarding");
            anyToGuard.hasExitTime = false;
            anyToGuard.duration = 0.05f;

            // Guard -> Idle (IsGuarding = false)
            var guardToIdle = guard.AddTransition(idle);
            guardToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGuarding");
            guardToIdle.hasExitTime = false;
            guardToIdle.duration = 0.1f;
        }

        // Any -> Damage (HitTrigger)
        if (damage != null)
        {
            var anyToDamage = rootStateMachine.AddAnyStateTransition(damage);
            anyToDamage.AddCondition(AnimatorConditionMode.If, 0, "HitTrigger");
            anyToDamage.hasExitTime = false;
            anyToDamage.duration = 0.05f;

            // Damage -> Idle (Exit Time)
            var damageToIdle = damage.AddTransition(idle);
            damageToIdle.hasExitTime = true;
            damageToIdle.exitTime = 0.9f;
            damageToIdle.duration = 0.1f;
        }

        // 저장
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        Debug.Log("Player Animator 전환 설정 완료!");
    }

    private static AnimatorState FindState(ChildAnimatorState[] states, string name)
    {
        foreach (var state in states)
        {
            if (state.state.name == name)
                return state.state;
        }
        return null;
    }

    private static void ClearTransitions(AnimatorState state)
    {
        if (state != null)
        {
            state.transitions = new AnimatorStateTransition[0];
        }
    }
}
