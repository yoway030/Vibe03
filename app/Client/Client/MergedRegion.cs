using System.Numerics;

namespace Client;

/// <summary>
/// 병합된 영역 정보 (충돌 최적화용)
/// </summary>
public class MergedRegion
{
    /// <summary>
    /// 소유자 ID
    /// </summary>
    public int OwnerId { get; }

    /// <summary>
    /// 시작 X 좌표 (그리드 인덱스)
    /// </summary>
    public int StartX { get; }

    /// <summary>
    /// 시작 Y 좌표 (그리드 인덱스)
    /// </summary>
    public int StartY { get; }

    /// <summary>
    /// 너비 (셀 개수)
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// 높이 (셀 개수)
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// 월드 좌표 바운딩 박스 (최소)
    /// </summary>
    public Vector2 WorldMin { get; }

    /// <summary>
    /// 월드 좌표 바운딩 박스 (최대)
    /// </summary>
    public Vector2 WorldMax { get; }

    public MergedRegion(int ownerId, int startX, int startY, int width, int height, float cellSize)
    {
        OwnerId = ownerId;
        StartX = startX;
        StartY = startY;
        Width = width;
        Height = height;

        // 월드 좌표 바운딩 박스 계산
        WorldMin = new Vector2(startX * cellSize, startY * cellSize);
        WorldMax = new Vector2((startX + width) * cellSize, (startY + height) * cellSize);
    }

    /// <summary>
    /// 점이 이 영역 내부에 있는지 확인
    /// </summary>
    public bool Contains(Vector2 point)
    {
        return point.X >= WorldMin.X && point.X <= WorldMax.X &&
               point.Y >= WorldMin.Y && point.Y <= WorldMax.Y;
    }

    /// <summary>
    /// 포탄과의 충돌 확인 (바운딩 박스)
    /// </summary>
    public bool CheckCollision(Vector2 position, float radius)
    {
        // AABB vs Circle 충돌 검사
        float closestX = Math.Clamp(position.X, WorldMin.X, WorldMax.X);
        float closestY = Math.Clamp(position.Y, WorldMin.Y, WorldMax.Y);
        
        float distX = position.X - closestX;
        float distY = position.Y - closestY;
        
        return (distX * distX + distY * distY) <= (radius * radius);
    }
}
