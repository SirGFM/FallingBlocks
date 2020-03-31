public class ParticleQuality : UnityEngine.MonoBehaviour {
    void Start() {
        UnityEngine.ParticleSystem ps;
        float quality = 1.0f;

        switch (Global.particleQuality) {
        case Global.ParticleQuality.High:
            quality = 1.0f;
            break;
        case Global.ParticleQuality.Mid:
            quality = 0.6f;
            break;
        case Global.ParticleQuality.Low:
            quality = 0.2f;
            break;
        case Global.ParticleQuality.Off:
            this.gameObject.SetActive(false);
            return;
        }

        ps = this.GetComponent<UnityEngine.ParticleSystem>();
        var main = ps.main;
        main.startLifetimeMultiplier *= quality;
        main.maxParticles = (int)(main.maxParticles * quality);
        var emission = ps.emission;
        emission.rateOverTimeMultiplier *= quality;
    }
}
