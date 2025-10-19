using System.ComponentModel;

public partial class SROptions
{
    private int aStarVisualizer_GridWidth = 20;
    private int aStarVisualizer_GridHeight = 15;
    private float aStarVisualizer_AnimationSpeed = 0.5f;
    private int aStarVisualizer_PolygonType = 0;
    private bool aStarVisualizer_AllowDiagonal = true;

    [Category("AStarVisualizer")]
    [DisplayName("Grid Width")]
    [NumberRange(5, 50)]
    public int AStarVisualizer_GridWidth
    {
        get => aStarVisualizer_GridWidth;
        set
        {
            aStarVisualizer_GridWidth = value;
            Devdy.AStarVisualizer.GameManager.Instance.RestartGame();
        }
    }

    [Category("AStarVisualizer")]
    [DisplayName("Grid Height")]
    [NumberRange(5, 50)]
    public int AStarVisualizer_GridHeight
    {
        get => aStarVisualizer_GridHeight;
        set
        {
            aStarVisualizer_GridHeight = value;
            Devdy.AStarVisualizer.GameManager.Instance.RestartGame();
        }
    }

    [Category("AStarVisualizer")]
    [DisplayName("Animation Speed")]
    [NumberRange(0.1f, 1.0f)]
    [Increment(0.1f)]
    public float AStarVisualizer_AnimationSpeed
    {
        get => aStarVisualizer_AnimationSpeed;
        set
        {
            aStarVisualizer_AnimationSpeed = value;
            float delay = (1f - aStarVisualizer_AnimationSpeed) * 0.5f;
            Devdy.AStarVisualizer.AStarPathfinder.Instance.SetStepDelay(delay);
        }
    }

    [Category("AStarVisualizer")]
    [DisplayName("Polygon Type (0=Square, 1=Hexagon)")]
    [NumberRange(0, 1)]
    public int AStarVisualizer_PolygonType
    {
        get => aStarVisualizer_PolygonType;
        set
        {
            aStarVisualizer_PolygonType = value;
            Devdy.AStarVisualizer.GameManager.Instance.RestartGame();
        }
    }

    [Category("AStarVisualizer")]
    [DisplayName("Allow Diagonal Movement")]
    public bool AStarVisualizer_AllowDiagonal
    {
        get => aStarVisualizer_AllowDiagonal;
        set
        {
            aStarVisualizer_AllowDiagonal = value;
            Devdy.AStarVisualizer.AStarPathfinder.Instance.SetDiagonalMovement(aStarVisualizer_AllowDiagonal);
        }
    }
}