using Dir = Movement.Direction;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;

public class BlockMovement : UnityEngine.MonoBehaviour, OnBlockEdge, iTiledMoved, iDetectFall {
    /** Last touched edge (used for effects) */
    private EdgeBase.Direction lastEdge;
    /** How many edges are currently touching this block */
    private int numEdges = 0;
    /** Reference to the object's faller */
    private Faller fall;

    /** How long moving a tile takes */
    public float MoveDelay = 0.6f;

    /**
     * Start is called before the first frame update.
     */
    void Start() {
        UnityEngine.Rigidbody rb = this.GetComponent<UnityEngine.Rigidbody>();
        if (rb == null)
            rb = this.gameObject.AddComponent<UnityEngine.Rigidbody>();

        UnityEngine.BoxCollider bc = this.gameObject.GetComponent<UnityEngine.BoxCollider>();
        if (bc == null)
            bc = this.gameObject.AddComponent<UnityEngine.BoxCollider>();
        bc.size = new UnityEngine.Vector3(0.9f, 0.9f, 0.9f);

        this.fall = this.gameObject.GetComponent<Faller>();
        if (this.fall == null)
            throw new System.Exception("Faller not found in BlockMovement");
    }

    public void OnTouchEdge(EdgeBase.Direction d) {
        this.lastEdge = d;
        this.numEdges++;
        if (this.numEdges == 1)
            /* Align the box to the grid and halt if there's at least one box bellow */
            EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                    this.gameObject, null, (x,y)=>x.Halt(this.gameObject));
    }

    public void OnReleaseEdge(EdgeBase.Direction d) {
        numEdges--;
        if (this.numEdges == 0)
            /* Start physics if there isn't any box bellow */
            EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                    this.gameObject, null, (x,y)=>x.Fall(this.gameObject));
    }

    public void OnStartMovement(Dir d, GO callee) {
        this.fall.block();
    }

    public void OnFinishMovement(Dir d, GO callee) {
        this.fall.unblock();
    }

    public void OnStartFalling(GO callee) {
    }

    public void OnFinishFalling(GO callee) {
    }
}
