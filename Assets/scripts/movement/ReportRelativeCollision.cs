using RelPos = ReportRelativeCollision.RelativePosition;
using EvSys = UnityEngine.EventSystems;

public interface OnRelativeCollisionEvent : EvSys.IEventSystemHandler {
    /**
     * Report that we started collision with something on a given relative
     * position.
     *
     * @param p The relative position (in local coordinates)
     * @param c The collision that triggered this
     */
    void OnEnterRelativeCollision(RelPos p, UnityEngine.Collider c);

    /**
     * Report that we started collision with something on a given relative
     * position.
     *
     * @param p The relative position (in local coordinates)
     * @param c The collision that triggered this
     */
    void OnExitRelativeCollision(RelPos p, UnityEngine.Collider c);
}

public static class RelativePositionMethods {
    /** Retrieve how many itens are in this enumeration. */
    public static int count(this RelPos p) {
        return 16;
    }

    /** Convert the enumeration to a sequential integer. */
    public static int toIdx(this RelPos p) {
        switch (p) {
        case RelPos.Top:
            return 0;
        case RelPos.Bottom:
            return 1;
        case RelPos.Front:
            return 2;
        case RelPos.Back:
            return 3;
        case RelPos.Right:
            return 4;
        case RelPos.Left:
            return 5;
        case RelPos.BottomFront:
            return 6;
        case RelPos.TopFront:
            return 7;
        case RelPos.FrontLeft:
            return 8;
        case RelPos.FrontRight:
            return 9;
        case RelPos.TopLeft:
            return 10;
        case RelPos.TopRight:
            return 11;
        case RelPos.FrontTopLeft:
            return 12;
        case RelPos.FrontTopRight:
            return 13;
        case RelPos.BottomBottomFront:
            return 14;
        case RelPos.BottomBack:
            return 15;
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
        FrontLeft = Front | Left,
        FrontRight = Front | Right,
        TopLeft = Top | Left,
        TopRight = Top | Right,
        FrontTopLeft = (Front << 8) | Top | Left,
        FrontTopRight = (Front << 8) | Top | Right,
        FrontTopSomething = (Front << 8) | Top,
        BottomBottomFront = (Bottom << 8) | Bottom | Front,
        BottomBack = Bottom | Back,
    }

    /** Relative position of this game object in reference to its parent. */
    public RelativePosition pos;

    /**
     * Start is called before the first frame update
     */
    void Start() {
    }

    void OnTriggerEnter(UnityEngine.Collider c) {
        EvSys.ExecuteEvents.ExecuteHierarchy<OnRelativeCollisionEvent>(
                this.gameObject, null, (x,y)=>x.OnEnterRelativeCollision(this.pos, c));
    }

    void OnTriggerExit(UnityEngine.Collider c) {
        EvSys.ExecuteEvents.ExecuteHierarchy<OnRelativeCollisionEvent>(
                this.gameObject, null, (x,y)=>x.OnExitRelativeCollision(this.pos, c));
    }
}
