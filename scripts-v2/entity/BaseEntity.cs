using Anim = BaseEntity.Animation;
using Dir = Movement.Direction;
using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;

public class BaseEntity : BaseRemoteAction, FallDetector, MovementDetector {
    public enum Animation {
        None   = 0x00,
        Stand  = 0x01,
        Turn   = 0x02,
        Move   = 0x04,
        Fall   = 0x08,
        Push   = 0x10, /* Player only */
        Shiver = 0x20, /* Minion only */
        Goal   = 0x40,
        Death  = 0x80,
    }

    /** Tracks whether we are already running a coroutine */
    protected Animation anim;

    /** Dumb-ly keep track of the rumbler component (since Unity is bad at
     * sending events downward) */
    private GO rumbler;

    /** How many blocks this object is currently over */
    public int downCount;

    /* == Base Methods ====================================================== */

    private void onCollisionDown(bool enter, RelPos p, GO other) {
        if (other.GetComponent<BaseBlock>() != null) {
            if (enter)
                downCount++;
            else
                downCount--;
        }
    }

    protected void setCollisionDownCallback(RelPos[] positions) {
        System.Action<bool, RelPos, GO> cb;

        cb = (x, y, z) => this.onCollisionDown(x, y, z);
        foreach (RelPos p in positions) {
            System.Tuple<RelPos, System.Action<bool, RelPos, GO>> arg;
            arg = new System.Tuple<RelPos, System.Action<bool, RelPos, GO>>(p, cb);
            this.BroadcastMessage("SetRelativePositionCallback", arg);
        }
    }

    /* == Unity Events ====================================================== */

    void Start() {
        this.start();
    }

    void Update() {
        this.updateState();
    }

    /* == Virtual Methods =================================================== */

    virtual protected void start() {
        RelPos[] positions = {RelPos.Bottom};

        this.anim = Animation.None;
        this.downCount = 0;

        this.setCollisionDownCallback(positions);
    }

    /**
     * Whether the entity can fall (e.g., if not on the ledge)
     */
    virtual protected bool canFall() {
        return true;
    }

    virtual protected void updateState() {
        if ((this.anim & ~Animation.Fall) != 0)
            return;

        if ((this.anim & Animation.Fall) != 0 && this.downCount > 0)
            this.issueEvent<FallController>( (x, y) => x.Halt(this.gameObject) );
        else if (this.downCount <= 0)
            this.issueEvent<FallController>( (x, y) => x.Fall(this.gameObject) );
    }

    /* == Custom Events ===================================================== */

    public void OnStartFalling(GO callee) {
        this.anim |= Animation.Fall;
    }

    public void OnFinishFalling(GO callee) {
        this.anim &= ~Animation.Fall;
    }

    public void OnStartMovement(Dir d) {
        this.anim |= Animation.Move;
    }

    public void OnFinishMovement(Dir d) {
        this.anim &= ~Animation.Move;
    }
}
