using StrList = System.Collections.Generic.List<string>;
using ResMode = UnityEngine.Resolution;
using Screen = UnityEngine.Screen;
using UiDropdown = UnityEngine.UI.Dropdown;
using UiToggle = UnityEngine.UI.Toggle;

public class ResolutionController : UnityEngine.MonoBehaviour {
    private ResMode[] resolutions;
    private UiDropdown modes;
    private UiToggle fullscreen;

    void Start() {
        this.modes = this.gameObject.GetComponentInChildren<UiDropdown>();
        this.fullscreen = this.gameObject.GetComponentInChildren<UiToggle>();

        this.resolutions = UnityEngine.Screen.resolutions;

        StrList options = new StrList();
        foreach (ResMode res in resolutions) {
            options.Add($"{res.width}x{res.height}@{res.refreshRate}");
        }

        this.modes.AddOptions(options);
        this.fullscreen.isOn = Screen.fullScreen;
    }

    public void Apply() {
        ResMode res = this.resolutions[modes.value];
        Screen.SetResolution(res.width, res.height, this.fullscreen.isOn, res.refreshRate);
    }
}
