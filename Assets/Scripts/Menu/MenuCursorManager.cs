using UnityEngine;

namespace AstroSurvive.Menu
{
    /// <summary>
    /// Ensures the mouse cursor is clearly visible and free to move in World Space UI menus.
    /// Matches CursorManagers from The Last Snooze.
    /// </summary>
    public class MenuCursorManager : MonoBehaviour
    {
        private void Start()
        {
            UnlockCursor();
        }

        private void OnEnable()
        {
            UnlockCursor();
        }

        public void UnlockCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
