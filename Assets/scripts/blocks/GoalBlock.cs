using Animator = UnityEngine.Animator;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;

public class GoalBlock : UnityEngine.MonoBehaviour, ActivateOnTop {
    /** The animation handler */
    private Animator anim;
    /** The trigger used to start the animation */
    private const string trigger = "StartGoalAnim";
    /** Tag used to identify a player */
    private const string playerTag = "Player";

    /** Scene to be played after this one (either a level or the credits).
     * Default to the next index in Unity's build settings. */
    public string NextScene;

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
        /* TODO:
         *   - Play 'you win' fanfare or whatever
         */
    }

    public void OnLeaveTop(UnityEngine.GameObject other) {
        /* Do nothing! */
    }

    public void OnAnimationFinished() {
        /* Transition to the next level */
        Global.curCheckpoint = 0;
        if (this.NextScene != "")
            SceneMng.LoadSceneAsync(this.NextScene, SceneMode.Single);
        else {
            int idx = SceneMng.GetActiveScene().buildIndex + 1;
            SceneMng.LoadSceneAsync(idx, SceneMode.Single);
        }
    }
}
