using Dir = Movement.Direction;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using Time = UnityEngine.Time;
using Vec3 = UnityEngine.Vector3;

public interface TurnController : EvSys.IEventSystemHandler {
    /** Rotate the entity. */
    void Rotate(Dir from, Dir to);

    /** Retrieve the TurnController receiver */
    void GetTurnComponent(GetComponentControllerParam param);
}

public interface TurnDetector : EvSys.IEventSystemHandler {
    /**
     * Signal that the entity started to rotate toward the given direction
     * (in world-space).
     */
    void OnStartTurning(Dir d);

    /**
     * Signal that the entity finished rotating toward the given direction
     * (in world-space).
     */
    void OnFinishTurning(Dir d);
}

public class Turn : BaseRemoteAction, TurnController {
    /** Whether the object is currently turning. */
    private bool isTurning = false;

    /** How long to delay movement after a turn */
    public float TurnDelay = 0.2f;

    /**
     * Rotate toward direction 'to' (i.e., to 'tgt' degrees), rotating 'dt'
     * degress.
     */
    private System.Collections.IEnumerator turn(float tgt, float dt, Dir to) {
        this.isTurning = true;
        this.issueEvent<TurnDetector>((x,y) => x.OnStartTurning(to));

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
        this.issueEvent<TurnDetector>((x,y) => x.OnFinishTurning(to));
    }

    public void Rotate(Dir from, Dir to) {
        if (this.isTurning)
            return;

        float tgtAngle, dtAngle;

        switch ((int)from | ((int)to << 4)) {
        case (int)Dir.Back | ((int)Dir.Front << 4):
            tgtAngle = 180f;
            dtAngle = 180f;
            break;
        case (int)Dir.Back | ((int)Dir.Left << 4):
            tgtAngle = 90f;
            dtAngle = 90f;
            break;
        case (int)Dir.Back | ((int)Dir.Right << 4):
            tgtAngle = -90f;
            dtAngle = -90f;
            break;
        case (int)Dir.Front | ((int)Dir.Back << 4):
            tgtAngle = 0f;
            dtAngle = 180f;
            break;
        case (int)Dir.Front | ((int)Dir.Left << 4):
            tgtAngle = 90f;
            dtAngle = -90f;
            break;
        case (int)Dir.Front | ((int)Dir.Right << 4):
            tgtAngle = -90f;
            dtAngle = 90f;
            break;
        case (int)Dir.Left | ((int)Dir.Front << 4):
            tgtAngle = 180f;
            dtAngle = 90f;
            break;
        case (int)Dir.Left | ((int)Dir.Back << 4):
            tgtAngle = 0f;
            dtAngle = -90f;
            break;
        case (int)Dir.Left | ((int)Dir.Right << 4):
            tgtAngle = -90f;
            dtAngle = 180f;
            break;
        case (int)Dir.Right | ((int)Dir.Front << 4):
            tgtAngle = 180f;
            dtAngle = -90f;
            break;
        case (int)Dir.Right | ((int)Dir.Back << 4):
            tgtAngle = 0f;
            dtAngle = 90f;
            break;
        case (int)Dir.Right | ((int)Dir.Left << 4):
            tgtAngle = 90f;
            dtAngle = 180f;
            break;
        default:
            tgtAngle = this.transform.eulerAngles.y;
            dtAngle = 0f;
            break;
        } /* switch */

        this.StartCoroutine(this.turn(tgtAngle, dtAngle, to));
    }

    public void GetTurnComponent(GetComponentControllerParam param) {
        param.obj = this.gameObject;
    }
}
