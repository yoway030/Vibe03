using Box2D.NET;
using System.Numerics;
using static Box2D.NET.B2Worlds;
using static Box2D.NET.B2Types;

namespace Physics_Box2D;

/// <summary>
/// Box2D 3.x 물리 엔진을 래핑한 클래스
/// 게임 월드의 물리 시뮬레이션을 관리합니다.
/// </summary>
public class PhysicsWorld
{
    private readonly B2WorldId _worldId;
    private readonly Dictionary<string, IPhysicsBody> _bodies;

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
        
        _worldId = b2CreateWorld(ref worldDef);
        _bodies = new Dictionary<string, IPhysicsBody>();
    }

    /// <summary>
    /// 물리 월드 ID
    /// </summary>
    public B2WorldId WorldId => _worldId;

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
    /// 정적 박스 바디를 생성하여 월드에 추가합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <param name="position">위치</param>
    /// <param name="width">폭</param>
    /// <param name="height">높이</param>
    /// <param name="friction">마찰력</param>
    /// <returns>생성된 PhysicsStaticBox</returns>
    public PhysicsStaticBox CreateStaticBox(string id, Vector2 position, float width, float height, float friction = 0.3f)
    {
        var staticBox = new PhysicsStaticBox(_worldId, id, position, width, height, friction);
        _bodies[id] = staticBox;
        return staticBox;
    }

    /// <summary>
    /// 동적 박스 바디를 생성하여 월드에 추가합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <param name="position">위치</param>
    /// <param name="width">폭</param>
    /// <param name="height">높이</param>
    /// <param name="density">밀도</param>
    /// <param name="friction">마찰력</param>
    /// <param name="restitution">반발력</param>
    /// <returns>생성된 PhysicsBox</returns>
    public PhysicsBox CreateDynamicBox(string id, Vector2 position, float width, float height, 
        float density = 1.0f, float friction = 0.3f, float restitution = 0.5f)
    {
        var box = new PhysicsBox(_worldId, id, position, width, height, density, friction, restitution);
        _bodies[id] = box;
        return box;
    }

    /// <summary>
    /// 동적 원 바디를 생성하여 월드에 추가합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <param name="position">위치</param>
    /// <param name="radius">반지름</param>
    /// <param name="density">밀도</param>
    /// <param name="friction">마찰력</param>
    /// <param name="restitution">반발력</param>
    /// <returns>생성된 PhysicsCircle</returns>
    public PhysicsCircle CreateDynamicCircle(string id, Vector2 position, float radius, 
        float density = 1.0f, float friction = 0.3f, float restitution = 0.5f)
    {
        var circle = new PhysicsCircle(_worldId, id, position, radius, density, friction, restitution);
        _bodies[id] = circle;
        return circle;
    }

    /// <summary>
    /// 바디를 ID로 가져옵니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <returns>바디 객체</returns>
    public IPhysicsBody? GetBody(string id)
    {
        return _bodies.TryGetValue(id, out var body) ? body : null;
    }

    /// <summary>
    /// 바디의 현재 위치를 가져옵니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <returns>위치 벡터</returns>
    public Vector2 GetPosition(string id)
    {
        return _bodies.TryGetValue(id, out var body) ? body.Position : Vector2.Zero;
    }

    /// <summary>
    /// 바디의 현재 회전 각도를 가져옵니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <returns>회전 각도 (라디안)</returns>
    public float GetAngle(string id)
    {
        return _bodies.TryGetValue(id, out var body) ? body.Angle : 0f;
    }

    /// <summary>
    /// 바디에 힘을 가합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <param name="force">힘 벡터</param>
    /// <param name="point">힘을 가할 점 (월드 좌표)</param>
    public void ApplyForce(string id, Vector2 force, Vector2 point)
    {
        if (_bodies.TryGetValue(id, out var body))
        {
            body.ApplyForce(force, point);
        }
    }

    /// <summary>
    /// 바디에 중심에 힘을 가합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <param name="force">힘 벡터</param>
    public void ApplyForceToCenter(string id, Vector2 force)
    {
        if (_bodies.TryGetValue(id, out var body))
        {
            body.ApplyForceToCenter(force);
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
        if (_bodies.TryGetValue(id, out var body))
        {
            body.ApplyLinearImpulse(impulse, point);
        }
    }

    /// <summary>
    /// 바디의 속도를 설정합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <param name="velocity">속도 벡터</param>
    public void SetLinearVelocity(string id, Vector2 velocity)
    {
        if (_bodies.TryGetValue(id, out var body))
        {
            body.LinearVelocity = velocity;
        }
    }

    /// <summary>
    /// 바디의 속도를 가져옵니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    /// <returns>속도 벡터</returns>
    public Vector2 GetLinearVelocity(string id)
    {
        return _bodies.TryGetValue(id, out var body) ? body.LinearVelocity : Vector2.Zero;
    }

    /// <summary>
    /// 바디를 제거합니다.
    /// </summary>
    /// <param name="id">바디 식별자</param>
    public void DestroyBody(string id)
    {
        if (_bodies.TryGetValue(id, out var body))
        {
            body.Destroy();
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
        if (_bodies.TryGetValue(id, out var body))
        {
            return body.BodyId;
        }
        return null;
    }

    /// <summary>
    /// 물리 월드를 정리합니다.
    /// </summary>
    public void Dispose()
    {
        b2DestroyWorld(_worldId);
    }
}
