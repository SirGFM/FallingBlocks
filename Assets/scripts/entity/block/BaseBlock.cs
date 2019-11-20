using Dir = Movement.Direction;
using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;

public class BaseBlock : BaseEntity {
    private const float fallWait = 1.0f;
    private const float blockFallDelay = 0.15f;

    static private RelPos[] downPositions = {RelPos.Bottom, RelPos.BottomLeft,
            RelPos.BottomRight, RelPos.FrontBottom, RelPos.BackBottom};

    override protected void start() {
        base.start();

        this.setCollisionDownCallback(downPositions);

        this.facing = Dir.None;
    }

    override protected float fallDelay() {
        return blockFallDelay;
    }

    override protected bool checkFirstEntityEnter() {
        foreach (RelPos p in downPositions) {
            GO block = this.getObjectAt(p);
            if (block != null && isBlock(block)) {
                int count = 0;
                this.issueEvent<GetDownCount>(
                        (x,y) => x.Get(out count), block);
                return (count > 0);
            }
        }

        /* Shouldn't ever happen... */
        return false;
    }

    private System.Collections.IEnumerator _onLastBlockExit() {
        this.issueEvent<FallController>( (x, y) => x.Block() );
        this.issueEvent<ShakeController>(
                (x, y) => x.StartShaking(), this.shaker);
        yield return new UnityEngine.WaitForSeconds(BaseBlock.fallWait);
        this.issueEvent<ShakeController>(
                (x, y) => x.StopShaking(), this.shaker);
        this.issueEvent<FallController>( (x, y) => x.Unblock() );
    }

    override protected void onLastBlockExit(RelPos p, GO other) {
        bool otherMoving = false;

        this.issueEvent<MovementController>(
                (x, y) => x.IsMoving(out otherMoving), other);
        if (otherMoving)
            this.StartCoroutine(this._onLastBlockExit());
    }
}
