using System.Numerics;
using Box2D.NET;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2Shapes;

namespace Physics;

/// <summary>
/// ?�적 ??바디
/// </summary>
public class Circle : BodyBase
{
    public float Radius { get; }
    public float Density { get; }
    public float Friction { get; }
    public float Restitution { get; }

    /// <summary>
    /// ?�적 ???�성
    /// </summary>
    /// <param name="worldId">물리 ?�드 ID</param>
    /// <param name="id">바디 ?�별??/param>
    /// <param name="position">초기 ?�치</param>
    /// <param name="radius">반�?�?/param>
    /// <param name="density">밀??/param>
    /// <param name="friction">마찰??/param>
    /// <param name="restitution">반발??/param>
    public Circle(
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

        // ?�적 바디 ?�성
        var bodyDef = b2DefaultBodyDef();
        bodyDef.type = B2BodyType.b2_dynamicBody;
        bodyDef.position = new B2Vec2(position.X, position.Y);

        _bodyId = b2CreateBody(worldId, ref bodyDef);

        // ???�태 ?�성
        var circle = new B2Circle(new B2Vec2(0, 0), radius);
        
        var shapeDef = b2DefaultShapeDef();
        shapeDef.density = density;
        shapeDef.material.friction = friction;
        shapeDef.material.restitution = restitution;

        b2CreateCircleShape(_bodyId, ref shapeDef, ref circle);
    }
}
