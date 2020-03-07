using GO = UnityEngine.GameObject;
using Obj = UnityEngine.Object;
using Quat = UnityEngine.Quaternion;
using Vec3 = UnityEngine.Vector3;

public class InitialPlayerPosition : BaseRemoteAction, GetPlayer {
    /** DEBUG ONLY: The player prefab */
    public GO dbgPlayer;
    /** DEBUG ONLY: The main camera */
    public GO dbgCamera;
    /** DEBUG ONLY: Instance of the player */
    private GO dbgPlayerInstance;

    public int checkPointIdx;

    void Start() {
        /* By default (for the initial position), ignore the checkpoint index */
        this.start(false);
    }

    virtual protected void start(bool getCheckpointIdx) {
        this.StartCoroutine(this.delayedSetPlayer(getCheckpointIdx));
    }

    private System.Collections.IEnumerator delayedSetPlayer(bool getCheckpointIdx) {
        Vec3 p;

        p = this.transform.position;
        float delay = p.y / 100.0f;
        yield return new UnityEngine.WaitForSeconds(delay);

        if (getCheckpointIdx)
            this.rootEvent<LoaderEvents>(
                    (x,y) => x.GetCheckpointCount(out this.checkPointIdx) );

        if (UnityEngine.Application.isEditor &&
                this.dbgCamera != null && UnityEngine.Camera.main == null) {
            /* XXX: If testing a particular scene, it won't have neither the
             * camera nor the player (as those are maintained by the loader
             * scene)... So spawn them here. */
            Obj.Instantiate(this.dbgCamera, new Vec3(), Quat.identity);
            this.dbgPlayerInstance = Obj.Instantiate(this.dbgPlayer, p,
                    Quat.identity);

            /* XXX: Set this as the root object so the camera may retrieve
             * the player */
            BaseRemoteAction.root = this.gameObject;
        }

        if (this.checkPointIdx > 0)
            p.y += 2.0f;
        this.rootEvent<LoaderEvents>(
                (x,y) => x.SetCheckpointPosition(this.checkPointIdx, p) );

        if (UnityEngine.Application.isEditor &&
                this.dbgCamera != null && UnityEngine.Camera.main == null)
            this.destroy();
    }

    private System.Collections.IEnumerator goDestroy() {
        /* TODO Play a VFX? */

        /* XXX: Forcefully move the entity away from any close entity before
         * destroying it, to avoid glitching the physics. */
        this.transform.position = new Vec3(0.0f, -10.0f, 0.0f);
        yield return new UnityEngine.WaitForFixedUpdate();
        GO.Destroy(this.gameObject);
    }

    protected void destroy() {
        this.StartCoroutine(this.goDestroy());
    }

    public void Get(out GO player) {
        player = this.dbgPlayerInstance;
    }
}
