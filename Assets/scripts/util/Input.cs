static public class Input {
    static private string horizontalAxis = "Horizontal";
    static private string verticalAxis = "Vertical";
    static private string actionAxis = "Action";

    static public bool CheckAnyKeyDown() {
        return UnityEngine.Input.anyKey;
    }

    static public bool CheckAnyKeyJustPressed() {
        return UnityEngine.Input.anyKeyDown;
    }

    static public float GetHorizontalAxis() {
        return UnityEngine.Input.GetAxisRaw(horizontalAxis);
    }

    static public float GetVerticalAxis() {
        return UnityEngine.Input.GetAxisRaw(verticalAxis);
    }

    static public bool GetActionButton() {
        return UnityEngine.Input.GetAxisRaw(actionAxis) > 0.5f;
    }

    static public bool GetResetButton() {
        return UnityEngine.Input.GetAxisRaw("Reset") > 0.5f;
    }

    static public bool GetPauseJustPressed() {
        return UnityEngine.Input.GetButtonDown("Pause");
    }

    static public bool GetMouseCameraEnabled() {
        return (UnityEngine.Input.GetAxisRaw("MouseCamera") != 0.0f);
    }

    static public float GetCameraX() {
        return UnityEngine.Input.GetAxis("CameraX");
    }

    static public float GetCameraY() {
        return UnityEngine.Input.GetAxis("CameraY");
    }

    static public UnityEngine.Vector3 GetMousePosition() {
        return UnityEngine.Input.mousePosition;
    }
}
