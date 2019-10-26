﻿using AsyncOp = UnityEngine.AsyncOperation;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using Input = UnityEngine.Input;
using Obj = UnityEngine.Object;
using Quat = UnityEngine.Quaternion;
using Scene = UnityEngine.SceneManagement.Scene;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;
using UiText = UnityEngine.UI.Text;
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
    /** The name of the level */
    public string title;

    /** Whether the current sub-scene has finished loading */
    private int done;
    /** Whether the player has already been spawned in this scene */
    private bool didSpawnPlayer;
    /** Name of the level's main scene (that loads everything) */
    private int mainScene;
    /** Whether the level is already resetting */
    private bool resetting;
    /** Whether reset has been pressed while loading */
    private bool doReset;

    /** Tag of UI elements for displaying a level's name */
    private const string titleTag = "Title";
    /** Name of the sub-scene used to display the loading progress */
    private const string uiScene = "LoadingUI";
    /** Scene with components related to displaying the loading progress */
    private Scene loadingUi;
    /** UI progress bar */
    private ProgressBar pb;
    /** Start position of the next sub-scene */
    private Vec3 nextBaseY;

    void Start() {
        Global.setup();
        MinionController.reset();
        this.mainScene = SceneMng.GetActiveScene().buildIndex;
        this.resetting = false;
        this.doReset = false;
        this.done = 0;
        this.StartCoroutine(this.load());
        this.nextBaseY = new Vec3(0.0f, 1.0f, 0.0f);
    }

    /**
     * Load every sub-scene in backgroud. Depends on some events getting
     * reported to properly work.
     */
    private System.Collections.IEnumerator load() {
        SceneMng.sceneLoaded += OnSceneLoaded;
        this.didSpawnPlayer = false;

        int first = Global.curCheckpoint;

        /* Retrieve all components from the loading scene */
        do {
            AsyncOp op;

            this.pb = null;
            op = SceneMng.LoadSceneAsync(Loader.uiScene, SceneMode.Additive);
            yield return op;
        } while (false);

        this.done = first;
        for (int i = first; i < this.subSceneList.Length; i++) {
            string s = this.subSceneList[i];

            /* XXX: When done, this dispatches an OnSceneLoaded */
            AsyncOp op = SceneMng.LoadSceneAsync(s, SceneMode.Additive);
            while (op.progress < 1.0f) {
                /* Update a progress bar */
                if (this.pb != null)
                    this.pb.progress = op.progress * 0.5f;
                yield return new UnityEngine.WaitForFixedUpdate();
            }

            /* XXX: The progress should be updated from OnUpdateProgress */
            while (this.done == i)
                yield return null;

            /* Remove progress bar (if active) */
            if (this.pb != null) {
                this.pb = null;
                SceneMng.UnloadSceneAsync(this.loadingUi);
            }
        }
        SceneMng.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, SceneMode mode) {
        if (scene.name == Loader.uiScene) {
            int idx = SceneMng.GetActiveScene().buildIndex;
            string levelName = $"Level {idx}\n{this.title}";

            /* Retrieve all components from the loading scene */
            this.loadingUi = scene;
            foreach (GO go in scene.GetRootGameObjects()) {
                if (this.pb == null)
                    this.pb = go.GetComponentInChildren<ProgressBar>();
                /* Set flavor text */
                foreach (UiText txt in go.GetComponentsInChildren<UiText>())
                    if (txt.tag == Loader.titleTag)
                        txt.text = levelName;
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
            sc.transform.position = this.nextBaseY;
            sc.fixPosition(scene, this.gameObject);
            if (sc.topRow != null)
                this.nextBaseY += sc.topRow.position;
            break;
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
        this.done++;
    }

    void Update() {
        if (!this.resetting && this.doReset || Input.GetAxisRaw("Reset") > 0.5f) {
            if (!this.doReset && this.done < this.subSceneList.Length) {
                this.doReset = true;
                /* TODO: Send in-game warning */
            }
            else if (this.done >= this.subSceneList.Length && !this.resetting) {
                SceneMng.LoadSceneAsync(this.mainScene, SceneMode.Single);
                this.resetting = true;
            }
        }
    }
}
