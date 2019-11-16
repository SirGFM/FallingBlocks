using Anim = BaseEntity.Animation;
using Dir = Movement.Direction;
using GO = UnityEngine.GameObject;
using Math = UnityEngine.Mathf;
using RelPos = RelativeCollision.RelativePosition;
using Type = GetType.Type;
using Vec3 = UnityEngine.Vector3;

public class Minion : BaseAnimatedEntity {
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
        System.Tuple<RelPos, System.Action<bool, RelPos, GO>> arg;
        RelPos p;

        base.start();

        cb = (x, y, z) => this.onCollision(x, y, z);
        p = RelPos.None;
        arg = new System.Tuple<RelPos, System.Action<bool, RelPos, GO>>(p, cb);
        this.BroadcastMessage("SetRelativePositionCallback", arg);

        this.target = null;
        this.targetPriority = Type.None;
        this.selfT = this.gameObject.transform;
        this.bgFunc = null;

        this.rootEvent<LoaderEvents>( (x,y) => x.IncreaseMaxMinion() );
    }

    private void onCollisionEnter(RelPos p, GO other) {
        Type otherPriority = Type.Error;

        this.issueEvent<RemoteGetType>(
                (x,y) => x.Get(out otherPriority), other);

        if (this.targetPriority >= otherPriority ||
                (otherPriority == Type.Minion && this.follower != null))
            return;

        this.target = other;
        this.otherT = other.transform;
        this.targetPriority = otherPriority;

        if (otherPriority == Type.Minion) {
            Minion mFollower = other.GetComponent<Minion>();
            mFollower.follower = this;
        }
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

    private void onCollisionExit(RelPos p, GO other) {
        if (this.target == other) {
            Dir moveDir;

            if (this.targetPriority == Type.Minion) {
                Minion mFollower = other.GetComponent<Minion>();
                mFollower.follower = null;
            }

            /* The target we were following just left, try to move after it */
            this.target = null;
            this.targetPriority = Type.None;
            moveDir = vec3ToDir(otherT.position - this.selfT.position);
            this.StartCoroutine(this.follow(other, moveDir));
        }
    }

    override protected void onCollision(bool enter, RelPos p, GO other) {
        if (enter)
            this.onCollisionEnter(p, other);
        else
            this.onCollisionExit(p, other);
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
}
