using Box2D.NET;
using System.Numerics;
using static Box2D.NET.B2Worlds;
using static Box2D.NET.B2Types;

namespace Physics;

/// <summary>
/// Box2D 3.x 물리 ?�진???�핑???�래??
/// 게임 ?�드??물리 ?��??�이?�을 관리합?�다.
/// </summary>
public class World
{
    private readonly B2WorldId _worldId;
    private readonly Dictionary<string, IBody> _bodies;

    /// <summary>
    /// World ?�성??
    /// </summary>
    /// <param name="gravity">중력 벡터 (?? new Vector2(0, -10) ?�는 Vector2.Zero for 무중??</param>
    public World(Vector2 gravity)
    {
        var worldDef = b2DefaultWorldDef();
        worldDef.gravity = new B2Vec2(gravity.X, gravity.Y);
        
        _worldId = b2CreateWorld(ref worldDef);
        _bodies = new Dictionary<string, IBody>();
    }

    /// <summary>
    /// 물리 ?�드 ID
    /// </summary>
    public B2WorldId WorldId => _worldId;

    /// <summary>
    /// 물리 ?��??�이???�데?�트
    /// </summary>
    /// <param name="timeStep">?�간 간격 (�?</param>
    /// <param name="subStepCount">?�브 ?�텝 ?�수 (기본�? 4)</param>
    public void Step(float timeStep, int subStepCount = 4)
    {
        b2World_Step(_worldId, timeStep, subStepCount);
    }

    /// <summary>
    /// ?�적 박스 바디�??�성?�여 ?�드??추�??�니??
    /// </summary>
    /// <param name="id">바디 ?�별??/param>
    /// <param name="position">?�치</param>
    /// <param name="width">??/param>
    /// <param name="height">?�이</param>
    /// <param name="friction">마찰??/param>
    /// <returns>?�성??StaticBox</returns>
    public StaticBox CreateStaticBox(string id, Vector2 position, float width, float height, float friction = 0.3f)
    {
        var staticBox = new StaticBox(_worldId, id, position, width, height, friction);
        _bodies[id] = staticBox;
        return staticBox;
    }

    /// <summary>
    /// ?�적 박스 바디�??�성?�여 ?�드??추�??�니??
    /// </summary>
    /// <param name="id">바디 ?�별??/param>
    /// <param name="position">?�치</param>
    /// <param name="width">??/param>
    /// <param name="height">?�이</param>
    /// <param name="density">밀??/param>
    /// <param name="friction">마찰??/param>
    /// <param name="restitution">반발??/param>
    /// <returns>?�성??Box</returns>
    public Box CreateDynamicBox(string id, Vector2 position, float width, float height, 
        float density = 1.0f, float friction = 0.3f, float restitution = 0.5f)
    {
        var box = new Box(_worldId, id, position, width, height, density, friction, restitution);
        _bodies[id] = box;
        return box;
    }

    /// <summary>
    /// ?�적 ??바디�??�성?�여 ?�드??추�??�니??
    /// </summary>
    /// <param name="id">바디 ?�별??/param>
    /// <param name="position">?�치</param>
    /// <param name="radius">반�?�?/param>
    /// <param name="density">밀??/param>
    /// <param name="friction">마찰??/param>
    /// <param name="restitution">반발??/param>
    /// <returns>?�성??Circle</returns>
    public Circle CreateDynamicCircle(string id, Vector2 position, float radius, 
        float density = 1.0f, float friction = 0.3f, float restitution = 0.5f)
    {
        var circle = new Circle(_worldId, id, position, radius, density, friction, restitution);
        _bodies[id] = circle;
        return circle;
    }

    /// <summary>
    /// 바디�?ID�?가?�옵?�다.
    /// </summary>
    /// <param name="id">바디 ?�별??/param>
    /// <returns>바디 객체</returns>
    public IBody? GetBody(string id)
    {
        return _bodies.TryGetValue(id, out var body) ? body : null;
    }

    /// <summary>
    /// 바디???�재 ?�치�?가?�옵?�다.
    /// </summary>
    /// <param name="id">바디 ?�별??/param>
    /// <returns>?�치 벡터</returns>
    public Vector2 GetPosition(string id)
    {
        return _bodies.TryGetValue(id, out var body) ? body.Position : Vector2.Zero;
    }

    /// <summary>
    /// 바디???�재 ?�전 각도�?가?�옵?�다.
    /// </summary>
    /// <param name="id">바디 ?�별??/param>
    /// <returns>?�전 각도 (?�디??</returns>
    public float GetAngle(string id)
    {
        return _bodies.TryGetValue(id, out var body) ? body.Angle : 0f;
    }

    /// <summary>
    /// 바디???�을 가?�니??
    /// </summary>
    /// <param name="id">바디 ?�별??/param>
    /// <param name="force">??벡터</param>
    /// <param name="point">?�을 가????(?�드 좌표)</param>
    public void ApplyForce(string id, Vector2 force, Vector2 point)
    {
        if (_bodies.TryGetValue(id, out var body))
        {
            body.ApplyForce(force, point);
        }
    }

    /// <summary>
    /// 바디??중심???�을 가?�니??
    /// </summary>
    /// <param name="id">바디 ?�별??/param>
    /// <param name="force">??벡터</param>
    public void ApplyForceToCenter(string id, Vector2 force)
    {
        if (_bodies.TryGetValue(id, out var body))
        {
            body.ApplyForceToCenter(force);
        }
    }

    /// <summary>
    /// 바디??충격??가?�니??
    /// </summary>
    /// <param name="id">바디 ?�별??/param>
    /// <param name="impulse">충격 벡터</param>
    /// <param name="point">충격??가????(?�드 좌표)</param>
    public void ApplyLinearImpulse(string id, Vector2 impulse, Vector2 point)
    {
        if (_bodies.TryGetValue(id, out var body))
        {
            body.ApplyLinearImpulse(impulse, point);
        }
    }

    /// <summary>
    /// 바디???�도�??�정?�니??
    /// </summary>
    /// <param name="id">바디 ?�별??/param>
    /// <param name="velocity">?�도 벡터</param>
    public void SetLinearVelocity(string id, Vector2 velocity)
    {
        if (_bodies.TryGetValue(id, out var body))
        {
            body.LinearVelocity = velocity;
        }
    }

    /// <summary>
    /// 바디???�도�?가?�옵?�다.
    /// </summary>
    /// <param name="id">바디 ?�별??/param>
    /// <returns>?�도 벡터</returns>
    public Vector2 GetLinearVelocity(string id)
    {
        return _bodies.TryGetValue(id, out var body) ? body.LinearVelocity : Vector2.Zero;
    }

    /// <summary>
    /// 바디�??�거?�니??
    /// </summary>
    /// <param name="id">바디 ?�별??/param>
    public void DestroyBody(string id)
    {
        if (_bodies.TryGetValue(id, out var body))
        {
            body.Destroy();
            _bodies.Remove(id);
        }
    }

    /// <summary>
    /// ?�록??모든 바디??ID 목록??가?�옵?�다.
    /// </summary>
    /// <returns>바디 ID 목록</returns>
    public IEnumerable<string> GetAllBodyIds()
    {
        return _bodies.Keys;
    }

    /// <summary>
    /// ?�드??존재?�는 바디가 ?�는지 ?�인?�니??
    /// </summary>
    /// <param name="id">바디 ?�별??/param>
    /// <returns>존재 ?��?</returns>
    public bool HasBody(string id)
    {
        return _bodies.ContainsKey(id);
    }

    /// <summary>
    /// 바디 ID�?가?�옵?�다. (그래???�더링용)
    /// </summary>
    /// <param name="id">바디 ?�별??/param>
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
    /// 물리 ?�드�??�리?�니??
    /// </summary>
    public void Dispose()
    {
        b2DestroyWorld(_worldId);
    }
}
