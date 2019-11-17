using EvSys = UnityEngine.EventSystems;

public interface RemoteGetType : EvSys.IEventSystemHandler {
    void Get(out GetType.Type type);
}

public class GetType : UnityEngine.MonoBehaviour, RemoteGetType {
    public enum Type {
        Error = 0,
        None,
        IceBlock,
        Followable,
        Minion,
        Player,
        Goal,
    }

    public Type type = Type.None;

    public void Get(out Type outType) {
        outType = this.type;
    }
}
