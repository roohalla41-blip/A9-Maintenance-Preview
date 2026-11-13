using UnityEngine;

/// <summary>
/// Shared logical 430x760 UI canvas scaled to fit compact phones and landscape screens.
/// Call Begin from OnGUI and restore the returned matrix after drawing.
/// </summary>
public static class MobileUiLayout
{
    public static float Scale
    {
        get
        {
            float widthScale = Screen.width / 430f;
            float heightScale = Screen.height / 760f;
            return Mathf.Clamp(Mathf.Min(widthScale, heightScale), 0.65f, 1.25f);
        }
    }

    public static Matrix4x4 Begin()
    {
        Matrix4x4 previous = GUI.matrix;
        float scale = Scale;
        GUI.matrix = previous * Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        return previous;
    }

    public static void End(Matrix4x4 previous)
    {
        GUI.matrix = previous;
    }

    public static Vector2 ToLogicalScreenPosition(Vector2 screenPosition)
    {
        return screenPosition / Scale;
    }
}
