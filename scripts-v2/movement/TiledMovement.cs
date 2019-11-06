using EvSys = UnityEngine.EventSystems;
using Dir = Movement.Direction;
using GO = UnityEngine.GameObject;
using Vec3 = UnityEngine.Vector3;
using Time = UnityEngine.Time;

public interface MovementController : EvSys.IEventSystemHandler {
    /**
     * Move the entity in a given direction, in world-space.
     */
    void Move(Dir d, float moveDelay);
}

public interface MovementDetector : EvSys.IEventSystemHandler {
    /**
     * Signal the the entity started to move in the given direction
     * (in world-space).
     */
    void OnStartMovement(Dir d);

    /**
     * Signal the the entity finished moving in the given direction
     * (in world-space).
     */
    void OnFinishMovement(Dir d);
}

public class TiledMovement : BaseRemoteAction, MovementController {
    /** Whether the object is currently moving. */
    private bool isMoving = false;

    /**
     * Move the object to a new position.
     */
    private System.Collections.IEnumerator move(Vec3 tgtPosition, Dir d, float moveDelay) {
        this.isMoving = true;
        this.issueEvent<MovementDetector>((x,y)=>x.OnStartMovement(d));

        int steps = (int)(moveDelay / Time.fixedDeltaTime);
        Vec3 dtMovement = tgtPosition / (float)steps;
        Vec3 finalPosition = this.transform.localPosition + tgtPosition;

        for (int i = 0; i < steps; i++) {
            /* TODO: Tween/Lerp? */
            this.transform.localPosition = this.transform.localPosition + dtMovement;
            yield return new UnityEngine.WaitForFixedUpdate();
        }
        this.transform.localPosition = finalPosition;

        /* XXX: Wait some extra time (so the collision updates) to signal that
         * this entity finished turning. Otherwise, next frame's movement may
         * break */
        yield return new UnityEngine.WaitForFixedUpdate();

        this.isMoving = false;
        this.issueEvent<MovementDetector>((x,y)=>x.OnFinishMovement(d));
    }

    public void Move(Dir d, float moveDelay) {
        if (this.isMoving)
            return;

        Vec3 tgtPosition = new Vec3();

        Dir tmp = d;
        for (int i = (int)Dir.First; tmp != Dir.None && i < (int)Dir.Max; i <<= 1) {
            switch (tmp & (Dir)i) {
            case Dir.Back:
                tgtPosition.z = -1.0f;
                break;
            case Dir.Left:
                tgtPosition.x = -1.0f;
                break;
            case Dir.Front:
                tgtPosition.z = 1.0f;
                break;
            case Dir.Right:
                tgtPosition.x = 1.0f;
                break;
            case Dir.Top:
                tgtPosition.y = 1.0f;
                break;
            case Dir.Bottom:
                tgtPosition.y = -1.0f;
                break;
            } /* switch */
            tmp = (Dir)(((int)tmp) & ~i);
        } /* for */

        this.StartCoroutine(this.move(tgtPosition, d, moveDelay));
    }
}
