using Animator = UnityEngine.Animator;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;

public class GoalBlock : BaseGoalBlock, ActivateOnTop {
    /** The animation handler */
    private Animator anim;
    /** The trigger used to start the animation */
    private const string trigger = "StartGoalAnim";
    /** Tag used to identify a player */
    private const string playerTag = "Player";

    private void getAnimator() {
        if (this.anim == null)
            this.anim = this.gameObject.GetComponentInChildren<Animator>();
    }

    void Start() {
        this.getAnimator();
    }

    public void OnEnterTop(GO other) {
        if (other.tag != GoalBlock.playerTag)
            return;
        this.getAnimator();
        this.anim.SetTrigger(GoalBlock.trigger);

        /* Halt player movement */
        EvSys.ExecuteEvents.ExecuteHierarchy<OnEntityDone>(
                other, null, (x,y)=>x.OnGoal());
        this.showWinScreen();
    }

    public void OnLeaveTop(UnityEngine.GameObject other) {
        /* Do nothing! */
    }

    public void OnAnimationFinished() {
        this.nextStage();
    }
}
