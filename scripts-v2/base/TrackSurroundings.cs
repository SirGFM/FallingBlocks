using EvSys = UnityEngine.EventSystems;
using Exec = UnityEngine.EventSystems.ExecuteEvents;
using GO = UnityEngine.GameObject;
using RelCol = RelativeCollision;
using RelPos = RelativeCollision.RelativePosition;

public interface iSetRelativePositionCallback : EvSys.IEventSystemHandler {
    /**
     * Set a callback for whenever a collision against a given relative
     * position is triggered.
     *
     * XXX: Since I couldn't figure out how to pass more than one parameter to
     *      GameObject.BroadcastMessage(), the argument had to be a tuple.
     *
     * @param args A tuple with two value:
     *                 -  p: The position triggering the callback
     *                 - cb: The callback:
     *                     -   bool: Whether the object entered (true) or exited
     *                     - RelPos: The position that triggered the callback
     *                     -     GO: The game object that entered/exited
     */
    void SetRelativePositionCallback(
            System.Tuple<RelPos, System.Action<bool, RelPos, GO>> arg);
}

public interface GetRelativeObject : EvSys.IEventSystemHandler {
    /**
     * Retrieve which object is at a given position, if any.
     *
     * @param g Object that triggered the collision
     * @param p The position getting checked
     */
    void GetObjectAt(out GO g, RelPos p);
}

public class NearbyObject {
    private RelPos rp;
    private GO obj;

    public NearbyObject(RelPos rp, GO obj) {
        this.rp = rp;
        this.obj = obj;
    }

    public NearbyObject() : this(0, null) {
    }

    public void empty() {
        this.rp = (RelPos)0;
        this.obj = null;
    }

    public void set(RelPos rp, GO obj) {
        this.rp = rp;
        this.obj = obj;
    }

    public bool isEqual(GO other) {
        return other == this.obj;
    }

    public GO getObject() {
        return this.obj;
    }
}

public class TrackSurroundings : UnityEngine.MonoBehaviour, OnRelativeCollisionEvent, GetRelativeObject, iSetRelativePositionCallback {
    /** List of close objects */
    private NearbyObject[] objs;
    private System.Action<bool, RelPos, GO>[] cbs;
    private System.Action<bool, RelPos, GO> undefCbs;

    void Start() {
        /* XXX: C# prohibits defining a fixed size array... */
        int count = ((RelPos)0).count();
        this.objs = new NearbyObject[count];
        for (int i = 0; i < count; i++)
            this.objs[i] = new NearbyObject();

        if (this.cbs == null)
            this.cbs = new System.Action<bool, RelPos, GO>[count];
    }

    public void OnEnterRelativeCollision(RelCol rc, UnityEngine.Collider c) {
        int idx = rc.pos.toIdx();
        if (idx >= 0) {
            this.objs[idx].set(rc.pos, c.gameObject);
            if (this.cbs[idx] != null)
                this.cbs[idx](true, rc.pos, c.gameObject);
            else if (this.undefCbs != null)
                this.undefCbs(true, rc.pos, c.gameObject);
        }
        else if (this.undefCbs != null) {
            this.undefCbs(true, rc.pos, c.gameObject);
        }
    }

    public void OnExitRelativeCollision(RelCol rc, UnityEngine.Collider c) {
        int idx = rc.pos.toIdx();
        if (idx >= 0) {
            if (this.objs[idx].isEqual(c.gameObject))
                this.objs[idx].empty();
            if (this.cbs[idx] != null)
                this.cbs[idx](false, rc.pos, c.gameObject);
            else if (this.undefCbs != null)
                this.undefCbs(false, rc.pos, c.gameObject);
        }
        else if (this.undefCbs != null) {
            this.undefCbs(false, rc.pos, c.gameObject);
        }
    }

    public void GetObjectAt(out GO g, RelPos p) {
        int idx = p.toIdx();
        if (idx >= 0)
            g = objs[idx].getObject();
        else
            g = null;
    }

    public void SetRelativePositionCallback(
            System.Tuple<RelPos, System.Action<bool, RelPos, GO>> arg) {
        RelPos p = arg.Item1;
        System.Action<bool, RelPos, GO> cb = arg.Item2;

        if (this.cbs == null) {
            int count = ((RelPos)0).count();
            this.cbs = new System.Action<bool, RelPos, GO>[count];
        }

        int idx = p.toIdx();
        if (idx >= 0)
            this.cbs[idx] = cb;
        else
            this.undefCbs = cb;
    }

    void OnDrawGizmosSelected() {
        UnityEngine.Gizmos.color = UnityEngine.Color.red;

        if (this.objs != null) {
            foreach (NearbyObject nb in this.objs) {
                if (nb.getObject() != null) {
                    UnityEngine.Gizmos.DrawWireSphere(nb.getObject().transform.position, 1.0f);
                }
            }
        }
    }
}
