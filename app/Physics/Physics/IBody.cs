using System.Numerics;
using Box2D.NET;

namespace Physics;

/// <summary>
/// 물리 바디의 공통 인터페이스
/// </summary>
public interface IBody
{
    /// <summary>
    /// 바디 식별자
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Box2D 바디 ID
    /// </summary>
    B2BodyId BodyId { get; }

    /// <summary>
    /// 바디의 현재 위치
    /// </summary>
    Vector2 Position { get; }

    /// <summary>
    /// 바디의 현재 회전 각도 (라디안)
    /// </summary>
    float Angle { get; }

    /// <summary>
    /// 바디의 선형 속도
    /// </summary>
    Vector2 LinearVelocity { get; set; }

    /// <summary>
    /// 바디에 힘을 가합니다
    /// </summary>
    /// <param name="force">힘 벡터</param>
    /// <param name="point">힘을 가할 위치 (월드 좌표)</param>
    void ApplyForce(Vector2 force, Vector2 point);

    /// <summary>
    /// 바디 중심에 힘을 가합니다
    /// </summary>
    /// <param name="force">힘 벡터</param>
    void ApplyForceToCenter(Vector2 force);

    /// <summary>
    /// 바디에 충격을 가합니다
    /// </summary>
    /// <param name="impulse">충격 벡터</param>
    /// <param name="point">충격을 가할 위치 (월드 좌표)</param>
    void ApplyLinearImpulse(Vector2 impulse, Vector2 point);

    /// <summary>
    /// 바디를 파괴합니다
    /// </summary>
    void Destroy();
}
