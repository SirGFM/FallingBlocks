using Animator = UnityEngine.Animator;
using Dir = Movement.Direction;
using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;
using Type = GetType.Type;

public class InputControlled : BaseAnimatedEntity {
    private const string moveAnim = "isWalking";
    private const string fallAnim = "isFalling";
    private const string forceAnim = "stopIdle";
    private const string climbAnim = "isClimbing";
    private const string ledgeAnim = "isOnLedge";

    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public string actionAxis = "Action";

    /** How fast (in seconds) the entity walks over a block */
    public float MoveDelay = 0.4f;

    /** The animation handler */
    private Animator unityAnimator;

    private void onCenter(bool enter, RelPos p, GO other) {
        Type otherType = Type.Error;

        this.issueEvent<RemoteGetType>(
                (x,y) => x.Get(out otherType), other);
        if (otherType != Type.Player && otherType != Type.Minion)
            this.rootEvent<Loader>( (x,y) => x.ReloadLevel() );
    }

    override protected void start() {
        System.Action<bool, RelPos, GO> cb;

        base.start();

        cb = (x, y, z) => this.onCenter(x, y, z);
        this.setCollisionCb(RelPos.Center, cb);

        this.resetAnimation();
    }

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
                        if (this.isOnLedge())
                            this.dropFromLedge();
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
        this.updateAnimationState();

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

    private bool checkState(Animation target) {
        return (this.anim & target) == target;
    }

    private void getAnimator() {
        GO self = this.gameObject;
        if (this.unityAnimator == null)
            this.unityAnimator = self.GetComponentInChildren<Animator>();
    }

    private void resetAnimation() {
        this.setAnimTrigger(forceAnim);
    }

    private void setAnimTrigger(string anim) {
        this.getAnimator();
        if (this.unityAnimator != null)
            this.unityAnimator.SetTrigger(anim);
    }

    private void setAnimBool(string anim, bool b) {
        this.getAnimator();
        if (this.unityAnimator != null)
            this.unityAnimator.SetBool(anim, b);
    }

    private void updateAnimationState() {
        if (this.anim != Animation.None)
            this.resetAnimation();

        this.setAnimBool(moveAnim, this.checkState(Animation.Move));
        this.setAnimBool(fallAnim, this.checkState(Animation.Fall));
        this.setAnimBool(climbAnim, this.getObjectAt(RelPos.Front) != null);
        this.setAnimBool(ledgeAnim, this.isOnLedge());

        if (this.checkState(Animation.Move) &&
                (this.getObjectAt(RelPos.FrontBottom) == null) &&
                (this.getObjectAt(RelPos.BottomBottomFront) != null)) {
            /* Set the falling animation for moving down a floor */
            this.setAnimBool(fallAnim, true);
        }
    }
}
