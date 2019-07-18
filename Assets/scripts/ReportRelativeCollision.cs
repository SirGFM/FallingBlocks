public interface OnRelativeCollisionEvent : UnityEngine.EventSystems.IEventSystemHandler {
    /**
     * Report that we started collision with something on a given relative
     * position.
     *
     * @param p The relative position (in local coordinates)
     * @param c The collision that triggered this
     */
    void OnEnterRelativeCollision(ReportRelativeCollision.RelativePosition p, UnityEngine.Collider c);

    /**
     * Report that we started collision with something on a given relative
     * position.
     *
     * @param p The relative position (in local coordinates)
     * @param c The collision that triggered this
     */
    void OnExitRelativeCollision(ReportRelativeCollision.RelativePosition p, UnityEngine.Collider c);
}

public static class RelativePositionMethods {
    /** Retrieve how many itens are in this enumeration. */
    public static int count(this ReportRelativeCollision.RelativePosition p) {
        return 8;
    }
    /** Convert the enumeration to a sequential integer. */
    public static int toIdx(this ReportRelativeCollision.RelativePosition p) {
        switch (p) {
        case ReportRelativeCollision.RelativePosition.Top:
            return 0;
        case ReportRelativeCollision.RelativePosition.Bottom:
            return 1;
        case ReportRelativeCollision.RelativePosition.Front:
            return 2;
        case ReportRelativeCollision.RelativePosition.Back:
            return 3;
        case ReportRelativeCollision.RelativePosition.Right:
            return 4;
        case ReportRelativeCollision.RelativePosition.Left:
            return 5;
        case ReportRelativeCollision.RelativePosition.BottomFront:
            return 6;
        case ReportRelativeCollision.RelativePosition.TopFront:
            return 7;
        default:
            return -1;
        }
    }
}

public class ReportRelativeCollision : UnityEngine.MonoBehaviour {
    public enum RelativePosition {
        Top    = 0x01,
        Bottom = 0x02,
        Front  = 0x04,
        Back   = 0x08,
        Right  = 0x10,
        Left   = 0x20,
        BottomFront = Bottom | Front,
        TopFront = Top | Front,
    }

    /** Relative position of this game object in reference to its parent. */
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
