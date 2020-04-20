using Axis = UnityEngine.RectTransform.Axis;
using Color = UnityEngine.Color;
using GO = UnityEngine.GameObject;
using Image = UnityEngine.UI.Image;
using Material = UnityEngine.Material;
using Obj = UnityEngine.Object;
using Quat = UnityEngine.Quaternion;
using RawImage = UnityEngine.UI.RawImage;
using SceneUtil = UnityEngine.SceneManagement.SceneUtility;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using TexBuffer = UnityEngine.RenderTexture;
using Transform = UnityEngine.Transform;
using UiText = UnityEngine.UI.Text;
using UiTransform = UnityEngine.RectTransform;
using Vec2 = UnityEngine.Vector2;

public class LevelSelectMenu : Menu, ScreenshotLevelController {
    private struct CachedLevel {
        public TexBuffer tex;
        public Material mat;
        public string name;
    };
    static private CachedLevel[] cache = null;

    public UiText LevelTitle = null;
    public RawImage LevelPortrait = null;
    public UiTransform Content = null;
    public Image LevelSelector = null;

    public UiTransform RowView = null;
    public Color CurrentRowColor = Color.white;
    public Color OtherRowsColor = Color.gray;
    private Image[] rowSelector;
    private RawImage[] thumbnails;

    public int ThumbSize = 24;
    public int ThumbBorder = 10;
    public int ThumbSpacing = 4;

    public string StopLoadingAt = "GameOver";
    private int lastIdx;
    private int curIdx;

    private int numCols;
    private int numRows;
    private int numItems;
    private int selectedItem;

    private void listLevels() {
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

        if (LevelSelectMenu.cache == null) {
            cache = new CachedLevel[this.lastIdx-1];
            for (int i = 0; i < LevelSelectMenu.cache.Length; i++) {
                LevelSelectMenu.cache[i].tex = null;
                LevelSelectMenu.cache[i].mat = null;
                LevelSelectMenu.cache[i].name = LevelNameList.GetLevel(i+1);
            }
        }

        this.numItems = LevelSelectMenu.cache.Length;
    }

    static private int getNumSlots(int space, int border, int dist, int size) {
        return (space - 2 * border + dist) /  (size + dist);
    }

    static private GO spawnCopy(GO template, Transform parent,
            float x, float y, float w, float h) {
        GO newGo;
        UiTransform rect;
        const Axis hor = Axis.Horizontal;
        const Axis vert = Axis.Vertical;

        newGo = Obj.Instantiate(template, parent.position, Quat.identity,
                parent);

        rect = newGo.AddComponent<UiTransform>();
        rect.pivot = new Vec2(0.0f, 1.0f);
        rect.anchorMin = new Vec2(0.0f, 1.0f);
        rect.anchorMax = new Vec2(0.0f, 1.0f);
        rect.anchoredPosition = new Vec2(x, -y);
        rect.SetSizeWithCurrentAnchors(vert, h);
        rect.SetSizeWithCurrentAnchors(hor, w);

        return newGo;
    }

    private void setupUi() {
        Transform t;
        UiTransform parent;
        GO template;
        GO thumbTemplate;
        float rowOffset;
        int w, h;
        const Axis vert = Axis.Vertical;

        parent = this.Content.parent.GetComponent<UiTransform>();

        w = (int)this.Content.rect.width;
        this.numCols = LevelSelectMenu.getNumSlots(w, this.ThumbBorder,
                this.ThumbSpacing, this.ThumbSize);

        this.numRows = this.numItems / this.numCols;
        if (this.numItems % this.numCols != 0)
            this.numRows++;

        h = this.numRows * (this.ThumbSpacing + this.ThumbSize);
        h += this.ThumbBorder * 2 - this.ThumbSpacing;
        this.Content.SetSizeWithCurrentAnchors(vert, h);

        this.rowSelector = new Image[this.numRows];
        template = new GO($"RowSelector");
        this.thumbnails = new RawImage[this.numItems];
        thumbTemplate = new GO($"Thumbnail");
        t = this.RowView.transform;
        w = (int)(this.RowView.rect.width / 2);
        h = (int)(this.RowView.rect.height / this.numRows);
        rowOffset = h * 0.05f;
        for (int y = 0; y < this.numRows; y++) {
            GO newGo;
            Image img;
            float _x = w * 0.25f;
            float _y = (float)(y * h) - rowOffset;
            float _w = w * 0.75f;
            float _h = h * 0.9f;

            newGo = LevelSelectMenu.spawnCopy(template, t, _x, _y, _w, _h);
            img = newGo.AddComponent<Image>();
            img.color = this.OtherRowsColor;
            this.rowSelector[y] = img;

            for (int x = 0; x < this.numCols; x++) {
                RawImage rimg;
                Transform _t = this.Content.transform;
                int i = x + y * this.numCols;

                if (i > this.numItems)
                    break;

                _x = this.ThumbBorder + x * (this.ThumbSize + this.ThumbSpacing);
                _y = this.ThumbBorder + y * (this.ThumbSize + this.ThumbSpacing);
                _w = this.ThumbSize;
                _h = this.ThumbSize;

                newGo = LevelSelectMenu.spawnCopy(thumbTemplate, _t,
                        _x, _y, _w, _h);
                rimg = newGo.AddComponent<RawImage>();
                rimg.texture = LevelSelectMenu.cache[i].tex;
                rimg.material = LevelSelectMenu.cache[i].mat;
                this.thumbnails[i] = rimg;
            }
        }
        GO.Destroy(template);
        GO.Destroy(thumbTemplate);
    }

    override protected void onLeft() {
        this.selectedItem--;
        if (this.selectedItem < 0 ||
                (this.selectedItem % this.numCols) == (this.numCols - 1)) {
            this.selectedItem += this.numCols;
            /* Adjust the position, in case the last row isn't full */
            if (this.selectedItem >= this.numItems)
                this.selectedItem = this.numItems - 1;
        }
        this.setSelectorPosition();
    }

    override protected void onRight() {
        this.selectedItem++;
        if (this.selectedItem % this.numCols == 0)
            this.selectedItem -= this.numCols;
        /* Adjust the position, in case the last row isn't full */
        else if (this.selectedItem >= this.numItems)
            this.selectedItem = this.numItems - (this.numItems % this.numCols);
        this.setSelectorPosition();
    }

    override protected void onUp() {
        this.selectedItem -= this.numCols;
        if (this.selectedItem < 0) {
            this.selectedItem += this.numCols * this.numRows;
            /* Adjust the position, in case the last row isn't full */
            if (this.selectedItem >= this.numItems)
                this.selectedItem -= this.numCols;
        }
        this.setSelectorPosition();
    }

    override protected void onDown() {
        this.selectedItem += this.numCols;
        if (this.selectedItem >= this.numItems) {
            this.selectedItem -= this.numCols * this.numRows;
            /* Adjust the position, in case the last row isn't full */
            if (this.selectedItem < 0)
                this.selectedItem += this.numCols;
        }
        this.setSelectorPosition();
    }

    private System.Collections.IEnumerator waitSSDone() {
        while (this.takingSS)
            yield return null;
    }

    private System.Collections.IEnumerator loadNewLevel() {
        yield return this.waitSSDone();
        this.LoadLevel(this.selectedItem + 1);
    }

    private System.Collections.IEnumerator loadNewScene(string scene) {
        yield return this.waitSSDone();
        this.LoadScene(scene);
    }

    private bool pressedInput = false;
    override protected void onSelect() {
        if (this.pressedInput)
            return;

        this.pressedInput = true;
        this.StartCoroutine(this.loadNewLevel());
    }

    override protected void onCancel() {
        if (this.pressedInput)
            return;

        this.pressedInput = true;
        this.StartCoroutine(this.loadNewScene("scenes/MainMenu"));
    }

    private void setPreview() {
        int i = this.selectedItem;
        this.LevelTitle.text = LevelSelectMenu.cache[i].name;
        this.LevelPortrait.texture = LevelSelectMenu.cache[i].tex;
        this.LevelPortrait.material = LevelSelectMenu.cache[i].mat;
    }

    private void setSelectorPosition() {
        UiTransform parent;
        int x = this.selectedItem % this.numCols;
        int iy = this.selectedItem / this.numCols;
        int y;

        for (int i = 0; i < this.numRows; i++) {
            this.rowSelector[i].color = this.OtherRowsColor;
        }
        this.rowSelector[iy].color = this.CurrentRowColor;

        x = this.ThumbBorder + x * (this.ThumbSize + this.ThumbSpacing);
        y = this.ThumbBorder + iy * (this.ThumbSize + this.ThumbSpacing);

        UiTransform rect = this.LevelSelector.rectTransform;
        rect.anchoredPosition = new Vec2((float)x, -(float)y);

        /* Set viewport */
        parent = this.Content.parent.GetComponent<UiTransform>();
        if (y < Content.anchoredPosition.y) {
            Vec2 pos;

            y = 0;
            if (iy > 0)
                y = iy * this.ThumbSize + (iy - 1) * this.ThumbSpacing;

            pos = new Vec2(Content.anchoredPosition.x, (float)y);
            Content.anchoredPosition = pos;
        }
        else if (y + this.ThumbSize >
                Content.anchoredPosition.y + parent.rect.height) {
            Vec2 pos;
            int visibleRows = (int)(parent.rect.height / (this.ThumbSize + this.ThumbSpacing));

            if (iy >= visibleRows)
                iy -= visibleRows - 1;
            else
                iy = 0;

            y = iy * this.ThumbSize + (iy - 1) * this.ThumbSpacing;
            pos = new Vec2(Content.anchoredPosition.x, (float)y);
            Content.anchoredPosition = pos;
        }

        this.setPreview();
    }

    private bool takingSS = false;
    private System.Collections.IEnumerator startTakeSS() {
        /* Wait until the previous event has ended */
        yield return null;

        /* Find the first non-cached thumbnail */
        for (; this.curIdx < this.lastIdx &&
                LevelSelectMenu.cache[this.curIdx - 1].tex != null;
                this.curIdx++);

        if (!this.pressedInput && this.curIdx < this.lastIdx) {
            this.takingSS = true;
            this.issueEvent<ScreenshotLevelEvents>(
                    (x, y) => x.TakeSS(this.gameObject, this.curIdx));
        }
    }

    override protected void start() {
        this.curIdx = 1;
        this.selectedItem = 0;

        this.listLevels();
        this.setupUi();
        this.setSelectorPosition();

        /* Avoid triggering the death scene while rendering the
         * level thumbnails */
        InputControlled.LevelSelectScene = SceneMng.GetActiveScene().name;

        base.start();

        this.StartCoroutine(this.startTakeSS());
    }

    public void OnSSTaken(TexBuffer tex, Material mat) {
        RawImage img;
        int i = this.curIdx - 1;

        LevelSelectMenu.cache[i].tex = tex;
        LevelSelectMenu.cache[i].mat = mat;

        img = this.thumbnails[i];
        img.texture = LevelSelectMenu.cache[i].tex;
        img.material = LevelSelectMenu.cache[i].mat;
        if (i == this.selectedItem)
            this.setPreview();

        this.curIdx++;
        this.takingSS = false;
        if (!this.pressedInput && this.curIdx < this.lastIdx)
            this.StartCoroutine(this.startTakeSS());
    }
}
