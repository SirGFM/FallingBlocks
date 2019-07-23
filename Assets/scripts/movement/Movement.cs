﻿using Dir = Movement.Direction;

namespace Movement {
    /** List of directions an object may face (in camera space),
     * sorted clock-wise. */
    public enum Direction {
        none   = 0x00,
        first  = 0x01,
        back   = 0x01, /* Camera facing */
        left   = 0x02,
        front  = 0x04,
        right  = 0x08,
        top    = 0x10,
        bottom = 0x20,
        max
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
        case Dir.front:
            return d;
        case Dir.back:
            return d.rotateClockWise().rotateClockWise();
        case Dir.left:
            return d.rotateCounterClockWise();
        case Dir.right:
            return d.rotateClockWise();
        default:
            throw new System.Exception("Invalid forward orientation");
        }
    }
}
