using UnityEngine;
using UnityEngine.InputSystem;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    // Sidebar is intentionally a placeholder. No mod-specific controls or preferences.
    private bool SidebarPointerCaptured=>false;
    private bool OverlayContains(Rect rect,string key)
    {
        key=RoleWindowKey(key);
        if(movingWindow==key)return true;
        if(movingWindow.Length>0)return false;
        var mouse=Mouse.current;if(mouse==null)return false;
        var point=mouse.position.ReadValue();
        return Cursor.visible&&rect.Contains(new Vector2(point.x,Screen.height-point.y));
    }
}

