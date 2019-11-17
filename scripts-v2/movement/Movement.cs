using Dir = Movement.Direction;
using Vec3 = UnityEngine.Vector3;

namespace Movement {
    /** List of directions an object may face (in camera space),
     * sorted clock-wise. */
    public enum Direction {
        None   = 0x00,
        First  = 0x01,
        Back   = 0x01, /* Camera facing */
        Left   = 0x02,
        Front  = 0x04,
        Right  = 0x08,
        Top    = 0x10,
        Bottom = 0x20,
        Max
    };
}

public static class MovementMethods {
    /**
     * Rotate this direction clock-wise.
     */
    public static Dir rotateClockWise(this Dir d) {
        int i = (int)d;
        if ((i << 1) > 0xf)
            return (Dir)0x1;
        return (Dir)(i << 1);
    }

    /**
     * Rotate this direction counter clock-wise.
     */
    public static Dir rotateCounterClockWise(this Dir d) {
        int i = (int)d;
        if ((i >> 1) == 0x0)
            return (Dir)0x8;
        return (Dir)(i >> 1);
    }

    /**
     * Rotates a given to local orientation.
     */
    public static Dir toLocal(this Dir d, Dir forward) {
        switch (forward) {
        case Dir.Front:
            return d;
        case Dir.Back:
            return d.rotateClockWise().rotateClockWise();
        case Dir.Left:
            return d.rotateCounterClockWise();
        case Dir.Right:
            return d.rotateClockWise();
        default:
            throw new System.Exception("Invalid forward orientation");
        }
    }

    /**
     * Rotates a given to local orientation.
     */
    public static Vec3 toVec3(this Dir d) {
        switch (d) {
        case Dir.None:
            return new Vec3(0.0f, 0.0f, 0.0f);
        case Dir.Front:
            return new Vec3(0.0f, 0.0f, 1.0f);
        case Dir.Back:
            return new Vec3(0.0f, 0.0f, -1.0f);
        case Dir.Left:
            return new Vec3(-1.0f, 0.0f, 0.0f);
        case Dir.Right:
            return new Vec3(1.0f, 0.0f, 0.0f);
        case Dir.Top:
            return new Vec3(0.0f, 1.0f, 0.0f);
        case Dir.Bottom:
            return new Vec3(0.0f, -1.0f, 0.0f);
        default:
            throw new System.Exception("Invalid direction");
        }
    }
}
