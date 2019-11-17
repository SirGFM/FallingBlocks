using EvSys = UnityEngine.EventSystems;
using ExecEv = UnityEngine.EventSystems.ExecuteEvents;
using GO = UnityEngine.GameObject;
using Handler = UnityEngine.EventSystems.IEventSystemHandler;

public class BaseRemoteAction : UnityEngine.MonoBehaviour {
    static protected GO root = null;

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
        else
            ExecEv.ExecuteHierarchy<T>(this.gameObject, null, cb);
    }

    /**
     * Send an event to the root game object (which must be manually set).
     *
     * @param cb The event being sent
     */
    protected void rootEvent<T>(ExecEv.EventFunction<T> cb) where T : Handler {
        ExecEv.ExecuteHierarchy<T>(root, null, cb);
    }
}
