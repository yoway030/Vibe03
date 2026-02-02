using System.Numerics;

namespace Client;

/// <summary>
/// 대포 - 각도를 왔다갔다하며 포탄을 발사
/// </summary>
public class Cannon
{
    /// <summary>
    /// 대포 ID (0-99)
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// 위치
    /// </summary>
    public Vector2 Position { get; }

    /// <summary>
    /// 중심을 향하는 방향 (기준 방향)
    /// </summary>
    public Vector2 CenterDirection { get; }

    /// <summary>
    /// 현재 발사 각도 (라디안, 중심 기준)
    /// </summary>
    private float _currentAngle;

    /// <summary>
    /// 각도 변화 속도 (라디안/초)
    /// </summary>
    private float _angleSpeed;

    /// <summary>
    /// 각도 변화 방향 (1 또는 -1)
    /// </summary>
    private int _angleDirection;

    /// <summary>
    /// 최대 발사 각도 (라디안, +90도)
    /// </summary>
    public const float MaxAngle = MathF.PI / 2; // 90도

    /// <summary>
    /// 최소 발사 각도 (라디안, -90도)
    /// </summary>
    public const float MinAngle = -MathF.PI / 2; // -90도

    /// <summary>
    /// 영역 색상 (RGB)
    /// </summary>
    public Vector3 Color { get; }

    /// <summary>
    /// 대포의 누적 점수
    /// </summary>
    public int Score { get; private set; }

    /// <summary>
    /// 발사 간격 (초)
    /// </summary>
    public float FireInterval { get; }

    /// <summary>
    /// 마지막 발사 시간
    /// </summary>
    private float _lastFireTime;

    /// <summary>
    /// 포탄 속도
    /// </summary>
    public float ProjectileSpeed { get; }

    /// <summary>
    /// 기본 발사 간격
    /// </summary>
    public const float DefaultFireInterval = 0.5f;

    /// <summary>
    /// 기본 포탄 속도
    /// </summary>
    public const float DefaultProjectileSpeed = 500.0f;

    /// <summary>
    /// 기본 각도 변화 속도 (라디안/초)
    /// </summary>
    public const float DefaultAngleSpeed = MathF.PI / 2; // 90도/초

    /// <summary>
    /// 포신 길이 (월드 좌표)
    /// </summary>
    public const float BarrelLength = GameSimulation.CellSize * 2.5f;

    /// <summary>
    /// 미리 생성된 100개의 색상 테이블
    /// </summary>
    private static readonly Vector3[] ColorTable = GenerateColorTable();

    public Cannon(int id, Vector2 position, Vector2 center, Vector3 color, float fireInterval = DefaultFireInterval, float projectileSpeed = DefaultProjectileSpeed)
    {
        Id = id;
        Position = position;
        Color = color;
        Score = 0;
        FireInterval = fireInterval;
        ProjectileSpeed = projectileSpeed;
        _lastFireTime = 0;

        // 중심을 향하는 방향 계산
        CenterDirection = Vector2.Normalize(center - position);

        // 초기 각도는 0 (중심 방향)
        _currentAngle = 0;
        _angleDirection = 1;
        _angleSpeed = DefaultAngleSpeed;
    }

    /// <summary>
    /// 현재 발사 방향 계산
    /// </summary>
    public Vector2 GetCurrentFireDirection()
    {
        // 중심 방향에서 현재 각도만큼 회전
        float cos = MathF.Cos(_currentAngle);
        float sin = MathF.Sin(_currentAngle);
        
        return new Vector2(
            CenterDirection.X * cos - CenterDirection.Y * sin,
            CenterDirection.X * sin + CenterDirection.Y * cos
        );
    }

    /// <summary>
    /// 각도 업데이트
    /// </summary>
    public void UpdateAngle(float deltaTime)
    {
        _currentAngle += _angleDirection * _angleSpeed * deltaTime;

        // 범위를 벗어나면 방향 전환
        if (_currentAngle >= MaxAngle)
        {
            _currentAngle = MaxAngle;
            _angleDirection = -1;
        }
        else if (_currentAngle <= MinAngle)
        {
            _currentAngle = MinAngle;
            _angleDirection = 1;
        }
    }

    /// <summary>
    /// 포탄 발사 가능 여부 확인 및 발사
    /// </summary>
    /// <param name="currentTime">현재 게임 시간</param>
    /// <param name="projectileId">생성할 포탄 ID</param>
    /// <returns>발사 가능하면 새 Projectile, 아니면 null</returns>
    public Projectile? TryFire(float currentTime, string projectileId)
    {
        if (currentTime - _lastFireTime >= FireInterval)
        {
            _lastFireTime = currentTime;
            Vector2 fireDirection = GetCurrentFireDirection();
            Vector2 velocity = fireDirection * ProjectileSpeed;
            
            // 포신 끝에서 포탄 발사
            Vector2 barrelEnd = Position + fireDirection * BarrelLength;
            return new Projectile(projectileId, Id, barrelEnd, velocity);
        }

        return null;
    }

    /// <summary>
    /// 점수 증가
    /// </summary>
    /// <param name="points">증가시킬 점수 (중립 영역 획득: 1, 타 영역 탈환: 2)</param>
    public void AddScore(int points)
    {
        Score += points;
    }

    /// <summary>
    /// 원 위에 배치된 대포들 생성
    /// </summary>
    /// <param name="count">대포 개수</param>
    /// <param name="center">중심점</param>
    /// <param name="radius">반지름</param>
    /// <returns>생성된 대포 배열</returns>
    public static Cannon[] CreateCannons(int count, Vector2 center, float radius)
    {
        var cannons = new Cannon[count];
        float angleStep = (float)(2 * Math.PI / count);

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            Vector2 position = new Vector2(
                center.X + radius * (float)Math.Cos(angle),
                center.Y + radius * (float)Math.Sin(angle)
            );

            // 미리 생성된 색상 테이블에서 색상 가져오기
            Vector3 color = ColorTable[i];

            cannons[i] = new Cannon(i, position, center, color);
        }

        return cannons;
    }

    /// <summary>
    /// 100개의 색상 테이블 생성 (다양한 색상)
    /// </summary>
    private static Vector3[] GenerateColorTable()
    {
        var colors = new Vector3[100];
        var random = new Random(42); // 고정 시드로 일관된 색상
        
        for (int i = 0; i < 100; i++)
        {
            // 황금비율을 사용하여 색상환에서 고르게 분산
            float goldenRatio = 0.618033988749895f;
            float hue = (i * goldenRatio * 360.0f) % 360.0f;
            
            // 채도와 명도를 다양하게 (너무 어둡거나 희미하지 않게)
            float saturation = 0.5f + (i % 5) * 0.1f;  // 0.5 ~ 0.9
            float value = 0.6f + (i % 4) * 0.1f;       // 0.6 ~ 0.9
            
            colors[i] = HsvToRgb(hue, saturation, value);
        }
        
        return colors;
    }

    /// <summary>
    /// HSV를 RGB로 변환
    /// </summary>
    private static Vector3 HsvToRgb(float h, float s, float v)
    {
        float c = v * s;
        float x = c * (1 - Math.Abs((h / 60.0f) % 2 - 1));
        float m = v - c;

        float r = 0, g = 0, b = 0;

        if (h < 60)
        {
            r = c; g = x; b = 0;
        }
        else if (h < 120)
        {
            r = x; g = c; b = 0;
        }
        else if (h < 180)
        {
            r = 0; g = c; b = x;
        }
        else if (h < 240)
        {
            r = 0; g = x; b = c;
        }
        else if (h < 300)
        {
            r = x; g = 0; b = c;
        }
        else
        {
            r = c; g = 0; b = x;
        }

        return new Vector3(r + m, g + m, b + m);
    }
}
