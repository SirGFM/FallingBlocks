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

    protected int checkPointIdx;

    void Start() {
        this.start();
        this.StartCoroutine(this.delayedSetPlayer());
    }

    virtual protected void start() {
        this.rootEvent<LoaderEvents>(
                (x,y) => x.GetCheckpointCount(out this.checkPointIdx) );
    }

    private System.Collections.IEnumerator delayedSetPlayer() {
        Vec3 p;

        yield return null;

        p = this.transform.position;

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
