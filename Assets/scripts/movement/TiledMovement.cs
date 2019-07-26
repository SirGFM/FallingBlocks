using EvSys = UnityEngine.EventSystems;
using Dir = Movement.Direction;
using GO = UnityEngine.GameObject;
using Vec3 = UnityEngine.Vector3;
using Time = UnityEngine.Time;

public interface iTiledMovement : EvSys.IEventSystemHandler {
    /**
     * Move the entity in a given direction, in world-space.
     */
    void Move(Dir d, GO caller);
}

public interface iTiledMoved : EvSys.IEventSystemHandler {
    /**
     * Signal the the entity started to move in the given direction
     * (in world-space).
     */
    void OnStartMovement(Dir d, GO callee);

    /**
     * Signal the the entity finished moving in the given direction
     * (in world-space).
     */
    void OnFinishMovement(Dir d, GO callee);
}

public class TiledMovement : UnityEngine.MonoBehaviour, iTiledMovement {
    /** Whether the object is currently moving. */
    private bool isMoving = false;
    /** Object that actually issued the event */
    private GO caller = null;

    /** How long moving a tile takes */
    public float MoveDelay = 0.6f;

    /**
     * Move the object to a new position.
     */
    private System.Collections.IEnumerator move(Vec3 tgtPosition, Dir d) {
        this.isMoving = true;
        EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMoved>(
                this.caller, null, (x,y)=>x.OnStartMovement(d, this.gameObject));

        int steps = (int)(this.MoveDelay / Time.fixedDeltaTime);
        Vec3 dtMovement = tgtPosition / (float)steps;
        Vec3 finalPosition = this.transform.localPosition + tgtPosition;

        for (int i = 0; i < steps; i++) {
            /* TODO: Tween/Lerp? */
            this.transform.localPosition = this.transform.localPosition + dtMovement;
            yield return new UnityEngine.WaitForFixedUpdate();
        }
        this.transform.localPosition = finalPosition;

        this.isMoving = false;
        EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMoved>(
                this.caller, null, (x,y)=>x.OnFinishMovement(d, this.gameObject));
    }

    public void Move(Dir d, GO caller) {
        if (this.isMoving)
            return;

        Vec3 tgtPosition = new Vec3();

        Dir tmp = d;
        for (int i = (int)Dir.first; tmp != Dir.none && i < (int)Dir.max; i <<= 1) {
            switch (tmp & (Dir)i) {
            case Dir.back:
                tgtPosition.z = -1.0f;
                break;
            case Dir.left:
                tgtPosition.x = -1.0f;
                break;
            case Dir.front:
                tgtPosition.z = 1.0f;
                break;
            case Dir.right:
                tgtPosition.x = 1.0f;
                break;
            case Dir.top:
                tgtPosition.y = 1.0f;
                break;
            case Dir.bottom:
                tgtPosition.y = -1.0f;
                break;
            } /* switch */
            tmp = (Dir)(((int)tmp) & ~i);
        } /* for */

        this.caller = caller;
        this.StartCoroutine(this.move(tgtPosition, d));
    }
}
