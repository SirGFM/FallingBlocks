using Model = UnityEngine.MeshFilter;
using Vec3 = UnityEngine.Vector3;

public class PositionBasedModel : UnityEngine.MonoBehaviour {
    public UnityEngine.Mesh modelA;
    public UnityEngine.Mesh modelB;

    void Start() {
        Model curModel = this.gameObject.GetComponentInChildren<Model>();
        Vec3 pos = this.transform.position;
        int x = 1 & (int)pos.x;
        int y = 1 & (int)pos.y;
        int z = 1 & (int)pos.z;

        if ((z == 0 && x == y) || (z == 1 && x != y))
            curModel.mesh = modelA;
        else
            curModel.mesh = modelB;
    }
}
