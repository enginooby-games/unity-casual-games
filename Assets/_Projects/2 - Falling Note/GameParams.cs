using System.ComponentModel;
using Project2;

public partial class SROptions {
    private int _P2_LaneAmount = 5;
    private int _P2_Bpm = 70;
    private float _P2_NoteSpeed = 6;
    private int _P2_MaxConcurrentNotes = 2;
    
    [Category("Project2")]
    [NumberRange(2, 7)]
    [DisplayName("Lane amount")]
    public int P2_LaneAmount {
        get => _P2_LaneAmount;
        set {
            _P2_LaneAmount = value;
            GameManager.instance.RestartGame();
        }
    }
    
    [Category("Project2")]
    [NumberRange(10, 300)]
    [DisplayName("PBM")]
    public int P2_Pbm {
        get => _P2_Bpm;
        set {
            _P2_Bpm = value;
            GameManager.instance.RestartGame();
        }
    }
    
    [Category("Project2")]
    [NumberRange(1, 5)]
    [DisplayName("Max concurrent note")]
    public int P2_MaxConcurrentNotes {
        get => _P2_MaxConcurrentNotes;
        set {
            _P2_MaxConcurrentNotes = value;
            GameManager.instance.RestartGame();
        }
    }
    
    [Category("Project2")]
    [NumberRange(1f, 20f)]
    [DisplayName("Note speed")]
    public float P2_NoteSpeed {
        get => _P2_NoteSpeed;
        set {
            _P2_NoteSpeed = value;
            GameManager.instance.RestartGame();
        }
    }
}