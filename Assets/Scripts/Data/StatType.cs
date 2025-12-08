/// <summary>
/// 모든 플레이어 스탯 타입 열거형
/// </summary>
public enum StatType
{
    // === 전투 스탯 (Combat) ===
    MaxHealth,          // 최대 체력
    AttackPower,        // 기본 공격력
    Defense,            // 방어력 (데미지 감소 %)
    AttackSpeed,        // 공격 속도 배율

    // === 이동 스탯 (Movement) ===
    MoveSpeed,          // 이동 속도
    SprintMultiplier,   // 스프린트 배율
    DodgeDistance,      // 닷지 거리
    DodgeCooldown,      // 닷지 쿨다운

    // === 방어 스탯 (Defense) ===
    GuardDamageReduction,   // 가드 데미지 감소율
    PerfectGuardWindow,     // 퍼펙트 가드 윈도우
    PerfectDodgeWindow,     // 퍼펙트 닷지 윈도우
    InvincibilityDuration,  // 무적 시간

    // === 특수 스탯 (Special) ===
    DurabilityDamage,       // 내구력 데미지 배율
    ExecutionDamage,        // 처형 데미지 배율
    SpecialResourceMax,     // 특수 자원 최대치
    SpecialResourceGain,    // 특수 자원 획득량
    SpecialAttackPower,     // 특수기 공격력 배율

    // === 로그라이트 스탯 (Roguelite) ===
    ItemDropRate,       // 아이템 드롭률
    GoldGain,           // 골드 획득량
    HealEfficiency,     // 회복 효율
    ShopDiscount        // 상점 할인율
}

/// <summary>
/// 스탯 수정자 타입
/// </summary>
public enum ModifierType
{
    Flat,       // 고정 수치 증가 (예: +10)
    Percent,    // 백분율 증가 (예: +10% -> 0.1f로 저장)
    Multiply    // 배율 적용 (예: x1.5)
}
