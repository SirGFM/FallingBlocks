using Model = UnityEngine.SkinnedMeshRenderer;

public class PlayerModel : UnityEngine.MonoBehaviour {
    /* The currently active model */
    static public int active = 0;

    /** List of possible models */
    public UnityEngine.Mesh[] Models;

    private int _curModel = -1;

    void Update() {
        if (this._curModel != active) {
            Model curModel = this.gameObject.GetComponentInChildren<Model>();
            curModel.sharedMesh = this.Models[active];
            this._curModel = active;
        }
    }
}
