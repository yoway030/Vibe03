using Client;

Console.WriteLine("=== 영역 전쟁 게임 시뮬레이션 ===");
Console.WriteLine("그래픽 모드로 시작합니다...\n");

Console.WriteLine($"게임 공간: {GameSimulation.WorldSize}x{GameSimulation.WorldSize}");
Console.WriteLine($"대포 개수: {GameSimulation.CannonCount}");
Console.WriteLine($"영역 상자 크기: {GameSimulation.CellSize}x{GameSimulation.CellSize}");
Console.WriteLine($"총 영역 상자 수: {GameSimulation.WorldSize / GameSimulation.CellSize}x{GameSimulation.WorldSize / GameSimulation.CellSize}");
Console.WriteLine("\n윈도우가 열립니다...\n");

// Raylib 초기화
Renderer.Initialize();

// 카메라 생성
var camera = new Camera(new System.Numerics.Vector2(50, 50), 0.1f);

// 게임 시뮬레이션 스레드 생성 및 시작
var simulationThread = new GameSimulationThread();
simulationThread.Start();

// 게임 루프
const float PrintInterval = 10.0f; // 10초마다 콘솔 출력
float nextPrintTime = PrintInterval;
float lastFrameTime = 0;

while (!Renderer.ShouldClose())
{
    // 실제 경과 시간 계산 (FPS에 무관하게)
    float currentTime = (float)Raylib_cs.Raylib.GetTime();
    float deltaTime = currentTime - lastFrameTime;
    lastFrameTime = currentTime;
    
    // deltaTime 제한 (너무 큰 값 방지)
    if (deltaTime > 0.1f) deltaTime = 0.1f;
    
    // 카메라 업데이트 (입력 처리)
    camera.Update(deltaTime);
    
    // 현재 게임 상태 가져오기 (스냅샷)
    var gameState = simulationThread.CurrentState;

    // 렌더링
    Renderer.Render(gameState, camera);

    // 일정 간격으로 콘솔에 상태 출력
    if (gameState.CurrentTime >= nextPrintTime)
    {
        PrintStats(gameState);
        nextPrintTime += PrintInterval;
    }
}

// 시뮬레이션 스레드 정지
simulationThread.Stop();

// 최종 결과 출력
Console.WriteLine("\n\n=== 최종 결과 ===");
var finalState = simulationThread.CurrentState;
PrintStats(finalState);

var finalStats = CalculateFinalStats(finalState);
var winner = finalStats.Where(kv => kv.Key >= 0).OrderByDescending(kv => kv.Value).FirstOrDefault();

if (winner.Value > 0)
{
    Console.WriteLine($"\n최다 영역 점령: 대포 #{winner.Key} - {winner.Value} 영역!");
}

Renderer.Close();

// 통계 출력 함수
static void PrintStats(GameState state)
{
    var stats = CalculateFinalStats(state);
    
    Console.WriteLine($"\n[시간: {state.CurrentTime:F1}초]");
    Console.WriteLine($"중립 영역: {stats[-1]}");
    
    var topCannons = stats
        .Where(kv => kv.Key >= 0 && kv.Value > 0)
        .OrderByDescending(kv => kv.Value)
        .Take(5);
    
    Console.WriteLine("상위 5 대포:");
    int rank = 1;
    foreach (var kv in topCannons)
    {
        Console.WriteLine($"  {rank}. 대포 #{kv.Key}: {kv.Value} 영역");
        rank++;
    }
}

// 최종 통계 계산 함수
static Dictionary<int, int> CalculateFinalStats(GameState state)
{
    var stats = new Dictionary<int, int>();
    
    // 초기화
    stats[-1] = 0;
    for (int i = 0; i < GameSimulation.CannonCount; i++)
    {
        stats[i] = 0;
    }
    
    // 셀 카운트
    foreach (var cell in state.Cells)
    {
        if (cell != null)
        {
            stats[cell.Value.OwnerCannonId]++;
        }
    }
    
    return stats;
}
Console.WriteLine("\n게임이 종료되었습니다.");

// Raylib 종료
Renderer.Close();
