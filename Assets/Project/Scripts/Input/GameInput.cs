using UnityEngine;

namespace CrackShot
{
    public static class GameInput
    {
        public static bool SelectPressed => Input.GetMouseButtonDown(0);
        public static bool SelectHeld => Input.GetMouseButton(0);
        public static bool SelectReleased => Input.GetMouseButtonUp(0);

        public static bool RotatePressed => Input.GetMouseButtonDown(1);
        public static bool RotateHeld => Input.GetMouseButton(1);
        public static bool RotateReleased => Input.GetMouseButtonUp(1);

        public static Vector3 PointerPosition => Input.mousePosition;
        public static float ZoomDelta => Input.GetAxis("Mouse ScrollWheel");

        public static bool NextBall => Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
        public static bool PrevBall => Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);

        public static bool ResetView => Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
        public static bool TopView => Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);

        public static bool AnyActivity =>
            Input.anyKey || Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f;
    }
}
