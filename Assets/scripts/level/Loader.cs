using AsyncOp = UnityEngine.AsyncOperation;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using Obj = UnityEngine.Object;
using Quat = UnityEngine.Quaternion;
using Scene = UnityEngine.SceneManagement.Scene;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;
using UiText = UnityEngine.UI.Text;
using Vec3 = UnityEngine.Vector3;

public interface LoaderEvents : EvSys.IEventSystemHandler {
    /**
     * Set the position of a given checkpoint within a scene.
     *
     * @param idx Index of the checkpoint
     * @param pos Position of the checkpoint
     */
    void SetCheckpointPosition(int idx, Vec3 pos);

    /**
     * Set the index of the currently active checkpoint.
     *
     * @param idx Index of the checkpoint
     */
    void SetActiveCheckpoint(int idx);

    /**
     * Retrieve how many checkpoint there currently are in the scene, increasing
     * the count afterwards.
     */
    void GetCheckpointCount(out int count);

    /** Reload the current level */
    void ReloadLevel();

    /** Load the next level */
    void NextLevel();

    /** Increase the number of minions on the current scene */
    void IncreaseMaxMinion();

    /**
     * Signal that another minion reached the goal.
     *
     * @param done Whether all minions have been saved (i.e., level may finish).
     */
    void SavedMinion(out bool done);
}

public interface GetPlayer : EvSys.IEventSystemHandler {
    /**
     * Retrieve the scene's player.
     *
     * @param player The instanced player (if any)
     */
    void Get(out GO player);
}

public class Loader : BaseRemoteAction, LoaderEvents, GetPlayer {
    /** XXX: The first scene in the game **must** be the mainmenu, while the
     * second one is the first stage...
     * To reset the game back to the first stage, this must be manually
     * assigned. */
    static public int currentLevel = 1;
    /** Track the currently active checkpoint within the scene */
    static public int checkpoint = 0;

    /** Tag of UI elements for displaying a level's name */
    private const string titleTag = "Title";
    /** Name of the sub-scene used to display the loading progress */
    private const string uiScene = "LoadingUI";
    /** Name of the sub-scene used to display the game UI */
    private const string gameUiScene = "GameUI";
    /** Name of the sub-scene used to display the pause UI */
    private const string pauseUiScene = "Pause";

    /** Scene with components related to displaying the pause menu */
    private Scene pauseUi;
    /** Scene with components related to displaying the loading progress */
    private Scene loadingUi;
    /** UI progress bar */
    private ProgressBar pb;
    /** Name of the loader scene */
    private int loaderScene;
    /** Text displaying the minion count */
    private UiText minionCountTxt;

    /** How many checkpoints this scene has (increase by an event) */
    private int checkpointCount;
    /** How many minions there are in this scene */
    private int minionCount;
    /** How many minions the player has found/saved */
    private int minionSaved;
    /** The instanced player */
    private GO playerInstance;

    private bool resetting;
    private bool doReset;
    private bool done;
    private bool loadingPause;
    private bool didSpawnPlayer;

    /** The player prefab */
    public GO player;

    void Start() {
        this.resetting = false;
        this.doReset = false;
        this.done = false;
        this.loadingPause = false;
        this.didSpawnPlayer = false;
        this.loaderScene = SceneMng.GetActiveScene().buildIndex;
        /* Starts at 1 since there's always the player position */
        this.checkpointCount = 1;
        this.minionCount = 0;
        this.minionSaved = 0;
        this.minionCountTxt = null;

        BaseRemoteAction.root = this.gameObject;

        this.StartCoroutine(this.load());
    }

    private string getLevelName(string sep) {
        string levelName = LevelNameList.GetLevel(currentLevel);
        return $"Level {currentLevel}{sep}{levelName}";
    }

    private void getUiComponents(Scene scene, SceneMode mode) {
        string sceneName = this.getLevelName("\n");

        /* Retrieve all components from the loading scene */
        this.loadingUi = scene;
        foreach (GO go in scene.GetRootGameObjects()) {
            if (this.pb == null)
                this.pb = go.GetComponentInChildren<ProgressBar>();
            /* Set flavor text */
            foreach (UiText txt in go.GetComponentsInChildren<UiText>())
                if (txt.tag == Loader.titleTag)
                    txt.text = sceneName;
        }
    }

    private void updateMinionText() {
        if (this.minionCountTxt != null) {
            /* For reference, this '$' denotes a "intepolated string" */
            this.minionCountTxt.text = $"{this.minionSaved:D2} / {this.minionCount:D2}";
        }
    }

    private void setupGameUI(Scene scene, SceneMode mode) {
        string title = this.getLevelName(": ");
        foreach (GO go in scene.GetRootGameObjects()) {
            UiText[] txts = go.GetComponentsInChildren<UiText>();
            foreach (UiText txt in txts) {
                this.issueEvent<MinionCountIface>(
                        (x,y) => x.GetText(out this.minionCountTxt), txt.gameObject);
                if (this.minionCountTxt == null) {
                    /* Not the minion count text, therefore(?) the title */
                    txt.text = title;
                }
                else if (this.minionCount == 0) {
                    /* No minions in this level, just hide the display */
                    UnityEngine.Transform txtT = this.minionCountTxt.transform;
                    GO txtParent = txtT.parent.gameObject;
                    txtParent.SetActive(false);
                }
                else {
                    /* This level has minions! Update the text */
                    this.updateMinionText();
                }
            }
        }
    }

    private System.Collections.IEnumerator load() {
        /* Retrieve all components from the loading scene */
        do {
            AsyncOp op;

            SceneMng.sceneLoaded += getUiComponents;
            this.pb = null;

            op = SceneMng.LoadSceneAsync(Loader.uiScene, SceneMode.Additive);
            yield return op;

            SceneMng.sceneLoaded -= getUiComponents;
        } while (false);

        do {
            AsyncOp op;

            op = SceneMng.LoadSceneAsync(currentLevel, SceneMode.Additive);
            while (op.progress < 1.0f) {
                /* Update a progress bar */
                if (this.pb != null)
                    this.pb.progress = op.progress * 0.95f;
                yield return new UnityEngine.WaitForFixedUpdate();
            }
        } while (false);

        /* Wait another frame, so this may get the position of every
         * checkpoint */
        yield return null;

        SceneMng.UnloadSceneAsync(this.loadingUi);

        do {
            AsyncOp op;

            SceneMng.sceneLoaded += setupGameUI;
            op = SceneMng.LoadSceneAsync(Loader.gameUiScene, SceneMode.Additive);
            yield return op;
            SceneMng.sceneLoaded -= setupGameUI;
        } while (false);

        this.done = true;
    }

    public void SetCheckpointPosition(int idx, Vec3 pos) {
        if (idx == checkpoint && !this.didSpawnPlayer) {
            this.playerInstance = Obj.Instantiate(this.player, pos,
                    Quat.identity);
            this.didSpawnPlayer = true;
        }
    }

    public void SetActiveCheckpoint(int idx) {
        checkpoint = idx;
    }

    public void GetCheckpointCount(out int count) {
        count = this.checkpointCount;
        this.checkpointCount++;
    }

    private void reload() {
        SceneMng.LoadSceneAsync(this.loaderScene, SceneMode.Single);
        this.resetting = true;
    }

    public void ReloadLevel() {
        this.reload();
    }

    public void NextLevel() {
        currentLevel++;
        checkpoint = 0;
        this.reload();
    }

    public void IncreaseMaxMinion() {
        this.minionCount++;
    }

    public void SavedMinion(out bool done) {
        this.minionSaved++;
        this.updateMinionText();
        done = (this.minionSaved >= this.minionCount);
    }

    public void Get(out GO player) {
        player = this.playerInstance;
    }

    private void onLoadPause(Scene scene, SceneMode mode) {
        Pause.scene = scene;
        this.pauseUi = scene;
        this.loadingPause = false;
        SceneMng.sceneLoaded -= onLoadPause;
    }

    void Update() {
        if (!this.resetting && this.doReset || Input.GetResetButton()) {
            if (!this.doReset && !this.done) {
                this.doReset = true;
                /* TODO: Send in-game warning */
            }
            else if (this.done && !this.resetting) {
                this.reload();
            }
        }

        if (this.done && !this.loadingPause && !this.pauseUi.isLoaded &&
                Input.GetPauseJustPressed()) {
            SceneMng.sceneLoaded += onLoadPause;
            this.loadingPause = true;
            SceneMng.LoadSceneAsync(Loader.pauseUiScene,
                    SceneMode.Additive);
        }
    }

    static public void LoadLevel(int idx) {
        Loader.currentLevel = idx;
        Loader.checkpoint = 0;
        SceneMng.LoadSceneAsync("Loader", SceneMode.Single);
    }
}
