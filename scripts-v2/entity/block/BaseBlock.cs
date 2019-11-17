using Dir = Movement.Direction;
using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;

public class BaseBlock : BaseEntity {
    private const float fallWait = 1.0f;
    private const float blockFallDelay = 0.25f;

    override protected void start() {
        base.start();

        RelPos[] positions = {RelPos.BottomLeft, RelPos.BottomRight, RelPos.FrontBottom, RelPos.BackBottom};
        this.setCollisionDownCallback(positions);

        this.facing = Dir.None;
    }

    override protected float fallDelay() {
        return blockFallDelay;
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
