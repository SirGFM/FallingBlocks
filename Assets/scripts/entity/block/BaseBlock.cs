using Col = UnityEngine.BoxCollider;
using Dir = Movement.Direction;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;
using Vec3 = UnityEngine.Vector3;

public interface IsShaking : EvSys.IEventSystemHandler {
    /** Check whether the block is currently shaking */
    void Check(out bool val);
}

public class BaseBlock : BaseEntity, IsShaking {
    private const float fallWait = 1.0f;
    private const float blockFallDelay = 0.15f;

    private bool isShaking;

    static private RelPos[] downPositions = {RelPos.Bottom, RelPos.BottomLeft,
            RelPos.BottomRight, RelPos.FrontBottom, RelPos.BackBottom};

    override protected void start() {
        base.start();

        this.setCollisionDownCallback(downPositions);

        this.facing = Dir.None;
        this.isShaking = false;

        /* XXX: Add an extra collider so moving the only block beneath another
         * doesn't cause the latter to shake */
        Col topCol = this.gameObject.AddComponent<Col>();
        topCol.size = new Vec3(0.75f, 0.1f, 0.75f);
        topCol.isTrigger = true;
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
        Global.Sfx.playBlockShaking();
        this.isShaking = true;
        yield return new UnityEngine.WaitForSeconds(BaseBlock.fallWait);
        this.issueEvent<ShakeController>(
                (x, y) => x.StopShaking(), this.shaker);
        this.isShaking = false;
        this.issueEvent<FallController>( (x, y) => x.Unblock() );
    }

    override protected void onLastBlockExit(RelPos p, GO other) {
        bool otherMoving = false;

        this.issueEvent<MovementController>(
                (x, y) => x.IsMoving(out otherMoving), other);
        if (otherMoving)
            this.StartCoroutine(this._onLastBlockExit());
    }

    public void Check(out bool val) {
        val = (this.isShaking || (this.anim & Animation.Shake) != 0);
    }

    override protected void onLand() {
        Global.Sfx.playBlockLanded();
    }
}
