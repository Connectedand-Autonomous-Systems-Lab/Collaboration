#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class FullscreenGameView
{
    static readonly Type GameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
    static readonly PropertyInfo ShowToolbarProperty =
        GameViewType?.GetProperty("showToolbar", BindingFlags.Instance | BindingFlags.NonPublic);

    static EditorWindow instance;

    // F11 to toggle
    [MenuItem("Window/General/Toggle Game (Fullscreen Primary) _F11", priority = 2)]
    public static void Toggle()
    {
        if (GameViewType == null)
        {
            Debug.LogError("GameView type not found.");
            return;
        }

        // Close if already open
        if (instance != null)
        {
            instance.Close();
            instance = null;
            return;
        }

        instance = EditorWindow.CreateInstance(GameViewType) as EditorWindow;
        if (instance == null)
        {
            Debug.LogError("Failed to create GameView window.");
            return;
        }

        // Hide the GameView toolbar if available (optional)
        try { ShowToolbarProperty?.SetValue(instance, false); } catch { /* ignore */ }

        // --- Primary-screen fullscreen in GUI points, not pixels ---
        // Screen.currentResolution is physical pixels on the PRIMARY monitor.
        var res = Screen.currentResolution;          // e.g., 3840x2160 (pixels)
        var ppp = Mathf.Max(1f, EditorGUIUtility.pixelsPerPoint); // e.g., 1.0, 1.25, 1.5, 2.0

        // Convert to points so EditorWindow.position is correct and won't spill to the next monitor.
        float widthPoints  = res.width  / ppp;
        float heightPoints = res.height / ppp;

        // Primary screen top-left is (0,0) in Unity editor coordinates.
        // (If your primary monitor is not at the virtual desktop origin, Unity still treats (0,0) as primary.)
        var primaryRectPoints = new Rect(0f, 0f, widthPoints, heightPoints);

        instance.Show();               // Use normal window (not ShowPopup)
        instance.position = primaryRectPoints;

        // Optional: maximize to fill, still respects single display when the rect is correct.
        try { instance.maximized = true; } catch { /* older Unity versions */ }

        instance.Focus();
    }

    [MenuItem("Window/General/Exit Game Fullscreen (Primary)", priority = 3)]
    public static void ExitFullscreen()
    {
        if (instance != null)
        {
            instance.Close();
            instance = null;
        }
    }
}
#endif
