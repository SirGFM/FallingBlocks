using Animator = UnityEngine.Animator;

public class GoalBlock : UnityEngine.MonoBehaviour, ActivateOnTop {
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

    public void OnEnterTop(UnityEngine.GameObject other) {
        if (other.tag != GoalBlock.playerTag)
            return;
        this.getAnimator();
        this.anim.SetTrigger(GoalBlock.trigger);
        /* TODO:
         *   - Halt player movement
         *   - Play 'you win' fanfare or whatever
         *   - Transition to the next level
         */
    }

    public void OnLeaveTop(UnityEngine.GameObject other) {
        /* Do nothing! */
    }
}
