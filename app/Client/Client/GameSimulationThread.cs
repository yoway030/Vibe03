using System.Diagnostics;

namespace Client;

/// <summary>
/// 게임 시뮬레이션 스레드 (물리 및 게임 로직 담당)
/// 렌더링과 완전히 분리되어 독립적으로 실행
/// </summary>
public class GameSimulationThread
{
    private readonly GameSimulation _simulation;
    private Thread? _thread;
    private volatile bool _running;
    private readonly object _stateLock = new object();
    private GameState _currentState;
    private readonly float _targetTickRate;

    /// <summary>
    /// 현재 게임 상태 (스레드 안전)
    /// </summary>
    public GameState CurrentState
    {
        get
        {
            lock (_stateLock)
            {
                return _currentState;
            }
        }
    }

    public GameSimulationThread(float tickRate = 60.0f)
    {
        _simulation = new GameSimulation();
        _targetTickRate = tickRate;
        _currentState = new GameState();
        _running = false;
    }

    /// <summary>
    /// 시뮬레이션 스레드 시작
    /// </summary>
    public void Start()
    {
        if (_running) return;

        _running = true;
        _thread = new Thread(SimulationLoop)
        {
            Name = "GameSimulation",
            IsBackground = true
        };
        _thread.Start();
    }

    /// <summary>
    /// 시뮬레이션 스레드 정지
    /// </summary>
    public void Stop()
    {
        _running = false;
        _thread?.Join();
    }

    /// <summary>
    /// 시뮬레이션 루프 (별도 스레드에서 실행)
    /// </summary>
    private void SimulationLoop()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        float accumulator = 0;
        float fixedDeltaTime = 1.0f / _targetTickRate;
        long lastTicks = stopwatch.ElapsedTicks;

        while (_running)
        {
            long currentTicks = stopwatch.ElapsedTicks;
            float deltaTime = (currentTicks - lastTicks) / (float)Stopwatch.Frequency;
            lastTicks = currentTicks;

            accumulator += deltaTime;

            // 고정 시간 간격으로 업데이트 (물리 안정성)
            while (accumulator >= fixedDeltaTime)
            {
                _simulation.Update(fixedDeltaTime);
                accumulator -= fixedDeltaTime;
            }

            // 상태 스냅샷 생성 및 업데이트
            UpdateStateSnapshot();

            // CPU 사용률 조절
            Thread.Sleep(1);
        }
    }

    /// <summary>
    /// 현재 시뮬레이션 상태를 스냅샷으로 복사
    /// </summary>
    private void UpdateStateSnapshot()
    {
        var newState = new GameState
        {
            CurrentTime = _simulation.CurrentTime,
            GridSize = _simulation.GridSize,
            Cannons = new CannonState[_simulation.Cannons.Length],
            Cells = new CellState?[_simulation.GridSize * _simulation.GridSize],
            Projectiles = new List<ProjectileState>(_simulation.Projectiles.Count),
            Explosions = new List<ExplosionState>(_simulation.Explosions.Count),
            MergedRegions = new List<MergedRegionState>(_simulation.MergedRegions.Count)
        };

        // 대포 상태 복사
        for (int i = 0; i < _simulation.Cannons.Length; i++)
        {
            var cannon = _simulation.Cannons[i];
            newState.Cannons[i] = new CannonState
            {
                Id = cannon.Id,
                Position = cannon.Position,
                Color = cannon.Color,
                FireDirection = cannon.GetCurrentFireDirection(),
                Score = cannon.Score
            };
        }

        // 셀 상태 복사 (2D -> 1D)
        for (int x = 0; x < _simulation.GridSize; x++)
        {
            for (int y = 0; y < _simulation.GridSize; y++)
            {
                var cell = _simulation.Cells[x, y];
                int index = y * _simulation.GridSize + x;
                
                if (cell != null)
                {
                    newState.Cells[index] = new CellState
                    {
                        Position = cell.Position,
                        Size = cell.Size,
                        OwnerCannonId = cell.OwnerCannonId,
                        PreviousOwnerCannonId = cell.PreviousOwnerCannonId,
                        TransitionStartTime = cell.TransitionStartTime,
                        Durability = cell.Durability
                    };
                }
                else
                {
                    newState.Cells[index] = null;
                }
            }
        }

        // 포탄 상태 복사
        foreach (var projectile in _simulation.Projectiles)
        {
            newState.Projectiles.Add(new ProjectileState
            {
                CannonId = projectile.CannonId,
                Position = projectile.Position,
                StartPosition = projectile.StartPosition,
                Radius = projectile.Radius
            });
        }

        // 폭발 상태 복사
        foreach (var explosion in _simulation.Explosions)
        {
            newState.Explosions.Add(new ExplosionState
            {
                Position = explosion.Position,
                Color = explosion.Color,
                StartTime = explosion.StartTime
            });
        }

        // 병합 영역 상태 복사
        foreach (var region in _simulation.MergedRegions)
        {
            newState.MergedRegions.Add(new MergedRegionState
            {
                OwnerId = region.OwnerId,
                StartX = region.StartX,
                StartY = region.StartY,
                Width = region.Width,
                Height = region.Height
            });
        }

        // 스레드 안전하게 상태 업데이트
        lock (_stateLock)
        {
            _currentState = newState;
        }
    }
}
