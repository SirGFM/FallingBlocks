using Color = UnityEngine.Color;
using Res = UnityEngine.Resources;
using Scene = UnityEngine.SceneManagement.Scene;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;
using UiText = UnityEngine.UI.Text;

public class DummyStart : UnityEngine.MonoBehaviour {
    private bool allowChange;
    UiText[] blink;
    public float delta = 0.75f;

    private System.Collections.IEnumerator setAllowChange() {
        foreach (UiText txt in Res.FindObjectsOfTypeAll<UiText>()) {
            if (!txt.gameObject.activeSelf) {
                txt.gameObject.SetActive(true);
                System.Array.Resize(ref this.blink, this.blink.Length + 1);
                this.blink[this.blink.Length - 1] = txt;
            }
        }

        yield return new UnityEngine.WaitForSeconds(1);
        this.allowChange = true;
    }

    void Start() {
        this.allowChange = false;
        this.blink = new UiText[0];
        this.StartCoroutine(this.setAllowChange());
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

        if (this.allowChange && Input.CheckAnyKeyDown()) {
            SceneMng.LoadSceneAsync("Loader", SceneMode.Single);
            this.allowChange = false;
        }
    }
}
