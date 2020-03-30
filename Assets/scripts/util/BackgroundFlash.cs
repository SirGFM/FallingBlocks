using Color = UnityEngine.Color;
using Time = UnityEngine.Time;
using CoroutineRet = System.Collections.IEnumerator;

public class BackgroundFlash : UnityEngine.MonoBehaviour {
    private UnityEngine.Camera cam;

    public Color baseColor;
    public Color flash;

    public float minFlashDelay = 1.0f;
    public float maxFlashDelay = 10.0f;

    public float delayIn = 0.1f;
    public float delayOut = 0.5f;

    private CoroutineRet lerp(Color from, Color to, float duration) {
        for (float delay = 0.0f; delay < duration; delay += Time.deltaTime) {
            float delta;

            delta = delay / duration;
            this.cam.backgroundColor = (1.0f - delta) * from + delta * to;

            yield return null;
        }
    }

    private CoroutineRet delayedFlash() {
        do {
            float delay;
            int min = (int)(this.minFlashDelay * 100.0f);
            int max = (int)(this.maxFlashDelay * 100.0f);

            delay = 0.01f * (float)Global.PRNG.fastRange(min, max);
            yield return new UnityEngine.WaitForSeconds(delay);

            yield return this.lerp(this.baseColor, this.flash, this.delayIn);
            this.cam.backgroundColor = this.flash;

            yield return this.lerp(this.flash, this.baseColor, this.delayOut);
            this.cam.backgroundColor = this.baseColor;
        } while (true);
    }

    void Start() {
        this.cam = this.GetComponent<UnityEngine.Camera>();
        this.cam.backgroundColor = this.baseColor;
        this.StartCoroutine(this.delayedFlash());
    }
}
