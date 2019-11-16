using Animator = UnityEngine.Animator;
using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;
using Type = GetType.Type;
using Vec3 = UnityEngine.Vector3;

public class GoalBlock : BaseBlock, LedgeTracker {
    /** The animation handler */
    private Animator animator;
    /** The trigger used to start the animation */
    private const string trigger = "StartGoalAnim";

    override protected void start() {
        base.start();
        this.getAnimator();
        this.setupCollision();
    }

    private void getAnimator() {
        if (this.animator == null)
            this.animator = this.gameObject.GetComponentInChildren<Animator>();
    }

    private void setupCollision() {
        System.Action<bool, RelPos, GO> cb;
        System.Tuple<RelPos, System.Action<bool, RelPos, GO>> arg;
        RelPos p = RelPos.Top;

        cb = (x, y, z) => this.onCollisionUp(x, y, z);
        arg = new System.Tuple<RelPos, System.Action<bool, RelPos, GO>>(p, cb);
        this.BroadcastMessage("SetRelativePositionCallback", arg);
    }

    private void showWinScreen() {
        /* TODO:
         *   - Play 'you win' fanfare or whatever
         */
    }

    private void checkCondition(GO other) {
        Type objType = Type.Error;

        this.issueEvent<RemoteGetType>( (x,y) => x.Get(out objType), other);
        if (objType != Type.Player)
            return;

        this.getAnimator();
        if (this.animator != null)
            this.animator.SetTrigger(GoalBlock.trigger);

        /* Halt player movement */
        this.issueEvent<SetOnGoal>( (x,y) => x.OnGoal(), other);
        this.showWinScreen();
    }

    public void OnAnimationFinished() {
        this.rootEvent<LoaderEvents>( (x,y) => x.NextLevel() );
    }

    public void JustDropped(GO other) {
        this.checkCondition(other);
    }

    private void onCollisionUp(bool enter, RelPos p, GO other) {
        bool onLedge = false;


        this.issueEvent<OnLedgeDetector>( (x,y) => x.Check(out onLedge), other);
        if (enter && !onLedge)
            this.checkCondition(other);
    }
}
