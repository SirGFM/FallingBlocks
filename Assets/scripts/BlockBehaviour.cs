/**
 * Handles edge detection and making the block fall.
 */
public class BlockBehaviour : UnityEngine.MonoBehaviour {
    /** List of collideable edges */
    private enum Direction {
        topBack = 0, /* Y = positive, Z = negative, X = zero     */
        topFront,    /* Y = positive, Z = positive, X = zero     */
        topLeft,     /* Y = positive, Z = zero,     X = negative */
        topRight,    /* Y = positive, Z = zero,     X = positive */
        bottomBack,  /* Y = negative, Z = negative, X = zero     */
        bottomFront, /* Y = negative, Z = positive, X = zero     */
        bottomLeft,  /* Y = negative, Z = zero,     X = negative */
        bottomRight, /* Y = negative, Z = zero,     X = positive */
        max
    };

    /** Reference to the object's parent rigid body */
    private UnityEngine.Rigidbody rb;
    /** Reference to the object's parent transform */
    private UnityEngine.Transform tf;

    /** Maps a SphereCollider's ID to a Direction */
    private System.Collections.Generic.Dictionary<int, Direction> id2Direction;
    /** List of boxes directly bellow this one */
    private System.Collections.Generic.List<int> bottomColliders;
    /** List of boxes directly above this one */
    private System.Collections.Generic.List<int> topColliders;
    /** Whether this box should stop on the next point on the grid */
    private bool haltMovement;
    /** Aligned position of the next vertical grid (assigned on collision) */
    private float alignedVerticalPosition;

    /** The object's dimension */
    public float Scale = 1.0f;

    /**
     * Retrieve the first component of type T in this object's ancestors.
     */
    private T GetAncestor<T>() where T : UnityEngine.Component {
        T[] ts = this.gameObject.GetComponentsInParent<T>();
        foreach (T t in ts) {
            if (t.gameObject.GetInstanceID() == this.gameObject.GetInstanceID())
                continue;
            return t;
        }
        return null;
    }

    /**
     * Start is called before the first frame update.
     */
    void Start() {
        float _scale = this.Scale * 0.5f;

        this.id2Direction = new System.Collections.Generic.Dictionary<int, Direction>((int)Direction.max);
        this.bottomColliders = new System.Collections.Generic.List<int>();
        this.topColliders = new System.Collections.Generic.List<int>();
        this.haltMovement = false;

        /* Make sure this object has every required component */
        if (this.gameObject.GetComponent<UnityEngine.Rigidbody>() == null) {
            UnityEngine.Rigidbody rb;
            rb = this.gameObject.AddComponent<UnityEngine.Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        if (this.gameObject.GetComponent<UnityEngine.BoxCollider>() == null) {
            UnityEngine.BoxCollider bc;
            bc = this.gameObject.AddComponent<UnityEngine.BoxCollider>();
            bc.size = new UnityEngine.Vector3(0.9f, 0.9f, 0.9f);
            bc.isTrigger = true;
        }
        this.rb = this.GetAncestor<UnityEngine.Rigidbody>();
        if (this.rb == null)
            UnityEngine.Debug.LogError("Missing RigidBody in parent object!");
        this.tf = this.GetAncestor<UnityEngine.Transform>();
        if (this.tf == null)
            UnityEngine.Debug.LogError("Somehow, missing transform in parent object!");

        /* Create every collider used for edge detection */
        for (Direction d = Direction.topBack; d < Direction.max; d++) {
            UnityEngine.SphereCollider sc;
            float _x, _y, _z;

            switch (d) {
            case Direction.topBack:
                _z = -1f;
                _y = 1f;
                _x = 0f;
                break;
            case Direction.topFront:
                _z = 1f;
                _y = 1f;
                _x = 0f;
                break;
            case Direction.topLeft:
                _z = 0f;
                _y = 1f;
                _x = -1f;
                break;
            case Direction.topRight:
                _z = 0f;
                _y = 1f;
                _x = 1f;
                break;
            case Direction.bottomBack:
                _z = -1f;
                _y = -1f;
                _x = 0f;
                break;
            case Direction.bottomFront:
                _z = 1f;
                _y = -1f;
                _x = 0f;
                break;
            case Direction.bottomLeft:
                _z = 0f;
                _y = -1f;
                _x = -1f;
                break;
            case Direction.bottomRight:
                _z = 0f;
                _y = -1f;
                _x = 1f;
                break;
            default:
                _z = 0f;
                _y = 0f;
                _x = 0f;
                /* TODO: Throw error */
                break;
            }

            sc = this.gameObject.AddComponent<UnityEngine.SphereCollider>();
            sc.center = new UnityEngine.Vector3(_x, _y, _z) * _scale;
            sc.isTrigger = true;
            sc.radius = 0.25f;
            id2Direction.Add(sc.GetInstanceID(), d);
        }
    }

    /**
     * Retrieve the next vertical position aligned to the grid.
     */
    private float getGridAlignedY() {
        float tmp = this.tf.localPosition.y;
        return (float)System.Math.Floor(tmp);
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
                tmp = this.tf.localPosition;

                this.tf.localPosition = new UnityEngine.Vector3(tmp.x, this.alignedVerticalPosition, tmp.z);

                this.rb.isKinematic = true;
                this.rb.useGravity = false;
                this.haltMovement = false;
            }
        }
    }

    /**
     * Check if a collider is a block and retrieve its colliding edge.
     */
    private Direction collider2Direction(UnityEngine.Collider c) {
        BlockBehaviour other;
        Direction d;

        /* XXX: Layer should ensure this is a BlockBehaviour */
        other = c.gameObject.GetComponent<BlockBehaviour>();
        if (!other.id2Direction.TryGetValue(c.GetInstanceID(), out d))
            return Direction.max;
        return d;
    }

    /**
     * Check whether the collider is above or bellow this block and retrieve
     * the list used to track objects in that position.
     */
    private System.Collections.Generic.List<int> getColliderList(UnityEngine.Collider c) {
        /* XXX: When this object collides with another's top, the own
         * object's **bottom** is colliding */
        switch (collider2Direction(c)) {
        case Direction.topBack:
        case Direction.topFront:
        case Direction.topLeft:
        case Direction.topRight:
            return this.bottomColliders;
        case Direction.bottomBack:
        case Direction.bottomFront:
        case Direction.bottomLeft:
        case Direction.bottomRight:
            return this.topColliders;
        default:
            return null;
        }
    }

    void OnTriggerEnter(UnityEngine.Collider c) {
        System.Collections.Generic.List<int> boxList;
        boxList = getColliderList(c);
        if (boxList == null || boxList.IndexOf(c.GetInstanceID()) != -1)
            return;
        boxList.Add(c.GetInstanceID());

        if (this.bottomColliders.Count == 1) {
            /* Align the box to the grid and halt if there's at least one box bellow */
            this.haltMovement = true;
            this.alignedVerticalPosition = this.getGridAlignedY();
        }
    }

    void OnTriggerExit(UnityEngine.Collider c) {
        System.Collections.Generic.List<int> boxList;
        boxList = getColliderList(c);
        if (boxList == null || boxList.IndexOf(c.GetInstanceID()) == -1)
            return;
        boxList.Remove(c.GetInstanceID());

        if (this.bottomColliders.Count == 0) {
            /* Start physics if there isn't any box bellow */
            this.rb.isKinematic = false;
            this.rb.useGravity = true;
            this.haltMovement = false;
        }
    }
}
