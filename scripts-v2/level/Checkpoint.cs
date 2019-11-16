using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;
using Type = GetType.Type;

public class Checkpoint : InitialPlayerPosition {
    override protected void start() {
        base.start();
        this.setupCollision( (x, y, z) => this.onCollisionDown(x, y, z) );
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

    void OnTriggerEnter(UnityEngine.Collider c) {
        GO obj = c.gameObject;
        Type other = Type.Error;

        this.issueEvent<RemoteGetType>( (x,y) => x.Get(out other), obj);
        if (other == Type.Player) {
            this.rootEvent<LoaderEvents>(
                    (x,y) => x.SetActiveCheckpoint(this.checkPointIdx) );
            this.destroy();
        }
    }
}
