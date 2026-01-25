namespace Physics;

/// <summary>
/// 물리 엔진의 기본 설정값을 정의하는 클래스
/// </summary>
public static class PhysicsConfig
{
    /// <summary>기본 밀도 (kg/m²) - 물의 밀도와 유사한 값</summary>
    public const float DefaultDensity = 1.0f;
    
    /// <summary>기본 마찰력 - 나무나 플라스틱 수준</summary>
    public const float DefaultFriction = 0.3f;
    
    /// <summary>기본 반발력 (탄성) - 중간 정도의 탄성</summary>
    public const float DefaultRestitution = 0.5f;
    
    /// <summary>기본 서브 스텝 횟수 - 안정적인 물리 시뮬레이션을 위한 권장값</summary>
    public const int DefaultSubStepCount = 4;
    
    /// <summary>정적 바디의 내부 밀도 (Box2D 내부 처리용)</summary>
    public const float StaticBodyDensity = 1.0f;
}
