using System.Globalization;
using UnityEngine;

namespace UiFramework.Editor.Scenes
{
    // Comparison tolerances and value formatting shared by the structure validator and the canvas utility.
    internal static class UiSceneValues
    {
        public const float PositionTolerance = 0.01f;
        public const float UnitTolerance = 0.0001f;
        public const float RotationToleranceDegrees = 0.01f;

        public static bool Approximately(Vector2 a, Vector2 b, float tolerance)
        {
            return Mathf.Abs(a.x - b.x) <= tolerance && Mathf.Abs(a.y - b.y) <= tolerance;
        }

        public static string FormatNumber(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        public static string FormatResolution(Vector2 value)
        {
            return FormatNumber(value.x) + "x" + FormatNumber(value.y);
        }

        public static string FormatVector(Vector2 value)
        {
            return "(" + FormatNumber(value.x) + ", " + FormatNumber(value.y) + ")";
        }

        public static string FormatVector(Vector3 value)
        {
            return "(" + FormatNumber(value.x) + ", " + FormatNumber(value.y) + ", " + FormatNumber(value.z) + ")";
        }
    }
}
