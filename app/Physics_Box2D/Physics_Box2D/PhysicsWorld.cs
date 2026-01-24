using Box2D.NET;
using System.Numerics;
using static Box2D.NET.B2Worlds;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Geometries;
using static Box2D.NET.B2Shapes;
using static Box2D.NET.B2MathFunction;

namespace Physics_Box2D;

/// <summary>
/// Box2D 3.x 물리 엔진을 래핑한 클래스
/// 게임 월드의 물리 시뮬레이션을 관리합니다.
/// </summary>
public class PhysicsWorld
{
    private readonly B2WorldId _worldId;
    private readonly Dictionary<string, B2BodyId> _bodies;

    /// <summary>
    /// PhysicsWorld 생성자
    /// </summary>
    /// <param name="gravity">중력 벡터 (기본값: (0, -10))</param>
    public PhysicsWorld(Vector2 gravity = default)
    {
        if (gravity == default)
        {
            gravity = new Vector2(0, -10f);
        }

        var worldDef = b2DefaultWorldDef();
        worldDef.gravity = new B2Vec2(gravity.X, gravity.Y);
        
        _worldId = b2CreateWorld(worldDef);
        _bodies = new Dictionary<string, B2BodyId>();
    }

    /// <summary>
    /// 물리 시뮬레이션 업데이트
    /// </summary>
    /// <param name="timeStep">시간 간격 (초)</param>
    /// <param name="subStepCount">서브 스텝 횟수 (기본값: 4)</param>
    public void Step(float timeStep, int subStepCount = 4)
    {
        b2World_Step(_worldId, timeStep, subStepCount);
    }

    /// <summary>
    /// 정적 바디(지면, 벽 등)를 생성합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <param name="position">위치</param>
    /// <param name="width">폭</param>
    /// <param name="height">높이</param>
    public void CreateStaticBox(string id, Vector2 position, float width, float height)
    {
        var bodyDef = b2DefaultBodyDef();
        bodyDef.type = B2BodyType.b2_staticBody;
        bodyDef.position = new B2Vec2(position.X, position.Y);

        var bodyId = b2CreateBody(_worldId, bodyDef);

        var box = b2MakeBox(width / 2, height / 2);
        
        var shapeDef = b2DefaultShapeDef();
        shapeDef.density = 1.0f;
        shapeDef.material.friction = 0.3f;

        b2CreatePolygonShape(bodyId, shapeDef, box);
        _bodies[id] = bodyId;
    }

    /// <summary>
    /// 동적 바디(플레이어, 오브젝트 등)를 생성합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <param name="position">위치</param>
    /// <param name="width">폭</param>
    /// <param name="height">높이</param>
    /// <param name="density">밀도</param>
    /// <param name="friction">마찰력</param>
    /// <param name="restitution">반발력</param>
    public void CreateDynamicBox(string id, Vector2 position, float width, float height, 
        float density = 1.0f, float friction = 0.3f, float restitution = 0.5f)
    {
        var bodyDef = b2DefaultBodyDef();
        bodyDef.type = B2BodyType.b2_dynamicBody;
        bodyDef.position = new B2Vec2(position.X, position.Y);

        var bodyId = b2CreateBody(_worldId, bodyDef);

        var box = b2MakeBox(width / 2, height / 2);
        
        var shapeDef = b2DefaultShapeDef();
        shapeDef.density = density;
        shapeDef.material.friction = friction;
        shapeDef.material.restitution = restitution;

        b2CreatePolygonShape(bodyId, shapeDef, box);
        _bodies[id] = bodyId;
    }

    /// <summary>
    /// 원형 동적 바디를 생성합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <param name="position">위치</param>
    /// <param name="radius">반지름</param>
    /// <param name="density">밀도</param>
    /// <param name="friction">마찰력</param>
    /// <param name="restitution">반발력</param>
    public void CreateDynamicCircle(string id, Vector2 position, float radius, 
        float density = 1.0f, float friction = 0.3f, float restitution = 0.5f)
    {
        var bodyDef = b2DefaultBodyDef();
        bodyDef.type = B2BodyType.b2_dynamicBody;
        bodyDef.position = new B2Vec2(position.X, position.Y);

        var bodyId = b2CreateBody(_worldId, bodyDef);

        var circle = new B2Circle(new B2Vec2(0, 0), radius);
        
        var shapeDef = b2DefaultShapeDef();
        shapeDef.density = density;
        shapeDef.material.friction = friction;
        shapeDef.material.restitution = restitution;

        b2CreateCircleShape(bodyId, shapeDef, circle);
        _bodies[id] = bodyId;
    }

    /// <summary>
    /// 바디의 현재 위치를 가져옵니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <returns>위치 벡터</returns>
    public Vector2 GetPosition(string id)
    {
        if (_bodies.TryGetValue(id, out var bodyId))
        {
            var pos = b2Body_GetPosition(bodyId);
            return new Vector2(pos.X, pos.Y);
        }
        return Vector2.Zero;
    }

    /// <summary>
    /// 바디의 현재 회전 각도를 가져옵니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <returns>회전 각도 (라디안)</returns>
    public float GetAngle(string id)
    {
        if (_bodies.TryGetValue(id, out var bodyId))
        {
            var rotation = b2Body_GetRotation(bodyId);
            return b2Rot_GetAngle(rotation);
        }
        return 0f;
    }

    /// <summary>
    /// 바디에 힘을 가합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <param name="force">힘 벡터</param>
    /// <param name="point">힘을 가할 점 (월드 좌표)</param>
    public void ApplyForce(string id, Vector2 force, Vector2 point)
    {
        if (_bodies.TryGetValue(id, out var bodyId))
        {
            var forceVec = new B2Vec2(force.X, force.Y);
            var pointVec = new B2Vec2(point.X, point.Y);
            b2Body_ApplyForce(bodyId, forceVec, pointVec, true);
        }
    }

    /// <summary>
    /// 바디에 중심에 힘을 가합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <param name="force">힘 벡터</param>
    public void ApplyForceToCenter(string id, Vector2 force)
    {
        if (_bodies.TryGetValue(id, out var bodyId))
        {
            var forceVec = new B2Vec2(force.X, force.Y);
            b2Body_ApplyForceToCenter(bodyId, forceVec, true);
        }
    }

    /// <summary>
    /// 바디에 충격을 가합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <param name="impulse">충격 벡터</param>
    /// <param name="point">충격을 가할 점 (월드 좌표)</param>
    public void ApplyLinearImpulse(string id, Vector2 impulse, Vector2 point)
    {
        if (_bodies.TryGetValue(id, out var bodyId))
        {
            var impulseVec = new B2Vec2(impulse.X, impulse.Y);
            var pointVec = new B2Vec2(point.X, point.Y);
            b2Body_ApplyLinearImpulse(bodyId, impulseVec, pointVec, true);
        }
    }

    /// <summary>
    /// 바디의 속도를 설정합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <param name="velocity">속도 벡터</param>
    public void SetLinearVelocity(string id, Vector2 velocity)
    {
        if (_bodies.TryGetValue(id, out var bodyId))
        {
            var velocityVec = new B2Vec2(velocity.X, velocity.Y);
            b2Body_SetLinearVelocity(bodyId, velocityVec);
        }
    }

    /// <summary>
    /// 바디의 속도를 가져옵니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <returns>속도 벡터</returns>
    public Vector2 GetLinearVelocity(string id)
    {
        if (_bodies.TryGetValue(id, out var bodyId))
        {
            var vel = b2Body_GetLinearVelocity(bodyId);
            return new Vector2(vel.X, vel.Y);
        }
        return Vector2.Zero;
    }

    /// <summary>
    /// 바디를 제거합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    public void DestroyBody(string id)
    {
        if (_bodies.TryGetValue(id, out var bodyId))
        {
            b2DestroyBody(bodyId);
            _bodies.Remove(id);
        }
    }

    /// <summary>
    /// 등록된 모든 바디의 ID 목록을 가져옵니다.
    /// </summary>
    /// <returns>바디 ID 목록</returns>
    public IEnumerable<string> GetAllBodyIds()
    {
        return _bodies.Keys;
    }

    /// <summary>
    /// 월드에 존재하는 바디가 있는지 확인합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <returns>존재 여부</returns>
    public bool HasBody(string id)
    {
        return _bodies.ContainsKey(id);
    }

    /// <summary>
    /// 바디 ID를 가져옵니다. (그래픽 렌더링용)
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <returns>바디 ID</returns>
    public B2BodyId? GetBodyId(string id)
    {
        if (_bodies.TryGetValue(id, out var bodyId))
        {
            return bodyId;
        }
        return null;
    }

    /// <summary>
    /// 월드 ID를 가져옵니다.
    /// </summary>
    public B2WorldId WorldId => _worldId;

    /// <summary>
    /// 물리 월드를 정리합니다.
    /// </summary>
    public void Dispose()
    {
        b2DestroyWorld(_worldId);
    }
}
