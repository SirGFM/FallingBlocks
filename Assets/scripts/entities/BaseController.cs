using UEColl = UnityEngine.Collider;
using Dir = Movement.Direction;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using RelPos = ReportRelativeCollision.RelativePosition;
using Vec3 = UnityEngine.Vector3;

public interface OnEntityDone : EvSys.IEventSystemHandler {
    /** Event dispatched when the entity reaches its goal */
    void OnGoal();
}

public class BaseController : UnityEngine.MonoBehaviour, OnRelativeCollisionEvent, iTurned, iDetectFall {
    protected enum Animation {
        None   = 0x00,
        Stand  = 0x01,
        Turn   = 0x02,
        Move   = 0x04,
        Fall   = 0x08,
        Push   = 0x10, /* Player only */
        Shiver = 0x20, /* Minion only */
        Goal   = 0x40,
        Death  = 0x80,
    };

    /** Previously started coroutine */
    private UnityEngine.Coroutine bgFunc;
    /** Keep track of collisions on the object's surroundings */
    protected int[] collisionTracker;
    /** Whether this entity can fall to a ledge */
    protected bool allowLedgeMovement;
    /** Currently facing direction */
    protected Dir facing = Dir.back;
    /** Tracks whether we are already running a coroutine */
    protected Animation anim;
    /** Whether the entity is on a block (and may ledge it) */
    private bool isOnBlock;
    /** Position of the entity before its last movement */
    private Vec3 lastPosition;

    /** How fast (in seconds) the entity walks over a block */
    public float MoveDelay = 0.4f;

    /**
     * Initialize everything common to all controllers. This must be
     * manually called because 'Start()' can't be overriden.
     */
    protected void commonInit() {
        RelPos p = 0;
        this.collisionTracker = new int[p.count()];
        this.anim = Animation.None;
    }

    /**
     * Whether the entity can move right now.
     */
    virtual protected bool isMoving() {
        return (this.anim & Animation.Move) == Animation.Move;
    }

    /**
     * Mark this entity as being on the ledge.
     */
    virtual protected void setOnLedge() { }

    protected void move(Dir d, float speed) {
        GO self = this.gameObject;
        EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(self, null,
                (x,y)=>x.Move(d, self, speed));
    }

    /**
     * Try to move the entity one block forward, depending on the colliders
     * around it.
     */
    protected void tryMoveForward(bool avoidJumping = false) {
        /* Avoid corner cases by checking before doing anything */
        if (this.isMoving())
            return;

        this.lastPosition = this.transform.position;

        /* Compound the movement by looking at the surroundings */
        bool frontWall = this.collisionTracker[RelPos.Front.toIdx()] > 0;
        if (!avoidJumping && frontWall) {
            /* Something ahead; Try to jump up */
            if (this.collisionTracker[RelPos.FrontTop.toIdx()] == 0 &&
                    this.collisionTracker[RelPos.Top.toIdx()] == 0) {
                /* There's a floor above; Jump toward it */
                Dir d = this.facing | Dir.top;
                this.move(d, this.MoveDelay);
            }
        }
        else {
            if (this.collisionTracker[RelPos.FrontBottom.toIdx()] > 0) {
                /* Front is clear and there's footing; Just move forward.
                 *
                 * XXX: This may not be the case for the AI movement, so it must
                 * be checked again. */
                if (!frontWall)
                    this.move(this.facing, this.MoveDelay);
            }
            else if (this.collisionTracker[RelPos.BottomBottomFront.toIdx()] > 0) {
                /* There's a floor bellow; Jump toward it */
                Dir d = this.facing | Dir.bottom;
                this.move(d, this.MoveDelay);
            }
            else if (this.allowLedgeMovement) {
                Dir newDir;

                /* Fall to the ledge! */
                switch (this.facing) {
                case Dir.back:
                    newDir = Dir.front;
                    break;
                case Dir.front:
                    newDir = Dir.back;
                    break;
                case Dir.left:
                    newDir = Dir.right;
                    break;
                case Dir.right:
                    newDir = Dir.left;
                    break;
                default:
                    newDir = Dir.none;
                    break;
                }

                Dir d = this.facing | Dir.bottom;
                this.move(d, this.MoveDelay);
                this.turn(newDir);
                if (this.isOnBlock)
                    this.setOnLedge();
            }
        }
    }

    protected Dir getRelativeDirection(UnityEngine.Vector3 other) {
        Dir d = Dir.none;
        UnityEngine.Vector3 self;

        self = this.transform.position;

        if (other.x > self.x)
            d = Dir.right;
        else if (other.x < self.x)
            d = Dir.left;
        else if (other.z > self.z)
            d = Dir.front;
        else if (other.z < self.z)
            d = Dir.back;
        else if (other.y > self.y)
            d = Dir.top;
        else if (other.y < self.y)
            d = Dir.bottom;

        return d;
    }

    protected Dir getRelativeDirection(GO target) {
        return this.getRelativeDirection(target.transform.position);
    }

    public Dir getFacing() {
        return this.facing;
    }

    public Vec3 getLastPosition() {
        return this.lastPosition;
    }

    /**
     * Whether the entity is currently falling.
     */
    virtual protected bool isFalling() {
        return (this.anim & Animation.Fall) == Animation.Fall;
    }

    /**
     * Whether the entity can fall right now.
     */
    virtual protected bool canFall() {
        return this.anim == Animation.None;
    }

    /**
     * Called on response of an 'OnEnterRelativeCollision()' event.
     */
    virtual protected void _onEnterRelativeCollision(RelPos p, UEColl c) { }

    /**
     * Called on response of an 'OnExitRelativeCollision()' event.
     */
    virtual protected void _onExitRelativeCollision(RelPos p, UEColl c) { }

    /**
     * Called after the death animation finishes playing.
     */
    virtual public void onDeath() { }

    private System.Collections.IEnumerator die() {
        this.anim |= Animation.Death;

        /* Wait until the only pending action is dying */
        while (this.anim != Animation.Death)
            yield return null;

        /* TODO: Play death animation */
        /* XXX: Death animation should send this event! */
        this.onDeath();
    }

    public void OnEnterRelativeCollision(RelPos p, UEColl c) {
        int idx = p.toIdx();
        this.collisionTracker[idx]++;
        if (p == RelPos.Bottom) {
            EvSys.ExecuteEvents.ExecuteHierarchy<ActivateOnTop>(
                    c.gameObject, null, (x,y)=>x.OnEnterTop(this.gameObject));
            /* Stops falling if there's anything bellow the entity */
            if (this.collisionTracker[idx] == 1 && this.isFalling())
                EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                        this.gameObject, null, (x,y)=>x.Halt(this.gameObject));
            this.isOnBlock = (c.gameObject.tag == "Block");
        }
        else if (p == RelPos.Center && c.gameObject != this.gameObject) {
            /* Only die if not colliding against an entity */
            if (c.gameObject.GetComponent<BaseController>() == null)
                this.StartCoroutine(this.die());
        }
        this._onEnterRelativeCollision(p, c);
    }

    private System.Collections.IEnumerator tryFall() {
        while (!this.canFall()) {
            if (this.collisionTracker[RelPos.Bottom.toIdx()] > 0 ||
                    this.isFalling()) {
                this.bgFunc = null;
                yield break;
            }
            yield return new UnityEngine.WaitForFixedUpdate();
        }
        EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                this.gameObject, null, (x,y)=>x.Fall(this.gameObject));
        this.bgFunc = null;
    }

    public void OnExitRelativeCollision(RelPos p, UEColl c) {
        this.collisionTracker[p.toIdx()]--;
        if (p == RelPos.Bottom) {
            EvSys.ExecuteEvents.ExecuteHierarchy<ActivateOnTop>(
                    c.gameObject, null, (x,y)=>x.OnLeaveTop(this.gameObject));
            if (this.collisionTracker[p.toIdx()] == 0) {
                if (this.bgFunc == null)
                    this.bgFunc = this.StartCoroutine(this.tryFall());
                this.isOnBlock = false;
            }
        }
        this._onExitRelativeCollision(p, c);
    }

    protected void turn(Dir d) {
        if (d == Dir.top || d == Dir.bottom)
            return;
        GO self = this.gameObject;
        /* TODO: Find a way to send messages downward and clean this mess */
        GO other = this.GetComponentInChildren<Turning>().gameObject;
        EvSys.ExecuteEvents.ExecuteHierarchy<iTurning>(
                other, null, (x,y)=>x.Turn(this.facing, d, other));
    }

    public void OnStartTurning(Dir d, GO callee) {
        this.anim |= Animation.Turn;
    }

    public void OnFinishTurning(Dir d, GO callee) {
        this.anim &= ~Animation.Turn;
        this.facing = d;
    }

    public void OnStartFalling(GO callee) {
        this.anim |= Animation.Fall;
    }

    public void OnFinishFalling(GO callee) {
        this.anim &= ~Animation.Fall;
    }

    private System.Collections.IEnumerator doShiver(float duration) {
        this.anim |= Animation.Shiver;
        /* TODO: Find a way to send messages downward and clean this mess */
        GO self = this.GetComponentInChildren<RumbleAnim>().gameObject;
        if (self != null) {
            EvSys.ExecuteEvents.ExecuteHierarchy<Rumbler>(
                    self, null, (x,y)=>x.StartRumbling());
            yield return new UnityEngine.WaitForSeconds(duration);
            EvSys.ExecuteEvents.ExecuteHierarchy<Rumbler>(
                    self, null, (x,y)=>x.StopRumbling());
        }
        this.anim &= ~Animation.Shiver;
    }

    /**
     * Rumbles the object for a random amount of time, in seconds
     *
     * @param min Minimum duration of shiver animation
     * @param max Maximum duration of shiver animation
     */
    protected void shiver(float min, float max) {
        if ((this.anim & Animation.Shiver) != 0)
            return;

        uint mod = 1 + (Global.PRNG.fastUint() % 1000);
        float fmod = ((float)mod) / 1000;

        this.StartCoroutine(this.doShiver(min + (max - min) * fmod));
    }
}
