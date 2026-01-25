using Physics;
using System.Numerics;
using Raylib_cs;

namespace Physics_Sample;

/// <summary>
/// Physics 그래픽 샘플 프로그램
/// Raylib을 사용하여 물리 시뮬레이션을 시각화합니다.
/// </summary>
public class Program
{
    private const int ScreenWidth = 1280;
    private const int ScreenHeight = 720;
    private const float PixelsPerMeter = 20.0f;
    private const float TimeStep = 1.0f / 60.0f;

    private static World? _world;
    private static List<VisualBody> _visualBodies = new();
    private static bool _isPaused = false;
    
    // 마우스 드래그 상태
    private static bool _isDragging = false;
    private static Vector2 _dragStart = Vector2.Zero;
    private static IBody? _selectedBody = null;

    public static void Main(string[] args)
    {
        // Raylib 초기화
        Raylib.InitWindow(ScreenWidth, ScreenHeight, "Physics Sample - 그래픽 시뮬레이션");
        Raylib.SetTargetFPS(60);

        // 물리 월드 생성
        InitializePhysicsWorld();

        // 메인 루프
        while (!Raylib.WindowShouldClose())
        {
            Update();
            Draw();
        }

        // 정리
        Raylib.CloseWindow();
    }

    private static void InitializePhysicsWorld()
    {
        _world = new World(new Vector2(0, -10));
        _visualBodies.Clear();

        // 지면 생성
        var ground = _world.CreateStaticBox("ground", new Vector2(0, -8), 30, 1);
        _visualBodies.Add(new VisualBody(ground, Color.Gray));

        // 좌우 벽 생성
        var leftWall = _world.CreateStaticBox("leftWall", new Vector2(-16, 0), 1, 20);
        _visualBodies.Add(new VisualBody(leftWall, Color.Gray));

        var rightWall = _world.CreateStaticBox("rightWall", new Vector2(16, 0), 1, 20);
        _visualBodies.Add(new VisualBody(rightWall, Color.Gray));

        // 동적 박스들 생성
        Random rand = new Random();
        for (int i = 0; i < 10; i++)
        {
            float x = rand.NextSingle() * 10 - 5;
            float y = rand.NextSingle() * 10 + 5;
            float size = rand.NextSingle() * 0.8f + 0.5f;
            
            string id = $"box{i}";
            var box = _world.CreateDynamicBox(id, new Vector2(x, y), size, size, restitution: 0.3f);
            
            Color color = new Color(
                rand.Next(100, 255),
                rand.Next(100, 255),
                rand.Next(100, 255),
                255
            );
            _visualBodies.Add(new VisualBody(box, color));
        }

        // 동적 원들 생성
        for (int i = 0; i < 5; i++)
        {
            float x = rand.NextSingle() * 8 - 4;
            float y = rand.NextSingle() * 8 + 8;
            float radius = rand.NextSingle() * 0.5f + 0.3f;
            
            string id = $"circle{i}";
            var circle = _world.CreateDynamicCircle(id, new Vector2(x, y), radius, restitution: 0.6f);
            
            Color color = new Color(
                rand.Next(100, 255),
                rand.Next(100, 255),
                rand.Next(100, 255),
                255
            );
            _visualBodies.Add(new VisualBody(circle, color));
        }
    }

    private static void Update()
    {
        // 키보드 입력 처리
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            _isPaused = !_isPaused;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.R))
        {
            InitializePhysicsWorld();
        }

        if (Raylib.IsKeyPressed(KeyboardKey.B))
        {
            // 랜덤 박스 추가
            Random rand = new Random();
            float x = rand.NextSingle() * 10 - 5;
            float y = 15;
            float size = rand.NextSingle() * 0.8f + 0.5f;
            
            string id = $"box{_visualBodies.Count}";
            var box = _world?.CreateDynamicBox(id, new Vector2(x, y), size, size, restitution: 0.3f);
            
            if (box != null)
            {
                Color color = new Color(
                    rand.Next(100, 255),
                    rand.Next(100, 255),
                    rand.Next(100, 255),
                    255
                );
                _visualBodies.Add(new VisualBody(box, color));
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.C))
        {
            // 랜덤 원 추가
            Random rand = new Random();
            float x = rand.NextSingle() * 8 - 4;
            float y = 15;
            float radius = rand.NextSingle() * 0.5f + 0.3f;
            
            string id = $"circle{_visualBodies.Count}";
            var circle = _world?.CreateDynamicCircle(id, new Vector2(x, y), radius, restitution: 0.6f);
            
            if (circle != null)
            {
                Color color = new Color(
                    rand.Next(100, 255),
                    rand.Next(100, 255),
                    rand.Next(100, 255),
                    255
                );
                _visualBodies.Add(new VisualBody(circle, color));
            }
        }

        // 마우스 드래그로 물체에 힘 적용
        HandleMouseDrag();

        // 물리 시뮬레이션 업데이트
        if (!_isPaused && _world != null)
        {
            _world.Step(TimeStep);
        }
    }

    private static void HandleMouseDrag()
    {
        Vector2 mousePos = Raylib.GetMousePosition();
        Vector2 worldPos = ScreenToWorld(mousePos);

        // 마우스 버튼 누름 - 드래그 시작
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            // 가까운 물체 찾기
            foreach (var visualBody in _visualBodies)
            {
                var body = visualBody.Body;
                
                // 정적 바디는 건너뜀
                if (body is StaticBox)
                    continue;

                float distance = Vector2.Distance(worldPos, body.Position);
                float threshold = body is Box box ? Math.Max(box.Width, box.Height) : 
                                 body is Circle circle ? circle.Radius * 2 : 1.0f;
                
                if (distance < threshold)
                {
                    _isDragging = true;
                    _dragStart = worldPos;
                    _selectedBody = body;
                    break;
                }
            }
        }

        // 마우스 버튼 떼기 - 힘 적용
        if (Raylib.IsMouseButtonReleased(MouseButton.Left) && _isDragging)
        {
            if (_selectedBody != null)
            {
                Vector2 dragEnd = worldPos;
                Vector2 dragVector = dragEnd - _dragStart;
                
                // 드래그 벡터를 충격으로 변환 (스케일 조정)
                Vector2 impulse = dragVector * 5.0f;
                _selectedBody.ApplyLinearImpulse(impulse, _selectedBody.Position);
            }

            _isDragging = false;
            _selectedBody = null;
        }

        // 드래그 중단 (ESC나 윈도우 포커스 잃을 때)
        if (_isDragging && !Raylib.IsMouseButtonDown(MouseButton.Left))
        {
            _isDragging = false;
            _selectedBody = null;
        }
    }

    private static void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(30, 30, 40, 255));

        // 물리 객체 그리기
        foreach (var visualBody in _visualBodies)
        {
            var body = visualBody.Body;
            Vector2 pos = body.Position;
            float angle = body.Angle;
            Vector2 screenPos = WorldToScreen(pos);

            if (body is Box box)
            {
                float width = box.Width * PixelsPerMeter;
                float height = box.Height * PixelsPerMeter;
                
                // 회전을 고려한 사각형 그리기
                var rectangle = new Rectangle(
                    screenPos.X,
                    screenPos.Y,
                    width,
                    height
                );
                
                Vector2 origin = new Vector2(width / 2, height / 2);
                float angleDeg = angle * (180.0f / MathF.PI);
                
                Raylib.DrawRectanglePro(rectangle, origin, angleDeg, visualBody.Color);
            }
            else if (body is Circle circle)
            {
                float radius = circle.Radius * PixelsPerMeter;
                Raylib.DrawCircleV(screenPos, radius, visualBody.Color);
            }
            else if (body is StaticBox staticBox)
            {
                float width = staticBox.Width * PixelsPerMeter;
                float height = staticBox.Height * PixelsPerMeter;
                
                var rectangle = new Rectangle(
                    screenPos.X,
                    screenPos.Y,
                    width,
                    height
                );
                
                Vector2 origin = new Vector2(width / 2, height / 2);
                float angleDeg = angle * (180.0f / MathF.PI);
                
                Raylib.DrawRectanglePro(rectangle, origin, angleDeg, visualBody.Color);
            }
        }

        // 드래그 중일 때 화살표 그리기
        if (_isDragging && _selectedBody != null)
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            Vector2 currentWorldPos = ScreenToWorld(mousePos);
            
            Vector2 dragStartScreen = WorldToScreen(_dragStart);
            Vector2 dragVector = currentWorldPos - _dragStart;
            Vector2 dragVectorScreen = new Vector2(dragVector.X * PixelsPerMeter, -dragVector.Y * PixelsPerMeter);
            
            // 드래그 화살표 그리기
            DrawArrow(dragStartScreen, dragStartScreen + dragVectorScreen, 4, Color.Yellow);
            
            // 힘의 크기 표시
            float forceMagnitude = dragVector.Length() * 5.0f;
            string forceText = $"Force: {forceMagnitude:F1}";
            Raylib.DrawText(forceText, (int)mousePos.X + 10, (int)mousePos.Y - 20, 16, Color.Yellow);
        }

        // UI 텍스트 그리기
        Raylib.DrawText("Physics Sample", 10, 10, 24, Color.White);
        Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 10, 40, 20, Color.LightGray);
        Raylib.DrawText($"Objects: {_visualBodies.Count}", 10, 65, 20, Color.LightGray);
        Raylib.DrawText(_isPaused ? "PAUSED" : "RUNNING", 10, 90, 20, _isPaused ? Color.Yellow : Color.Green);
        
        Raylib.DrawText("Controls:", 10, ScreenHeight - 150, 18, Color.White);
        Raylib.DrawText("  SPACE - Pause/Resume", 10, ScreenHeight - 125, 16, Color.LightGray);
        Raylib.DrawText("  R - Reset", 10, ScreenHeight - 105, 16, Color.LightGray);
        Raylib.DrawText("  B - Add Box", 10, ScreenHeight - 85, 16, Color.LightGray);
        Raylib.DrawText("  C - Add Circle", 10, ScreenHeight - 65, 16, Color.LightGray);
        Raylib.DrawText("  Left Click & Drag - Apply Impulse", 10, ScreenHeight - 45, 16, Color.LightGray);
        Raylib.DrawText("  ESC - Exit", 10, ScreenHeight - 25, 16, Color.LightGray);

        Raylib.EndDrawing();
    }

    private static void DrawArrow(Vector2 start, Vector2 end, float thickness, Color color)
    {
        // 화살표 본체
        Raylib.DrawLineEx(start, end, thickness, color);
        
        // 화살표 머리
        Vector2 direction = Vector2.Normalize(end - start);
        float arrowLength = 15f;
        float arrowAngle = 25f * MathF.PI / 180f;
        
        // 회전 행렬을 사용한 화살표 끝 계산
        Vector2 arrowLeft = new Vector2(
            direction.X * MathF.Cos(MathF.PI - arrowAngle) - direction.Y * MathF.Sin(MathF.PI - arrowAngle),
            direction.X * MathF.Sin(MathF.PI - arrowAngle) + direction.Y * MathF.Cos(MathF.PI - arrowAngle)
        ) * arrowLength;
        
        Vector2 arrowRight = new Vector2(
            direction.X * MathF.Cos(MathF.PI + arrowAngle) - direction.Y * MathF.Sin(MathF.PI + arrowAngle),
            direction.X * MathF.Sin(MathF.PI + arrowAngle) + direction.Y * MathF.Cos(MathF.PI + arrowAngle)
        ) * arrowLength;
        
        Raylib.DrawLineEx(end, end + arrowLeft, thickness, color);
        Raylib.DrawLineEx(end, end + arrowRight, thickness, color);
    }

    private static Vector2 WorldToScreen(Vector2 worldPos)
    {
        return new Vector2(
            ScreenWidth / 2 + worldPos.X * PixelsPerMeter,
            ScreenHeight / 2 - worldPos.Y * PixelsPerMeter
        );
    }

    private static Vector2 ScreenToWorld(Vector2 screenPos)
    {
        return new Vector2(
            (screenPos.X - ScreenWidth / 2) / PixelsPerMeter,
            -(screenPos.Y - ScreenHeight / 2) / PixelsPerMeter
        );
    }

    /// <summary>
    /// 물리 바디와 시각적 표현을 연결하는 클래스
    /// </summary>
    private record VisualBody(IBody Body, Color Color);
}
