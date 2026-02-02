using System.Numerics;
using Raylib_cs;

namespace Client;

/// <summary>
/// 렌더링 헬퍼 메서드들
/// </summary>
public static class RenderHelper
{
    /// <summary>
    /// 카드 뒤집기 효과로 셀 렌더링
    /// </summary>
    public static void RenderFlippingCell(CellState cell, Vector2 screenPos, Camera camera, GameState gameState, int lodSkip)
    {
        float elapsed = gameState.CurrentTime - cell.TransitionStartTime;
        float flipProgress = Math.Min(elapsed / TerritoryCell.FlipDuration, 1.0f);
        float flipAngle = flipProgress * MathF.PI; // 0 ~ 180도
        
        // Y축 회전에 따른 너비 변화 (cos 값)
        float scaleX = MathF.Abs(MathF.Cos(flipAngle));
        
        // 절반 이전에는 이전 색상, 이후에는 새 색상
        bool showNewColor = flipProgress > 0.5f;
        
        Vector3 oldColorVec = cell.PreviousOwnerCannonId == -1 
            ? new Vector3(40f/255f, 40f/255f, 40f/255f) 
            : gameState.Cannons[cell.PreviousOwnerCannonId].Color;
        
        Vector3 newColorVec = cell.OwnerCannonId == -1 
            ? new Vector3(40f/255f, 40f/255f, 40f/255f) 
            : gameState.Cannons[cell.OwnerCannonId].Color;
        
        Vector3 displayColor = showNewColor ? newColorVec : oldColorVec;
        
        Color color = new Color(
            (int)(displayColor.X * 255),
            (int)(displayColor.Y * 255),
            (int)(displayColor.Z * 255),
            255
        );

        float cellScreenSize = camera.WorldToScreenSize(cell.Size * lodSkip);
        float width = cellScreenSize * scaleX;
        float height = cellScreenSize;
        
        // 중심 기준으로 그리기
        Raylib.DrawRectangle(
            (int)(screenPos.X - width / 2),
            (int)(screenPos.Y - height / 2),
            (int)width,
            (int)height,
            color
        );
        
        // 외곽선 (회전 강조)
        if (scaleX > 0.1f)
        {
            Raylib.DrawRectangleLines(
                (int)(screenPos.X - width / 2),
                (int)(screenPos.Y - height / 2),
                (int)width,
                (int)height,
                new Color(255, 255, 255, (int)(100 * (1.0f - scaleX)))
            );
        }
    }
}
