using GO = UnityEngine.GameObject;
using RelPos = RelativeCollision.RelativePosition;
using Type = GetType.Type;

public class Checkpoint : InitialPlayerPosition {
    override protected void start(bool getCheckpointIdx) {
        /* Forcefully request the checkpoint index */
        base.start(true);
        this.setCollisionCb(RelPos.Bottom,
                (x, y, z) => this.onCollisionDown(x, y, z) );
        this.setCollisionCb(RelPos.Center,
                (x, y, z) => this.onCollisionCenter(x, y, z) );
    }

    private void setCollisionCb(RelPos p, System.Action<bool, RelPos, GO> cb) {
        System.Tuple<RelPos, System.Action<bool, RelPos, GO>> arg;
        arg = new System.Tuple<RelPos, System.Action<bool, RelPos, GO>>(p, cb);
        this.BroadcastMessage("SetRelativePositionCallback", arg);
    }

    private void onCollisionDown(bool enter, RelPos p, GO other) {
        if (enter && other.GetComponent<BaseBlock>() != null) {
            this.transform.SetParent(other.transform);
            this.setCollisionCb(RelPos.Bottom, null);
        }
    }

    private void onCollisionCenter(bool enter, RelPos p, GO other) {
        Type type = Type.Error;

        this.issueEvent<RemoteGetType>( (x,y) => x.Get(out type), other);
        if (enter && type == Type.Player) {
            Global.Sfx.playCheckpoint(this.transform);
            this.rootEvent<LoaderEvents>(
                    (x,y) => x.SetActiveCheckpoint(this.checkPointIdx) );
            this.destroy();
            this.setCollisionCb(RelPos.Center, null);
        }
    }
}
