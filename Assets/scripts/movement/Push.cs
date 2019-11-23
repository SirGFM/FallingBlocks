using Dir = Movement.Direction;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;

public interface PushController : EvSys.IEventSystemHandler {
    /**
     * Try to push the entity in a given direction. The entity will only
     * be pushed if all entities on the given direction are pushable, and
     * the entities will be pushed at the slowest speed of them all.
     *
     * @param delay How long it will take to push this entity
     * @param didPush Whether the entity was pushed
     * @param d The direction of the movement, in world space
     * @param ignored Object that pushed the others, and should be ignored
     */
    void TryPush(ref float delay, ref bool didPush, Dir d, GO ignored = null);
}

public class Push : BaseRemoteAction, PushController {
    private Dir localDir = Dir.Front;

    /** How long it takes to push this entity */
    public float moveDelay = 0.6f;

    public void TryPush(ref float delay, ref bool didPush, Dir d,
            GO ignored = null) {
        GO next = null;
        RelPos p;
        float localDelay;
        bool isShaking = false;

        this.issueEvent<IsShaking>( (x,y) => x.Check(out isShaking) );
        if (isShaking) {
            /* Pushing while shaking causes a bug, so block it */
            didPush = false;
            return;
        }

        /* Use the slowest movement */
        if (this.moveDelay > delay)
            localDelay = this.moveDelay;
        else
            localDelay = delay;

        switch (d.toLocal(this.localDir)) {
            case Dir.Back:
                p = RelPos.Front;
                break;
            case Dir.Left:
                p = RelPos.Right;
                break;
            case Dir.Right:
                p = RelPos.Left;
                break;
            case Dir.Front:
                p = RelPos.Back;
                break;
            default:
                p = RelPos.None;
                break;
        }

        this.issueEvent<GetRelativeObject>(
                (x,y) => x.GetObjectAt(out next, p) );
        if (next != null && next != ignored) {
            bool localPush = false;
            /* Check whether the next object in this direction can be pushed */
            this.issueEvent<PushController>(
                    (x,y) => x.TryPush(ref localDelay, ref localPush, d), next);
            if (!localPush) {
                /* Since the object can't be pushed, bail out! */
                didPush = false;
                return;
            }
        }

        /* Reaching this points means:
         *   1. That every touching object can be pushed
         *   2. That delay has the slowest movement
         * So just move the object */
        this.issueEvent<MovementController>(
                (x,y) => x.Move(d, localDelay) );

        /* Assign the return variables */
        delay = localDelay;
        didPush = true;
    }
}
