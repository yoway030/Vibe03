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
    /// 영역 셀 렌더링 (GameSimulation에서 계산된 병합 영역 사용)
    /// </summary>
    private static void RenderCells(GameState gameState, Camera camera)
    {
        // 대포별 폴리곤 그룹화
        var cannonPolygons = new Dictionary<int, List<Rectangle>>(GameSimulation.CannonCount + 1);
        var cannonColors = new Dictionary<int, Color>(GameSimulation.CannonCount + 1);

        // GameSimulation에서 이미 계산된 병합 영역 사용
        foreach (var region in gameState.MergedRegions)
        {
            // 색상 초기화
            if (!cannonColors.ContainsKey(region.OwnerId))
            {
                if (region.OwnerId == -1)
                {
                    cannonColors[region.OwnerId] = new Color(40, 40, 40, 255);
                }
                else
                {
                    var cannonColor = gameState.Cannons[region.OwnerId].Color;
                    cannonColors[region.OwnerId] = new Color(
                        (int)(cannonColor.X * 255),
                        (int)(cannonColor.Y * 255),
                        (int)(cannonColor.Z * 255),
                        255
                    );
                }
                cannonPolygons[region.OwnerId] = new List<Rectangle>();
            }

            // 병합된 사각형을 월드 좌표로 계산
            Vector2 worldPos = new Vector2(
                region.StartX * GameSimulation.CellSize + (region.Width * GameSimulation.CellSize) / 2,
                region.StartY * GameSimulation.CellSize + (region.Height * GameSimulation.CellSize) / 2
            );
            Vector2 screenPos = camera.WorldToScreen(worldPos);
            float screenWidth = camera.WorldToScreenSize(region.Width * GameSimulation.CellSize);
            float screenHeight = camera.WorldToScreenSize(region.Height * GameSimulation.CellSize);

            // 화면 밖이면 스킵
            if (screenPos.X + screenWidth / 2 < 0 || screenPos.X - screenWidth / 2 > ScreenWidth ||
                screenPos.Y + screenHeight / 2 < 0 || screenPos.Y - screenHeight / 2 > ScreenHeight)
                continue;

            // 1픽셀 오버랩으로 검은색 간격 제거
            Rectangle rect = new Rectangle(
                screenPos.X - screenWidth / 2 - 0.5f,
                screenPos.Y - screenHeight / 2 - 0.5f,
                screenWidth + 1.0f,
                screenHeight + 1.0f
            );

            cannonPolygons[region.OwnerId].Add(rect);
        }

        // 대포별로 병합된 폴리곤 렌더링
        foreach (var kvp in cannonPolygons)
        {
            int owner = kvp.Key;
            var polygons = kvp.Value;
            
            if (polygons.Count == 0) continue;

            Color color = cannonColors[owner];
            
            // 병합된 폴리곤들을 한번에 그리기 (간격 없이)
            foreach (var rect in polygons)
            {
                Raylib.DrawRectangle(
                    (int)rect.X,
                    (int)rect.Y,
                    (int)Math.Ceiling(rect.Width),
                    (int)Math.Ceiling(rect.Height),
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

            // 2. 포신 (선형으로 표현) - CellSize 기준으로 크기 조정
            float barrelLength = camera.WorldToScreenSize(GameSimulation.CellSize * 2.5f);
            float barrelWidth = camera.WorldToScreenSize(GameSimulation.CellSize * 0.75f);
            Vector2 barrelEnd = screenPos + fireDirection * barrelLength;
            
            // 포신 본체
            Raylib.DrawLineEx(
                new Vector2(screenPos.X, screenPos.Y),
                new Vector2(barrelEnd.X, barrelEnd.Y),
                barrelWidth,
                new Color(90, 90, 90, 255)
            );

            // 포신 끝부분 (색상으로 강조)
            Raylib.DrawCircle((int)barrelEnd.X, (int)barrelEnd.Y, barrelWidth / 2, color);

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

            // 포탄 렌더링
            Raylib.DrawCircle(
                (int)screenPos.X, 
                (int)screenPos.Y, 
                radius,
                brightColor
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

        Raylib.DrawText("상위 대포 (영역/점수):", uiX, uiY, 18, Color.White);
        uiY += 25;

        // 점수 기준으로 정렬
        var topCannons = gameState.Cannons
            .OrderByDescending(c => c.Score)
            .Take(10);

        int rank = 1;
        foreach (var cannon in topCannons)
        {
            int territoryCount = stats[cannon.Id];
            
            var color = new Color(
                (int)(cannon.Color.X * 255),
                (int)(cannon.Color.Y * 255),
                (int)(cannon.Color.Z * 255),
                255
            );

            // 색상 박스
            Raylib.DrawRectangle(uiX, uiY, 15, 15, color);
            
            // 텍스트: 랭킹, ID, 영역 수, 점수
            Raylib.DrawText($"{rank}. #{cannon.Id}: {territoryCount} / {cannon.Score}점", uiX + 20, uiY, 16, Color.White);
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
