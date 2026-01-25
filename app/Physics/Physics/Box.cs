using System.Numerics;
using Box2D.NET;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2Geometries;
using static Box2D.NET.B2Shapes;

namespace Physics;

/// <summary>
/// 동적 박스 바디
/// </summary>
public class Box : BodyBase
{
    /// <summary>기본 밀도 (kg/m²)</summary>
    public const float DefaultDensity = 1.0f;
    
    /// <summary>기본 마찰력</summary>
    public const float DefaultFriction = 0.3f;
    
    /// <summary>기본 반발력 (탄성)</summary>
    public const float DefaultRestitution = 0.5f;

    public float Width { get; }
    public float Height { get; }
    public float Density { get; }
    public float Friction { get; }
    public float Restitution { get; }

    /// <summary>
    /// 동적 박스 생성
    /// </summary>
    /// <param name="worldId">물리 월드 ID</param>
    /// <param name="id">바디 식별자</param>
    /// <param name="position">초기 위치</param>
    /// <param name="width">너비</param>
    /// <param name="height">높이</param>
    /// <param name="density">밀도</param>
    /// <param name="friction">마찰력</param>
    /// <param name="restitution">반발력</param>
    public Box(
        B2WorldId worldId,
        string id, 
        Vector2 position, 
        float width, 
        float height,
        float density = DefaultDensity, 
        float friction = DefaultFriction, 
        float restitution = DefaultRestitution) 
        : base(id)
    {
        Width = width;
        Height = height;
        Density = density;
        Friction = friction;
        Restitution = restitution;

        // 동적 바디 생성
        var bodyDef = b2DefaultBodyDef();
        bodyDef.type = B2BodyType.b2_dynamicBody;
        bodyDef.position = new B2Vec2(position.X, position.Y);

        _bodyId = b2CreateBody(worldId, ref bodyDef);

        // 박스 셔이프 생성
        var box = b2MakeBox(width / 2, height / 2);
        
        var shapeDef = b2DefaultShapeDef();
        shapeDef.density = density;
        shapeDef.material.friction = friction;
        shapeDef.material.restitution = restitution;

        b2CreatePolygonShape(_bodyId, ref shapeDef, ref box);
    }
}
