using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;

public class UnloadSelfOnClick : UnityEngine.MonoBehaviour {
    private bool isRunning = false;

    public void UnloadScene() {
        if (this.isRunning)
            return;

        this.isRunning = true;
        SceneMng.UnloadSceneAsync(this.gameObject.scene);
    }
}
