using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이스타일 데이터 - ScriptableObject
/// 각 플레이스타일의 스탯 수정자 정의
/// </summary>
[CreateAssetMenu(fileName = "NewPlaystyle", menuName = "Burnout/Playstyle Data")]
public class PlaystyleData : ScriptableObject
{
    [Header("기본 정보")]
    public PlaystyleType PlaystyleType;
    public string DisplayName;
    [TextArea(2, 4)]
    public string Description;
    public Sprite Icon;

    [Header("스탯 수정자")]
    public List<PlaystyleStatModifier> StatModifiers = new List<PlaystyleStatModifier>();

    /// <summary>
    /// StatModifier 리스트로 변환
    /// </summary>
    public List<StatModifier> GetStatModifiers()
    {
        var result = new List<StatModifier>();
        foreach (var mod in StatModifiers)
        {
            result.Add(new StatModifier(mod.StatType, mod.ModifierType, mod.Value));
        }
        return result;
    }
}

/// <summary>
/// 플레이스타일용 스탯 수정자 (Inspector에서 편집 가능)
/// </summary>
[System.Serializable]
public class PlaystyleStatModifier
{
    public StatType StatType;
    public ModifierType ModifierType = ModifierType.Percent;
    [Tooltip("Percent: 0.1 = +10%, Flat: 실제 수치, Multiply: 1.5 = x1.5")]
    public float Value;
}
