using Anim = BaseEntity.Animation;
using Dir = Movement.Direction;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using Math = UnityEngine.Mathf;
using RelPos = RelativeCollision.RelativePosition;
using Type = GetType.Type;
using Vec3 = UnityEngine.Vector3;

public interface Leader : EvSys.IEventSystemHandler {
    /**
     * Assigns a leader's follower
     *
     * @param follower The follower
     */
    void SetFollower(Minion follower);

    /**
     * Remove a leader's follower
     *
     * @param callee Whoever is removing the leader's follower
     */
    void RemoveFollower(Minion callee);
}

public class Minion : BaseAnimatedEntity, Leader {
    private struct collCtx {
        public bool enter;
        public RelPos p;
        public GO other;
        public Type otherType;
    };

    /** Minimum duration of the shiver animation */
    private const float minShiverTime = 0.5f;
    /** Maximum duration of the shiver animation */
    private const float maxShiverTime = 1.5f;

    /** How fast (in seconds) the entity walks over a block */
    public float MoveDelay = 0.4f;

    private GO target;
    Type targetPriority;

    private UnityEngine.Transform selfT;
    private UnityEngine.Transform otherT;
    private UnityEngine.Coroutine bgFunc;

    protected Minion follower;

    static private Dir vec3ToDir(Vec3 pos) {
        /* XXX: Only check axis X and Z */
        int[] axisOrder = {0, 2};
        Dir[,] perAxis = {
            { Dir.Left, Dir.Right },
            { Dir.Bottom, Dir.Top },
            { Dir.Back, Dir.Front },
        };
        float absDist = 0.0f;
        Dir d = Dir.None;

        /* Select the direction of the axis with the greater distance, so the
         * entity will follow the last position of the targeted object */
        for (int i = 0; i < axisOrder.Length; i++) {
            int axis = axisOrder[i];
            if (Math.Abs(pos[axis]) > absDist) {
                absDist = Math.Abs(pos[axis]);
                if (pos[axis] < 0)
                    d = perAxis[axis, 0];
                else if (pos[axis] > 0)
                    d = perAxis[axis, 1];
            }
        }

        return d;
    }

    override protected void start() {
        System.Action<bool, RelPos, GO> cb;

        base.start();

        cb = (x, y, z) => this.onCollision(x, y, z);
        this.setCollisionCb(RelPos.None, cb);

        this.target = null;
        this.targetPriority = Type.None;
        this.selfT = this.gameObject.transform;
        this.bgFunc = null;

        this.rootEvent<LoaderEvents>( (x,y) => x.IncreaseMaxMinion() );
    }

    private void onCollisionEnter(collCtx ctx) {
        if (this.targetPriority >= ctx.otherType ||
                (ctx.otherType == Type.Minion && this.follower != null))
            return;

        this.target = ctx.other;
        this.otherT = ctx.other.transform;
        this.targetPriority = ctx.otherType;

        if (ctx.otherType == Type.Minion)
            this.issueEvent<Leader>(
                    (x,y) => x.SetFollower(this), ctx.other);
    }

    private System.Collections.IEnumerator follow(GO other, Dir moveDir) {
        bool isOtherOnLedge = false;

        if (moveDir != this.facing) {
            this.turn(moveDir);
            yield return new UnityEngine.WaitForFixedUpdate();
            while ((this.anim & Animation.Turn) != 0)
                yield return new UnityEngine.WaitForFixedUpdate();
        }

        this.issueEvent<OnLedgeDetector>(
                (x,y) => x.Check(out isOtherOnLedge), other);
        if (!isOtherOnLedge)
            this.tryMoveForward(this.MoveDelay);
    }

    private void onCollisionExit(collCtx ctx) {
        if (this.target == ctx.other) {
            Dir moveDir;

            if (this.targetPriority == Type.Minion)
                this.issueEvent<Leader>(
                        (x,y) => x.RemoveFollower(this), ctx.other);

            /* The target we were following just left, try to move after it */
            this.target = null;
            this.targetPriority = Type.None;
            moveDir = vec3ToDir(otherT.position - this.selfT.position);
            this.StartCoroutine(this.follow(ctx.other, moveDir));
        }
    }

    override protected void onCollision(bool enter, RelPos p, GO other) {
        Type otherType = Type.Error;

        this.issueEvent<RemoteGetType>(
                (x,y) => x.Get(out otherType), other);
        if (otherType > Type.Followable) {
            collCtx ctx;

            ctx.enter = enter;
            ctx.p = p;
            ctx.other = other;
            ctx.otherType = otherType;

            if (enter)
                this.onCollisionEnter(ctx);
            else
                this.onCollisionExit(ctx);
        }
    }

    private System.Collections.IEnumerator wander() {
        if (this.follower == null) {
            this.shake(minShiverTime, maxShiverTime);
            while ((this.anim & Anim.Shake) != 0)
                yield return new UnityEngine.WaitForFixedUpdate();
        }

        do {
            if ((this.anim & ~Anim.Move) != 0 || this.target != null)
                break;

            if ((this.anim & Anim.Move) == 0) {
                bool floor, frontTile;

                frontTile = (getObjectAt(RelPos.Front) != null);
                floor = (getBlockAt(RelPos.FrontBottom) != null);

                if (!frontTile && floor)
                    this.tryMoveForward(this.MoveDelay);
                else {
                    this.turn(Dir.Right.toLocal(this.facing));
                    break;
                }
            }

            yield return new UnityEngine.WaitForFixedUpdate();
        } while (true);

        this.bgFunc = null;
    }

    override protected void updateState() {
        base.updateState();

        if (this.target == null && this.anim == Anim.None &&
                this.bgFunc == null)
            this.bgFunc = this.StartCoroutine(this.wander());
    }

    private System.Collections.IEnumerator destroy() {
        /* TODO Play a VFX? */

        /* XXX: Forcefully move the entity away from any close entity before
         * destroying it, to avoid glitching the physics. */
        this.transform.position = new Vec3(0.0f, -10.0f, 0.0f);
        yield return new UnityEngine.WaitForFixedUpdate();
        GO.Destroy(this.gameObject);
    }

    override protected void onGoal() {
        this.StartCoroutine(this.destroy());
    }

    public void SetFollower(Minion newFollower) {
        this.follower = newFollower;
    }

    public void RemoveFollower(Minion callee) {
        if (callee == this.follower) {
            this.follower = null;
        }
    }
}
