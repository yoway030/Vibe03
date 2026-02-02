using System.Numerics;

namespace Client;

/// <summary>
/// 게임 시뮬레이션 관리
/// </summary>
public class GameSimulation
{
    /// <summary>
    /// 게임 공간 크기
    /// </summary>
    public const float WorldSize = 100000.0f;

    /// <summary>
    /// 대포 개수
    /// </summary>
    public const int CannonCount = 100;

    /// <summary>
    /// 대포가 배치된 원의 반지름
    /// </summary>
    public const float CannonRadius = 45000.0f;

    /// <summary>
    /// 영역 상자 크기 (성능을 위해 500으로 설정)
    /// </summary>
    public const float CellSize = 500.0f;

    /// <summary>
    /// 게임 중심
    /// </summary>
    public Vector2 Center { get; }

    /// <summary>
    /// 대포 배열
    /// </summary>
    public Cannon[] Cannons { get; }

    /// <summary>
    /// 영역 상자들
    /// </summary>
    public TerritoryCell[,] Cells { get; }

    /// <summary>
    /// 활성 포탄들
    /// </summary>
    public List<Projectile> Projectiles { get; }

    /// <summary>
    /// 활성 폭발 이펙트들
    /// </summary>
    public List<Explosion> Explosions { get; }

    /// <summary>
    /// 병합된 영역 정보 (충돌 최적화용)
    /// </summary>
    public List<MergedRegion> MergedRegions { get; private set; }

    /// <summary>
    /// 그리드 크기 (한 변의 셀 개수)
    /// </summary>
    public int GridSize { get; }

    /// <summary>
    /// 현재 게임 시간
    /// </summary>
    public float CurrentTime { get; private set; }

    /// <summary>
    /// 포탄 생성 카운터
    /// </summary>
    private int _projectileCounter;

    public GameSimulation()
    {
        Center = new Vector2(WorldSize / 2, WorldSize / 2);
        
        // 대포 생성
        Cannons = Cannon.CreateCannons(CannonCount, Center, CannonRadius);

        // 영역 상자 그리드 생성 (원 내부만)
        GridSize = (int)(WorldSize / CellSize);
        Cells = new TerritoryCell[GridSize, GridSize];

        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                Vector2 position = new Vector2(
                    x * CellSize + CellSize / 2,
                    y * CellSize + CellSize / 2
                );
                
                // 중심으로부터의 거리 계산
                float distanceFromCenter = Vector2.Distance(position, Center);
                
                // 원 내부에만 셀 생성 (대포 배치 반지름 이내)
                if (distanceFromCenter <= CannonRadius)
                {
                    Cells[x, y] = new TerritoryCell($"cell_{x}_{y}", position, CellSize);
                }
                else
                {
                    Cells[x, y] = null; // 원 밖은 null
                }
            }
        }

        Projectiles = new List<Projectile>();
        Explosions = new List<Explosion>();
        MergedRegions = new List<MergedRegion>();
        CurrentTime = 0;
        _projectileCounter = 0;
        
        // 초기 병합 영역 생성
        RebuildMergedRegions();
    }

    /// <summary>
    /// 병합 영역 재구성 (탐욕적 알고리즘)
    /// </summary>
    private void RebuildMergedRegions()
    {
        MergedRegions.Clear();
        var processed = new bool[GridSize, GridSize];

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (processed[x, y]) continue;
                
                var cell = Cells[x, y];
                if (cell == null) continue;

                int owner = cell.OwnerCannonId;

                // 가로 방향 병합
                int width = 1;
                while (x + width < GridSize && !processed[x + width, y])
                {
                    var checkCell = Cells[x + width, y];
                    if (checkCell == null || checkCell.OwnerCannonId != owner) break;
                    width++;
                }

                // 세로 방향 병합
                int height = 1;
                bool canExtend = true;
                
                while (y + height < GridSize && canExtend)
                {
                    for (int dx = 0; dx < width; dx++)
                    {
                        if (processed[x + dx, y + height])
                        {
                            canExtend = false;
                            break;
                        }
                        
                        var checkCell = Cells[x + dx, y + height];
                        if (checkCell == null || checkCell.OwnerCannonId != owner)
                        {
                            canExtend = false;
                            break;
                        }
                    }
                    
                    if (canExtend) height++;
                }

                // 병합 영역 추가
                MergedRegions.Add(new MergedRegion(owner, x, y, width, height, CellSize));

                // 처리 표시
                for (int dy = 0; dy < height; dy++)
                {
                    for (int dx = 0; dx < width; dx++)
                    {
                        processed[x + dx, y + dy] = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 게임 업데이트
    /// </summary>
    /// <param name="deltaTime">시간 간격 (초)</param>
    public void Update(float deltaTime)
    {
        CurrentTime += deltaTime;

        // 대포 각도 업데이트
        foreach (var cannon in Cannons)
        {
            cannon.UpdateAngle(deltaTime);
        }

        // 대포들이 포탄 발사
        foreach (var cannon in Cannons)
        {
            var projectile = cannon.TryFire(CurrentTime, $"proj_{_projectileCounter++}");
            if (projectile != null)
            {
                Projectiles.Add(projectile);
            }
        }

        // 포탄 업데이트 및 충돌 검사
        for (int i = Projectiles.Count - 1; i >= 0; i--)
        {
            var projectile = Projectiles[i];
            projectile.Update(deltaTime);

            // 월드 범위를 벗어나면 제거
            if (projectile.Position.X < 0 || projectile.Position.X > WorldSize ||
                projectile.Position.Y < 0 || projectile.Position.Y > WorldSize)
            {
                projectile.IsActive = false;
                Projectiles.RemoveAt(i);
                continue;
            }

            // 영역 상자와 충돌 검사
            bool hit = CheckProjectileCollision(projectile);
            if (hit)
            {
                projectile.IsActive = false;
                Projectiles.RemoveAt(i);
            }
        }

        // 폭발 이펙트 업데이트
        for (int i = Explosions.Count - 1; i >= 0; i--)
        {
            Explosions[i].Update(CurrentTime);
            if (!Explosions[i].IsActive)
            {
                Explosions.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 포탄과 영역 상자의 충돌 검사 (병합 영역 최적화)
    /// </summary>
    private bool CheckProjectileCollision(Projectile projectile)
    {
        // 1단계: 병합 영역과 먼저 충돌 검사 (빠름)
        MergedRegion? hitRegion = null;
        
        foreach (var region in MergedRegions)
        {
            // 자기 영역은 스킵
            if (region.OwnerId == projectile.CannonId)
                continue;
                
            if (region.CheckCollision(projectile.Position, projectile.Radius))
            {
                hitRegion = region;
                break;
            }
        }

        if (hitRegion == null)
            return false;

        // 2단계: 병합 영역 내에서 정확한 셀 찾기
        int cellX = (int)(projectile.Position.X / CellSize);
        int cellY = (int)(projectile.Position.Y / CellSize);

        // 그리드 범위 체크
        if (cellX < 0 || cellX >= GridSize || cellY < 0 || cellY >= GridSize)
            return false;

        // 포탄 반지름 기반으로 주변 셀 확인
        int checkRadius = (int)Math.Ceiling(projectile.Radius / CellSize);
        
        for (int dx = -checkRadius; dx <= checkRadius; dx++)
        {
            for (int dy = -checkRadius; dy <= checkRadius; dy++)
            {
                int x = cellX + dx;
                int y = cellY + dy;

                if (x >= 0 && x < GridSize && y >= 0 && y < GridSize)
                {
                    var cell = Cells[x, y];
                    
                    if (cell == null)
                        continue;
                    
                    // 같은 영역의 셀은 충돌하지 않음
                    if (cell.OwnerCannonId == projectile.CannonId)
                        continue;

                    if (projectile.CheckCollision(cell.Position, cell.Size))
                    {
                        int previousOwnerId = cell.OwnerCannonId;
                        bool conquered = cell.TakeDamage(projectile.Damage, projectile.CannonId, CurrentTime);
                        
                        // 영역 획득 시 점수 부여
                        if (conquered)
                        {
                            if (previousOwnerId == -1)
                            {
                                Cannons[projectile.CannonId].AddScore(1);
                            }
                            else
                            {
                                Cannons[projectile.CannonId].AddScore(2);
                            }
                            
                            // 병합 영역 재구성 (영역 변경됨)
                            RebuildMergedRegions();
                        }
                        
                        // 폭발 이펙트 생성
                        var cannonColor = Cannons[projectile.CannonId].Color;
                        var explosion = new Explosion(projectile.Position, cannonColor, CurrentTime);
                        Explosions.Add(explosion);
                        
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 각 대포가 소유한 영역 개수 계산
    /// </summary>
    public Dictionary<int, int> GetTerritoryStats()
    {
        var stats = new Dictionary<int, int>();
        
        // 대포별 카운터 초기화
        for (int i = 0; i < CannonCount; i++)
        {
            stats[i] = 0;
        }
        stats[-1] = 0; // 중립 영역

        // 영역 카운트
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                var cell = Cells[x, y];
                if (cell == null) continue; // 원 밖 셀 무시
                
                int owner = cell.OwnerCannonId;
                stats[owner]++;
            }
        }

        return stats;
    }

    /// <summary>
    /// 게임 상태 출력
    /// </summary>
    public void PrintStats()
    {
        var stats = GetTerritoryStats();
        
        Console.WriteLine($"\n=== 게임 시간: {CurrentTime:F2}초 ===");
        Console.WriteLine($"활성 포탄 수: {Projectiles.Count}");
        Console.WriteLine($"중립 영역: {stats[-1]}");
        
        // 상위 10개 대포의 영역 출력
        var topCannons = stats
            .Where(kv => kv.Key >= 0)
            .OrderByDescending(kv => kv.Value)
            .Take(10);

        Console.WriteLine("\n상위 10개 대포:");
        foreach (var kv in topCannons)
        {
            if (kv.Value > 0)
            {
                var color = Cannons[kv.Key].Color;
                Console.WriteLine($"  대포 #{kv.Key} (RGB: {color.X:F2}, {color.Y:F2}, {color.Z:F2}): {kv.Value} 영역");
            }
        }
    }
}
