using AsyncOp = UnityEngine.AsyncOperation;
using Color = UnityEngine.Color;
using GO = UnityEngine.GameObject;
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
    UiText[] blink;

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
        foreach (GO go in scenes[mainIdx].GetRootGameObjects()) {
            go.SetActive(true);
        }

        /* Make sure every text is visible */
        yield return new UnityEngine.WaitForSeconds(5);
        foreach (UiText txt in Res.FindObjectsOfTypeAll<UiText>()) {
            if (!txt.gameObject.activeSelf) {
                txt.gameObject.SetActive(true);
                System.Array.Resize(ref this.blink, this.blink.Length + 1);
                this.blink[this.blink.Length - 1] = txt;
            }
        }

        this.allowReset = true;
    }

    void Start() {
        this.allowReset = false;
        this.blink = new UiText[0];
        this.StartCoroutine(this.makeSceneUnique());
    }

    void Update() {
        bool swap = false;
        float dv = this.delta * UnityEngine.Time.deltaTime;
        Color deltaColor = new Color(0.0f, 0.0f, 0.0f, dv);

        foreach (UiText txt in this.blink) {
            if (txt.color.a + dv > 1.0f)
                swap = true;
            else if (txt.color.a + dv < 0.0f)
                swap = true;
            else 
                txt.color = txt.color + deltaColor;
        }

        if (swap)
            this.delta *= -1.0f;

        if (this.allowReset && Input.CheckAnyKeyDown()) {
            SceneMng.LoadSceneAsync(this.MainMenuSceneName, SceneMode.Single);
            this.allowReset = false;
        }
    }
}
