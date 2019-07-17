public enum RelativePosition {
    Top    = 0x01,
    Bottom = 0x02,
    Front  = 0x04,
    Back   = 0x08,
    Right  = 0x10,
    Left   = 0x20,
    BottomFront = Bottom | Front,
}

public interface OnRelativeCollisionEvent : UnityEngine.EventSystems.IEventSystemHandler {
    /**
     * Report that we started collision with something on a given relative
     * position.
     *
     * @param p The relative position (in local coordinates)
     * @param c The collision that triggered this
     */
    void OnEnterRelativeCollision(RelativePosition p, UnityEngine.Collider c);

    /**
     * Report that we started collision with something on a given relative
     * position.
     *
     * @param p The relative position (in local coordinates)
     * @param c The collision that triggered this
     */
    void OnExitRelativeCollision(RelativePosition p, UnityEngine.Collider c);
}

public class ReportRelativeCollision : UnityEngine.MonoBehaviour {
    public RelativePosition pos;

    /**
     * Start is called before the first frame update
     */
    void Start() {
    }

    void OnTriggerEnter(UnityEngine.Collider c) {
        UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy<OnRelativeCollisionEvent>(
                this.gameObject, null, (x,y)=>x.OnEnterRelativeCollision(this.pos, c));
    }

    void OnTriggerExit(UnityEngine.Collider c) {
        UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy<OnRelativeCollisionEvent>(
                this.gameObject, null, (x,y)=>x.OnExitRelativeCollision(this.pos, c));
    }
}
