using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;

public class MinionGoalBlock : BaseGoalBlock, ActivateOnTop {
    /** Tag used to identify a player */
    private const string minionTag = "Minion";

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

    public void OnEnterTop(GO other) {
        if (other.tag != MinionGoalBlock.minionTag)
            return;

        /* Make the minion disappear */
        EvSys.ExecuteEvents.ExecuteHierarchy<OnEntityDone>(
                other, null, (x,y)=>x.OnGoal());

        /* Check whether the stage is over */
        this.count++;
        if (this.count == this.max) {
            this.showWinScreen();
            /* TODO: Wait some time so the fanfare/screen is on for a while */
            this.nextStage();
        }
    }

    public void OnLeaveTop(UnityEngine.GameObject other) {
        /* Do nothing! */
    }
}
