using EvSys = UnityEngine.EventSystems;
using ExecEv = UnityEngine.EventSystems.ExecuteEvents;
using GO = UnityEngine.GameObject;
using Handler = UnityEngine.EventSystems.IEventSystemHandler;

public class BaseRemoteAction : UnityEngine.MonoBehaviour {
    /** Object that actually issued the event */
    protected GO caller = null;

    protected void issueEvent<T>(ExecEv.EventFunction<T> cb) where T : Handler {
        ExecEv.ExecuteHierarchy<T>(this.caller, null, cb);
        if (this.caller != this.gameObject)
            ExecEv.ExecuteHierarchy<T>(this.gameObject, null, cb);
    }
}
