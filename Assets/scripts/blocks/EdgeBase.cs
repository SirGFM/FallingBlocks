public interface OnBlockEdge : UnityEngine.EventSystems.IEventSystemHandler {
    /**
     * Report that we started collision with something on a given relative
     * position.
     *
     * @param d The direction of the edge **on the colliding object**.
     * @param c The collision that triggered this
     */
    void OnTouchEdge(EdgeBase.Direction d);

    /**
     * Report that we started collision with something on a given relative
     * position.
     *
     * @param d The direction of the edge **on the object that's leaving**.
     * @param isOtherMoving Whether the releasing object it currently moving.
     */
    void OnReleaseEdge(EdgeBase.Direction d, bool isOtherMoving);
}

public interface ActivateOnTop : UnityEngine.EventSystems.IEventSystemHandler {
    /**
     * Signals that something got on top of this object. Should really only be
     * used by the player and by minions, to break cracked blocks.
     */
    void OnEnterTop(UnityEngine.GameObject other);

    /**
     * Signals that something just got off the top of this object. Should really
     * only be used by the player and by minions, to break cracked blocks.
     */
    void OnLeaveTop(UnityEngine.GameObject other);
}

public class EdgeBase : UnityEngine.MonoBehaviour {
    /** List of collideable edges */
    public enum Direction {
        min = 0,
        topBack = 0, /* Y = positive, Z = negative, X = zero     */
        topFront,    /* Y = positive, Z = positive, X = zero     */
        topLeft,     /* Y = positive, Z = zero,     X = negative */
        topRight,    /* Y = positive, Z = zero,     X = positive */
        max
    };

    /**
     * Initializes the object's Rigidbody, adding a new one if not found.
     */
    protected void setupRigidbody() {
        UnityEngine.Rigidbody rb = this.gameObject.GetComponent<UnityEngine.Rigidbody>();
        if (rb == null)
            rb = this.gameObject.AddComponent<UnityEngine.Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    /**
     * Convert a direction d into a unit 1 Vector 3.
     */
    protected UnityEngine.Vector3 directionToV3(Direction d) {
        switch (d) {
        case Direction.topBack:
            return new UnityEngine.Vector3(0f, 1f, -1f);
        case Direction.topFront:
            return new UnityEngine.Vector3(0f, 1f, 1f);
        case Direction.topLeft:
            return new UnityEngine.Vector3(-1f, 1f, 0f);
        case Direction.topRight:
            return new UnityEngine.Vector3(1f, 1f, 0f);
#if false
        case Direction.bottomBack:
            return new UnityEngine.Vector3(0f, -1f, -1f);
        case Direction.bottomFront:
            return new UnityEngine.Vector3(0f, -1f, 1f);
        case Direction.bottomLeft:
            return new UnityEngine.Vector3(-1f, -1f, 0f);
        case Direction.bottomRight:
            return new UnityEngine.Vector3(1f, -1f, 0f);
#endif
        default:
            throw new System.Exception("Invalid direction");
        }
    }
}
