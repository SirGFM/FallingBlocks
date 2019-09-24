public class MinionGoalBlock : BaseGoalBlock {
    /** Default size of the light */
    private const float defaultRadius = 1.5f;
    /** The halo object */
    private UnityEngine.Light halo;
    /** Modifies the radius in 1 unit */
    private float radius;
    /** Number of minions in this scene */
    private int max;
    /** Number of minions that touched the goal */
    private int count;

    void Start() {
        this.halo = this.gameObject.GetComponentInChildren<UnityEngine.Light>();
        this.radius = 0.0f;
        this.max = 0;
        this.count = 0;
    }

    public void increaseMaxMinion() {
        this.max++;
    }

    public void foundMinion() {
        /* Looking back at this, I could have used ActivateOnTop... but I
         * completely forgot about it */
        this.count++;
        if (this.count == this.max) {
            this.showWinScreen();
            /* TODO: Wait some time so the fanfare/screen is on for a while */
            this.nextStage();
        }
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
