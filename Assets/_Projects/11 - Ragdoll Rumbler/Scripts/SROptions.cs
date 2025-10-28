using System.ComponentModel;
using Devdy.RagdollTumbler;

/// <summary>
/// Exposes customizable game parameters to SRDebugger for runtime adjustment.
/// </summary>
public partial class SROptions
{
    #region Ragdoll Tumbler Parameters
    
    private float ragdollTumbler_TumbleForce = 12f;
    private float ragdollTumbler_GravityScale = 2f;
    private float ragdollTumbler_RagdollMass = 3f;
    private int ragdollTumbler_MaxLevel = 5;

    [Category("RagdollTumbler")]
    [DisplayName("Tumble Force")]
    [NumberRange(1f, 20f)]
    public float RagdollTumbler_TumbleForce
    {
        get => ragdollTumbler_TumbleForce;
        set
        {
            ragdollTumbler_TumbleForce = value;
            if (GameManager.Instance != null && GameManager.Instance.Config != null)
            {
                GameManager.Instance.Config.tumbleForce = value;
                GameManager.RestartGame();
            }
        }
    }

    [Category("RagdollTumbler")]
    [DisplayName("Gravity Scale")]
    [NumberRange(1f, 5f)]
    public float RagdollTumbler_GravityScale
    {
        get => ragdollTumbler_GravityScale;
        set
        {
            ragdollTumbler_GravityScale = value;
            if (GameManager.Instance != null && GameManager.Instance.Config != null)
            {
                GameManager.Instance.Config.gravityScale = value;
                GameManager.RestartGame();
            }
        }
    }

    [Category("RagdollTumbler")]
    [DisplayName("Ragdoll Mass")]
    [NumberRange(1f, 10f)]
    public float RagdollTumbler_RagdollMass
    {
        get => ragdollTumbler_RagdollMass;
        set
        {
            ragdollTumbler_RagdollMass = value;
            if (GameManager.Instance != null && GameManager.Instance.Config != null)
            {
                GameManager.Instance.Config.ragdollMass = value;
                GameManager.RestartGame();
            }
        }
    }

    [Category("RagdollTumbler")]
    [DisplayName("Max Level")]
    [NumberRange(1, 10)]
    public int RagdollTumbler_MaxLevel
    {
        get => ragdollTumbler_MaxLevel;
        set
        {
            ragdollTumbler_MaxLevel = value;
            if (GameManager.Instance != null && GameManager.Instance.Config != null)
            {
                GameManager.Instance.Config.maxLevel = value;
                GameManager.RestartGame();
            }
        }
    }

    #endregion ==================================================================
}
