using UnityEngine;

public class CursorManager : MonoBehaviour
{
    // Assign your custom texture in the Inspector
    public Texture2D cursorTexture;

    // Define the click point (0,0 is the top-left corner)
    public Vector2 hotSpot = Vector2.zero;

    void Start()
    {
        // Change the cursor globally at the start
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    }

    // Example: Reverting to default on specific events
    public void ResetCursor()
    {
        // Passing 'null' restores the default system cursor
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
