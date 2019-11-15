using Dir = Movement.Direction;
using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;

public class InputControlled : BaseAnimatedEntity {
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public string actionAxis = "Action";

    /** How fast (in seconds) the entity walks over a block */
    public float MoveDelay = 0.4f;

    /**
     * Retrieve the current input direction, if any.
     */
    private Dir getInputDirection() {
        float tmp = UnityEngine.Input.GetAxisRaw(this.horizontalAxis);
        if (tmp > 0.5)
            return Dir.Right;
        else if (tmp < -0.5)
            return Dir.Left;
        tmp = UnityEngine.Input.GetAxisRaw(this.verticalAxis);
        if (tmp > 0.5)
            return Dir.Front;
        else if (tmp < -0.5)
            return Dir.Back;
        return Dir.None;
    }

    private bool checkActionButton() {
        return UnityEngine.Input.GetAxisRaw(this.actionAxis) > 0.5;
    }

    override protected bool canLedge() {
        return true;
    }

    private System.Collections.IEnumerator doPush(float duration) {
        this.anim |= Animation.Push;

        yield return new UnityEngine.WaitForSeconds(duration);

        this.anim &= ~Animation.Push;
    }

    private System.Collections.IEnumerator delayedFall(float duration) {
        yield return new UnityEngine.WaitForSeconds(duration);

        while ((this.anim & Animation.Fall) == 0)
            yield return null;
        yield return new UnityEngine.WaitForSeconds(0.1f);
        this.issueEvent<FallController>( (x, y) => x.Halt(this.gameObject));

        this.setOnLedge();
    }

    private void tryPushBlock(Dir pushDir) {
        GO block = null;
        float delay = 0.0f;
        bool didPush = false;

        if (turnToClosestBlock(out block)) {
            if (pushDir == this.facing) {
                this.issueEvent<PushController>(
                        (x,y) => x.TryPush(ref delay, ref didPush, pushDir),
                        block);
                if (didPush)
                    this.StartCoroutine(this.doPush(delay));
            }
            else if (pushDir == this.facing.toLocal(Dir.Back)) {
                this.issueEvent<PushController>(
                        (x,y) => x.TryPush(ref delay, ref didPush, pushDir,
                                this.gameObject),
                        block);
                if (didPush) {
                    if (!getBlockAt(RelPos.BackBottom)) {
                        this.move(pushDir, delay);
                        this.StartCoroutine(this.delayedFall(delay));
                    }
                    else {
                        this.move(pushDir, delay);
                    }
                }
            }
        }
    }

    private bool turnToClosestBlock(out GO obj) {
        GO tmp = null;
        Dir objDir = Dir.None;

        getClosestBlock(out tmp, out objDir);
        obj = tmp;

        if (tmp != null && objDir != this.facing) {
            this.turn(objDir);
            return false;
        }
        return true;
    }

    override protected void updateState() {
        base.updateState();

        if (this.anim != Animation.None)
            return;

        Dir newDir = this.getInputDirection();
        if (newDir != Dir.None) {
            if (this.isOnLedge())
                if (this.checkActionButton() &&
                        this.getBlockAt(RelPos.Bottom) != null)
                    this.tryPushBlock(newDir);
                else
                    this.tryMoveLedge(newDir, this.MoveDelay);
            else if (this.checkActionButton())
                this.tryPushBlock(newDir);
            else if (this.facing != newDir)
                this.turn(newDir);
            else
                this.tryMoveForward(this.MoveDelay);
        }
        else if (!this.isOnLedge() && this.checkActionButton()) {
            GO obj = null;
            this.turnToClosestBlock(out obj);
        }
    }
}
