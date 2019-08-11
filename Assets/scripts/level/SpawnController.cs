using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using Vec3 = UnityEngine.Vector3;

public class SpawnController : UnityEngine.MonoBehaviour {
    /** Tag of objects that should be vertically spaced out */
    private const string blockTag = "Block";

    void Start() {
        this.StartCoroutine(this.fixPosition());
    }

    private System.Collections.IEnumerator fixPosition() {
        /* Wait for every object to be properly spawned */
        yield return null;

        GO[] blocks = GO.FindGameObjectsWithTag(blockTag);
        int max = blocks.Length;
        float baseY = transform.position.y;

        foreach (GO b in blocks) {
            UnityEngine.Transform t = b.transform;
            Vec3 p = t.position;
            float newY = baseY + p.y * 1.3f;
            t.position = new Vec3(p.x, newY, p.z);
        }

        foreach (GO b in blocks)
            EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                    b, null, (x,y)=>x.Fall(b));
    }
}
