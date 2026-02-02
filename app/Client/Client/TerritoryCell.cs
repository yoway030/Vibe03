using System.Numerics;

namespace Client;

/// <summary>
/// 영역 상자 - 게임 공간을 구성하는 작은 네모 영역
/// </summary>
public class TerritoryCell
{
    /// <summary>
    /// 영역 상자의 고유 ID
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 위치
    /// </summary>
    public Vector2 Position { get; }

    /// <summary>
    /// 크기
    /// </summary>
    public float Size { get; }

    /// <summary>
    /// 현재 내구도
    /// </summary>
    public float Durability { get; private set; }

    /// <summary>
    /// 소유한 대포의 ID (-1이면 중립)
    /// </summary>
    public int OwnerCannonId { get; private set; }

    /// <summary>
    /// 이전 소유자 ID (애니메이션용)
    /// </summary>
    public int PreviousOwnerCannonId { get; private set; }

    /// <summary>
    /// 색상 전환 애니메이션 시작 시간
    /// </summary>
    public float TransitionStartTime { get; private set; }

    /// <summary>
    /// 색상 전환 애니메이션 지속 시간
    /// </summary>
    public const float TransitionDuration = 0.4f;

    /// <summary>
    /// 회전 애니메이션 지속 시간 (카드 뒤집기)
    /// </summary>
    public const float FlipDuration = 0.3f;

    /// <summary>
    /// 최대 내구도
    /// </summary>
    public const float MaxDurability = 1.0f;

    public TerritoryCell(string id, Vector2 position, float size)
    {
        Id = id;
        Position = position;
        Size = size;
        Durability = MaxDurability;
        OwnerCannonId = -1; // 중립 상태
        PreviousOwnerCannonId = -1;
        TransitionStartTime = 0;
    }

    /// <summary>
    /// 포탄 공격을 받음
    /// </summary>
    /// <param name="damage">공격력</param>
    /// <param name="attackerCannonId">공격한 대포의 ID</param>
    /// <param name="currentTime">현재 게임 시간</param>
    /// <returns>영역이 점령되었으면 true</returns>
    public bool TakeDamage(float damage, int attackerCannonId, float currentTime)
    {
        // 같은 대포의 영역이면 공격받지 않음
        if (OwnerCannonId == attackerCannonId)
            return false;

        Durability -= damage;

        if (Durability <= 0)
        {
            // 영역 점령 - 애니메이션 시작
            PreviousOwnerCannonId = OwnerCannonId;
            OwnerCannonId = attackerCannonId;
            TransitionStartTime = currentTime;
            Durability = MaxDurability;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 색상 전환 진행도 (0.0 ~ 1.0)
    /// </summary>
    public float GetTransitionProgress(float currentTime)
    {
        float elapsed = currentTime - TransitionStartTime;
        return Math.Min(elapsed / TransitionDuration, 1.0f);
    }

    /// <summary>
    /// 회전 진행도 (0.0 ~ 1.0, 카드 뒤집기용)
    /// </summary>
    public float GetFlipProgress(float currentTime)
    {
        float elapsed = currentTime - TransitionStartTime;
        return Math.Min(elapsed / FlipDuration, 1.0f);
    }

    /// <summary>
    /// 회전 각도 (0 ~ 180도, Y축 회전)
    /// </summary>
    public float GetFlipAngle(float currentTime)
    {
        float progress = GetFlipProgress(currentTime);
        return progress * MathF.PI; // 0 ~ 180도 (0 ~ π 라디안)
    }

    /// <summary>
    /// 영역을 리셋 (중립 상태로)
    /// </summary>
    public void Reset()
    {
        Durability = MaxDurability;
        OwnerCannonId = -1;
        PreviousOwnerCannonId = -1;
    }
}
