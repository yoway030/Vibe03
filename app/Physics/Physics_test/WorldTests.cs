using System.Numerics;
using Xunit;
using Physics;

namespace Physics_test;

/// <summary>
/// World ?¥Îûò?§Ïùò Í∏∞Î≥∏ Í∏∞Îä•???åÏä§?∏Ìï©?àÎã§.
/// </summary>
public class WorldTests
{
    [Fact]
    public void World_Creation_ShouldSucceed()
    {
        // Arrange & Act
        var world = new World(new Vector2(0, -10f));

        // Assert
        Assert.NotNull(world);
    }

    [Fact]
    public void World_Creation_WithCustomGravity_ShouldSucceed()
    {
        // Arrange
        var gravity = new Vector2(0, -20f);

        // Act
        var world = new World(gravity);

        // Assert
        Assert.NotNull(world);
    }

    [Fact]
    public void CreateStaticBox_ShouldAddBodyToWorld()
    {
        // Arrange
        var world = new World(new Vector2(0, -10f));
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
        var world = new World(new Vector2(0, -10f));
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
        var world = new World(new Vector2(0, -10f));
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
        var world = new World(new Vector2(0, -10f));
        var id = "box";
        var initialPosition = new Vector2(0, 10);
        world.CreateDynamicBox(id, initialPosition, 1f, 1f);

        // Act
        for (int i = 0; i < 60; i++) // 1Ï¥??úÎ??àÏù¥??(60 ?ÑÎ†à??
        {
            world.Step(1f / 60f);
        }
        var finalPosition = world.GetPosition(id);

        // Assert
        Assert.True(finalPosition.Y < initialPosition.Y, "Î∞ïÏä§Í∞Ä Ï§ëÎ†•???òÌï¥ ?ÑÎûòÎ°??®Ïñ¥?∏Ïïº ?©Îãà??");
    }

    [Fact]
    public void SetLinearVelocity_ShouldUpdateVelocity()
    {
        // Arrange
        var world = new World(new Vector2(0, -10f));
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
        var world = new World(new Vector2(0, 0)); // Ï§ëÎ†• ?ÜÏùå
        var id = "box";
        world.CreateDynamicBox(id, new Vector2(0, 0), 1f, 1f);
        var force = new Vector2(100f, 0f);

        // Act
        world.ApplyForceToCenter(id, force);
        for (int i = 0; i < 60; i++) // 1Ï¥??úÎ??àÏù¥??
        {
            world.Step(1f / 60f);
        }
        var velocity = world.GetLinearVelocity(id);

        // Assert
        Assert.True(velocity.X > 0, "?òÏùÑ Í∞Ä??Î∞©Ìñ•?ºÎ°ú ?çÎèÑÍ∞Ä Ï¶ùÍ??¥Ïïº ?©Îãà??");
    }

    [Fact]
    public void ApplyLinearImpulse_ShouldImmediatelyChangeVelocity()
    {
        // Arrange
        var world = new World(new Vector2(0, 0));
        var id = "box";
        var position = new Vector2(0, 0);
        world.CreateDynamicBox(id, position, 1f, 1f, density: 1.0f);
        var impulse = new Vector2(10f, 0f);

        // Act
        world.ApplyLinearImpulse(id, impulse, position);
        var velocity = world.GetLinearVelocity(id);

        // Assert
        Assert.True(velocity.X > 0, "Ï∂©Í≤©??Í∞Ä?¥ÏßÑ ???çÎèÑÍ∞Ä Ï¶âÏãú Î≥ÄÍ≤ΩÎêò?¥Ïïº ?©Îãà??");
    }

    [Fact]
    public void DestroyBody_ShouldRemoveBodyFromWorld()
    {
        // Arrange
        var world = new World(new Vector2(0, -10f));
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
        var world = new World(new Vector2(0, -10f));
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
        var world = new World(new Vector2(0, -10f));
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
        var world = new World(new Vector2(0, -10f));

        // Act
        var position = world.GetPosition("nonexistent");

        // Assert
        Assert.Equal(Vector2.Zero, position);
    }

    [Fact]
    public void HasBody_ForNonExistentBody_ShouldReturnFalse()
    {
        // Arrange
        var world = new World(new Vector2(0, -10f));

        // Act & Assert
        Assert.False(world.HasBody("nonexistent"));
    }

    [Fact]
    public void World_WithZeroGravity_ShouldNotFall()
    {
        // Arrange
        var world = new World(Vector2.Zero); // Î¨¥Ï§ë??
        var id = "box";
        var initialPosition = new Vector2(0, 10);
        world.CreateDynamicBox(id, initialPosition, 1f, 1f);

        // Act
        for (int i = 0; i < 60; i++) // 1Ï¥??úÎ??àÏù¥??
        {
            world.Step(1f / 60f);
        }
        var finalPosition = world.GetPosition(id);

        // Assert
        Assert.Equal(initialPosition.Y, finalPosition.Y, 0.1f);
    }
}
