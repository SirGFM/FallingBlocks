using Anim = BaseEntity.Animation;
using Dir = Movement.Direction;
using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;
using BroadOpts = UnityEngine.SendMessageOptions;

public class GetComponentControllerParam {
    public UnityEngine.GameObject obj;
}

public class BaseEntity : BaseRemoteAction, FallDetector, MovementDetector,
        TurnDetector {
    public enum Animation {
        None   = 0x00,
        Stand  = 0x01,
        Turn   = 0x02,
        Move   = 0x04,
        Fall   = 0x08,
        Push   = 0x10, /* Player only */
        Shake  = 0x20, /* Minion only */
        Goal   = 0x40,
        Death  = 0x80,
    }

    /** Direction that the entity is currently facing */
    protected Dir facing;

    /** Tracks whether we are already running a coroutine */
    protected Animation anim;

    /** Dumb-ly keep track of the shaker component (since Unity is bad at
     * sending events downward) */
    protected GO shaker;

    /** Dumb-ly keep track of the turner component (since Unity is bad at
     * sending events downward) */
    protected GO turner;

    /** How many blocks this object is currently over */
    private int downCount;

    /** Whether the object is delaying starting to fall */
    private bool delayingFall;

    /* == Base Methods ====================================================== */

    virtual protected void onLastBlockExit(RelPos p, GO other) {
    }

    private System.Collections.IEnumerator delayedOnLastBlockExit(RelPos p, GO other) {
        yield return new UnityEngine.WaitForFixedUpdate();
        if (this.downCount == 0)
            this.onLastBlockExit(p, other);
    }

    virtual protected void onCollision(bool enter, RelPos p, GO other) {
    }

    private void onCollisionDown(bool enter, RelPos p, GO other) {
        if (other.GetComponent<BaseBlock>() != null) {
            if (enter) {
                this.downCount++;
            }
            else {
                this.downCount--;
                if (this.downCount == 0) {
                    /* Call as a coroutine, to delay until the end of frame */
                    this.StartCoroutine(this.delayedOnLastBlockExit(p, other));
                }
            }
        }
        this.onCollision(enter, p, other);
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
        GetComponentControllerParam subObj = new GetComponentControllerParam();
        RelPos[] positions = {RelPos.Bottom};

        this.anim = Animation.None;
        this.downCount = 0;
        this.facing = Dir.Back;

        this.BroadcastMessage("GetShakeComponent", subObj,
                BroadOpts.DontRequireReceiver);
        this.shaker = subObj.obj;

        this.BroadcastMessage("GetTurnComponent", subObj,
                BroadOpts.DontRequireReceiver);
        this.turner = subObj.obj;

        this.setCollisionDownCallback(positions);

        this.delayingFall = false;
    }

    /**
     * Whether the entity can fall (e.g., if not on the ledge)
     */
    virtual protected bool canFall() {
        return this.delayingFall == false;
    }

    private System.Collections.IEnumerator delayedFall() {
        this.delayingFall = true;
        yield return new UnityEngine.WaitForSeconds(this.fallDelay());
        this.delayingFall = false;

        if (this.downCount <= 0 && this.canFall())
            this.issueEvent<FallController>(
                    (x, y) => x.Fall(this.gameObject) );
    }

    virtual protected float fallDelay() {
        return 0.0f;
    }

    virtual protected void updateState() {
        if ((this.anim & ~Animation.Fall) != 0)
            return;

        if ((this.anim & Animation.Fall) != 0 && this.downCount > 0) {
            this.issueEvent<FallController>( (x, y) => x.Halt(this.gameObject) );
        }
        else if (this.downCount <= 0 && this.canFall()) {
            if (this.fallDelay() <= 0.0f)
                this.issueEvent<FallController>(
                        (x, y) => x.Fall(this.gameObject) );
            else
                this.StartCoroutine(this.delayedFall());
        }
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

    public void OnStartTurning(Dir d) {
        this.anim |= Animation.Turn;
    }

    public void OnFinishTurning(Dir d) {
        this.anim &= ~Animation.Turn;
        this.facing = d;
    }

    /* == Event wrappers ==================================================== */

    private System.Collections.IEnumerator doShake (float duration) {
        if (this.shaker != null) {
            this.anim |= Animation.Shake ;
            this.issueEvent<ShakeController>(
                    (x, y) => x.StartShaking(), this.shaker);

            yield return new UnityEngine.WaitForSeconds(duration);

            this.issueEvent<ShakeController>(
                    (x, y) => x.StopShaking(), this.shaker);

            this.anim &= ~Animation.Shake ;
        }
    }

    /**
     * Shake the object for a random amount of time, in seconds
     *
     * @param min Minimum duration of shiver animation
     * @param max Maximum duration of shiver animation
     */
    protected void shake(float min, float max) {
        if ((this.anim & Animation.Shake) != 0)
            return;

        uint mod = 1 + (Global.PRNG.fastUint() % 1000);
        float fmod = ((float)mod) / 1000;

        this.StartCoroutine(this.doShake (min + (max - min) * fmod));
    }

    /**
     * Rotate the object to a given orientation
     *
     * @param to The new orientation
     */
    protected void turn(Dir to) {
        if ((this.anim & Animation.Turn) != 0)
            return;
        else if (this.turner != null)
            this.issueEvent<TurnController>(
                    (x, y) => x.Rotate(this.facing, to), this.turner);
    }

    /**
     * Move the object in a given direction
     *
     * @param to The movement direction
     * @param delay How long the movement shall take
     */
    protected void move(Dir to, float delay) {
        if ((this.anim & Animation.Move) != 0)
            return;
        this.issueEvent<MovementController>( (x, y) => x.Move(to, delay) );
    }
}
