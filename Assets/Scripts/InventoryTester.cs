using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 인벤토리 테스트용 스크립트
/// 키보드 입력으로 아이템 추가/제거 테스트
/// </summary>
public class InventoryTester : MonoBehaviour
{
    [Header("테스트 아이템")]
    [SerializeField] private ItemData testItem;  // Inspector에서 할당

    [Header("테스트 설정")]
    [SerializeField] private bool showInstructions = true;

    private Keyboard _keyboard;

    private void Start()
    {
        // Keyboard 참조 가져오기
        _keyboard = Keyboard.current;

        if (_keyboard == null)
        {
            Debug.LogWarning("InventoryTester: 키보드를 찾을 수 없습니다!");
        }

        if (showInstructions)
        {
            Debug.Log("=== 인벤토리 테스트 키 ===");
            Debug.Log("1: 아이템 1개 추가");
            Debug.Log("2: 카드키 1개 추가");
            Debug.Log("3: 슬롯 0 아이템 사용");
            Debug.Log("4: 카드키 1개 사용");
            Debug.Log("5: 인벤토리 전체 비우기");
            Debug.Log("Q: Player 체력 10 감소");
            Debug.Log("W: Player 체력 완전 회복");
            Debug.Log("========================");
        }
    }

    private void Update()
    {
        if (_keyboard == null)
            return;

        // 아이템 추가 테스트
        if (_keyboard.digit1Key.wasPressedThisFrame)
        {
            if (testItem != null)
            {
                bool success = InventoryManager.Instance?.AddItem(testItem, 1) ?? false;
                if (success)
                {
                    Debug.Log($"✓ {testItem.ItemName} 추가 성공!");
                }
                else
                {
                    Debug.LogWarning("✗ 인벤토리가 가득 찼습니다!");
                }
            }
            else
            {
                Debug.LogError("TestItem이 설정되지 않았습니다! Inspector에서 ItemData를 드래그하세요.");
            }
        }

        // 카드키 추가
        if (_keyboard.digit2Key.wasPressedThisFrame)
        {
            InventoryManager.Instance?.AddCardKey(1);
            Debug.Log("✓ 카드키 1개 추가!");
        }

        // 슬롯 0 아이템 사용
        if (_keyboard.digit3Key.wasPressedThisFrame)
        {
            bool success = InventoryManager.Instance?.UseItem(0) ?? false;
            if (success)
            {
                Debug.Log("✓ 슬롯 0 아이템 사용!");
            }
            else
            {
                Debug.LogWarning("✗ 슬롯 0이 비어있습니다!");
            }
        }

        // 카드키 사용
        if (_keyboard.digit4Key.wasPressedThisFrame)
        {
            bool success = InventoryManager.Instance?.UseCardKey(1) ?? false;
            if (success)
            {
                Debug.Log("✓ 카드키 1개 사용!");
            }
            else
            {
                Debug.LogWarning("✗ 카드키가 없습니다!");
            }
        }

        // 인벤토리 비우기
        if (_keyboard.digit5Key.wasPressedThisFrame)
        {
            InventoryManager.Instance?.ClearInventory();
            Debug.Log("✓ 인벤토리를 비웠습니다!");
        }

        // 체력 테스트
        if (_keyboard.qKey.wasPressedThisFrame)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null && player.Health != null)
            {
                player.Health.TakeDamage(10);
                Debug.Log($"✓ Player 체력 10 감소! (현재: {player.Health.CurrentHealth}/{player.Health.MaxHealth})");
            }
            else
            {
                Debug.LogWarning("✗ PlayerController를 찾을 수 없습니다!");
            }
        }

        if (_keyboard.wKey.wasPressedThisFrame)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null && player.Health != null)
            {
                player.Health.FullHeal();
                Debug.Log($"✓ Player 체력 완전 회복! (현재: {player.Health.CurrentHealth}/{player.Health.MaxHealth})");
            }
            else
            {
                Debug.LogWarning("✗ PlayerController를 찾을 수 없습니다!");
            }
        }
    }

    private void OnGUI()
    {
        if (!showInstructions)
            return;

        // 화면 좌측 상단에 키 가이드 표시
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        style.padding = new RectOffset(10, 10, 10, 10);

        string instructions =
            "=== 테스트 키 ===\n" +
            "1: 아이템 추가\n" +
            "2: 카드키 추가\n" +
            "3: 슬롯 0 사용\n" +
            "4: 카드키 사용\n" +
            "5: 인벤토리 비우기\n" +
            "Q: 체력 -10\n" +
            "W: 체력 회복";

        GUI.Label(new Rect(10, 10, 200, 200), instructions, style);

        // 현재 상태 표시
        if (InventoryManager.Instance != null)
        {
            string status = $"카드키: {InventoryManager.Instance.CardKeyCount}개";
            GUI.Label(new Rect(10, 220, 200, 30), status, style);
        }
    }
}
