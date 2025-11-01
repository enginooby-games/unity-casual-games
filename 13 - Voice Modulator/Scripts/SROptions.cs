using System.ComponentModel;

/// <summary>
/// SRDebugger options for Voice Modulator customizable parameters.
/// Allows runtime adjustment of pitch, reverb, and gain settings.
/// </summary>
public partial class SROptions
{
    #region Voice Modulator Parameters ==================================================================
    
    private float voiceModulator_PitchShift = 0f;
    private float voiceModulator_ReverbRoomSize = 0.3f;
    private float voiceModulator_ReverbMix = 0.3f;
    private float voiceModulator_InputGain = 1.0f;
    
    [Category("VoiceModulator")]
    [DisplayName("Pitch Shift (semitones)")]
    [NumberRange(-12, 12)]
    [Description("Adjust pitch from -12 (lower) to +12 (higher) semitones")]
    public float VoiceModulator_PitchShift
    {
        get => voiceModulator_PitchShift;
        set
        {
            voiceModulator_PitchShift = value;
            UpdateVoiceModulatorParameters();
        }
    }
    
    [Category("VoiceModulator")]
    [DisplayName("Reverb Room Size")]
    [NumberRange(0f, 1f)]
    [Description("Control the size of the reverb room (0 = small, 1 = large)")]
    public float VoiceModulator_ReverbRoomSize
    {
        get => voiceModulator_ReverbRoomSize;
        set
        {
            voiceModulator_ReverbRoomSize = value;
            UpdateVoiceModulatorParameters();
        }
    }
    
    [Category("VoiceModulator")]
    [DisplayName("Reverb Mix")]
    [NumberRange(0f, 1f)]
    [Description("Control wet/dry mix of reverb effect (0 = dry, 1 = wet)")]
    public float VoiceModulator_ReverbMix
    {
        get => voiceModulator_ReverbMix;
        set
        {
            voiceModulator_ReverbMix = value;
            UpdateVoiceModulatorParameters();
        }
    }
    
    [Category("VoiceModulator")]
    [DisplayName("Input Gain")]
    [NumberRange(0.1f, 3f)]
    [Description("Adjust input audio gain (0.1 = quiet, 3 = loud)")]
    public float VoiceModulator_InputGain
    {
        get => voiceModulator_InputGain;
        set
        {
            voiceModulator_InputGain = value;
            UpdateVoiceModulatorParameters();
        }
    }
    
    #endregion ==================================================================
    
    #region Update Methods ==================================================================
    
    /// <summary>
    /// Updates all voice modulator parameters in the manager.
    /// </summary>
    private void UpdateVoiceModulatorParameters()
    {
        var manager = Devdy.VoiceModulator.VoiceModulatorManager.Instance;
        if (manager != null)
        {
            manager.UpdateEffectParameters(
                voiceModulator_PitchShift,
                voiceModulator_ReverbRoomSize,
                voiceModulator_ReverbMix,
                voiceModulator_InputGain
            );
        }
    }
    
    #endregion ==================================================================
}
