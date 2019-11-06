using EvSys = UnityEngine.EventSystems;
using ExecEv = UnityEngine.EventSystems.ExecuteEvents;
using GO = UnityEngine.GameObject;
using Handler = UnityEngine.EventSystems.IEventSystemHandler;

public class BaseRemoteAction : UnityEngine.MonoBehaviour {

    /**
     * Send an event upwards. If no target is specified, the event is sent to
     * the object itself.
     *
     * @param cb The event being sent
     * @param customTarget the event receiver, if any
     */
    protected void issueEvent<T>(ExecEv.EventFunction<T> cb,
            GO customTarget = null) where T : Handler {
        if (customTarget != null)
            ExecEv.ExecuteHierarchy<T>(customTarget, null, cb);
        if (customTarget != this.gameObject || customTarget == null)
            ExecEv.ExecuteHierarchy<T>(this.gameObject, null, cb);
    }
}
