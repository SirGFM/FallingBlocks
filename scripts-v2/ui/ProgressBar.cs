using Axis = UnityEngine.RectTransform.Axis;
using Image = UnityEngine.UI.RawImage;
using Math = UnityEngine.Mathf;
using Rect = UnityEngine.Rect;
using Transform = UnityEngine.RectTransform;

public class ProgressBar : UnityEngine.MonoBehaviour {
    /** The image that represents the current progress */
    private Image img;
    /** Rectangle that contains the image */
    private Transform uit;
    /** Original width of the image's rectangle */
    private float width;
    /** Offset within the image, used to animate it */
    private float offx;
    /** Width of the texture, in pixels */
    private const float texWidth = 256.0f;
    /** Speed of the animation, in pixels/s */
    private const float speed = 16.0f / texWidth;

    /** Track whether the progress changed, and the image must be expanded */
    private float lastProgress;
    /** Current progress, in the [0.0f, 1.0f] range */
    public float progress;

    void Start() {
        this.img = null;
        this.uit = null;
        this.offx = 0.0f;
        this.getSelf();
    }

    private bool getSelf() {
        if (this.img == null)
            this.img = this.GetComponent<Image>();
        if (this.uit == null) {
            this.uit = this.GetComponent<Transform>();
            this.width = this.uit.rect.width;
            this.uit.SetSizeWithCurrentAnchors(Axis.Horizontal, 0);
        }
        return (this.img != null) && (this.uit != null);
    }

    void Update() {
        if (!this.getSelf())
            return;

        Rect r = this.img.uvRect;

        if (this.lastProgress != this.progress) {
            if (this.progress < 0.0f)
                this.progress = 0.0f;
            else if (this.progress > 1.0f)
                this.progress = 1.0f;

            int size = (int)Math.Floor(this.progress * this.width);
            this.uit.SetSizeWithCurrentAnchors(Axis.Horizontal, size);
            r.width = this.progress * (this.width / texWidth);
            this.lastProgress = this.progress;
        }

        this.offx += UnityEngine.Time.deltaTime * ProgressBar.speed;
        if (this.offx > 1.0f)
            this.offx -= 1.0f;
        r.x = this.offx;
        this.img.uvRect = r;
    }
}
