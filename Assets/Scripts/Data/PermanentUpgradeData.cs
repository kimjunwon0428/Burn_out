using UnityEngine;

/// <summary>
/// 영구 업그레이드 데이터 - ScriptableObject
/// 허브에서 구매 가능한 영구 강화
/// </summary>
[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Burnout/Permanent Upgrade Data")]
public class PermanentUpgradeData : ScriptableObject
{
    [Header("기본 정보")]
    public string UpgradeId;
    public string DisplayName;
    [TextArea(2, 4)]
    public string Description;
    public Sprite Icon;
    public UpgradeCategory Category;

    [Header("스탯 효과")]
    public StatType AffectedStat;
    public ModifierType ModifierType = ModifierType.Flat;

    [Header("레벨 설정")]
    public int MaxLevel = 5;
    [Tooltip("각 레벨별 스탯 수치")]
    public float[] ValuesPerLevel;
    [Tooltip("각 레벨별 업그레이드 비용")]
    public int[] CostsPerLevel;

    [Header("해금 조건")]
    public int RequiredPlayerLevel = 0;
    public PermanentUpgradeData[] Prerequisites;

    /// <summary>
    /// 특정 레벨의 스탯 값 반환
    /// </summary>
    public float GetValueAtLevel(int level)
    {
        if (level <= 0) return 0f;
        int index = Mathf.Clamp(level - 1, 0, ValuesPerLevel.Length - 1);
        return ValuesPerLevel[index];
    }

    /// <summary>
    /// 특정 레벨로 업그레이드하는 비용
    /// </summary>
    public int GetCostForLevel(int targetLevel)
    {
        if (targetLevel <= 0 || targetLevel > MaxLevel) return 0;
        int index = Mathf.Clamp(targetLevel - 1, 0, CostsPerLevel.Length - 1);
        return CostsPerLevel[index];
    }

    /// <summary>
    /// 총 비용 (레벨 1부터 targetLevel까지)
    /// </summary>
    public int GetTotalCostToLevel(int targetLevel)
    {
        int total = 0;
        for (int i = 1; i <= targetLevel && i <= MaxLevel; i++)
        {
            total += GetCostForLevel(i);
        }
        return total;
    }
}

/// <summary>
/// 업그레이드 카테고리
/// </summary>
public enum UpgradeCategory
{
    Survival,   // 생존 (체력, 방어)
    Offense,    // 공세 (공격력, 속도)
    Utility,    // 유틸 (이동, 닷지)
    Fortune     // 행운 (드롭률, 골드)
}
