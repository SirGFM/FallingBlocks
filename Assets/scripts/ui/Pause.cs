using CoroutineRet = System.Collections.IEnumerator;
using RectT = UnityEngine.RectTransform;
using Scene = UnityEngine.SceneManagement.Scene;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;
using UiText = UnityEngine.UI.Text;
using Vec3 = UnityEngine.Vector3;

public class Pause : UnityEngine.MonoBehaviour {
    static public Scene scene;
    public UiText quit;
    public UiText reset;
    public RectT quiting;

    static private string bold(string str) {
        return $"<b>{str}</b>";
    }

    static private string lightBlue(string str) {
        return $"<color=#cbdbfcff>{str}</color>";
    }

    private string getActionButtons(Input.Actions act) {
        string str = "";

        for (int i = 0; i < 3; i++) {
            string tmp = Input.AxisName(act, i);
            if (tmp.Length > 0)
                str += $" or {bold(lightBlue(tmp))}";
        }

        if (str.Length > 4)
            str = str.Substring(4);
        return str;
    }

    private CoroutineRet waitAction() {
        string str = "Quiting in ";
        UiText text = this.quiting.GetComponentInChildren<UiText>();

        for (int i = 0; i < 11 && Input.GetPauseDown(); i++) {
            quiting.localScale = new Vec3(1.0f, 0.1f * (float)i, 1.0f);
            yield return new UnityEngine.WaitForSeconds(0.01f);
            continue;
        }
        for (int i = 0; i < str.Length && Input.GetPauseDown(); i++) {
            text.text = str.Substring(0, i);
            yield return new UnityEngine.WaitForSeconds(0.025f);
            continue;
        }
        for (int j = 3; j >= 0 && Input.GetPauseDown(); j--) {
            for (int i = 0; i < 5 && Input.GetPauseDown(); i++) {
                switch (i) {
                case 0:
                    text.text = str + $"{j}";
                    break;
                case 1:
                case 2:
                case 3:
                    text.text += ".";
                    break;
                }
                yield return new UnityEngine.WaitForSeconds(0.15f);
                continue;
            }
        }

        if (Input.GetPauseDown())
            SceneMng.LoadSceneAsync(0, SceneMode.Single);
        else
            SceneMng.UnloadSceneAsync(Pause.scene);
    }

    void Update() {
        if (Input.GetPauseJustPressed()) {
            this.StartCoroutine(this.waitAction());
        }
    }

    void Start() {
        this.quiting.localScale = new Vec3(1.0f, 0, 1.0f);
        this.quit.text = $"Hold {this.getActionButtons(Input.Actions.Pause)} to quit to the Main Menu.\nTap to close this menu.";
        this.reset.text = $"Press {this.getActionButtons(Input.Actions.Reset)} to restart from the last checkpoint.";
    }
}
