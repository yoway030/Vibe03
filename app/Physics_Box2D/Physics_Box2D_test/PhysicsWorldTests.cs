using System.Numerics;
using Xunit;

namespace Physics_Box2D_test;

/// <summary>
/// PhysicsWorld 클래스의 기본 기능을 테스트합니다.
/// </summary>
public class PhysicsWorldTests
{
    [Fact]
    public void PhysicsWorld_Creation_ShouldSucceed()
    {
        // Arrange & Act
        var world = new PhysicsWorld();

        // Assert
        Assert.NotNull(world);
    }

    [Fact]
    public void PhysicsWorld_Creation_WithCustomGravity_ShouldSucceed()
    {
        // Arrange
        var gravity = new Vector2(0, -20f);

        // Act
        var world = new PhysicsWorld(gravity);

        // Assert
        Assert.NotNull(world);
    }

    [Fact]
    public void CreateStaticBox_ShouldAddBodyToWorld()
    {
        // Arrange
        var world = new PhysicsWorld();
        var id = "ground";
        var position = new Vector2(0, 0);

        // Act
        world.CreateStaticBox(id, position, 10f, 1f);

        // Assert
        Assert.True(world.HasBody(id));
        Assert.Equal(position, world.GetPosition(id));
    }

    [Fact]
    public void CreateDynamicBox_ShouldAddBodyToWorld()
    {
        // Arrange
        var world = new PhysicsWorld();
        var id = "box";
        var position = new Vector2(0, 10);

        // Act
        world.CreateDynamicBox(id, position, 1f, 1f);

        // Assert
        Assert.True(world.HasBody(id));
        Assert.Equal(position, world.GetPosition(id));
    }

    [Fact]
    public void CreateDynamicCircle_ShouldAddBodyToWorld()
    {
        // Arrange
        var world = new PhysicsWorld();
        var id = "ball";
        var position = new Vector2(0, 10);

        // Act
        world.CreateDynamicCircle(id, position, 0.5f);

        // Assert
        Assert.True(world.HasBody(id));
        Assert.Equal(position, world.GetPosition(id));
    }

    [Fact]
    public void Step_WithDynamicBody_ShouldUpdatePosition()
    {
        // Arrange
        var world = new PhysicsWorld(new Vector2(0, -10f));
        var id = "box";
        var initialPosition = new Vector2(0, 10);
        world.CreateDynamicBox(id, initialPosition, 1f, 1f);

        // Act
        for (int i = 0; i < 60; i++) // 1초 시뮬레이션 (60 프레임)
        {
            world.Step(1f / 60f);
        }
        var finalPosition = world.GetPosition(id);

        // Assert
        Assert.True(finalPosition.Y < initialPosition.Y, "박스가 중력에 의해 아래로 떨어져야 합니다.");
    }

    [Fact]
    public void SetLinearVelocity_ShouldUpdateVelocity()
    {
        // Arrange
        var world = new PhysicsWorld();
        var id = "box";
        world.CreateDynamicBox(id, new Vector2(0, 0), 1f, 1f);
        var targetVelocity = new Vector2(5f, 0f);

        // Act
        world.SetLinearVelocity(id, targetVelocity);
        var actualVelocity = world.GetLinearVelocity(id);

        // Assert
        Assert.Equal(targetVelocity.X, actualVelocity.X, 0.01f);
        Assert.Equal(targetVelocity.Y, actualVelocity.Y, 0.01f);
    }

    [Fact]
    public void ApplyForceToCenter_ShouldAffectVelocity()
    {
        // Arrange
        var world = new PhysicsWorld(new Vector2(0, 0)); // 중력 없음
        var id = "box";
        world.CreateDynamicBox(id, new Vector2(0, 0), 1f, 1f);
        var force = new Vector2(100f, 0f);

        // Act
        world.ApplyForceToCenter(id, force);
        for (int i = 0; i < 60; i++) // 1초 시뮬레이션
        {
            world.Step(1f / 60f);
        }
        var velocity = world.GetLinearVelocity(id);

        // Assert
        Assert.True(velocity.X > 0, "힘을 가한 방향으로 속도가 증가해야 합니다.");
    }

    [Fact]
    public void ApplyLinearImpulse_ShouldImmediatelyChangeVelocity()
    {
        // Arrange
        var world = new PhysicsWorld(new Vector2(0, 0));
        var id = "box";
        var position = new Vector2(0, 0);
        world.CreateDynamicBox(id, position, 1f, 1f, density: 1.0f);
        var impulse = new Vector2(10f, 0f);

        // Act
        world.ApplyLinearImpulse(id, impulse, position);
        var velocity = world.GetLinearVelocity(id);

        // Assert
        Assert.True(velocity.X > 0, "충격이 가해진 후 속도가 즉시 변경되어야 합니다.");
    }

    [Fact]
    public void DestroyBody_ShouldRemoveBodyFromWorld()
    {
        // Arrange
        var world = new PhysicsWorld();
        var id = "box";
        world.CreateDynamicBox(id, new Vector2(0, 0), 1f, 1f);
        Assert.True(world.HasBody(id));

        // Act
        world.DestroyBody(id);

        // Assert
        Assert.False(world.HasBody(id));
    }

    [Fact]
    public void GetAllBodyIds_ShouldReturnAllBodies()
    {
        // Arrange
        var world = new PhysicsWorld();
        world.CreateDynamicBox("box1", new Vector2(0, 0), 1f, 1f);
        world.CreateDynamicBox("box2", new Vector2(2, 0), 1f, 1f);
        world.CreateStaticBox("ground", new Vector2(0, -5), 10f, 1f);

        // Act
        var ids = world.GetAllBodyIds().ToList();

        // Assert
        Assert.Equal(3, ids.Count);
        Assert.Contains("box1", ids);
        Assert.Contains("box2", ids);
        Assert.Contains("ground", ids);
    }

    [Fact]
    public void GetAngle_ShouldReturnBodyAngle()
    {
        // Arrange
        var world = new PhysicsWorld();
        var id = "box";
        world.CreateDynamicBox(id, new Vector2(0, 0), 1f, 1f);

        // Act
        var angle = world.GetAngle(id);

        // Assert
        Assert.Equal(0f, angle, 0.01f);
    }

    [Fact]
    public void GetPosition_ForNonExistentBody_ShouldReturnZero()
    {
        // Arrange
        var world = new PhysicsWorld();

        // Act
        var position = world.GetPosition("nonexistent");

        // Assert
        Assert.Equal(Vector2.Zero, position);
    }

    [Fact]
    public void HasBody_ForNonExistentBody_ShouldReturnFalse()
    {
        // Arrange
        var world = new PhysicsWorld();

        // Act & Assert
        Assert.False(world.HasBody("nonexistent"));
    }
}
