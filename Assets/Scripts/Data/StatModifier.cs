using System;

/// <summary>
/// 스탯 수정자 - 아이템, 버프, 플레이스타일 등에서 사용
/// </summary>
[Serializable]
public struct StatModifier : IComparable<StatModifier>
{
    public StatType StatType;
    public ModifierType ModifierType;
    public float Value;
    public int Priority;  // 낮을수록 먼저 적용 (Flat < Percent < Multiply)

    /// <summary>
    /// 수정자 생성
    /// </summary>
    public StatModifier(StatType statType, ModifierType modifierType, float value, int priority = -1)
    {
        StatType = statType;
        ModifierType = modifierType;
        Value = value;

        // 기본 우선순위: Flat(0) -> Percent(100) -> Multiply(200)
        if (priority < 0)
        {
            Priority = modifierType switch
            {
                ModifierType.Flat => 0,
                ModifierType.Percent => 100,
                ModifierType.Multiply => 200,
                _ => 0
            };
        }
        else
        {
            Priority = priority;
        }
    }

    /// <summary>
    /// 우선순위 기반 정렬
    /// </summary>
    public int CompareTo(StatModifier other)
    {
        return Priority.CompareTo(other.Priority);
    }

    /// <summary>
    /// Flat 수정자 생성 헬퍼
    /// </summary>
    public static StatModifier Flat(StatType statType, float value, int priority = 0)
    {
        return new StatModifier(statType, ModifierType.Flat, value, priority);
    }

    /// <summary>
    /// Percent 수정자 생성 헬퍼 (0.1 = +10%)
    /// </summary>
    public static StatModifier Percent(StatType statType, float value, int priority = 100)
    {
        return new StatModifier(statType, ModifierType.Percent, value, priority);
    }

    /// <summary>
    /// Multiply 수정자 생성 헬퍼 (1.5 = x1.5)
    /// </summary>
    public static StatModifier Multiply(StatType statType, float value, int priority = 200)
    {
        return new StatModifier(statType, ModifierType.Multiply, value, priority);
    }

    public override string ToString()
    {
        string sign = Value >= 0 ? "+" : "";
        return ModifierType switch
        {
            ModifierType.Flat => $"{StatType}: {sign}{Value}",
            ModifierType.Percent => $"{StatType}: {sign}{Value * 100:F0}%",
            ModifierType.Multiply => $"{StatType}: x{Value:F2}",
            _ => $"{StatType}: {Value}"
        };
    }
}
