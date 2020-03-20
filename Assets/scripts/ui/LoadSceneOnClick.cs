using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;

public class LoadSceneOnClick : UnityEngine.MonoBehaviour {
    private bool isLoading = false;

    public string scene = "";

    public void LoadLevel() {
        if (this.isLoading)
            return;

        this.isLoading = true;
        SceneMng.LoadSceneAsync(this.scene, SceneMode.Single);
    }
}
