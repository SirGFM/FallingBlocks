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
        return 27;
    }

    /** Convert the enumeration to a sequential integer. */
    public static int toIdx(this RelPos p) {
        switch (p) {
        case RelPos.Top:
            return 0;
        case RelPos.Left:
            return 1;
        case RelPos.Right:
            return 2;
        case RelPos.Bottom:
            return 3;
        case RelPos.TopLeft:
            return 4;
        case RelPos.TopRight:
            return 5;
        case RelPos.BottomLeft:
            return 6;
        case RelPos.BottomRight:
            return 7;
        case RelPos.Back:
            return 8;
        case RelPos.BackTop:
            return 9;
        case RelPos.BackLeft:
            return 10;
        case RelPos.BackRight:
            return 11;
        case RelPos.BackBottom:
            return 12;
        case RelPos.BackTopLeft:
            return 13;
        case RelPos.BackTopRight:
            return 14;
        case RelPos.BackBottomLeft:
            return 15;
        case RelPos.BackBottomRight:
            return 16;
        case RelPos.Front:
            return 17;
        case RelPos.FrontTop:
            return 18;
        case RelPos.FrontLeft:
            return 19;
        case RelPos.FrontRight:
            return 20;
        case RelPos.FrontBottom:
            return 21;
        case RelPos.FrontTopLeft:
            return 22;
        case RelPos.FrontTopRight:
            return 23;
        case RelPos.FrontBottomLeft:
            return 24;
        case RelPos.FrontBottomRight:
            return 25;
        case RelPos.BottomBottomFront:
            return 26;
        default:
            return -1;
        }
    }

    public static UnityEngine.Vector3 toPosition(this RelPos p) {
        switch (p) {
        case RelPos.Top:
            return new UnityEngine.Vector3(0.0f, 1.0f, 0.0f);
        case RelPos.Left:
            return new UnityEngine.Vector3(1.0f, 0.0f, 0.0f);
        case RelPos.Right:
            return new UnityEngine.Vector3(-1.0f, 0.0f, 0.0f);
        case RelPos.Bottom:
            return new UnityEngine.Vector3(0.0f, -1.0f, 0.0f);
        case RelPos.Back:
            return new UnityEngine.Vector3(0.0f, 0.0f, 1.0f);
        case RelPos.Front:
            return new UnityEngine.Vector3(0.0f, 0.0f, -1.0f);
        default:
            return new UnityEngine.Vector3(0.0f, 0.0f, 0.0f);
        }
    }

    public static RelPos shift(this RelPos p) {
        return (RelPos)(((int)p) >> 8);
    }

    public static RelPos masked(this RelPos p) {
        return (RelPos)(((int)p) & 0xff);
    }
}

public class ReportRelativeCollision : UnityEngine.MonoBehaviour {
    public enum RelativePosition {
        /* Middle slice */
        Top             = 0x01,
        Left            = 0x02,
        Right           = 0x04,
        Bottom          = 0x08,
        TopSomething    = (Top << 8),
        BottomSomething = (Bottom << 8),
        TopLeft         = TopSomething | Left,
        TopRight        = TopSomething | Right,
        BottomLeft      = BottomSomething | Left,
        BottomRight     = BottomSomething | Right,
        /* Back slice */
        Back            = 0x10,
        BackSomething   = (Back << 16),
        BackTop         = BackSomething | Top,
        BackLeft        = BackSomething | Left,
        BackRight       = BackSomething | Right,
        BackBottom      = BackSomething | Bottom,
        BackTopLeft     = BackSomething | TopSomething | Left,
        BackTopRight    = BackSomething | TopSomething | Right,
        BackBottomLeft  = BackSomething | BottomSomething | Left,
        BackBottomRight = BackSomething | BottomSomething | Right,
        /* Front slice */
        Front            = 0x20,
        FrontSomething   = (Front << 16),
        FrontTop         = FrontSomething | Top,
        FrontLeft        = FrontSomething | Left,
        FrontRight       = FrontSomething | Right,
        FrontBottom      = FrontSomething | Bottom,
        FrontTopLeft     = FrontSomething | (Top << 8) | Left,
        FrontTopRight    = FrontSomething | (Top << 8) | Right,
        FrontBottomLeft  = FrontSomething | (Bottom << 8) | Left,
        FrontBottomRight = FrontSomething | (Bottom << 8) | Right,
        /* Extra directions */
        BottomBottomFront = (Bottom << 16) | BottomSomething | Front,
        FrontTopSomething = FrontSomething | TopSomething,
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
