using Anim = BaseEntity.Animation;
using Dir = Movement.Direction;
using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;
using BroadOpts = UnityEngine.SendMessageOptions;

public class BaseEntity : BaseRemoteAction, FallDetector, MovementDetector {
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

    /** Tracks whether we are already running a coroutine */
    protected Animation anim;

    /** Dumb-ly keep track of the shaker component (since Unity is bad at
     * sending events downward) */
    protected GO shaker;

    /** How many blocks this object is currently over */
    private int downCount;

    /* == Base Methods ====================================================== */

    virtual protected void onLastBlockExit(RelPos p, GO other) {
    }

    private System.Collections.IEnumerator delayedOnLastBlockExit(RelPos p, GO other) {
        yield return new UnityEngine.WaitForFixedUpdate();
        if (this.downCount == 0)
            this.onLastBlockExit(p, other);
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

        ShakeControllerOutParam shakeParam = new ShakeControllerOutParam();
        this.BroadcastMessage("GetShakeComponent", shakeParam,
                BroadOpts.DontRequireReceiver);
        this.shaker = shakeParam.obj;

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
        if ((this.anim & Animation.Shake ) != 0)
            return;

        uint mod = 1 + (Global.PRNG.fastUint() % 1000);
        float fmod = ((float)mod) / 1000;

        this.StartCoroutine(this.doShake (min + (max - min) * fmod));
    }
}
