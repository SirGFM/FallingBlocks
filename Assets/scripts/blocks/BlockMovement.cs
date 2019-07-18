public class BlockMovement : UnityEngine.MonoBehaviour, OnBlockEdge {
    /** Last touched edge (used for effects) */
    private EdgeBase.Direction lastEdge;
    /** How many edges are currently touching this block */
    private int numEdges = 0;

    /** Whether this box should stop on the next point on the grid */
    private bool haltMovement;
    /** Aligned position of the next vertical grid (assigned on collision) */
    private float alignedVerticalPosition;

    /** Reference to the object's rigid body */
    private UnityEngine.Rigidbody rb;

    /** Maximum allowed fall speed */
    public float MaxFallSpeed = -4.5f;

    /**
     * Start is called before the first frame update.
     */
    void Start() {
        this.rb = this.GetComponent<UnityEngine.Rigidbody>();
        if (this.rb == null)
            this.rb = this.gameObject.AddComponent<UnityEngine.Rigidbody>();
        this.rb.isKinematic = false;
        this.rb.useGravity = true;
        this.rb.constraints = UnityEngine.RigidbodyConstraints.FreezeRotation;

        UnityEngine.BoxCollider bc = this.gameObject.GetComponent<UnityEngine.BoxCollider>();
        if (bc == null)
            bc = this.gameObject.AddComponent<UnityEngine.BoxCollider>();
        bc.size = new UnityEngine.Vector3(0.9f, 0.9f, 0.9f);
    }

    /**
     * Retrieve the next vertical position aligned to the grid.
     */
    private float getGridAlignedY() {
        float tmp = this.transform.localPosition.y;
        return (float)System.Math.Floor(tmp);
    }

    void FixedUpdate() {
        if (this.rb.velocity.y < MaxFallSpeed)
            this.rb.velocity = new UnityEngine.Vector3(this.rb.velocity.x, MaxFallSpeed, this.rb.velocity.z);
    }

    /**
     * Update is called once per frame.
     */
    void Update() {
        /* On the first bottom collision, wait until we aligned with the
         * grid and halt. */
        if (this.haltMovement) {
            float curVerticalPosition = this.getGridAlignedY();
            if (curVerticalPosition < this.alignedVerticalPosition) {
                UnityEngine.Vector3 tmp;
                tmp = this.transform.localPosition;

                this.transform.localPosition = new UnityEngine.Vector3(tmp.x, this.alignedVerticalPosition, tmp.z);

                this.rb.isKinematic = true;
                this.rb.useGravity = false;
                this.haltMovement = false;
            }
        }
    }

    public void OnTouchEdge(EdgeBase.Direction d) {
        this.lastEdge = d;
        this.numEdges++;
        if (this.numEdges == 1) {
            /* Align the box to the grid and halt if there's at least one box bellow */
            this.haltMovement = true;
            this.alignedVerticalPosition = this.getGridAlignedY();
        }
    }

    public void OnReleaseEdge(EdgeBase.Direction d) {
        numEdges--;
        if (this.numEdges == 0) {
            /* Start physics if there isn't any box bellow */
            this.rb.isKinematic = false;
            this.rb.useGravity = true;
            this.haltMovement = false;
        }
    }
}
