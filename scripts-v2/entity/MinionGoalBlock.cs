using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;
using Type = GetType.Type;
using Vec3 = UnityEngine.Vector3;

public class MinionGoalBlock : BaseBlock, LedgeTracker {
    /** Default size of the light */
    private const float defaultRadius = 1.5f;
    /** The halo object */
    private UnityEngine.Light halo;
    /** Modifies the radius in 1 unit */
    private float radius;

    override protected void start() {
        base.start();
        this.setupCollision();

        this.halo = this.gameObject.GetComponentInChildren<UnityEngine.Light>();
        this.radius = 0.0f;
    }

    private void setupCollision() {
        System.Action<bool, RelPos, GO> cb;
        System.Tuple<RelPos, System.Action<bool, RelPos, GO>> arg;
        RelPos p = RelPos.Top;

        cb = (x, y, z) => this.onCollisionUp(x, y, z);
        arg = new System.Tuple<RelPos, System.Action<bool, RelPos, GO>>(p, cb);
        this.BroadcastMessage("SetRelativePositionCallback", arg);
    }

    private void showWinScreen() {
        /* TODO:
         *   - Play 'you win' fanfare or whatever
         */
    }

    private void checkCondition(GO other) {
        Type objType = Type.Error;
        bool done = false;

        this.issueEvent<RemoteGetType>( (x,y) => x.Get(out objType), other);
        if (objType != Type.Minion)
            return;

        this.issueEvent<SetOnGoal>( (x,y) => x.OnGoal(), other);

        this.rootEvent<LoaderEvents>( (x,y) => x.SavedMinion(out done) );
        if (done)
            this.rootEvent<LoaderEvents>( (x,y) => x.NextLevel() );
    }

    public void JustDropped(GO other) {
        this.checkCondition(other);
    }

    private void onCollisionUp(bool enter, RelPos p, GO other) {
        bool onLedge = false;


        this.issueEvent<OnLedgeDetector>( (x,y) => x.Check(out onLedge), other);
        if (enter && !onLedge)
            this.checkCondition(other);
    }

    override protected void updateState() {
        float y;

        base.updateState();

        this.radius += UnityEngine.Time.deltaTime;
        if (this.radius > 1.0f)
            this.radius -= 1.0f;

        /* Use a parabola to modify the halo's radius */
        y = this.radius;
        y = 4 * y * (1.0f - y);
        if (this.halo != null)
            this.halo.range = y + defaultRadius;
    }
}
