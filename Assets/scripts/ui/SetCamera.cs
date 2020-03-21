using UiToggle = UnityEngine.UI.Toggle;

public class SetCamera : UnityEngine.MonoBehaviour {
    public void SetHorizontal(UiToggle toggle) {
        if (toggle.isOn)
            Global.camX = -1.0f;
        else
            Global.camX = 1.0f;
    }

    public void SetVertical(UiToggle toggle) {
        if (toggle.isOn)
            Global.camY = -1.0f;
        else
            Global.camY = 1.0f;
    }
}
