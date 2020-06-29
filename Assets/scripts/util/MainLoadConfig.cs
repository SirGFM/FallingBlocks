using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;

public class MainLoadConfig : UnityEngine.MonoBehaviour {
    void Start() {
        try {
            foreach (string arg in System.Environment.GetCommandLineArgs())
                if (arg == "--reset-config")
                    Config.reset();
        } catch (System.Exception) {
        }

        Config.load();
        SceneMng.LoadScene("scenes/MainMenu");
    }
}
