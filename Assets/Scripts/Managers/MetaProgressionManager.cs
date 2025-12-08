using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 메타 진행 매니저 - 영구 성장 데이터 관리
/// 허브 업그레이드, 해금된 아이템, 재화 등
/// </summary>
public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance { get; private set; }

    [Header("재화")]
    [SerializeField] private int _permanentCurrency = 0;

    [Header("업그레이드 데이터")]
    [SerializeField] private List<PermanentUpgradeData> _allUpgrades = new List<PermanentUpgradeData>();

    // 업그레이드 레벨 저장
    private Dictionary<string, int> _upgradeLevels = new Dictionary<string, int>();

    // 이벤트
    public event Action<int> OnCurrencyChanged;
    public event Action<string, int> OnUpgradePurchased;  // (upgradeId, newLevel)

    // 프로퍼티
    public int PermanentCurrency => _permanentCurrency;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadProgress();
    }

    private void OnApplicationQuit()
    {
        SaveProgress();
    }

    /// <summary>
    /// 재화 추가
    /// </summary>
    public void AddCurrency(int amount)
    {
        if (amount <= 0) return;
        _permanentCurrency += amount;
        OnCurrencyChanged?.Invoke(_permanentCurrency);
        Debug.Log($"Permanent currency added: {amount}. Total: {_permanentCurrency}");
    }

    /// <summary>
    /// 재화 사용
    /// </summary>
    public bool SpendCurrency(int amount)
    {
        if (amount <= 0 || _permanentCurrency < amount) return false;
        _permanentCurrency -= amount;
        OnCurrencyChanged?.Invoke(_permanentCurrency);
        return true;
    }

    /// <summary>
    /// 업그레이드 현재 레벨 조회
    /// </summary>
    public int GetUpgradeLevel(string upgradeId)
    {
        return _upgradeLevels.TryGetValue(upgradeId, out int level) ? level : 0;
    }

    /// <summary>
    /// 업그레이드 구매 가능 여부
    /// </summary>
    public bool CanPurchaseUpgrade(PermanentUpgradeData upgrade)
    {
        if (upgrade == null) return false;

        int currentLevel = GetUpgradeLevel(upgrade.UpgradeId);
        if (currentLevel >= upgrade.MaxLevel) return false;

        int cost = upgrade.GetCostForLevel(currentLevel + 1);
        if (_permanentCurrency < cost) return false;

        // 선행 조건 확인
        if (upgrade.Prerequisites != null)
        {
            foreach (var prereq in upgrade.Prerequisites)
            {
                if (prereq != null && GetUpgradeLevel(prereq.UpgradeId) <= 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 업그레이드 구매
    /// </summary>
    public bool PurchaseUpgrade(PermanentUpgradeData upgrade)
    {
        if (!CanPurchaseUpgrade(upgrade)) return false;

        int currentLevel = GetUpgradeLevel(upgrade.UpgradeId);
        int nextLevel = currentLevel + 1;
        int cost = upgrade.GetCostForLevel(nextLevel);

        if (!SpendCurrency(cost)) return false;

        _upgradeLevels[upgrade.UpgradeId] = nextLevel;

        // PlayerStats에 영구 보너스 적용
        ApplyUpgradeToStats(upgrade, nextLevel);

        OnUpgradePurchased?.Invoke(upgrade.UpgradeId, nextLevel);
        Debug.Log($"Upgrade purchased: {upgrade.DisplayName} -> Level {nextLevel}");

        SaveProgress();
        return true;
    }

    /// <summary>
    /// 업그레이드를 PlayerStats에 적용
    /// </summary>
    private void ApplyUpgradeToStats(PermanentUpgradeData upgrade, int level)
    {
        if (PlayerStats.Instance == null) return;

        float value = upgrade.GetValueAtLevel(level);
        float previousValue = level > 1 ? upgrade.GetValueAtLevel(level - 1) : 0f;
        float addedValue = value - previousValue;

        PlayerStats.Instance.AddPermanentBonus(upgrade.AffectedStat, addedValue);
    }

    /// <summary>
    /// 모든 영구 업그레이드를 PlayerStats에 적용 (게임 시작 시)
    /// </summary>
    public void ApplyAllUpgradesToStats()
    {
        if (PlayerStats.Instance == null) return;

        foreach (var upgrade in _allUpgrades)
        {
            int level = GetUpgradeLevel(upgrade.UpgradeId);
            if (level > 0)
            {
                float value = upgrade.GetValueAtLevel(level);
                PlayerStats.Instance.AddPermanentBonus(upgrade.AffectedStat, value);
            }
        }

        Debug.Log("All permanent upgrades applied to PlayerStats");
    }

    /// <summary>
    /// 업그레이드 정보 조회
    /// </summary>
    public PermanentUpgradeData GetUpgradeData(string upgradeId)
    {
        return _allUpgrades.Find(u => u.UpgradeId == upgradeId);
    }

    /// <summary>
    /// 카테고리별 업그레이드 목록
    /// </summary>
    public List<PermanentUpgradeData> GetUpgradesByCategory(UpgradeCategory category)
    {
        return _allUpgrades.FindAll(u => u.Category == category);
    }

    /// <summary>
    /// 진행 상황 저장
    /// </summary>
    public void SaveProgress()
    {
        PlayerPrefs.SetInt("MetaCurrency", _permanentCurrency);

        // 업그레이드 레벨 저장
        foreach (var kvp in _upgradeLevels)
        {
            PlayerPrefs.SetInt($"Upgrade_{kvp.Key}", kvp.Value);
        }

        PlayerPrefs.Save();
        Debug.Log("Meta progression saved");
    }

    /// <summary>
    /// 진행 상황 로드
    /// </summary>
    public void LoadProgress()
    {
        _permanentCurrency = PlayerPrefs.GetInt("MetaCurrency", 0);

        // 업그레이드 레벨 로드
        _upgradeLevels.Clear();
        foreach (var upgrade in _allUpgrades)
        {
            int level = PlayerPrefs.GetInt($"Upgrade_{upgrade.UpgradeId}", 0);
            if (level > 0)
            {
                _upgradeLevels[upgrade.UpgradeId] = level;
            }
        }

        Debug.Log($"Meta progression loaded - Currency: {_permanentCurrency}");
    }

    /// <summary>
    /// 진행 상황 초기화 (디버그용)
    /// </summary>
    [ContextMenu("Reset All Progress")]
    public void ResetAllProgress()
    {
        _permanentCurrency = 0;
        _upgradeLevels.Clear();

        // PlayerPrefs에서도 삭제
        PlayerPrefs.DeleteKey("MetaCurrency");
        foreach (var upgrade in _allUpgrades)
        {
            PlayerPrefs.DeleteKey($"Upgrade_{upgrade.UpgradeId}");
        }
        PlayerPrefs.Save();

        OnCurrencyChanged?.Invoke(0);
        Debug.Log("All meta progression reset!");
    }

    /// <summary>
    /// 테스트용 재화 추가
    /// </summary>
    [ContextMenu("Add 1000 Currency")]
    public void DebugAddCurrency()
    {
        AddCurrency(1000);
    }
}
