using UiToggle = UnityEngine.UI.Toggle;

public class Enabler : UnityEngine.MonoBehaviour {
    public void SetEnabled(UiToggle toggle) {
        this.gameObject.SetActive(toggle.isOn);
    }
}
