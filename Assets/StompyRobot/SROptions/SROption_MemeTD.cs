using System.ComponentModel;
using UnityEngine;

public partial class SROptions {
    [Category("General")] 
    public void CopyJWT() {
        MemeTDUtils.Copy(PlayerDataManager.UserAccessToken);
    }
    
    [Category("General")] 
    public void CopySessionTicket() {
        MemeTDUtils.Copy(PlayerDataManager.PlayfabSessionTicket);
    }

    [Category("General")]
    public void CloseDebugger()
    {
        SRDebug.Instance.HideDebugPanel();
    }
}