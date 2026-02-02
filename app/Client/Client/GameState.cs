using System.Numerics;

namespace Client;

/// <summary>
/// 게임 상태 스냅샷 (렌더링용 읽기 전용 데이터)
/// </summary>
public class GameState
{
    /// <summary>
    /// 대포 상태 배열
    /// </summary>
    public CannonState[] Cannons { get; set; }

    /// <summary>
    /// 영역 셀 상태 배열 (2D 배열을 1D로 펼침)
    /// </summary>
    public CellState?[] Cells { get; set; }

    /// <summary>
    /// 포탄 상태 리스트
    /// </summary>
    public List<ProjectileState> Projectiles { get; set; }

    /// <summary>
    /// 폭발 상태 리스트
    /// </summary>
    public List<ExplosionState> Explosions { get; set; }

    /// <summary>
    /// 병합된 영역 정보 (렌더링 최적화용)
    /// </summary>
    public List<MergedRegionState> MergedRegions { get; set; }

    /// <summary>
    /// 현재 게임 시간
    /// </summary>
    public float CurrentTime { get; set; }

    /// <summary>
    /// 그리드 크기
    /// </summary>
    public int GridSize { get; set; }

    public GameState()
    {
        Cannons = Array.Empty<CannonState>();
        Cells = Array.Empty<CellState?>();
        Projectiles = new List<ProjectileState>();
        Explosions = new List<ExplosionState>();
        MergedRegions = new List<MergedRegionState>();
        CurrentTime = 0;
        GridSize = 0;
    }
}

/// <summary>
/// 대포 상태
/// </summary>
public struct CannonState
{
    public int Id;
    public Vector2 Position;
    public Vector3 Color;
    public Vector2 FireDirection;
    public int Score;
}

/// <summary>
/// 셀 상태
/// </summary>
public struct CellState
{
    public Vector2 Position;
    public float Size;
    public int OwnerCannonId;
    public int PreviousOwnerCannonId;
    public float TransitionStartTime;
    public float Durability;
}

/// <summary>
/// 포탄 상태
/// </summary>
public struct ProjectileState
{
    public int CannonId;
    public Vector2 Position;
    public Vector2 StartPosition;
    public float Radius;
}

/// <summary>
/// 폭발 상태
/// </summary>
public struct ExplosionState
{
    public Vector2 Position;
    public Vector3 Color;
    public float StartTime;
}

/// <summary>
/// 병합된 영역 상태
/// </summary>
public struct MergedRegionState
{
    public int OwnerId;
    public int StartX;
    public int StartY;
    public int Width;
    public int Height;
}
