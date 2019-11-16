using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;
using Type = GetType.Type;
using Vec3 = UnityEngine.Vector3;

public class Checkpoint : BaseRemoteAction {
    private int idx;

    void Start() {
        this.setupCollision( (x, y, z) => this.onCollisionDown(x, y, z) );
        this.rootEvent<LoaderEvents>(
                (x,y) => x.GetCheckpointCount(out this.idx) );
    }

    private void setupCollision(System.Action<bool, RelPos, GO> cb) {
        System.Tuple<RelPos, System.Action<bool, RelPos, GO>> arg;
        RelPos p;

        p = RelPos.Bottom;
        arg = new System.Tuple<RelPos, System.Action<bool, RelPos, GO>>(p, cb);
        this.BroadcastMessage("SetRelativePositionCallback", arg);
    }

    private void onCollisionDown(bool enter, RelPos p, GO other) {
        if (enter && other.GetComponent<BaseBlock>() != null) {
            this.transform.SetParent(other.transform);
            this.setupCollision(null);
        }
    }

    private System.Collections.IEnumerator destroy() {
        /* TODO Play a VFX? */

        /* XXX: Forcefully move the entity away from any close entity before
         * destroying it, to avoid glitching the physics. */
        this.transform.position = new Vec3(0.0f, -10.0f, 0.0f);
        yield return new UnityEngine.WaitForFixedUpdate();
        GO.Destroy(this.gameObject);
    }

    void OnTriggerEnter(UnityEngine.Collider c) {
        GO obj = c.gameObject;
        Type other = Type.Error;

        this.issueEvent<RemoteGetType>( (x,y) => x.Get(out other), obj);
        if (other == Type.Player) {
            this.rootEvent<LoaderEvents>(
                    (x,y) => x.SetActiveCheckpoint(this.idx) );
            this.StartCoroutine(this.destroy());
        }
    }
}
