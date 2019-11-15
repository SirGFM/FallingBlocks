using Dir = Movement.Direction;
using GO = UnityEngine.GameObject;
using Math = UnityEngine.Mathf;
using RelPos = RelativeCollision.RelativePosition;
using Type = GetType.Type;
using Vec3 = UnityEngine.Vector3;

public class Minion : BaseAnimatedEntity {
    private GO target;
    Type targetPriority;

    /** How fast (in seconds) the entity walks over a block */
    public float MoveDelay = 0.4f;

    private UnityEngine.Transform selfT;
    private UnityEngine.Transform otherT;

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
    }

    private void onCollisionEnter(RelPos p, GO other) {
        Type otherPriority = Type.Error;

        this.issueEvent<RemoteGetType>(
                (x,y) => x.Get(out otherPriority), other);

        if (this.target != null && this.targetPriority > otherPriority)
            return;

        this.target = other;
        this.otherT = other.transform;
        this.targetPriority = otherPriority;
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
}
