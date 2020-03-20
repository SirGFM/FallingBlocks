using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;

public class AsyncAddLoadOnClick : UnityEngine.MonoBehaviour {
    private bool isLoading = false;

    public string scene = "";

    private System.Collections.IEnumerator load() {
        yield return SceneMng.LoadSceneAsync(this.scene, SceneMode.Additive);
        this.isLoading = false;
    }

    public void Load() {
        if (this.isLoading)
            return;

        this.isLoading = true;
        this.StartCoroutine(this.load());
    }
}
