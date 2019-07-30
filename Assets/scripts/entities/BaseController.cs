using EvSys = UnityEngine.EventSystems;
using RelPos = ReportRelativeCollision.RelativePosition;

public class BaseController : UnityEngine.MonoBehaviour, OnRelativeCollisionEvent {
    /** Previously started coroutine */
    private UnityEngine.Coroutine bgFunc;
    /** Keep track of collisions on the object's surroundings */
    protected int[] collisionTracker;

    protected void commonInit() {
        RelPos p = 0;
        this.collisionTracker = new int[p.count()];
    }

    virtual protected bool isFalling() {
        return false;
    }

    virtual protected bool canFall() {
        return true;
    }

    virtual protected void _onEnterRelativeCollision(RelPos p,
            UnityEngine.Collider c) { }
    virtual protected void _onExitRelativeCollision(RelPos p,
            UnityEngine.Collider c) { }

    public void OnEnterRelativeCollision(RelPos p, UnityEngine.Collider c) {
        int idx = p.toIdx();
        this.collisionTracker[idx]++;
        if (p == RelPos.Bottom) {
            EvSys.ExecuteEvents.ExecuteHierarchy<ActivateOnTop>(
                    c.gameObject, null, (x,y)=>x.OnEnterTop(this.gameObject));
            /* Stops falling if there's anything bellow the entity */
            if (this.collisionTracker[idx] == 1 && this.isFalling())
                EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                        this.gameObject, null, (x,y)=>x.Halt(this.gameObject));
        }
        this._onEnterRelativeCollision(p, c);
    }

    private System.Collections.IEnumerator tryFall() {
        while (!this.canFall()) {
            if (this.collisionTracker[RelPos.Bottom.toIdx()] > 0 ||
                    this.isFalling()) {
                this.bgFunc = null;
                yield break;
            }
            yield return new UnityEngine.WaitForFixedUpdate();
        }
        EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                this.gameObject, null, (x,y)=>x.Fall(this.gameObject));
        this.bgFunc = null;
    }

    public void OnExitRelativeCollision(RelPos p, UnityEngine.Collider c) {
        this.collisionTracker[p.toIdx()]--;
        if (p == RelPos.Bottom) {
            EvSys.ExecuteEvents.ExecuteHierarchy<ActivateOnTop>(
                    c.gameObject, null, (x,y)=>x.OnLeaveTop(this.gameObject));
            if (this.collisionTracker[p.toIdx()] == 0 && this.bgFunc == null)
                this.bgFunc = this.StartCoroutine(this.tryFall());
        }
        this._onExitRelativeCollision(p, c);
    }
}
