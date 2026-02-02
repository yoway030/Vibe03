using System.Numerics;
using Raylib_cs;

namespace Client;

/// <summary>
/// 카메라 - 뷰포트 변환 및 컨트롤 담당
/// </summary>
public class Camera
{
    /// <summary>
    /// 화면 오프셋 (좌상단 여백)
    /// </summary>
    public Vector2 ScreenOffset { get; }

    /// <summary>
    /// 카메라 이동 오프셋
    /// </summary>
    public Vector2 CameraOffset { get; private set; }

    /// <summary>
    /// 목표 스케일 팩터
    /// </summary>
    private float _targetScaleFactor;

    /// <summary>
    /// 현재 스케일 팩터 (부드러운 줌을 위한 보간)
    /// </summary>
    private float _scaleFactor;

    /// <summary>
    /// 드래그 시작 마우스 위치
    /// </summary>
    private Vector2 _dragStartMousePos;

    /// <summary>
    /// 드래그 시작 카메라 오프셋
    /// </summary>
    private Vector2 _dragStartCameraOffset;

    /// <summary>
    /// 드래그 중 여부
    /// </summary>
    private bool _isDragging;

    /// <summary>
    /// 최소 줌 레벨 (2%)
    /// </summary>
    public const float MinZoom = 0.02f;

    /// <summary>
    /// 최대 줌 레벨 (300%)
    /// </summary>
    public const float MaxZoom = 3.0f;

    /// <summary>
    /// 줌 보간 속도
    /// </summary>
    public const float ZoomLerpSpeed = 10.0f;

    /// <summary>
    /// 현재 스케일 팩터
    /// </summary>
    public float ScaleFactor => _scaleFactor;

    public Camera(Vector2 screenOffset, float initialZoom = 0.1f)
    {
        ScreenOffset = screenOffset;
        CameraOffset = Vector2.Zero;
        _targetScaleFactor = initialZoom;
        _scaleFactor = initialZoom;
        _isDragging = false;
    }

    /// <summary>
    /// 카메라 업데이트 (입력 처리)
    /// </summary>
    public void Update(float deltaTime)
    {
        HandleDrag();
        HandleZoom();
        UpdateZoomInterpolation(deltaTime);
    }

    /// <summary>
    /// 월드 좌표를 화면 좌표로 변환
    /// </summary>
    public Vector2 WorldToScreen(Vector2 worldPos)
    {
        return new Vector2(
            ScreenOffset.X + worldPos.X * _scaleFactor + CameraOffset.X,
            ScreenOffset.Y + worldPos.Y * _scaleFactor + CameraOffset.Y
        );
    }

    /// <summary>
    /// 화면 좌표를 월드 좌표로 변환
    /// </summary>
    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        return new Vector2(
            (screenPos.X - ScreenOffset.X - CameraOffset.X) / _scaleFactor,
            (screenPos.Y - ScreenOffset.Y - CameraOffset.Y) / _scaleFactor
        );
    }

    /// <summary>
    /// 월드 크기를 화면 크기로 변환
    /// </summary>
    public float WorldToScreenSize(float worldSize)
    {
        return worldSize * _scaleFactor;
    }

    /// <summary>
    /// 드래그 처리
    /// </summary>
    private void HandleDrag()
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            _isDragging = true;
            _dragStartMousePos = Raylib.GetMousePosition();
            _dragStartCameraOffset = CameraOffset;
        }

        if (Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            _isDragging = false;
        }

        if (_isDragging)
        {
            Vector2 currentMousePos = Raylib.GetMousePosition();
            Vector2 delta = currentMousePos - _dragStartMousePos;
            CameraOffset = _dragStartCameraOffset + delta;
        }
    }

    /// <summary>
    /// 줌 입력 처리 (마우스 위치 기준)
    /// </summary>
    private void HandleZoom()
    {
        Vector2 mousePos = Raylib.GetMousePosition();
        
        float wheelMove = Raylib.GetMouseWheelMove();
        if (wheelMove != 0)
        {
            // 줌 전 마우스가 가리키는 월드 좌표 저장
            Vector2 worldPosBeforeZoom = ScreenToWorld(mousePos);
            
            // 마우스 휠로 줌
            float zoomSpeed = 0.1f;
            _targetScaleFactor += wheelMove * zoomSpeed * _targetScaleFactor;
            _targetScaleFactor = Math.Clamp(_targetScaleFactor, MinZoom, MaxZoom);
            
            // 즉시 적용 (부드러운 보간은 UpdateZoomInterpolation에서)
            _scaleFactor = _targetScaleFactor;
            
            // 줌 후 같은 월드 좌표가 화면에서 어디에 있는지 계산
            Vector2 newScreenPos = WorldToScreen(worldPosBeforeZoom);
            
            // 마우스 위치로 돌아오도록 카메라 오프셋 조정
            Vector2 screenDelta = mousePos - newScreenPos;
            CameraOffset += screenDelta;
        }

        // 키보드로 줌 (+ / -)
        if (Raylib.IsKeyDown(KeyboardKey.Equal) || Raylib.IsKeyDown(KeyboardKey.KpAdd))
        {
            _targetScaleFactor *= 1.02f;
            _targetScaleFactor = Math.Min(_targetScaleFactor, MaxZoom);
        }
        if (Raylib.IsKeyDown(KeyboardKey.Minus) || Raylib.IsKeyDown(KeyboardKey.KpSubtract))
        {
            _targetScaleFactor *= 0.98f;
            _targetScaleFactor = Math.Max(_targetScaleFactor, MinZoom);
        }
    }

    /// <summary>
    /// 줌 보간 업데이트 (부드러운 줌) - 키보드 줌용
    /// </summary>
    private void UpdateZoomInterpolation(float deltaTime)
    {
        if (Math.Abs(_scaleFactor - _targetScaleFactor) > 0.001f)
        {
            // 키보드 줌은 부드럽게 보간
            _scaleFactor += (_targetScaleFactor - _scaleFactor) * ZoomLerpSpeed * deltaTime;
        }
    }

    /// <summary>
    /// 뷰포트 범위 계산 (화면에 보이는 월드 영역)
    /// </summary>
    public (int minX, int maxX, int minY, int maxY) CalculateViewport(int screenWidth, int screenHeight, float cellSize, int gridWidth, int gridHeight, int lodSkip = 1)
    {
        float scaledCellSize = cellSize * _scaleFactor;

        int minX = Math.Max(0, (int)(((-ScreenOffset.X - CameraOffset.X) / scaledCellSize) / lodSkip) * lodSkip);
        int minY = Math.Max(0, (int)(((-ScreenOffset.Y - CameraOffset.Y) / scaledCellSize) / lodSkip) * lodSkip);
        int maxX = Math.Min(gridWidth - 1, (int)(((-ScreenOffset.X - CameraOffset.X + screenWidth) / scaledCellSize) / lodSkip) * lodSkip + lodSkip);
        int maxY = Math.Min(gridHeight - 1, (int)(((-ScreenOffset.Y - CameraOffset.Y + screenHeight) / scaledCellSize) / lodSkip) * lodSkip + lodSkip);

        return (minX, maxX, minY, maxY);
    }

    /// <summary>
    /// LOD (Level of Detail) 스킵 계산
    /// </summary>
    public int CalculateLodSkip()
    {
        if (_scaleFactor < 0.05f) return 10;      // 5% 이하: 10개 중 1개
        if (_scaleFactor < 0.1f) return 5;        // 10% 이하: 5개 중 1개
        if (_scaleFactor < 0.2f) return 2;        // 20% 이하: 2개 중 1개
        return 1;                                 // 20% 이상: 모든 셀
    }
}
