using UiToggle = UnityEngine.UI.Toggle;

public class SetParticlesQuality : UnityEngine.MonoBehaviour {
    public Global.ParticleQuality particleQuality;

    public void SetEnabled(UiToggle toggle) {
        if (toggle.isOn)
            Global.particleQuality = this.particleQuality;
    }
}
