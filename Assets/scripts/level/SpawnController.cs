using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using IEnumerator = System.Collections.IEnumerator;
using Vec3 = UnityEngine.Vector3;
using Scene = UnityEngine.SceneManagement.Scene;

public class SpawnController : UnityEngine.MonoBehaviour {
    /** Tag of objects that should be vertically spaced out */
    private const string blockTag = "Block";

    /** The player starting position on the sub-scene */
    public UnityEngine.Transform startPosition;
    /** The starting position of the next sub-scene */
    public UnityEngine.Transform topRow;

    void Start() {
    }

    /**
     * Start to fix the position of every block in this scene
     */
    public void fixPosition(Scene scene, GO caller) {
        this.StartCoroutine(this._fixPosition(scene, caller));
    }

    private IEnumerator _fixPosition(Scene scene, GO caller) {
        /* Wait for every object to be properly spawned */
        yield return null;

        GO[] objs = scene.GetRootGameObjects();
        Vec3 bPos = transform.position;

        foreach (GO b in objs) {
            if (b.tag != blockTag)
                continue;
            UnityEngine.Transform t = b.transform;
            Vec3 p = t.position;
            float newY = bPos.y + p.y * 1.3f;
            t.position = new Vec3(bPos.x + p.x, newY, bPos.z + p.z);
        }

        foreach (GO b in objs) {
            if (b.tag != blockTag)
                continue;
            EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                    b, null, (x,y)=>x.Fall(b));
        }

        /** Wait some time so most blocks fall nicely in place */
        int i;
        for (i = 0; i < 12; i++) {
            EvSys.ExecuteEvents.ExecuteHierarchy<OnSceneEvent>(
                    caller, null, (x,y)=>x.OnUpdateProgress(i, 15));
            yield return new UnityEngine.WaitForSeconds(0.15f);
        }

        if (caller != null) {
            Vec3 pos = this.startPosition.position;
            EvSys.ExecuteEvents.ExecuteHierarchy<OnSceneEvent>(
                    caller, null, (x,y)=>x.OnSceneReady(pos));
        }

        for (; i < 16; i++) {
            EvSys.ExecuteEvents.ExecuteHierarchy<OnSceneEvent>(
                    caller, null, (x,y)=>x.OnUpdateProgress(i, 15));
            yield return new UnityEngine.WaitForSeconds(0.15f);
        }

        EvSys.ExecuteEvents.ExecuteHierarchy<OnSceneEvent>(
                caller, null, (x,y)=>x.OnSceneDone());
    }
}
