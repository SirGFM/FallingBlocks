using Animator = UnityEngine.Animator;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;
using Type = GetType.Type;
using Vec3 = UnityEngine.Vector3;

public interface Goal : EvSys.IEventSystemHandler {
    void AnimationFinished();
}

public class GoalBlock : BaseBlock, LedgeTracker, Goal {
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
        cb = (x, y, z) => this.onCollisionUp(x, y, z);
        this.setCollisionCb(RelPos.Top, cb);
    }

    private void showWinScreen() {
        SceneMng.LoadSceneAsync("YouWin", SceneMode.Additive);
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

    public void AnimationFinished() {
        // XXX: Do nothing as we wait for the UI to finish and change levels
        //this.rootEvent<LoaderEvents>( (x,y) => x.NextLevel() );
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
