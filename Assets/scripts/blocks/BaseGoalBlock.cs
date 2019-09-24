using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;

public class BaseGoalBlock : UnityEngine.MonoBehaviour {
    /** Scene to be played after this one (either a level or the credits).
     * Default to the next index in Unity's build settings. */
    public string NextScene;

    protected void showWinScreen() {
        /* TODO:
         *   - Play 'you win' fanfare or whatever
         */
    }

    protected void nextStage() {
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
