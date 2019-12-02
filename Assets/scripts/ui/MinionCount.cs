using EvSys = UnityEngine.EventSystems;
using UiText = UnityEngine.UI.Text;

public interface MinionCountIface : EvSys.IEventSystemHandler {
    void GetText(out UiText text);
}

public class MinionCount : UnityEngine.MonoBehaviour, MinionCountIface {
    public void GetText(out UiText text) {
        text = this.gameObject.GetComponent<UiText>();
    }
}
