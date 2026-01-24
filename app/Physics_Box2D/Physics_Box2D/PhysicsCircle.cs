using System.Numerics;
using Box2D.NET;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2Shapes;

namespace Physics_Box2D;

/// <summary>
/// 동적 원 바디
/// </summary>
public class PhysicsCircle : PhysicsBodyBase
{
    public float Radius { get; }
    public float Density { get; }
    public float Friction { get; }
    public float Restitution { get; }

    /// <summary>
    /// 동적 원 생성
    /// </summary>
    /// <param name="worldId">물리 월드 ID</param>
    /// <param name="id">바디 식별자</param>
    /// <param name="position">초기 위치</param>
    /// <param name="radius">반지름</param>
    /// <param name="density">밀도</param>
    /// <param name="friction">마찰력</param>
    /// <param name="restitution">반발력</param>
    public PhysicsCircle(
        B2WorldId worldId,
        string id, 
        Vector2 position, 
        float radius,
        float density = 1.0f, 
        float friction = 0.3f, 
        float restitution = 0.5f) 
        : base(id)
    {
        Radius = radius;
        Density = density;
        Friction = friction;
        Restitution = restitution;

        // 동적 바디 생성
        var bodyDef = b2DefaultBodyDef();
        bodyDef.type = B2BodyType.b2_dynamicBody;
        bodyDef.position = new B2Vec2(position.X, position.Y);

        _bodyId = b2CreateBody(worldId, ref bodyDef);

        // 원 형태 생성
        var circle = new B2Circle(new B2Vec2(0, 0), radius);
        
        var shapeDef = b2DefaultShapeDef();
        shapeDef.density = density;
        shapeDef.material.friction = friction;
        shapeDef.material.restitution = restitution;

        b2CreateCircleShape(_bodyId, ref shapeDef, ref circle);
    }
}
