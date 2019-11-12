using Dir = Movement.Direction;
using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;

public class BaseAnimatedEntity : BaseEntity {
    protected bool onLedge;

    override protected void start() {
        base.start();
        this.onLedge = false;
    }

    override protected void updateState() {
        if (this.onLedge && this.anim == Animation.None &&
                getBlockAt(RelPos.Front) == null)
            this.onLedge = false;

        base.updateState();
    }

    override protected bool canFall() {
        return (this.onLedge == false);
    }

    private Dir CardinalRelPosToLocalDir(RelPos p) {
        switch (p) {
        case RelPos.Front:
            return Dir.Front.toLocal(this.facing);
        case RelPos.Right:
            return Dir.Right.toLocal(this.facing);
        case RelPos.Left:
            return Dir.Left.toLocal(this.facing);
        case RelPos.Back:
            return Dir.Back.toLocal(this.facing);
        default:
            return Dir.None;
        }
    }

    private static bool isBlock(GO obj) {
        return (obj != null && obj.GetComponent<BaseBlock>() != null);
    }

    private GO getObjectAt(RelPos p) {
        GO ret = null;
        this.issueEvent<GetRelativeObject>(
                (x, y) => x.GetObjectAt(out ret, p));
        return ret;
    }

    protected GO getBlockAt(RelPos p) {
        GO ret = this.getObjectAt(p);
        if (!isBlock(ret))
            ret = null;
        return ret;
    }

    protected bool checkBlockAt(out GO obj, out Dir dir, RelPos p) {
        obj = this.getBlockAt(p);
        dir = this.CardinalRelPosToLocalDir(p);
        return (obj != null);
    }

    protected void getClosestBlock(out GO obj, out Dir dir) {
        GO retObj = null;
        Dir retDir = Dir.None;

        RelPos[] testPos = {RelPos.Front, RelPos.Right, RelPos.Left, RelPos.Back};
        foreach (RelPos p in testPos) {
            if (checkBlockAt(out retObj, out retDir, p))
                break;
        }

        obj = retObj;
        dir = retDir;
    }

    virtual protected bool canLedge() {
        return false;
    }

    /**
     * Try to move forward, going up/down stairs or into the ledge.
     *
     * @param moveDelay How long the movement should take
     */
    protected void tryMoveForward(float moveDelay) {
        GO frontObj = this.getObjectAt(RelPos.Front);
        if (isBlock(frontObj)) {
            /* Block ahead; Try to jump up */
            if (this.getObjectAt(RelPos.FrontTop) == null &&
                    this.getObjectAt(RelPos.Top) == null) {
                /* There's a floor above; Jump toward it */
                Dir d = this.facing | Dir.Top;
                this.move(d, moveDelay);
            }
        }
        else if (frontObj == null) {
            if (this.getBlockAt(RelPos.FrontBottom) != null) {
                /* Front is clear and there's footing; Just move forward. */
                this.move(this.facing, moveDelay);
            }
            else if (this.getBlockAt(RelPos.BottomBottomFront) != null) {
                /* There's a floor bellow; Jump toward it */
                Dir d = this.facing | Dir.Bottom;
                this.move(d, moveDelay);
            }
            else if (this.canLedge()) {
                /* Fall to the ledge! */
                Dir newDir = this.facing.toLocal(Dir.Back);
                Dir d = this.facing | Dir.Bottom;
                this.move(d, moveDelay);
                this.turn(newDir);
                this.onLedge = true;
            }
        }
    }

    /**
     * Try to move sideways while in a ledge. One can simply move to the sides,
     * around a block or toward a block.
     *
     * @param dir Relative direction of the movement (Left or Right)
     * @param innerTurn New direction when moving **toward** a block
     * @param outerTurn New direction when moving **around** a block
     * @param moveDir Base direction of sideways movement
     * @param moveDelay How long the movement should take
     */
    private void tryMoveLedgeSide(RelPos dir, Dir innerTurn, Dir outerTurn,
            Dir moveDir, float moveDelay) {
        GO dirObj, frontDirObj, topDirObj;
        bool isWall, isInner;

        dirObj = this.getBlockAt(dir);
        frontDirObj = this.getBlockAt(RelPos.FrontSomething | dir);
        topDirObj = this.getBlockAt(RelPos.TopSomething | dir);

        isWall = (dirObj == null) && (frontDirObj != null);
        isInner = (dirObj != null);

        if (isWall && (topDirObj == null)) {
            Dir d = moveDir.toLocal(this.facing);
            this.move(d, moveDelay);
        }
        else if (isInner) {
            this.turn(innerTurn);
        }
        else {
            GO frontTopDirObj;
            bool isOuter;

            frontTopDirObj = this.getBlockAt(RelPos.FrontTopSomething | dir);
            isOuter = (dirObj == null) && (frontDirObj == null);

            if (isOuter && (frontTopDirObj == null) && (topDirObj == null)) {
                Dir d = moveDir.toLocal(this.facing) | Dir.Front.toLocal(this.facing);
                this.move(d, moveDelay);
                this.turn(outerTurn);
            }
        }
    }

    /**
     * Try to move within the ledge.
     *
     * @param moveDir Base direction of sideways movement
     * @param moveDelay How long the movement should take
     */
    protected void tryMoveLedge(Dir moveDir, float moveDelay) {
        switch (moveDir) {
        case Dir.Front:
            /* Move up, if there's enough room */
            if (this.getObjectAt(RelPos.FrontTop) == null) {
                Dir d = this.facing | Dir.Top;
                this.move(d, moveDelay);
                this.onLedge = false;
            }
            break;
        case Dir.Back:
            /* Simply start to fall */
            this.onLedge = false;
            break;
        case Dir.Right:
            this.tryMoveLedgeSide(RelPos.Right,
                    this.facing.rotateClockWise() /* innerTurn */,
                    this.facing.rotateCounterClockWise() /* outterTurn */,
                    moveDir, moveDelay);
            break;
        case Dir.Left:
            this.tryMoveLedgeSide(RelPos.Left,
                    this.facing.rotateCounterClockWise() /* innerTurn */,
                    this.facing.rotateClockWise() /* outterTurn */,
                    moveDir, moveDelay);
            break;
        }
    }
}
