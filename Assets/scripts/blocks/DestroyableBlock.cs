using EvSys = UnityEngine.EventSystems;

public interface iDeathOnFall : EvSys.IEventSystemHandler {
    /** Triggered whenever the entity touches the kill plane */
    void OnKillPlane();
}

public class DestroyableBlock : UnityEngine.MonoBehaviour {
    protected System.Collections.IEnumerator corDestroyBlock() {
        /* XXX: Forcefully move the entity away from any close entity before
         * destroying it, to avoid glitching the physics. */
        this.transform.position = new UnityEngine.Vector3(0.0f, -10.0f, 0.0f);
        yield return new UnityEngine.WaitForFixedUpdate();
        UnityEngine.GameObject.Destroy(this.gameObject);
    }

    protected void destroyBlock() {
        this.StartCoroutine(this.corDestroyBlock());
    }

}
