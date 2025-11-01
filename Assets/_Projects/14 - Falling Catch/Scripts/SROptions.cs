using System.ComponentModel;

/// <summary>
/// SRDebugger options for runtime customization of FallingCatch parameters.
/// </summary>
public partial class SROptions
{
    #region Spawn Settings
    private float fallingCatch_SpawnInterval = 1f;

    [Category("FallingCatch")]
    [NumberRange(0.1f, 5f)]
    [DisplayName("Spawn Interval")]
    public float FallingCatch_SpawnInterval
    {
        get => fallingCatch_SpawnInterval;
        set
        {
            fallingCatch_SpawnInterval = value;
        }
    }

    private float fallingCatch_GoodObjectRatio = 0.6f;

    [Category("FallingCatch")]
    [NumberRange(0f, 1f)]
    [DisplayName("Good Object Ratio")]
    public float FallingCatch_GoodObjectRatio
    {
        get => fallingCatch_GoodObjectRatio;
        set
        {
            fallingCatch_GoodObjectRatio = value;
        }
    }
    #endregion

    #region Object Settings
    private float fallingCatch_FallSpeed = 3f;

    [Category("FallingCatch")]
    [NumberRange(1f, 10f)]
    [DisplayName("Fall Speed")]
    public float FallingCatch_FallSpeed
    {
        get => fallingCatch_FallSpeed;
        set
        {
            fallingCatch_FallSpeed = value;
        }
    }
    #endregion

    #region Game Settings
    private float fallingCatch_GameDuration = 60f;

    [Category("FallingCatch")]
    [NumberRange(10f, 300f)]
    [DisplayName("Game Duration")]
    public float FallingCatch_GameDuration
    {
        get => fallingCatch_GameDuration;
        set
        {
            fallingCatch_GameDuration = value;
            Devdy.FallingCatch.GameManager.Instance.RestartGame();
        }
    }

    private int fallingCatch_StartingHealth = 3;

    [Category("FallingCatch")]
    [NumberRange(1, 10)]
    [DisplayName("Starting Health")]
    public int FallingCatch_StartingHealth
    {
        get => fallingCatch_StartingHealth;
        set
        {
            fallingCatch_StartingHealth = value;
            Devdy.FallingCatch.GameManager.Instance.RestartGame();
        }
    }
    #endregion

    #region Player Settings
    private float fallingCatch_PlayerSpeed = 10f;

    [Category("FallingCatch")]
    [NumberRange(1f, 20f)]
    [DisplayName("Player Speed")]
    public float FallingCatch_PlayerSpeed
    {
        get => fallingCatch_PlayerSpeed;
        set
        {
            fallingCatch_PlayerSpeed = value;
        }
    }
    #endregion
}