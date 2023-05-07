using CoroutineRet = System.Collections.IEnumerator;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;
using Time = UnityEngine.Time;

public class Menu : BaseRemoteAction {
    public float waitRepeat = 0.5f;
    public float holdRepeat = 0.15f;

    private bool anyDirDown() {
        return Input.MenuLeft() || Input.MenuRight() || Input.MenuUp() ||
                Input.MenuDown();
    }

    virtual protected bool ignoreInputs() {
        return false;
    }

    virtual protected void onLeft() {
    }

    virtual protected void onRight() {
    }

    virtual protected void onUp() {
    }

    virtual protected void onDown() {
    }

    virtual protected void onSelect() {
    }

    virtual protected void onCancel() {
    }

    private CoroutineRet handleInputs() {
        float delay = this.waitRepeat;

#if UNITY_WEBGL
        while (Input.MenuSelect())
            yield return null;
        yield return null;
#endif

        while (true) {
            if (this.ignoreInputs()) {
                yield return null;
                continue;
            }

            if (Input.MenuSelect()) {
                Global.Sfx.playEnterMenu();
                this.onSelect();
                while (Input.MenuSelect())
                    yield return null;
            }
            else if (Input.MenuCancel()) {
                Global.Sfx.playCancelMenu();
                this.onCancel();
                while (Input.MenuCancel())
                    yield return null;
            }
            else if (anyDirDown()) {
                bool playSound = true;

                if (Input.MenuLeft())
                    this.onLeft();
                else if (Input.MenuRight())
                    this.onRight();
                else if (Input.MenuUp())
                    this.onUp();
                else if (Input.MenuDown())
                    this.onDown();
                else
                    playSound = false;

                /* Should never be false, but... */
                if (playSound)
                    Global.Sfx.playMoveMenu();

                for (float t = 0; t < delay && this.anyDirDown();
                        t += Time.deltaTime) {
                    /* Do nothing until timeout or the key is released */
                    yield return null;
                }
                delay = this.holdRepeat;
            }
            else {
                /* No key pressed:
                 *   1. Reset the delay between repeated presses
                 *   2. Try-again next frame
                 */
                delay = this.waitRepeat;
                yield return null;
            }
        }
    }

    virtual protected void start() {
    }

    void Start() {
        this.start();
        this.StartCoroutine(this.handleInputs());
    }

    private bool isLoading = false;
    protected void LoadLevel(int idx) {
        if (this.isLoading)
            return;

        this.isLoading = true;
        Loader.LoadLevel(idx);
    }

    private System.Collections.IEnumerator load(string scene, SceneMode mode) {
        yield return SceneMng.LoadSceneAsync(scene, mode);
        this.isLoading = false;
    }

    protected void CombinedLoadScene(string scene) {
        if (this.isLoading)
            return;

        this.isLoading = true;
        this.StartCoroutine(this.load(scene, SceneMode.Additive));
    }

    protected void LoadScene(string scene) {
        if (this.isLoading)
            return;

        this.isLoading = true;
        this.StartCoroutine(this.load(scene, SceneMode.Single));
    }
}
