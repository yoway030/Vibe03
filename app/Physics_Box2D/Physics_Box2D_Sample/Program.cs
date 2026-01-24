using System.Numerics;
using Physics_Box2D;

namespace Physics_Box2D_Sample;

/// <summary>
/// Box2D 물리 엔진의 기본 기능을 보여주는 콘솔 샘플 프로그램
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Physics_Box2D 샘플 프로그램 ===");
        Console.WriteLine();

        // 1. 기본 낙하 시뮬레이션
        Console.WriteLine("1. 기본 낙하 시뮬레이션");
        Console.WriteLine("------------------------");
        RunBasicFallSimulation();
        Console.WriteLine();

        // 2. 충돌 시뮬레이션
        Console.WriteLine("2. 충돌 시뮬레이션");
        Console.WriteLine("------------------------");
        RunCollisionSimulation();
        Console.WriteLine();

        // 3. 힘 적용 시뮬레이션
        Console.WriteLine("3. 힘 적용 시뮬레이션");
        Console.WriteLine("------------------------");
        RunForceSimulation();
        Console.WriteLine();

        // 4. 복수 오브젝트 시뮬레이션
        Console.WriteLine("4. 복수 오브젝트 시뮬레이션");
        Console.WriteLine("------------------------");
        RunMultipleObjectsSimulation();
        Console.WriteLine();

        Console.WriteLine("샘플 프로그램이 완료되었습니다.");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    /// <summary>
    /// 기본 낙하 시뮬레이션: 박스가 중력에 의해 떨어지는 모습을 시뮬레이션
    /// </summary>
    static void RunBasicFallSimulation()
    {
        var world = new PhysicsWorld(new Vector2(0, -10f));

        // 지면 생성
        world.CreateStaticBox("ground", new Vector2(0, -5), 20f, 1f);
        Console.WriteLine("지면을 생성했습니다. (위치: Y = -5)");

        // 떨어지는 박스 생성
        world.CreateDynamicBox("fallingBox", new Vector2(0, 10), 1f, 1f);
        Console.WriteLine("박스를 생성했습니다. (초기 위치: Y = 10)");
        Console.WriteLine();

        // 시뮬레이션 실행 (3초)
        float timeStep = 1f / 60f;
        int steps = 180; // 3초

        Console.WriteLine("시뮬레이션 시작...");
        for (int i = 0; i < steps; i++)
        {
            world.Step(timeStep);

            if (i % 30 == 0) // 0.5초마다 출력
            {
                var pos = world.GetPosition("fallingBox");
                var vel = world.GetLinearVelocity("fallingBox");
                Console.WriteLine($"  시간 {i * timeStep:F2}초: 위치 Y = {pos.Y:F2}, 속도 Y = {vel.Y:F2}");
            }
        }

        var finalPos = world.GetPosition("fallingBox");
        Console.WriteLine($"최종 위치: Y = {finalPos.Y:F2}");
    }

    /// <summary>
    /// 충돌 시뮬레이션: 튀는 공이 바닥과 충돌
    /// </summary>
    static void RunCollisionSimulation()
    {
        var world = new PhysicsWorld(new Vector2(0, -10f));

        // 지면 생성
        world.CreateStaticBox("ground", new Vector2(0, 0), 20f, 1f);

        // 튀는 공 생성 (반발력 높음)
        world.CreateDynamicCircle("ball", new Vector2(0, 10), 0.5f, 
            density: 1.0f, friction: 0.2f, restitution: 0.8f);
        
        Console.WriteLine("튀는 공이 떨어집니다. (반발력: 0.8)");
        Console.WriteLine();

        float timeStep = 1f / 60f;
        int steps = 240; // 4초

        float maxHeight = 10f;
        for (int i = 0; i < steps; i++)
        {
            world.Step(timeStep);

            var pos = world.GetPosition("ball");
            var vel = world.GetLinearVelocity("ball");

            // 속도 방향이 바뀌는 순간 (충돌 감지)
            if (i > 0 && Math.Abs(vel.Y) < 0.1f && pos.Y > 0.6f)
            {
                Console.WriteLine($"  바운스! 높이: {pos.Y:F2}m");
                maxHeight = pos.Y;
            }

            if (i % 60 == 0)
            {
                Console.WriteLine($"  시간 {i * timeStep:F2}초: 위치 Y = {pos.Y:F2}");
            }
        }
    }

    /// <summary>
    /// 힘 적용 시뮬레이션: 박스에 힘을 가해 이동
    /// </summary>
    static void RunForceSimulation()
    {
        var world = new PhysicsWorld(new Vector2(0, -10f));

        // 지면 생성
        world.CreateStaticBox("ground", new Vector2(0, 0), 50f, 1f);

        // 박스 생성
        world.CreateDynamicBox("box", new Vector2(0, 2), 1f, 1f);
        Console.WriteLine("박스에 오른쪽으로 힘을 가합니다.");
        Console.WriteLine();

        float timeStep = 1f / 60f;
        int steps = 180; // 3초

        for (int i = 0; i < steps; i++)
        {
            // 처음 1초 동안만 힘을 가함
            if (i < 60)
            {
                world.ApplyForceToCenter("box", new Vector2(50f, 0f));
            }

            world.Step(timeStep);

            if (i % 30 == 0)
            {
                var pos = world.GetPosition("box");
                var vel = world.GetLinearVelocity("box");
                Console.WriteLine($"  시간 {i * timeStep:F2}초: 위치 X = {pos.X:F2}, 속도 X = {vel.X:F2}");
            }
        }

        var finalPos = world.GetPosition("box");
        Console.WriteLine($"최종 위치: X = {finalPos.X:F2}");
    }

    /// <summary>
    /// 복수 오브젝트 시뮬레이션: 여러 박스가 동시에 떨어짐
    /// </summary>
    static void RunMultipleObjectsSimulation()
    {
        var world = new PhysicsWorld(new Vector2(0, -10f));

        // 지면 생성
        world.CreateStaticBox("ground", new Vector2(0, 0), 20f, 1f);

        // 여러 박스 생성
        Console.WriteLine("5개의 박스를 다른 높이에서 생성합니다.");
        for (int i = 0; i < 5; i++)
        {
            float x = (i - 2) * 2f;
            float y = 5f + i * 2f;
            world.CreateDynamicBox($"box{i}", new Vector2(x, y), 1f, 1f);
            Console.WriteLine($"  박스 {i}: X = {x:F1}, Y = {y:F1}");
        }
        Console.WriteLine();

        float timeStep = 1f / 60f;
        int steps = 180; // 3초

        Console.WriteLine("시뮬레이션 진행...");
        for (int i = 0; i < steps; i++)
        {
            world.Step(timeStep);

            if (i % 60 == 0)
            {
                Console.WriteLine($"\n시간 {i * timeStep:F2}초:");
                for (int j = 0; j < 5; j++)
                {
                    var pos = world.GetPosition($"box{j}");
                    Console.WriteLine($"  박스 {j}: X = {pos.X:F2}, Y = {pos.Y:F2}");
                }
            }
        }

        Console.WriteLine("\n최종 위치:");
        for (int j = 0; j < 5; j++)
        {
            var pos = world.GetPosition($"box{j}");
            Console.WriteLine($"  박스 {j}: X = {pos.X:F2}, Y = {pos.Y:F2}");
        }

        Console.WriteLine($"\n총 오브젝트 수: {world.GetAllBodyIds().Count()}");
    }
}
