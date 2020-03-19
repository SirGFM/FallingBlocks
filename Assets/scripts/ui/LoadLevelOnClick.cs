using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;

public class LoadLevelOnClick : UnityEngine.MonoBehaviour {
    public int idx = -1;
    private bool isLoading = false;

    public void LoadLevel() {
        if (this.isLoading)
            return;

        this.isLoading = true;
        Loader.currentLevel = this.idx;
        SceneMng.LoadSceneAsync("Loader", SceneMode.Single);
    }
}
