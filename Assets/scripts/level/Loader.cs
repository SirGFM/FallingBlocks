using AsyncOp = UnityEngine.AsyncOperation;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using Obj = UnityEngine.Object;
using Quat = UnityEngine.Quaternion;
using Scene = UnityEngine.SceneManagement.Scene;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;
using Vec3 = UnityEngine.Vector3;

/**
 * Events used by sub-scenes to report back their status to this scene.
 */
public interface OnSceneEvent : EvSys.IEventSystemHandler {
    /**
     * Report how much of the initialization has been done.
     *
     * @param cur The current step of the initialization (0-based)
     * @param max How many steps there are in the initialization
     */
    void OnUpdateProgress(int cur, int max);

    /**
     * Report that the scene finished loading and moving into place, and that
     * the player may be spawned.
     *
     * @param pos Initial position of the player within this sub-scene
     */
    void OnSceneReady(Vec3 pos);

    /**
     * Report that the scene is ready to be played.
     */
    void OnSceneDone();
}

public class Loader : UnityEngine.MonoBehaviour, OnSceneEvent {
    /** List of sub-scenes to be loaded by this scene */
    public string[] subSceneList;
    /** The player prefab */
    public GO player;

    /** Whether the current sub-scene has finished loading */
    private bool done;
    /** Whether the player has already been spawned in this scene */
    private bool didSpawnPlayer;

    /** Name of the sub-scene used to display the loading progress */
    private const string uiScene = "LoadingUI";
    /** Scene with components related to displaying the loading progress */
    private Scene loadingUi;
    /** UI progress bar */
    private ProgressBar pb;

    void Start() {
        Global.setup();
        this.StartCoroutine(this.load());
    }

    /**
     * Load every sub-scene in backgroud. Depends on some events getting
     * reported to properly work.
     */
    private System.Collections.IEnumerator load() {
        SceneMng.sceneLoaded += OnSceneLoaded;
        this.didSpawnPlayer = false;

        /* TODO: Use this to load from the checkpoint */
        int first = 0;

        /* Retrieve all components from the loading scene */
        do {
            AsyncOp op;

            this.pb = null;
            op = SceneMng.LoadSceneAsync(Loader.uiScene, SceneMode.Additive);
            yield return op;
        } while (false);

        for (int i = first; i < this.subSceneList.Length; i++) {
            string s = this.subSceneList[i];

            this.done = false;
            /* XXX: When done, this dispatches an OnSceneLoaded */
            AsyncOp op = SceneMng.LoadSceneAsync(s, SceneMode.Additive);
            while (op.progress < 1.0f) {
                /* Update a progress bar */
                this.pb.progress = op.progress * 0.5f;
                yield return new UnityEngine.WaitForFixedUpdate();
            }

            /* XXX: The progress should be updated from OnUpdateProgress */
            while (!this.done)
                yield return null;

            /* Remove progress bar (if active) */
            this.pb = null;
            SceneMng.UnloadSceneAsync(this.loadingUi);
        }
        SceneMng.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, SceneMode mode) {
        if (scene.name == Loader.uiScene) {
            /* Retrieve all components from the loading scene */
            this.loadingUi = scene;
            foreach (GO go in scene.GetRootGameObjects()) {
                if (this.pb == null)
                    this.pb = go.GetComponentInChildren<ProgressBar>();
                /* TODO: Get flavor text */
            }
            return;
        }

        foreach (GO go in scene.GetRootGameObjects()) {
            SpawnController sc = go.GetComponent<SpawnController>();
            if (sc == null)
                continue;
            /* XXX: When done, this dispatches an OnSceneReady and later an
             * OnSceneDone.
             * It also dispatches OnUpdateProgress every now and then. */
            sc.fixPosition(scene, this.gameObject);
        }
    }

    public void OnSceneReady(Vec3 pos) {
        if (!this.didSpawnPlayer)
            Obj.Instantiate(this.player, pos, Quat.identity);
        this.didSpawnPlayer = true;
    }

    public void OnUpdateProgress(int cur, int max) {
        if (this.pb != null)
            this.pb.progress = 0.5f + 0.5f * (cur / (float)max);
    }

    public void OnSceneDone() {
        this.done = true;
    }
}
