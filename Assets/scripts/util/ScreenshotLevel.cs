using AsyncOp = UnityEngine.AsyncOperation;
using TexBuffer = UnityEngine.RenderTexture;
using Camera = UnityEngine.Camera;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using Scene = UnityEngine.SceneManagement.Scene;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;

using RTF = UnityEngine.RenderTextureFormat;
using RTRW = UnityEngine.RenderTextureReadWrite;
using FilterMode = UnityEngine.FilterMode;
using TextureWrapMode = UnityEngine.TextureWrapMode;
using Material = UnityEngine.Material;
using Shader = UnityEngine.Shader;

public interface ScreenshotLevelEvents : EvSys.IEventSystemHandler {
    void TakeSS(GO caller, int sceneIdx);
}

public interface ScreenshotLevelController : EvSys.IEventSystemHandler {
    void OnSSTaken(TexBuffer tex, Material mat);
}

public class ScreenshotLevel : BaseRemoteAction, ScreenshotLevelEvents {
    public int thumbWidth = 256;
    public int thumbHeight = 256;
    public Shader shader;

    private const int depth = 16;
    private const RTF fmt = RTF.ARGB32;
    private const RTRW texMode = RTRW.Default;
    private const FilterMode filterMode = FilterMode.Bilinear;
	private const int anisoLevel = 1;
	private const int antiAliasing = 1;
	private const TextureWrapMode wrapMode = TextureWrapMode.Clamp;

    private Camera bbCamera;
    private bool running;

    void Start() {
        this.bbCamera = this.gameObject.AddComponent<Camera>();
        this.bbCamera.depth = -1;
        this.bbCamera.enabled = false;
        this.running = false;
    }

    private void sceneLoaded(Scene scene, SceneMode mode) {
        float x = 0.0f;
        float y = 0.0f;
        int count = 0;

        foreach (GO go in scene.GetRootGameObjects()) {
            UnityEngine.Vector3 pos = go.transform.position;
            x += pos.x;
            y += pos.y;
            count++;
        }
        x /= (float)count;
        y /= (float)count;

        this.transform.position = new UnityEngine.Vector3(x, y, -5.0f);
    }

    private System.Collections.IEnumerator _takeSS(GO caller, int sceneIdx) {
        AsyncOp op;

        SceneMng.sceneLoaded += this.sceneLoaded;
        op = SceneMng.LoadSceneAsync(sceneIdx, SceneMode.Additive);
        yield return op;
        SceneMng.sceneLoaded -= this.sceneLoaded;

        TexBuffer tb = new TexBuffer(this.thumbWidth, this.thumbHeight,
                ScreenshotLevel.depth, ScreenshotLevel.fmt,
                ScreenshotLevel.texMode);
        tb.name = $"{sceneIdx}_screenshot.tex";
        tb.filterMode = ScreenshotLevel.filterMode;
		tb.anisoLevel = ScreenshotLevel.anisoLevel;
		tb.antiAliasing = ScreenshotLevel.antiAliasing;
		tb.wrapMode = ScreenshotLevel.wrapMode;
		tb.depth = ScreenshotLevel.depth;
		tb.Create();

        this.bbCamera.targetTexture = tb;
        this.bbCamera.enabled = true;
        yield return null;

        Material mat = new Material(this.shader);
		mat.mainTexture = tb;
        mat.name = $"{sceneIdx}_screenshot.mat";

        this.bbCamera.enabled = false;
        this.bbCamera.targetTexture = null;

        op = SceneMng.UnloadSceneAsync(sceneIdx);
        yield return op;

        this.issueEvent<ScreenshotLevelController>(
                (x, y) => x.OnSSTaken(tb,  mat), caller);

        this.running = false;
    }

    public void TakeSS(GO caller, int sceneIdx) {
        if (!this.running) {
            this.running = true;
            this.StartCoroutine(this._takeSS(caller, sceneIdx));
        }
    }
}
