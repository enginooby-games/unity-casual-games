using System.ComponentModel;
using UnityEngine.Rendering;

public partial class SROptions
{
    private int birthdayCake_TotalLayers = 5;
    private float birthdayCake_DropSpeed = 5f;
    private float birthdayCake_StabilityThreshold = 0.25f;
    private float birthdayCake_SizeScale = 1f;
    private int birthdayCake_CandleCount = 1;
    private float birthdayCake_SizeReduction = 0.15f;

    [Category("BirthdayCake")]
    [NumberRange(3, 8)]
    [DisplayName("Total Layers")]
    public int BirthdayCake_TotalLayers
    {
        get => birthdayCake_TotalLayers;
        set
        {
            birthdayCake_TotalLayers = value;
            Devdy.BirthdayCake.GameManager.Instance.RestartGame();
        }
    }

    [Category("BirthdayCake")]
    [NumberRange(0.5f, 10.0f)]
    [DisplayName("Speed")]
    public float BirthdayCake_DropSpeed
    {
        get => birthdayCake_DropSpeed;
        set
        {
            birthdayCake_DropSpeed = value;
            Devdy.BirthdayCake.GameManager.Instance.RestartGame();
        }
    }
    
    [Category("BirthdayCake")]
    [NumberRange(0.2f, 5.0f)]
    [DisplayName("Size Scale")]
    public float BirthdayCake_SizeScale
    {
        get => birthdayCake_SizeScale;
        set
        {
            birthdayCake_SizeScale = value;
            Devdy.BirthdayCake.GameManager.Instance.RestartGame();
        }
    }

    [Category("BirthdayCake")]
    [NumberRange(0.1f, 0.8f)]
    [DisplayName("Stability Threshold")]
    public float BirthdayCake_StabilityThreshold
    {
        get => birthdayCake_StabilityThreshold;
        set
        {
            birthdayCake_StabilityThreshold = value;
            Devdy.BirthdayCake.GameManager.Instance.RestartGame();
        }
    }

    [Category("BirthdayCake")]
    [NumberRange(0f, 0.2f)]
    [DisplayName("Size Reduction/Layer")]
    public float BirthdayCake_SizeReduction
    {
        get => birthdayCake_SizeReduction;
        set
        {
            birthdayCake_SizeReduction = value;
            Devdy.BirthdayCake.GameManager.Instance.RestartGame();
        }
    }
}