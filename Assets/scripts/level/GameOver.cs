using AsyncOp = UnityEngine.AsyncOperation;
using Color = UnityEngine.Color;
using GO = UnityEngine.GameObject;
using UiImage = UnityEngine.UI.Image;
using Res = UnityEngine.Resources;
using Scene = UnityEngine.SceneManagement.Scene;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;
using UiText = UnityEngine.UI.Text;

public class GameOver : UnityEngine.MonoBehaviour {
    private bool allowReset;

    public string MainMenuSceneName = "MainMenu";
    public string GameOverSceneName = "GameOver";
    public float delta = 0.5f;

    public UiText[] blink;
    public UiText[] flavor;
    public string[] description;
    public UiImage fadein;

    /* Make sure this scene is unique, since the loader will actually load it
     * when in-game */
    private System.Collections.IEnumerator makeSceneUnique() {
        Scene[] scenes;
        AsyncOp op;
        int mainIdx;

        scenes = new Scene[SceneMng.sceneCount];
        mainIdx = -1;
        for (int i = 0; i < scenes.Length; i++) {
            scenes[i] = SceneMng.GetSceneAt(i);
            if (scenes[i].name == this.GameOverSceneName)
                mainIdx = i;
        }
        if (mainIdx == -1)
            throw new System.Exception($"Didn't find the expected scene ({this.GameOverSceneName})");

        for (int i = 0; i < scenes.Length; i++) {
            if (i == mainIdx)
                continue;
            op = SceneMng.UnloadSceneAsync(scenes[i]);
            yield return op;
        }

        SceneMng.SetActiveScene(scenes[mainIdx]);
        yield return SceneMng.LoadSceneAsync(
                "scenes/000-game-controller/bg-scenes/GameOverBG",
                SceneMode.Additive);
        yield return runGameOverAnim();
    }

    private System.Collections.IEnumerator runGameOverAnim() {
        /* Show the screen */
        for (float t = 1.0f; t > 0.0f; t -= UnityEngine.Time.deltaTime) {
            Color alpha = new Color(0.0f, 0.0f, 0.0f, t);
            fadein.color = alpha;
            yield return null;
        }
        fadein.gameObject.SetActive(false);

        this.allowReset = true;
        yield return new UnityEngine.WaitForSeconds(5);
        this.StartCoroutine(this.showFlavorText());
        yield return blinkText();
    }

    private void setTextAlpha(UiText[] list, float alpha) {
        foreach (UiText txt in list) {
            Color src = txt.color;
            Color color = new Color(src.r, src.g, src.b, alpha);
            txt.color = color;
        }
    }

    private System.Collections.IEnumerator blinkText() {
        while (true) {
            for (float t = 0.0f; t < 1.0f; t += UnityEngine.Time.deltaTime) {
                this.setTextAlpha(this.blink, t);
                yield return null;
            }
            for (float t = 1.0f; t > 0.0f; t -= UnityEngine.Time.deltaTime) {
                this.setTextAlpha(this.blink, t);
                yield return null;
            }
        }
    }

    private void setFlavor(int idx, int len) {
        foreach (UiText txt in this.flavor) {
            txt.text = this.description[idx].Substring(0, len);
        }
    }

    private System.Collections.IEnumerator showFlavorText() {
        yield return new UnityEngine.WaitForSeconds(2);

        for (int i = 0; i < this.description.Length; i++) {
            for (int len = 0; len <= this.description[i].Length; len++) {
                this.setFlavor(i, len);
                yield return null;
            }

            float time = 6.0f;
            for (float t = time; t > 0.0f; t -= UnityEngine.Time.deltaTime) {
                this.setTextAlpha(this.flavor, t / time);
                yield return null;
            }
            this.setFlavor(0, 0);
            this.setTextAlpha(this.flavor, 1.0f);
        }
    }

    void Start() {
        this.allowReset = false;
        this.StartCoroutine(this.makeSceneUnique());
    }

    void Update() {
        if (this.allowReset && Input.CheckAnyKeyDown()) {
            SceneMng.LoadSceneAsync(this.MainMenuSceneName, SceneMode.Single);
            this.allowReset = false;
        }
    }
}
