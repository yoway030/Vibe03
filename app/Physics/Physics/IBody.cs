using System.Numerics;
using Box2D.NET;

namespace Physics;

/// <summary>
/// 물리 바디??공통 ?�터?�이??
/// </summary>
public interface IBody
{
    /// <summary>
    /// 바디 ?�별??
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Box2D 바디 ID
    /// </summary>
    B2BodyId BodyId { get; }

    /// <summary>
    /// 바디???�재 ?�치
    /// </summary>
    Vector2 Position { get; }

    /// <summary>
    /// 바디???�재 ?�전 각도 (?�디??
    /// </summary>
    float Angle { get; }

    /// <summary>
    /// 바디???�형 ?�도
    /// </summary>
    Vector2 LinearVelocity { get; set; }

    /// <summary>
    /// 바디???�을 가?�니??
    /// </summary>
    /// <param name="force">??벡터</param>
    /// <param name="point">?�을 가????(?�드 좌표)</param>
    void ApplyForce(Vector2 force, Vector2 point);

    /// <summary>
    /// 바디 중심???�을 가?�니??
    /// </summary>
    /// <param name="force">??벡터</param>
    void ApplyForceToCenter(Vector2 force);

    /// <summary>
    /// 바디??충격??가?�니??
    /// </summary>
    /// <param name="impulse">충격 벡터</param>
    /// <param name="point">충격??가????(?�드 좌표)</param>
    void ApplyLinearImpulse(Vector2 impulse, Vector2 point);

    /// <summary>
    /// 바디�??�괴?�니??
    /// </summary>
    void Destroy();
}
