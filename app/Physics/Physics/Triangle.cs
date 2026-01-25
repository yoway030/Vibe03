using System.Numerics;
using Box2D.NET;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2Geometries;
using static Box2D.NET.B2Shapes;

namespace Physics;

/// <summary>
/// 동적 삼각형 바디
/// </summary>
public class Triangle : BodyBase
{
    /// <summary>삼각형의 기본 크기</summary>
    public const float DefaultSize = 1.0f;
    
    public float Size { get; }
    public float Density { get; }
    public float Friction { get; }
    public float Restitution { get; }

    /// <summary>
    /// 동적 삼각형 바디 생성
    /// </summary>
    /// <param name="worldId">월드 ID</param>
    /// <param name="id">바디 식별자</param>
    /// <param name="position">위치</param>
    /// <param name="size">삼각형 크기 (중심에서 정점까지 거리)</param>
    /// <param name="density">밀도</param>
    /// <param name="friction">마찰력</param>
    /// <param name="restitution">반발력</param>
    public Triangle(
        B2WorldId worldId,
        string id, 
        Vector2 position, 
        float size = DefaultSize,
        float density = PhysicsConfig.DefaultDensity, 
        float friction = PhysicsConfig.DefaultFriction, 
        float restitution = PhysicsConfig.DefaultRestitution) 
        : base(id)
    {
        Size = size;
        Density = density;
        Friction = friction;
        Restitution = restitution;

        // 동적 바디 생성
        var bodyDef = b2DefaultBodyDef();
        bodyDef.type = B2BodyType.b2_dynamicBody;
        bodyDef.position = new B2Vec2(position.X, position.Y);

        _bodyId = b2CreateBody(worldId, ref bodyDef);

        // 정삼각형 정점 생성 (위쪽 정점이 12시 방향)
        B2Vec2[] vertices = new B2Vec2[3];
        vertices[0] = new B2Vec2(0, size);                              // 위
        vertices[1] = new B2Vec2(-size * 0.866f, -size * 0.5f);        // 왼쪽 아래
        vertices[2] = new B2Vec2(size * 0.866f, -size * 0.5f);         // 오른쪽 아래

        // 폴리곤 셰이프 생성
        var hull = new B2Hull();
        for (int i = 0; i < 3; i++)
        {
            hull.points[i] = vertices[i];
        }
        hull.count = 3;
        
        var polygon = b2MakePolygon(ref hull, 0);
        
        var shapeDef = b2DefaultShapeDef();
        shapeDef.density = density;
        shapeDef.material.friction = friction;
        shapeDef.material.restitution = restitution;

        b2CreatePolygonShape(_bodyId, ref shapeDef, ref polygon);
    }
}
