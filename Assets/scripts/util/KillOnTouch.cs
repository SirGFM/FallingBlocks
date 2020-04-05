using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;

public class KillOnTouch : UnityEngine.MonoBehaviour {
    public string[] killableTags;

    void OnTriggerEnter(UnityEngine.Collider c) {
        foreach (string s in this.killableTags) {
            if (c.tag == s) {
                SceneMng.LoadSceneAsync("YouLose", SceneMode.Additive);
                this.gameObject.SetActive(false);
                break;
            }
        }
    }
}
