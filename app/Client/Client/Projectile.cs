using System.Numerics;

namespace Client;

/// <summary>
/// 포탄 - 대포에서 발사되는 투사체
/// </summary>
public class Projectile
{
    /// <summary>
    /// 포탄 ID
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 발사한 대포의 ID
    /// </summary>
    public int CannonId { get; }

    /// <summary>
    /// 현재 위치
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// 발사 위치 (사거리 계산용)
    /// </summary>
    public Vector2 StartPosition { get; }

    /// <summary>
    /// 속도
    /// </summary>
    public Vector2 Velocity { get; set; }

    /// <summary>
    /// 공격력
    /// </summary>
    public float Damage { get; }

    /// <summary>
    /// 반지름 (충돌 검사용)
    /// </summary>
    public float Radius { get; }

    /// <summary>
    /// 활성 상태
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 기본 공격력
    /// </summary>
    public const float DefaultDamage = 1.0f;

    /// <summary>
    /// 기본 반지름
    /// </summary>
    public const float DefaultRadius = 10.0f;

    /// <summary>
    /// 최대 사거리 (이 거리 이상 날아가면 소멸)
    /// </summary>
    public const float MaxRange = 20000.0f;

    public Projectile(string id, int cannonId, Vector2 position, Vector2 velocity, float damage = DefaultDamage, float radius = DefaultRadius)
    {
        Id = id;
        CannonId = cannonId;
        Position = position;
        StartPosition = position;
        Velocity = velocity;
        Damage = damage;
        Radius = radius;
        IsActive = true;
    }

    /// <summary>
    /// 포탄 위치 업데이트
    /// </summary>
    /// <param name="deltaTime">시간 간격</param>
    public void Update(float deltaTime)
    {
        if (!IsActive) return;

        Position += Velocity * deltaTime;
    }

    /// <summary>
    /// 사거리 초과 여부 확인
    /// </summary>
    public bool IsOutOfRange()
    {
        float distanceTraveled = Vector2.Distance(Position, StartPosition);
        return distanceTraveled >= MaxRange;
    }

    /// <summary>
    /// 다른 위치와의 충돌 검사
    /// </summary>
    public bool CheckCollision(Vector2 targetPosition, float targetSize)
    {
        if (!IsActive) return false;

        float distance = Vector2.Distance(Position, targetPosition);
        return distance < (Radius + targetSize / 2);
    }
}
