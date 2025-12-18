using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 처형 가능 시 "E키로 처형" 프롬프트 표시
/// </summary>
public class ExecutionPromptUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject _promptPanel;
    [SerializeField] private TextMeshProUGUI _promptText;

    [Header("설정")]
    [SerializeField] private string _promptMessage = "E키로 처형";

    private Transform _playerTransform;

    private void Start()
    {
        // 플레이어 찾기
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
            _playerTransform = player.transform;

        // 초기 숨김
        if (_promptPanel != null)
            _promptPanel.SetActive(false);

        // 프롬프트 텍스트 설정
        if (_promptText != null)
            _promptText.text = _promptMessage;
    }

    private void Update()
    {
        if (_playerTransform == null || ExecutionSystem.Instance == null)
            return;

        // 처형 가능 여부 체크
        bool canExecute = !ExecutionSystem.Instance.IsExecuting &&
                          ExecutionSystem.Instance.HasExecutableEnemyInRange(_playerTransform.position);

        if (_promptPanel != null)
            _promptPanel.SetActive(canExecute);
    }
}
