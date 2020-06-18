using Animator = UnityEngine.Animator;
using Coroutine = UnityEngine.Coroutine;
using CoroutineRet = System.Collections.IEnumerator;
using Dir = Movement.Direction;
using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;
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

    static public string LevelSelectScene;

    /** How fast (in seconds) the entity walks over a block */
    public float MoveDelay = 0.4f;

    /** The animation handler */
    private Animator unityAnimator;

    private void onCenter(bool enter, RelPos p, GO other) {
        Type otherType = Type.Error;

        this.issueEvent<RemoteGetType>(
                (x,y) => x.Get(out otherType), other);
        if (otherType != Type.Player && otherType != Type.Minion) {
            /* Avoid triggering the death scene while rendering the
             * level thumbnails */
            if (SceneMng.GetActiveScene().name != LevelSelectScene) {
                Global.Sfx.playPlayerCrushed();
                SceneMng.LoadSceneAsync("YouLose", SceneMode.Additive);
            }
            this.gameObject.SetActive(false);
        }
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
        float tmp = Input.GetHorizontalAxis();
        if (tmp > 0.5)
            return Dir.Right;
        else if (tmp < -0.5)
            return Dir.Left;
        tmp = Input.GetVerticalAxis();
        if (tmp > 0.5)
            return Dir.Front;
        else if (tmp < -0.5)
            return Dir.Back;
        return Dir.None;
    }

    private bool checkActionButton() {
        return Input.GetActionButton();
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
        this.dropFromLedge();

        while ((this.anim & Animation.Fall) == 0)
            yield return null;
        yield return new UnityEngine.WaitForSeconds(0.1f);
        this.issueEvent<FallController>( (x, y) => x.Halt(this.gameObject));

        this.setOnLedge();
    }

    private bool isPlayingCantPushSfx = false;
    private System.Collections.IEnumerator playCantPushSfx() {
        if (!this.isPlayingCantPushSfx) {
            this.isPlayingCantPushSfx = true;
            Global.Sfx.playPlayerCantPush();
            yield return new UnityEngine.WaitForSeconds(0.5f);
            this.isPlayingCantPushSfx = false;
        }
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
                if (didPush) {
                    Global.Sfx.playPushBlock(delay);
                    this.StartCoroutine(this.doPush(delay));
                }
                else {
                    this.StartCoroutine(this.playCantPushSfx());
                }
            }
            else if (pushDir == this.facing.toLocal(Dir.Back) &&
                    getObjectAt(RelPos.Back) == null) {
                this.issueEvent<PushController>(
                        (x,y) => x.TryPush(ref delay, ref didPush, pushDir,
                                this.gameObject),
                        block);
                if (didPush) {
                    Global.Sfx.playPullBlock(delay);

                    /* Make sure any block bellow becomes cracked */
                    if (this.isOnLedge())
                        this.dropFromLedge();

                    if (!getBlockAt(RelPos.BackBottom)) {
                        this.move(pushDir, delay);
                        this.StartCoroutine(this.delayedFall(delay));
                    }
                    else {
                        this.move(pushDir, delay);
                    }
                }
                else {
                    this.StartCoroutine(this.playCantPushSfx());
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

    override protected void move(Dir to, float delay) {
        if ((this.anim & Animation.Move) == 0) {
            /* Starting to move: play walking sound */
            if (to == this.facing)
                Global.Sfx.playPlayerMoving();
            else if ((to & Dir.Top) == Dir.Top)
                if (!this.isOnLedge())
                    Global.Sfx.playPlayerClimbBlock();
                else
                    Global.Sfx.playPlayerClimbLedge();
            else if ((to & Dir.Bottom) == Dir.Bottom)
                if (!this.isOnLedge())
                    Global.Sfx.playPlayerWalkDownBlock();
                else
                    Global.Sfx.playPlayerDropToLedge();
            else if (this.isOnLedge())
                Global.Sfx.playPlayerMoveLedge();
        }
        base.move(to, delay);
    }

    override protected void turn(Dir to) {
        if (!this.isOnLedge() && (this.anim & Animation.Turn) == 0)
            Global.Sfx.playPlayerTurning();
        base.turn(to);
    }

    private Coroutine _playFallSfx = null;
    private CoroutineRet playFallSfx() {
        while ((this.anim & Animation.Fall) != 0) {
            Global.Sfx.playPlayerFalling();
            yield return new UnityEngine.WaitForSeconds(0.6f);
        }

        this._playFallSfx = null;
    }

    override protected void onFall() {
        if (this._playFallSfx == null)
            this._playFallSfx = this.StartCoroutine(this.playFallSfx());
    }

    override protected void onLand() {
        if (this._playFallSfx != null) {
            this.StopCoroutine(this._playFallSfx);
            this._playFallSfx = null;
        }
        Global.Sfx.playPlayerLand();
    }
}
