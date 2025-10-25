using System.ComponentModel;

/// <summary>
/// SRDebugger options for Rush Hour game.
/// Exposes runtime adjustable parameters for tweaking gameplay.
/// </summary>
public partial class SROptions
{
    #region Player Settings
    private float rushHour_MoveDelay = 0.3f;
    private int rushHour_StartingLives = 3;

    [Category("RushHour")]
    [DisplayName("Move Delay")]
    [NumberRange(0.05f, 0.5f)]
    public float RushHour_MoveDelay
    {
        get => rushHour_MoveDelay;
        set
        {
            rushHour_MoveDelay = value;
            // Player will pick up new value on next move
        }
    }

    // [Category("RushHour")]
    [DisplayName("Starting Lives")]
    [NumberRange(1, 10)]
    public int RushHour_StartingLives
    {
        get => rushHour_StartingLives;
        set
        {
            rushHour_StartingLives = value;
            if (Devdy.RushHour.GameManager.Instance != null)
            {
                Devdy.RushHour.GameManager.Instance.RestartGame();
            }
        }
    }
    #endregion

    #region Difficulty Settings
    private float rushHour_BaseCarSpeed = 2.5f;
    private float rushHour_CarSpawnRate = 0.1f;
    private int rushHour_MaxCarsOnScreen = 20;

    [Category("RushHour")]
    [DisplayName("Base Car Speed")]
    [NumberRange(1f, 10f)]
    public float RushHour_BaseCarSpeed
    {
        get => rushHour_BaseCarSpeed;
        set
        {
            rushHour_BaseCarSpeed = value;
            // CarSpawner will use new value for next spawned cars
        }
    }

    [Category("RushHour")]
    [DisplayName("Car Spawn Rate")]
    [NumberRange(0.01f, 1f)]
    public float RushHour_CarSpawnRate
    {
        get => rushHour_CarSpawnRate;
        set
        {
            rushHour_CarSpawnRate = value;
        }
    }

    [Category("RushHour")]
    [DisplayName("Max Cars On Screen")]
    [NumberRange(5, 40)]
    public int RushHour_MaxCarsOnScreen
    {
        get => rushHour_MaxCarsOnScreen;
        set
        {
            rushHour_MaxCarsOnScreen = value;
        }
    }
    #endregion

    #region Powerup Durations
    private float rushHour_ShieldDuration = 10f;
    private float rushHour_SlowMoDuration = 7f;
    private float rushHour_MagnetDuration = 8f;

    [Category("RushHour")]
    [DisplayName("Shield Duration (s)")]
    [NumberRange(5f, 30f)]
    public float RushHour_ShieldDuration
    {
        get => rushHour_ShieldDuration;
        set
        {
            rushHour_ShieldDuration = value;
        }
    }

    [Category("RushHour")]
    [DisplayName("Slow-Mo Duration (s)")]
    [NumberRange(3f, 20f)]
    public float RushHour_SlowMoDuration
    {
        get => rushHour_SlowMoDuration;
        set
        {
            rushHour_SlowMoDuration = value;
        }
    }

    [Category("RushHour")]
    [DisplayName("Magnet Duration (s)")]
    [NumberRange(5f, 30f)]
    public float RushHour_MagnetDuration
    {
        get => rushHour_MagnetDuration;
        set
        {
            rushHour_MagnetDuration = value;
        }
    }
    #endregion
}