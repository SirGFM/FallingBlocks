using GO = UnityEngine.GameObject;
using Vec3 = UnityEngine.Vector3;

public class InitialPlayerPosition : BaseRemoteAction {
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
        this.rootEvent<LoaderEvents>(
                (x,y) => x.SetCheckpointPosition(this.checkPointIdx, p) );
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
