using Axis = UnityEngine.RectTransform.Axis;
using GO = UnityEngine.GameObject;
using Material = UnityEngine.Material;
using Obj = UnityEngine.Object;
using Quat = UnityEngine.Quaternion;
using Rect = UnityEngine.Rect;
using RectT = UnityEngine.RectTransform;
using SceneUtil = UnityEngine.SceneManagement.SceneUtility;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using TexBuffer = UnityEngine.RenderTexture;
using Vec2 = UnityEngine.Vector2;
using Vec3 = UnityEngine.Vector3;

using RawImage = UnityEngine.UI.RawImage;
using View = UnityEngine.UI.ScrollRect;
using UiText = UnityEngine.UI.Text;

public class CreateLevelSelectors : BaseRemoteAction, ScreenshotLevelController {
    const float elementHeight = 56;
    const float elementDist = 4;

    public string StopLoadingAt = "GameOver";
    private int lastIdx;
    private int curIdx;

    private View view;
    private float viewWidth;
    private float viewHeight;

    public GO target;
    public GO template;

    private System.Collections.IEnumerator startLoadLevel() {
        /* Wait until the previous event has ended */
        yield return null;

        this.issueEvent<ScreenshotLevelEvents>(
                (x, y) => x.TakeSS(this.gameObject, this.curIdx),
                this.target);
    }

    private System.Collections.IEnumerator start() {
        /* XXX: This must be delayed, otherwise unity ignores the changes to
         * the UI ¯\_(ツ)_/¯ */
        yield return null;

        int count = this.lastIdx - this.curIdx;
        float size = 1.0f / (float)count;
        this.view = this.gameObject.GetComponentInChildren<View>();

        const Axis ax = Axis.Vertical;
        this.viewWidth = this.view.content.rect.width;
        this.viewHeight = elementDist + count * (elementHeight + elementDist);
        this.view.content.SetSizeWithCurrentAnchors(ax, this.viewHeight);
        this.view.verticalScrollbar.size = size;

        yield return this.startLoadLevel();
    }

    void Start() {
        this.curIdx = 1;
        this.lastIdx = -1;

        for (int i = SceneMng.sceneCountInBuildSettings; i > 0; i--) {
            string name = SceneUtil.GetScenePathByBuildIndex(i - 1);
            if (name.IndexOf(this.StopLoadingAt) != -1) {
                this.lastIdx = i - 1;
                break;
            }
        }
        if (this.lastIdx == -1)
            throw new System.Exception("Couldn't find the last level");

        this.StartCoroutine(this.start());
    }

    public void OnSSTaken(TexBuffer tex, Material mat) {
        UnityEngine.Transform t = this.view.content.transform;
        GO lvl;
        RectT rect;
        RawImage img;
        UiText txt;
        
        lvl = Obj.Instantiate(this.template, t.position, Quat.identity, t);
        rect = lvl.GetComponent<RectT>();

        float left = 16.0f / this.viewWidth;
        float right = 1.0f - 16.0f / this.viewWidth;
        /* XXX: 1.0f == top, 0.0f == bottom */
        float bottom = elementDist;
        bottom += (this.curIdx - 1) * (elementHeight + elementDist);
        float top = bottom + elementHeight;

        bottom = 1.0f - bottom / this.viewHeight;
        top = 1.0f - top / this.viewHeight;

        rect.anchorMin = new Vec2(left, top);
        rect.anchorMax = new Vec2(right, bottom);
        rect.anchoredPosition = new Vec2(0 ,0);

        /* XXX: Unity is terrible at sending events downward... So just...
         * whatever */
        lvl.GetComponentInChildren<LoadLevelOnClick>().idx = this.curIdx;

        img = lvl.GetComponentInChildren<RawImage>();
        img.texture = tex;
        img.material = mat;
        /* TODO: Cache texture/materials */

        string levelName = LevelNameList.GetLevel(this.curIdx);
        txt = lvl.GetComponentInChildren<UiText>();
        txt.text = $"Level {this.curIdx} - {levelName}";

        this.curIdx++;
        if (this.curIdx < this.lastIdx)
            this.StartCoroutine(this.startLoadLevel());
    }
}
