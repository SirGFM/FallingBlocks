using UiToggle = UnityEngine.UI.Toggle;

public class SetCamera : UnityEngine.MonoBehaviour {
    public bool isHorizontal = true;

    void Start() {
        UiToggle toggle = this.GetComponent<UiToggle>();
        if (isHorizontal)
            toggle.isOn = (Global.camX == -1.0f);
        else
            toggle.isOn = (Global.camY == -1.0f);
    }

    public void SetCameraAxis(UiToggle toggle) {
        if (this.isHorizontal)
            this.SetHorizontal(toggle);
        else
            this.SetVertical(toggle);
    }

    private void SetHorizontal(UiToggle toggle) {
        if (toggle.isOn)
            Global.camX = -1.0f;
        else
            Global.camX = 1.0f;
    }

    private void SetVertical(UiToggle toggle) {
        if (toggle.isOn)
            Global.camY = -1.0f;
        else
            Global.camY = 1.0f;
    }
}
