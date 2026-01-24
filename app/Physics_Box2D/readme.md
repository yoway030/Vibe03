# Physics_Box2D

Box2D.NET 3.x 물리 엔진을 래핑한 C# 모듈로, 2D 물리 시뮬레이션을 쉽게 구현할 수 있도록 지원합니다.

## ⚠️ 현재 상태

이 모듈은 Box2D.NET 3.1.x를 기반으로 개발 중입니다. Box2D.NET 3.x는 이전 버전과 완전히 다른 API를 사용하며, 현재 API 래핑 작업이 진행 중입니다.

**진행 상황:**
- ✅ 프로젝트 구조 및 솔루션 구성 완료
- ✅ 기본 PhysicsWorld 클래스 스켈레톤 완료
- 🔄 Box2D.NET 3.x API 통합 작업 중
- ⏳ 유닛 테스트 업데이트 대기
- ⏳ 샘플 프로그램 업데이트 대기

## 개요

Physics_Box2D는 [Box2D.NET](https://github.com/ikpil/Box2D.NET) 라이브러리를 기반으로 하여, 게임 개발에 필요한 물리 시뮬레이션 기능을 제공하는 모듈입니다. 직관적인 API를 통해 복잡한 Box2D 기능을 쉽게 사용할 수 있도록 설계되었습니다.

## 주요 기능

- **물리 월드 관리**: 중력, 시간 스텝 등 물리 시뮬레이션 환경 설정
- **바디 생성**: 정적/동적 바디를 박스 또는 원형으로 생성
- **물리적 속성 제어**: 밀도, 마찰력, 반발력 설정
- **힘과 충격**: 바디에 힘(Force) 또는 충격(Impulse) 적용
- **위치 및 속도 관리**: 바디의 위치, 각도, 속도 조회 및 설정
- **바디 생명주기 관리**: 바디 생성, 조회, 삭제

## 설치

### NuGet 패키지

프로젝트에 다음 패키지를 설치하세요:

```bash
dotnet add package Box2D.NET --version 3.1.0.500
```

**참고**: Box2D.NET 3.x는 이전 버전과 API가 완전히 다릅니다.

### 프로젝트 참조

솔루션 내에서 사용하는 경우 프로젝트 참조를 추가하세요:

```xml
<ItemGroup>
  <ProjectReference Include="..\Physics_Box2D\Physics_Box2D.csproj" />
</ItemGroup>
```

## 사용 방법

### 기본 사용 예제

```csharp
using System.Numerics;
using Physics_Box2D;

// 1. 물리 월드 생성 (중력: Y축 방향으로 -10m/s²)
var world = new PhysicsWorld(new Vector2(0, -10f));

// 2. 정적 바디(지면) 생성
world.CreateStaticBox("ground", new Vector2(0, 0), 20f, 1f);

// 3. 동적 바디(떨어지는 박스) 생성
world.CreateDynamicBox("box", new Vector2(0, 10), 1f, 1f);

// 4. 물리 시뮬레이션 실행
for (int i = 0; i < 60; i++) // 1초 동안 60 프레임
{
    world.Step(1f / 60f);
    
    var position = world.GetPosition("box");
    Console.WriteLine($"Box position: {position}");
}
```

### 원형 바디 생성

```csharp
// 튀는 공 생성 (반발력 0.8)
world.CreateDynamicCircle(
    id: "ball",
    position: new Vector2(0, 10),
    radius: 0.5f,
    density: 1.0f,
    friction: 0.2f,
    restitution: 0.8f
);
```

### 힘과 충격 적용

```csharp
// 바디에 지속적인 힘 가하기
world.ApplyForceToCenter("box", new Vector2(100f, 0f));

// 바디에 즉각적인 충격 가하기
world.ApplyLinearImpulse("box", new Vector2(10f, 0f), world.GetPosition("box"));

// 속도 직접 설정
world.SetLinearVelocity("box", new Vector2(5f, 0f));
```

### 바디 정보 조회

```csharp
// 위치 조회
Vector2 position = world.GetPosition("box");

// 회전 각도 조회 (라디안)
float angle = world.GetAngle("box");

// 속도 조회
Vector2 velocity = world.GetLinearVelocity("box");

// 바디 존재 여부 확인
bool exists = world.HasBody("box");

// 모든 바디 ID 조회
IEnumerable<string> allIds = world.GetAllBodyIds();
```

### 바디 삭제

```csharp
world.DestroyBody("box");
```

## API 레퍼런스

### PhysicsWorld 클래스

#### 생성자

```csharp
PhysicsWorld(Vector2 gravity = default)
```
- `gravity`: 중력 벡터. 기본값은 (0, -10)

#### 주요 메서드

| 메서드 | 설명 |
|--------|------|
| `Step(float timeStep, int velocityIterations = 8, int positionIterations = 3)` | 물리 시뮬레이션 한 스텝 실행 |
| `CreateStaticBox(string id, Vector2 position, float width, float height)` | 정적 박스 바디 생성 |
| `CreateDynamicBox(string id, Vector2 position, float width, float height, ...)` | 동적 박스 바디 생성 |
| `CreateDynamicCircle(string id, Vector2 position, float radius, ...)` | 동적 원형 바디 생성 |
| `GetPosition(string id)` | 바디의 현재 위치 반환 |
| `GetAngle(string id)` | 바디의 현재 회전 각도 반환 (라디안) |
| `ApplyForce(string id, Vector2 force, Vector2 point)` | 특정 지점에 힘 적용 |
| `ApplyForceToCenter(string id, Vector2 force)` | 중심에 힘 적용 |
| `ApplyLinearImpulse(string id, Vector2 impulse, Vector2 point)` | 특정 지점에 충격 적용 |
| `SetLinearVelocity(string id, Vector2 velocity)` | 바디의 속도 설정 |
| `GetLinearVelocity(string id)` | 바디의 현재 속도 반환 |
| `DestroyBody(string id)` | 바디 삭제 |
| `GetAllBodyIds()` | 모든 바디 ID 목록 반환 |
| `HasBody(string id)` | 바디 존재 여부 확인 |

## 프로젝트 구조

```
Physics_Box2D/
├── Physics_Box2D/              # 메인 모듈 프로젝트
│   ├── PhysicsWorld.cs         # 물리 월드 래퍼 클래스
│   └── Physics_Box2D.csproj
├── Physics_Box2D_test/         # 유닛 테스트 프로젝트
│   ├── PhysicsWorldTests.cs    # 테스트 코드
│   └── Physics_Box2D_test.csproj
├── Physics_Box2D_Sample/       # 샘플 프로젝트
│   ├── Program.cs              # 샘플 코드
│   └── Physics_Box2D_Sample.csproj
├── Physics_Box2D.sln           # 솔루션 파일
├── Physics_Box2D_plan.md       # 개발 계획
├── Physics_Box2D_progress.md   # 개발 기록
└── readme.md                   # 이 파일
```

## 테스트 실행

⚠️ **주의**: 현재 테스트는 Box2D.NET 3.x API에 맞춰 업데이트가 필요합니다.

```bash
cd Physics_Box2D_test
dotnet test
```

## 샘플 실행

⚠️ **주의**: 현재 샘플은 Box2D.NET 3.x API에 맞춰 업데이트가 필요합니다.

```bash
cd Physics_Box2D_Sample
dotnet run
```

## 기술적 세부사항

### Box2D.NET 3.x API
Box2D.NET 3.x는 Box2D 3.0을 기반으로 하며, 이전 버전과 완전히 다른 API를 사용합니다:
- C 스타일의 정적 함수 기반 API (`b2CreateWorld`, `b2Body_GetPosition` 등)
- Struct 기반 ID 시스템 (`B2WorldId`, `B2BodyId`)
- `in`/`ref` 키워드를 사용한 메모리 효율적 설계

### 좌표계
- Box2D는 미터 단위를 사용합니다
- Y축이 위를 향하는 우수 좌표계

### 물리 파라미터 기본값
- **중력**: (0, -10) m/s²
- **밀도**: 1.0 kg/m²
- **마찰력**: 0.3
- **반발력**: 0.5

### 시뮬레이션 설정
- 권장 타임스텝: 1/60초 (60 FPS)
- Sub-step count: 4

## 향후 계획

- [ ] 충돌 감지 및 콜백 시스템
- [ ] 조인트(Joint) 지원
- [ ] 레이캐스팅(Raycasting)
- [ ] 영역 쿼리(Area Query)
- [ ] 네트워크 동기화를 위한 상태 직렬화
- [ ] 물리 디버그 렌더링 도구

## 라이선스

이 프로젝트는 Box2D.NET 라이브러리를 사용합니다.
- Box2D.NET: https://github.com/ikpil/Box2D.NET

## 참고자료

- [Box2D.NET GitHub](https://github.com/ikpil/Box2D.NET)
- [Box2D 공식 문서](https://box2d.org/documentation/)
- [Box2D 매뉴얼](https://box2d.org/documentation/md__d_1__git_hub_box2d_docs_dynamics.html)
