using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;

public class MainLoadConfig : UnityEngine.MonoBehaviour {
    void Start() {
        Config.load();
        SceneMng.LoadScene("scenes/MainMenu");
    }
}
