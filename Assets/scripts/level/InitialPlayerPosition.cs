using GO = UnityEngine.GameObject;
using Obj = UnityEngine.Object;
using Quat = UnityEngine.Quaternion;
using Vec3 = UnityEngine.Vector3;

public class InitialPlayerPosition : BaseRemoteAction {
    /** DEBUG ONLY:The player prefab */
    public GO dbgPlayer;
    /** DEBUG ONLY:The main camera */
    public GO dbgCamera;

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
                UnityEngine.Camera.main == null) {
            /* XXX: If testing a particular scene, it won't have neither the
             * camera nor the player (as those are maintained by the loader
             * scene)... So spawn them here. */
            Obj.Instantiate(this.dbgCamera, new Vec3(), Quat.identity);
            Obj.Instantiate(this.dbgPlayer, p, Quat.identity);
        }

        this.rootEvent<LoaderEvents>(
                (x,y) => x.SetCheckpointPosition(this.checkPointIdx, p) );

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
}
