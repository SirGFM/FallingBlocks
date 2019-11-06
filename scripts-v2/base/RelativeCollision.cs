using EvSys = UnityEngine.EventSystems;
using RelPos = RelativeCollision.RelativePosition;
using Vec3 = UnityEngine.Vector3;

public interface OnRelativeCollisionEvent : EvSys.IEventSystemHandler {
    /**
     * Report that we started collision with something on a given relative
     * position.
     *
     * @param rc Object that detected the collision
     * @param  c The collision that triggered this
     */
    void OnEnterRelativeCollision(RelativeCollision rc, UnityEngine.Collider c);

    /**
     * Report that we started collision with something on a given relative
     * position.
     *
     * @param rc Object that detected the collision
     * @param  c The collision that triggered this
     */
    void OnExitRelativeCollision(RelativeCollision rc, UnityEngine.Collider c);
}

public static class RelativePositionMethods {
    /** Retrieve how many itens are in this enumeration. */
    public static int count(this RelPos p) {
        return 37;
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
        case RelPos.FrontFront:
            return 27;
        case RelPos.BackBack:
            return 28;
        case RelPos.LeftLeft:
            return 29;
        case RelPos.RightRight:
            return 30;
        case RelPos.FrontTopFrontTop:
            return 31;
        case RelPos.FrontFrontTop:
            return 32;
        case RelPos.FrontFrontBottom:
            return 33;
        case RelPos.FrontBottomFrontBottom:
            return 34;
        case RelPos.Center:
            return 35;
        case RelPos.Nearby:
            return 36;
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
        return (RelPos)((int)p >> (int)RelPos.Shift);
    }

    public static RelPos masked(this RelPos p) {
        return (RelPos)((int)p & (int)RelPos.Mask);
    }
    public static int toInt(this RelPos p) {
        return (int)p;
    }
}

public class RelativeCollision : BaseRemoteAction {
    public enum RelativePosition {
        /* Helpers */
        Mask                   = 0x3f,
        Shift                  = 6,
        /* Middle slice */
        Top                    = 0x01,
        Left                   = 0x02,
        Right                  = 0x04,
        Bottom                 = 0x08,
        TopSomething           = (Top << Shift),
        BottomSomething        = (Bottom << Shift),
        TopLeft                = TopSomething | Left,
        TopRight               = TopSomething | Right,
        BottomLeft             = BottomSomething | Left,
        BottomRight            = BottomSomething | Right,
        /* Back slice */
        Back                   = 0x10,
        BackSomething          = (Back << (Shift * 2)),
        BackTop                = BackSomething | Top,
        BackLeft               = BackSomething | Left,
        BackRight              = BackSomething | Right,
        BackBottom             = BackSomething | Bottom,
        BackTopLeft            = BackSomething | TopSomething | Left,
        BackTopRight           = BackSomething | TopSomething | Right,
        BackBottomLeft         = BackSomething | BottomSomething | Left,
        BackBottomRight        = BackSomething | BottomSomething | Right,
        /* Front slice */
        Front                  = 0x20,
        FrontSomething         = (Front << (Shift * 2)),
        FrontTop               = FrontSomething | Top,
        FrontLeft              = FrontSomething | Left,
        FrontRight             = FrontSomething | Right,
        FrontBottom            = FrontSomething | Bottom,
        FrontTopLeft           = FrontSomething | (Top << Shift) | Left,
        FrontTopRight          = FrontSomething | (Top << Shift) | Right,
        FrontBottomLeft        = FrontSomething | (Bottom << Shift) | Left,
        FrontBottomRight       = FrontSomething | (Bottom << Shift) | Right,
        /* Extra directions */
        BottomBottomFront      = (Bottom << (Shift * 2)) | BottomSomething | Front,
        FrontTopSomething      = FrontSomething | TopSomething,
        FrontFront             = (Front << (Shift * 2)) | Front,
        BackBack               = (Back << (Shift * 2)) | Back,
        LeftLeft               = (Left << (Shift * 2)) | Left,
        RightRight             = (Right << (Shift * 2)) | Right,
        FrontTopFrontTop       = (Front << (Shift * 4)) | (Top << (Shift * 3)) | (Front << (Shift * 2)) | Top,
        FrontFrontTop          = (Front << (Shift * 3)) | (Front << (Shift * 2)) | Top,
        FrontFrontBottom       = (Front << (Shift * 3)) | (Front << (Shift * 2)) | Bottom,
        FrontBottomFrontBottom = (Front << (Shift * 4)) | (Bottom << (Shift * 3)) | (Front << (Shift * 2)) | Bottom,
        Center                 = Top | Left | Right | Bottom | Front | Back,
        Nearby                 = 0,
    }

    /** Relative position of this game object in reference to its parent. */
    public RelativePosition pos;

    void OnTriggerEnter(UnityEngine.Collider c) {
        this.issueEvent<OnRelativeCollisionEvent>(
                (x,y) => x.OnEnterRelativeCollision(this, c) );
    }

    void OnTriggerExit(UnityEngine.Collider c) {
        this.issueEvent<OnRelativeCollisionEvent>(
                (x,y) => x.OnExitRelativeCollision(this, c) );
    }
}
