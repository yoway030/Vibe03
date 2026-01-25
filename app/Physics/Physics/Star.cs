using System.Numerics;
using Box2D.NET;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2Geometries;
using static Box2D.NET.B2Shapes;

namespace Physics;

/// <summary>
/// 동적 별 바디
/// </summary>
public class Star : BodyBase
{
    /// <summary>별의 기본 꼭지점 개수</summary>
    public const int DefaultPointCount = 5;
        /// <summary>별의 최대 꼭지점 개수 (B2Hull 제약: 8 정점 = 4 포인트)</summary>
    public const int MaxPointCount = 4;
        /// <summary>내부 반지름 대비 외부 반지름 비율</summary>
    public const float InnerRadiusRatio = 0.4f;

    public float OuterRadius { get; }
    public int PointCount { get; }
    public float Density { get; }
    public float Friction { get; }
    public float Restitution { get; }

    /// <summary>
    /// 동적 별 생성
    /// </summary>
    /// <param name="worldId">물리 월드 ID</param>
    /// <param name="id">바디 식별자</param>
    /// <param name="position">초기 위치</param>
    /// <param name="outerRadius">외부 반지름</param>
    /// <param name="pointCount">별의 꼭지점 개수 (기본 5)</param>
    /// <param name="density">밀도</param>
    /// <param name="friction">마찰력</param>
    /// <param name="restitution">반발력</param>
    public Star(
        B2WorldId worldId,
        string id, 
        Vector2 position, 
        float outerRadius,
        int pointCount = DefaultPointCount,
        float density = PhysicsConfig.DefaultDensity, 
        float friction = PhysicsConfig.DefaultFriction, 
        float restitution = PhysicsConfig.DefaultRestitution) 
        : base(id)
    {
        // B2Hull의 최대 정점 제한 (일반적으로 8)
        if (pointCount < 3) pointCount = 3;
        if (pointCount > MaxPointCount) pointCount = MaxPointCount;
        
        OuterRadius = outerRadius;
        PointCount = pointCount;
        Density = density;
        Friction = friction;
        Restitution = restitution;

        // 동적 바디 생성
        var bodyDef = b2DefaultBodyDef();
        bodyDef.type = B2BodyType.b2_dynamicBody;
        bodyDef.position = new B2Vec2(position.X, position.Y);

        _bodyId = b2CreateBody(worldId, ref bodyDef);

        // 별 모양 정점 생성
        float innerRadius = outerRadius * InnerRadiusRatio;
        int vertexCount = pointCount * 2; // 외부점 + 내부점 교차
        B2Vec2[] vertices = new B2Vec2[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            float angle = i * MathF.PI / pointCount;
            float radius = (i % 2 == 0) ? outerRadius : innerRadius;
            
            vertices[i] = new B2Vec2(
                radius * MathF.Cos(angle),
                radius * MathF.Sin(angle)
            );
        }

        // 폴리곤 셰이프 생성
        var hull = new B2Hull();
        for (int i = 0; i < vertexCount; i++)
        {
            hull.points[i] = vertices[i];
        }
        hull.count = vertexCount;
        
        var polygon = b2MakePolygon(ref hull, 0);
        
        var shapeDef = b2DefaultShapeDef();
        shapeDef.density = density;
        shapeDef.material.friction = friction;
        shapeDef.material.restitution = restitution;

        b2CreatePolygonShape(_bodyId, ref shapeDef, ref polygon);
    }
}
