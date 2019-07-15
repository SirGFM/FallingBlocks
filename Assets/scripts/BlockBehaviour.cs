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

    /** Reference to the object's rigid body */
    private UnityEngine.Rigidbody rb;

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
     * Start is called before the first frame update.
     */
    void Start() {
        float _scale = this.Scale * 0.5f;

        this.id2Direction = new System.Collections.Generic.Dictionary<int, Direction>((int)Direction.max);
        this.bottomColliders = new System.Collections.Generic.List<int>();
        this.topColliders = new System.Collections.Generic.List<int>();
        this.haltMovement = false;

        this.rb = this.gameObject.GetComponent<UnityEngine.Rigidbody>();
        if (this.rb == null)
            this.rb = this.gameObject.AddComponent<UnityEngine.Rigidbody>();

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
        float tmp = this.gameObject.transform.localPosition.y;
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
                tmp = this.gameObject.transform.localPosition;

                this.gameObject.transform.localPosition = new UnityEngine.Vector3(tmp.x, this.alignedVerticalPosition, tmp.z);

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

        /* Check the type to ensure it's a block */
        if (c.tag != this.tag)
            return Direction.max;
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
        /* NOTE: When this object collides with another's top, the own
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
