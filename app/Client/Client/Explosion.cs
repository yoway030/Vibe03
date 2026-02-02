using System.Numerics;

namespace Client;

/// <summary>
/// 폭발 이펙트
/// </summary>
public class Explosion
{
    /// <summary>
    /// 폭발 위치
    /// </summary>
    public Vector2 Position { get; }

    /// <summary>
    /// 폭발 색상
    /// </summary>
    public Vector3 Color { get; }

    /// <summary>
    /// 폭발 생성 시간
    /// </summary>
    public float StartTime { get; }

    /// <summary>
    /// 폭발 지속 시간
    /// </summary>
    public const float Duration = 0.3f;

    /// <summary>
    /// 최대 반지름
    /// </summary>
    public const float MaxRadius = 75.0f;

    /// <summary>
    /// 활성 상태
    /// </summary>
    public bool IsActive { get; set; }

    public Explosion(Vector2 position, Vector3 color, float currentTime)
    {
        Position = position;
        Color = color;
        StartTime = currentTime;
        IsActive = true;
    }

    /// <summary>
    /// 현재 진행도 (0.0 ~ 1.0)
    /// </summary>
    public float GetProgress(float currentTime)
    {
        float elapsed = currentTime - StartTime;
        return Math.Min(elapsed / Duration, 1.0f);
    }

    /// <summary>
    /// 현재 반지름
    /// </summary>
    public float GetRadius(float currentTime)
    {
        float progress = GetProgress(currentTime);
        return MaxRadius * progress;
    }

    /// <summary>
    /// 현재 투명도 (1.0 ~ 0.0)
    /// </summary>
    public float GetAlpha(float currentTime)
    {
        float progress = GetProgress(currentTime);
        return 1.0f - progress;
    }

    /// <summary>
    /// 폭발 업데이트
    /// </summary>
    public void Update(float currentTime)
    {
        if (GetProgress(currentTime) >= 1.0f)
        {
            IsActive = false;
        }
    }
}
