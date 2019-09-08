using GO = UnityEngine.GameObject;

public class Checkpoint : UnityEngine.MonoBehaviour {
    /** Tag used to identify a player */
    private const string playerTag = "Player";

    /** Set the object's parent (to avoid breaking prefabs) */
    public GO Parent;
    /** Index of the sub-scene in the loader scene */
    public int CheckpointIndex;

    void Start() {
        if (this.Parent != null)
            this.transform.SetParent(this.Parent.transform);
    }

    private System.Collections.IEnumerator destroy() {
        /* TODO Play the effects */
        yield return new UnityEngine.WaitForFixedUpdate();

        /* XXX: Forcefully move the entity away from any close entity before
         * destroying it, to avoid glitching the physics. */
        this.transform.position = new UnityEngine.Vector3(0.0f, -10.0f, 0.0f);
        yield return new UnityEngine.WaitForFixedUpdate();
        UnityEngine.GameObject.Destroy(this.gameObject);
    }

    void OnTriggerEnter(UnityEngine.Collider c) {
        if (this.CheckpointIndex != -1 && c.gameObject.tag == playerTag) {
            Global.curCheckpoint = this.CheckpointIndex;
            this.StartCoroutine(this.destroy());
            this.CheckpointIndex = -1;
        }
    }
}
