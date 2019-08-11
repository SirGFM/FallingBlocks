using Model = UnityEngine.MeshFilter;

public class RandomModel : UnityEngine.MonoBehaviour {
    /** List of possible models */
    public UnityEngine.Mesh[] Models;

    void Start() {
        Model curModel = this.gameObject.GetComponentInChildren<Model>();

        Global.setup();
        int idx = Global.PRNG.fastRange(0, Models.Length - 1);

        curModel.mesh = Models[idx];
    }
}
