using UnityEngine;

/// <summary>
/// 전역 게임 상태 관리 싱글톤
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

        Initialize();
    }

    private void Initialize()
    {
        // 게임 초기화 로직
        Debug.Log("GameManager initialized");
    }
}
