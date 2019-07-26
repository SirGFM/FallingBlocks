using EvSys = UnityEngine.EventSystems;
using Dir = Movement.Direction;
using GO = UnityEngine.GameObject;
using Vec3 = UnityEngine.Vector3;
using Time = UnityEngine.Time;

public interface iTurning : EvSys.IEventSystemHandler {
    /**
     * Rotate the entity.
     */
    void Turn(Dir from, Dir to, GO caller);
}

public interface iTurned : EvSys.IEventSystemHandler {
    /**
     * Signal that the entity started to rotate toward the given direction
     * (in world-space).
     */
    void OnStartTurning(Dir d, GO callee);

    /**
     * Signal that the entity finished rotating toward the given direction
     * (in world-space).
     */
    void OnFinishTurning(Dir d, GO callee);
}

public class Turning : UnityEngine.MonoBehaviour, iTurning {
    /** Whether the object is currently turning. */
    private bool isTurning = false;
    /** Object that actually issued the event */
    private GO caller = null;

    /** How long to delay movement after a turn */
    public float TurnDelay = 0.3f;

    /**
     * Rotate toward direction 'to' (i.e., to 'tgt' degrees), rotating 'dt'
     * degress.
     */
    private System.Collections.IEnumerator turn(float tgt, float dt, Dir to) {
        this.isTurning = true;
        EvSys.ExecuteEvents.ExecuteHierarchy<iTurned>(
                this.caller, null, (x,y)=>x.OnStartTurning(to, this.gameObject));

        int steps = (int)(this.TurnDelay / Time.fixedDeltaTime);
        dt /= (float)steps;

        Vec3 axis = new Vec3(0, 1, 0);
        for (int i = 0; i < steps; i++) {
            this.transform.Rotate(axis, dt * (i / (float)steps) * 2f);
            yield return new UnityEngine.WaitForFixedUpdate();
        }

        Vec3 tmp = this.transform.eulerAngles;
        this.transform.eulerAngles = new Vec3(tmp.x, tgt, tmp.z);

        /* XXX: Wait some extra time (so the collision updates) to signal that
         * this entity finished turning. Otherwise, next frame's movement may
         * break */
        yield return new UnityEngine.WaitForFixedUpdate();

        this.isTurning = false;
        EvSys.ExecuteEvents.ExecuteHierarchy<iTurned>(
                this.caller, null, (x,y)=>x.OnFinishTurning(to, this.gameObject));
    }

    public void Turn(Dir from, Dir to, GO caller) {
        if (this.isTurning)
            return;

        float tgtAngle, dtAngle;

        switch ((int)from | ((int)to << 4)) {
        case (int)Dir.back | ((int)Dir.front << 4):
            tgtAngle = 180f;
            dtAngle = 180f;
            break;
        case (int)Dir.back | ((int)Dir.left << 4):
            tgtAngle = 90f;
            dtAngle = 90f;
            break;
        case (int)Dir.back | ((int)Dir.right << 4):
            tgtAngle = -90f;
            dtAngle = -90f;
            break;
        case (int)Dir.front | ((int)Dir.back << 4):
            tgtAngle = 0f;
            dtAngle = 180f;
            break;
        case (int)Dir.front | ((int)Dir.left << 4):
            tgtAngle = 90f;
            dtAngle = -90f;
            break;
        case (int)Dir.front | ((int)Dir.right << 4):
            tgtAngle = -90f;
            dtAngle = 90f;
            break;
        case (int)Dir.left | ((int)Dir.front << 4):
            tgtAngle = 180f;
            dtAngle = 90f;
            break;
        case (int)Dir.left | ((int)Dir.back << 4):
            tgtAngle = 0f;
            dtAngle = -90f;
            break;
        case (int)Dir.left | ((int)Dir.right << 4):
            tgtAngle = -90f;
            dtAngle = 180f;
            break;
        case (int)Dir.right | ((int)Dir.front << 4):
            tgtAngle = 180f;
            dtAngle = -90f;
            break;
        case (int)Dir.right | ((int)Dir.back << 4):
            tgtAngle = 0f;
            dtAngle = 90f;
            break;
        case (int)Dir.right | ((int)Dir.left << 4):
            tgtAngle = 90f;
            dtAngle = 180f;
            break;
        default:
            tgtAngle = this.transform.eulerAngles.y;
            dtAngle = 0f;
            break;
        } /* switch */

        this.caller = caller;
        this.StartCoroutine(this.turn(tgtAngle, dtAngle, to));
    }
}
