using System.Numerics;
using Box2D.NET;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2MathFunction;

namespace Physics;

/// <summary>
/// 물리 바디??기본 구현
/// </summary>
public abstract class BodyBase : IBody
{
    protected B2BodyId _bodyId;
    protected bool _isDestroyed;

    public string Id { get; }
    public B2BodyId BodyId => _bodyId;

    public Vector2 Position
    {
        get
        {
            if (_isDestroyed) return Vector2.Zero;
            var pos = b2Body_GetPosition(_bodyId);
            return new Vector2(pos.X, pos.Y);
        }
    }

    public float Angle
    {
        get
        {
            if (_isDestroyed) return 0f;
            var rotation = b2Body_GetRotation(_bodyId);
            return b2Rot_GetAngle(rotation);
        }
    }

    public Vector2 LinearVelocity
    {
        get
        {
            if (_isDestroyed) return Vector2.Zero;
            var vel = b2Body_GetLinearVelocity(_bodyId);
            return new Vector2(vel.X, vel.Y);
        }
        set
        {
            if (_isDestroyed) return;
            var velocityVec = new B2Vec2(value.X, value.Y);
            b2Body_SetLinearVelocity(_bodyId, velocityVec);
        }
    }

    protected BodyBase(string id)
    {
        Id = id;
        _isDestroyed = false;
    }

    public void ApplyForce(Vector2 force, Vector2 point)
    {
        if (_isDestroyed) return;
        var forceVec = new B2Vec2(force.X, force.Y);
        var pointVec = new B2Vec2(point.X, point.Y);
        b2Body_ApplyForce(_bodyId, forceVec, pointVec, true);
    }

    public void ApplyForceToCenter(Vector2 force)
    {
        if (_isDestroyed) return;
        var forceVec = new B2Vec2(force.X, force.Y);
        b2Body_ApplyForceToCenter(_bodyId, forceVec, true);
    }

    public void ApplyLinearImpulse(Vector2 impulse, Vector2 point)
    {
        if (_isDestroyed) return;
        var impulseVec = new B2Vec2(impulse.X, impulse.Y);
        var pointVec = new B2Vec2(point.X, point.Y);
        b2Body_ApplyLinearImpulse(_bodyId, impulseVec, pointVec, true);
    }

    public void Destroy()
    {
        if (_isDestroyed) return;
        b2DestroyBody(_bodyId);
        _isDestroyed = true;
    }
}
