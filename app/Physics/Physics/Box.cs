using System.Numerics;
using Box2D.NET;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2Geometries;
using static Box2D.NET.B2Shapes;

namespace Physics;

/// <summary>
/// ?�적 박스 바디
/// </summary>
public class Box : BodyBase
{
    public float Width { get; }
    public float Height { get; }
    public float Density { get; }
    public float Friction { get; }
    public float Restitution { get; }

    /// <summary>
    /// ?�적 박스 ?�성
    /// </summary>
    /// <param name="worldId">물리 ?�드 ID</param>
    /// <param name="id">바디 ?�별??/param>
    /// <param name="position">초기 ?�치</param>
    /// <param name="width">??/param>
    /// <param name="height">?�이</param>
    /// <param name="density">밀??/param>
    /// <param name="friction">마찰??/param>
    /// <param name="restitution">반발??/param>
    public Box(
        B2WorldId worldId,
        string id, 
        Vector2 position, 
        float width, 
        float height,
        float density = 1.0f, 
        float friction = 0.3f, 
        float restitution = 0.5f) 
        : base(id)
    {
        Width = width;
        Height = height;
        Density = density;
        Friction = friction;
        Restitution = restitution;

        // ?�적 바디 ?�성
        var bodyDef = b2DefaultBodyDef();
        bodyDef.type = B2BodyType.b2_dynamicBody;
        bodyDef.position = new B2Vec2(position.X, position.Y);

        _bodyId = b2CreateBody(worldId, ref bodyDef);

        // 박스 ?�태 ?�성
        var box = b2MakeBox(width / 2, height / 2);
        
        var shapeDef = b2DefaultShapeDef();
        shapeDef.density = density;
        shapeDef.material.friction = friction;
        shapeDef.material.restitution = restitution;

        b2CreatePolygonShape(_bodyId, ref shapeDef, ref box);
    }
}
