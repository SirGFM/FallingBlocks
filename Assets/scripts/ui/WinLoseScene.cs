using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;
using RawImage = UnityEngine.UI.RawImage;
using RectT = UnityEngine.RectTransform;
using Time = UnityEngine.Time;
using UiText = UnityEngine.UI.Text;
using Vec2 = UnityEngine.Vector2;
using Vec3 = UnityEngine.Vector3;

public class WinLoseScene : BaseRemoteAction {

    public RectT bgColor;
    public RectT image;
    public RectT imageFx;
    public RectT flavor;
    public RectT pressToPlay;

    public float scaleDelay = 3.0f;
    public float initialPos;
    public float finalPos;
    public float fxDelay = 0.3f;
    public float txtDelay = 0.5f;
    public float inputDelay = 1.0f;

    private bool waitingForInput;

    protected virtual void onJustPressed() {
    }

    private System.Collections.IEnumerator run() {
        float dt;

        RawImage rimg = this.bgColor.GetComponent<RawImage>();

        /* Drop the "You Win" image and upscale it */
        for (dt = 0.0f; dt < this.scaleDelay; dt += Time.deltaTime) {
            yield return null;

            float scale = 2.0f * dt / scaleDelay;
            this.image.localScale = new Vec3(scale, scale, 1.0f);

            float pos = initialPos + (finalPos - initialPos) * dt / scaleDelay;
            this.image.anchoredPosition = new Vec2(0 ,pos);

            float alpha = 0.5f * dt / scaleDelay;
            Color c = rimg.color;
            rimg.color = new Color(c.r, c.g, c.b, alpha);
        }

        /* Show and hide the effect over the "You Win" image */
        this.imageFx.gameObject.SetActive(true);
        Image img = this.imageFx.GetComponent<Image>();

        float _fxDelay = this.fxDelay / 2.0f;
        for (dt = 0.0f; dt < _fxDelay; dt += Time.deltaTime) {
            yield return null;

            img.color = new Color(1.0f, 1.0f, 1.0f, 0.5f * dt / _fxDelay);
            float scale = 2.0f + 0.25f * dt / _fxDelay;
            this.imageFx.localScale = new Vec3(scale, scale, 1.0f);
        }
        for (; dt > 0.0f; dt -= Time.deltaTime) {
            yield return null;

            img.color = new Color(1.0f, 1.0f, 1.0f, 0.5f * dt / _fxDelay);
            float scale = 2.0f + 0.25f * dt / _fxDelay;
            this.imageFx.localScale = new Vec3(scale, scale, 1.0f);
        }
        this.imageFx.gameObject.SetActive(false);

        UiText txt = this.flavor.GetComponent<UiText>();

        /* Show the extra flavor text */
        for (dt = 0.0f; dt < this.txtDelay; dt += Time.deltaTime) {
            yield return null;

            txt.color = new Color(1.0f, 1.0f, 1.0f, dt / this.txtDelay);

            float scale = dt / this.txtDelay;
            this.flavor.localScale = new Vec3(1.0f, scale, 1.0f);
        }
        txt.color = Color.white;

        /* Run indefinitely waiting for input */
        UiText[] txts = this.pressToPlay.GetComponentsInChildren<UiText>();
        this.waitingForInput = true;
        while (true) {
            float _inputDelay = this.inputDelay / 2.0f;
            for (dt = 0.0f; dt < _inputDelay; dt += Time.deltaTime) {
                yield return null;
                float alpha = dt / _inputDelay;
                foreach (UiText t in txts)
                    t.color = new Color(1.0f, 1.0f, 1.0f, alpha);
            }
            for (; dt > 0.0f; dt -= Time.deltaTime) {
                yield return null;
                float alpha = dt / _inputDelay;
                foreach (UiText t in txts)
                    t.color = new Color(1.0f, 1.0f, 1.0f, alpha);
            }
        }
    }

    void Start() {
        this.waitingForInput = false;
        this.StartCoroutine(this.run());
    }

    void Update() {
        if (this.waitingForInput && Input.CheckAnyKeyJustPressed())
            this.onJustPressed();
    }
}
