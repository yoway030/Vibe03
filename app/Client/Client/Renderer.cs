using System.Numerics;
using Raylib_cs;

namespace Client;

/// <summary>
/// 게임 그래픽 렌더링 시스템 (순수 렌더링만 담당)
/// </summary>
public class Renderer
{
    private const int ScreenWidth = 1400;
    private const int ScreenHeight = 1000;
    private const int OffsetX = 50;
    private const int OffsetY = 50;

    /// <summary>
    /// Raylib 초기화
    /// </summary>
    public static void Initialize()
    {
        Raylib.InitWindow(ScreenWidth, ScreenHeight, "영역 전쟁 - Territory War");
        Raylib.SetTargetFPS(60);
    }

    /// <summary>
    /// Raylib 종료
    /// </summary>
    public static void Close()
    {
        Raylib.CloseWindow();
    }

    /// <summary>
    /// 윈도우가 닫혀야 하는지 확인
    /// </summary>
    public static bool ShouldClose()
    {
        return Raylib.WindowShouldClose();
    }

    /// <summary>
    /// 게임 렌더링 (GameState 기반)
    /// </summary>
    public static void Render(GameState gameState, Camera camera)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        // 게임 영역 테두리
        Raylib.DrawRectangleLines(OffsetX - 2, OffsetY - 2, 
            (int)(GameSimulation.WorldSize * camera.ScaleFactor) + 4, 
            (int)(GameSimulation.WorldSize * camera.ScaleFactor) + 4, 
            Color.White);

        // 영역 상자 렌더링
        RenderCells(gameState, camera);

        // 대포 렌더링
        RenderCannons(gameState, camera);

        // 포탄 렌더링
        RenderProjectiles(gameState, camera);

        // 폭발 렌더링
        RenderExplosions(gameState, camera);

        // UI 렌더링
        RenderUI(gameState, camera);

        Raylib.EndDrawing();
    }

    /// <summary>
    /// 영역 셀 렌더링 (최적화: 배치 렌더링 + LOD + 카드 뒤집기 효과)
    /// </summary>
    private static void RenderCells(GameState gameState, Camera camera)
    {
        // LOD 계산
        int lodSkip = camera.CalculateLodSkip();

        // 뷰포트 계산
        var (minX, maxX, minY, maxY) = camera.CalculateViewport(
            ScreenWidth, ScreenHeight, 
            GameSimulation.CellSize, 
            gameState.GridSize, gameState.GridSize, 
            lodSkip);

        // 대포별 영역 그룹화 (배치 렌더링)
        var cannonCells = new Dictionary<int, List<Rectangle>>(GameSimulation.CannonCount + 1);
        var cannonColors = new Dictionary<int, Color>(GameSimulation.CannonCount + 1);

        // 셀을 대포별로 그룹화 (뷰포트 내부만 + LOD 적용)
        for (int x = minX; x <= maxX; x += lodSkip)
        {
            for (int y = minY; y <= maxY; y += lodSkip)
            {
                int index = y * gameState.GridSize + x;
                var cellState = gameState.Cells[index];
                
                // null 체크 (원 밖 영역)
                if (cellState == null) continue;
                
                var cell = cellState.Value;
                Vector2 screenPos = camera.WorldToScreen(cell.Position);
                int owner = cell.OwnerCannonId;
                
                // 애니메이션 중이면 카드 뒤집기 효과로 개별 렌더링 (LOD가 높을 때만)
                float elapsed = gameState.CurrentTime - cell.TransitionStartTime;
                float flipProgress = Math.Min(elapsed / TerritoryCell.FlipDuration, 1.0f);
                bool isFlipping = flipProgress < 1.0f && cell.PreviousOwnerCannonId != cell.OwnerCannonId;
                
                // LOD가 높을 때(확대 시)만 애니메이션 표시
                if (isFlipping && lodSkip == 1)
                {
                    RenderHelper.RenderFlippingCell(cell, screenPos, camera, gameState, lodSkip);
                }
                else
                {
                    // 색상 딕셔너리에 없으면 추가 (지연 초기화)
                    if (!cannonColors.ContainsKey(owner))
                    {
                        if (owner == -1)
                        {
                            cannonColors[owner] = new Color(40, 40, 40, 255);
                        }
                        else
                        {
                            var cannonColor = gameState.Cannons[owner].Color;
                            cannonColors[owner] = new Color(
                                (int)(cannonColor.X * 255),
                                (int)(cannonColor.Y * 255),
                                (int)(cannonColor.Z * 255),
                                255
                            );
                        }
                        cannonCells[owner] = new List<Rectangle>();
                    }
                    
                    // 배치 렌더링을 위해 그룹에 추가
                    float cellScreenSize = camera.WorldToScreenSize(cell.Size * lodSkip);
                    Rectangle rect = new Rectangle(
                        screenPos.X - cellScreenSize / 2,
                        screenPos.Y - cellScreenSize / 2,
                        cellScreenSize,
                        cellScreenSize
                    );
                    cannonCells[owner].Add(rect);
                }
            }
        }

        // 대포별로 배치 렌더링
        foreach (var kvp in cannonCells)
        {
            int owner = kvp.Key;
            var rects = kvp.Value;
            
            if (rects.Count == 0) continue;

            Color color = cannonColors[owner];
            
            // 같은 색상의 사각형들을 한번에 그리기
            foreach (var rect in rects)
            {
                Raylib.DrawRectangle(
                    (int)rect.X,
                    (int)rect.Y,
                    (int)rect.Width,
                    (int)rect.Height,
                    color
                );
            }
        }
    }

    /// <summary>
    /// 대포 렌더링
    /// </summary>
    private static void RenderCannons(GameState gameState, Camera camera)
    {
        foreach (var cannon in gameState.Cannons)
        {
            Vector2 screenPos = camera.WorldToScreen(cannon.Position);
            
            var color = new Color(
                (int)(cannon.Color.X * 255),
                (int)(cannon.Color.Y * 255),
                (int)(cannon.Color.Z * 255),
                255
            );

            // 현재 발사 방향
            Vector2 fireDirection = cannon.FireDirection;

            // 1. 포대 베이스 (원형) - CellSize 기준으로 크기 조정
            float baseRadius = camera.WorldToScreenSize(GameSimulation.CellSize * 1.5f);
            Raylib.DrawCircle((int)screenPos.X, (int)screenPos.Y, baseRadius, new Color(60, 60, 60, 255));
            Raylib.DrawCircleLines((int)screenPos.X, (int)screenPos.Y, baseRadius, color);

            // 2. 포신 (선형으로 표현) - CellSize 기준으로 크기 조정
            float barrelLength = camera.WorldToScreenSize(GameSimulation.CellSize * 4f);
            float barrelWidth = camera.WorldToScreenSize(GameSimulation.CellSize * 0.75f);
            Vector2 barrelEnd = screenPos + fireDirection * barrelLength;
            
            // 포신 그림자/외곽
            Raylib.DrawLineEx(
                new Vector2(screenPos.X, screenPos.Y),
                new Vector2(barrelEnd.X, barrelEnd.Y),
                barrelWidth + camera.WorldToScreenSize(GameSimulation.CellSize * 0.4f),
                new Color(40, 40, 40, 255)
            );
            
            // 포신 본체
            Raylib.DrawLineEx(
                new Vector2(screenPos.X, screenPos.Y),
                new Vector2(barrelEnd.X, barrelEnd.Y),
                barrelWidth,
                new Color(90, 90, 90, 255)
            );

            // 포신 끝부분 (색상으로 강조)
            Raylib.DrawCircle((int)barrelEnd.X, (int)barrelEnd.Y, 
                barrelWidth / 2 + camera.WorldToScreenSize(GameSimulation.CellSize * 0.4f), color);
            Raylib.DrawCircle((int)barrelEnd.X, (int)barrelEnd.Y, barrelWidth / 2, new Color(30, 30, 30, 255));

            // 3. 포탑 중심부 (색상)
            Raylib.DrawCircle((int)screenPos.X, (int)screenPos.Y, baseRadius * 0.6f, color);
            
            // 4. 중심점
            Raylib.DrawCircle((int)screenPos.X, (int)screenPos.Y, 
                Math.Max(2f, camera.WorldToScreenSize(GameSimulation.CellSize)), Color.White);
        }
    }

    /// <summary>
    /// 포탄 렌더링
    /// </summary>
    private static void RenderProjectiles(GameState gameState, Camera camera)
    {
        foreach (var projectile in gameState.Projectiles)
        {
            Vector2 screenPos = camera.WorldToScreen(projectile.Position);
            
            var cannonColor = gameState.Cannons[projectile.CannonId].Color;
            
            // 포탄 색상을 더 밝고 눈에 띄게
            var brightColor = new Color(
                Math.Min(255, (int)(cannonColor.X * 255 * 1.5f + 50)),
                Math.Min(255, (int)(cannonColor.Y * 255 * 1.5f + 50)),
                Math.Min(255, (int)(cannonColor.Z * 255 * 1.5f + 50)),
                255
            );

            float radius = camera.WorldToScreenSize(projectile.Radius);

            // 외곽 글로우 효과
            Raylib.DrawCircle(
                (int)screenPos.X,
                (int)screenPos.Y,
                radius + 1.5f,
                new Color(
                    Math.Min(255, (int)(cannonColor.X * 255 * 1.2f)),
                    Math.Min(255, (int)(cannonColor.Y * 255 * 1.2f)),
                    Math.Min(255, (int)(cannonColor.Z * 255 * 1.2f)),
                    150
                )
            );

            // 메인 포탄
            Raylib.DrawCircle(
                (int)screenPos.X, 
                (int)screenPos.Y, 
                radius,
                brightColor
            );

            // 중심부 하이라이트
            Raylib.DrawCircle(
                (int)screenPos.X,
                (int)screenPos.Y,
                radius * 0.5f,
                new Color(255, 255, 255, 200)
            );
        }
    }

    /// <summary>
    /// 폭발 이펙트 렌더링
    /// </summary>
    private static void RenderExplosions(GameState gameState, Camera camera)
    {
        foreach (var explosion in gameState.Explosions)
        {
            Vector2 screenPos = camera.WorldToScreen(explosion.Position);
            
            float elapsed = gameState.CurrentTime - explosion.StartTime;
            float progress = Math.Min(elapsed / Explosion.Duration, 1.0f);
            float radius = camera.WorldToScreenSize(Explosion.MaxRadius * progress);
            float alpha = 1.0f - progress;

            var color = new Color(
                (int)(explosion.Color.X * 255),
                (int)(explosion.Color.Y * 255),
                (int)(explosion.Color.Z * 255),
                (int)(alpha * 255)
            );

            // 폭발 원 (외부)
            Raylib.DrawCircle(
                (int)screenPos.X,
                (int)screenPos.Y,
                radius,
                color
            );

            // 폭발 원 (내부 - 더 밝게)
            var innerColor = new Color(
                Math.Min(255, (int)(explosion.Color.X * 255 * 1.5f)),
                Math.Min(255, (int)(explosion.Color.Y * 255 * 1.5f)),
                Math.Min(255, (int)(explosion.Color.Z * 255 * 1.5f)),
                (int)(alpha * 200)
            );

            Raylib.DrawCircle(
                (int)screenPos.X,
                (int)screenPos.Y,
                radius * 0.6f,
                innerColor
            );

            // 중심부 (흰색)
            Raylib.DrawCircle(
                (int)screenPos.X,
                (int)screenPos.Y,
                radius * 0.3f,
                new Color(255, 255, 255, (int)(alpha * 255))
            );
        }
    }

    /// <summary>
    /// UI 렌더링
    /// </summary>
    private static void RenderUI(GameState gameState, Camera camera)
    {
        int uiX = OffsetX + (int)(GameSimulation.WorldSize * camera.ScaleFactor) + 20;
        int uiY = OffsetY;

        // 게임 정보
        Raylib.DrawText($"시간: {gameState.CurrentTime:F1}초", uiX, uiY, 20, Color.White);
        uiY += 30;
        Raylib.DrawText($"포탄: {gameState.Projectiles.Count}", uiX, uiY, 20, Color.White);
        uiY += 30;
        Raylib.DrawText($"줌: {camera.ScaleFactor * 100:F0}%", uiX, uiY, 18, Color.LightGray);
        uiY += 40;

        // 영역 통계 계산
        var stats = CalculateTerritoryStats(gameState);

        Raylib.DrawText($"중립 영역: {stats[-1]}", uiX, uiY, 18, Color.Gray);
        uiY += 30;

        Raylib.DrawText("상위 대포:", uiX, uiY, 18, Color.White);
        uiY += 25;

        var topCannons = stats
            .Where(kv => kv.Key >= 0)
            .OrderByDescending(kv => kv.Value)
            .Take(10);

        int rank = 1;
        foreach (var kv in topCannons)
        {
            if (kv.Value == 0) continue;

            var cannonColor = gameState.Cannons[kv.Key].Color;
            var color = new Color(
                (int)(cannonColor.X * 255),
                (int)(cannonColor.Y * 255),
                (int)(cannonColor.Z * 255),
                255
            );

            // 색상 박스
            Raylib.DrawRectangle(uiX, uiY, 15, 15, color);
            
            // 텍스트
            Raylib.DrawText($"{rank}. #{kv.Key}: {kv.Value}", uiX + 20, uiY, 16, Color.White);
            uiY += 22;
            rank++;
        }

        // 하단 안내
        Raylib.DrawText("ESC: 종료", uiX, ScreenHeight - 80, 16, Color.LightGray);
        Raylib.DrawText("마우스: 드래그", uiX, ScreenHeight - 60, 16, Color.LightGray);
        Raylib.DrawText("휠/+/-: 줌", uiX, ScreenHeight - 40, 16, Color.LightGray);
    }

    /// <summary>
    /// 영역 통계 계산
    /// </summary>
    private static Dictionary<int, int> CalculateTerritoryStats(GameState gameState)
    {
        var stats = new Dictionary<int, int>();
        
        // 초기화 (-1: 중립, 0~99: 각 대포)
        stats[-1] = 0;
        for (int i = 0; i < GameSimulation.CannonCount; i++)
        {
            stats[i] = 0;
        }
        
        // 셀 카운트
        foreach (var cell in gameState.Cells)
        {
            if (cell != null)
            {
                stats[cell.Value.OwnerCannonId]++;
            }
        }
        
        return stats;
    }
}
