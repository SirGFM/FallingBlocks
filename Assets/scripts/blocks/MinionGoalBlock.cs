public class MinionGoalBlock : UnityEngine.MonoBehaviour {
    /* Default size of the light */
    private const float defaultRadius = 1.5f;
    /** The halo object */
    private UnityEngine.Light halo;
    /** Modifies the radius in 1 unit */
    private float radius;
    /** Number of minions in this scene */
    private int max;

    void Start() {
        this.halo = this.gameObject.GetComponentInChildren<UnityEngine.Light>();
        this.radius = 0.0f;
        this.max = 0;
    }

    public void increaseMaxMinion() {
        this.max++;
    }

    // Update is called once per frame
    void Update() {
        float y;

        this.radius += UnityEngine.Time.deltaTime;
        if (this.radius > 1.0f)
            this.radius -= 1.0f;

        /* Use a parabola to modify the halo's radius */
        y = this.radius;
        y = 4 * y * (1.0f - y);
        this.halo.range = y + MinionGoalBlock.defaultRadius;
    }
}
