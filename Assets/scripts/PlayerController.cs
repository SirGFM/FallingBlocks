using RelPos = ReportRelativeCollision.RelativePosition;

public class PlayerController : UnityEngine.MonoBehaviour, OnRelativeCollisionEvent {
    /** List of directions the player may face (in camera space) */
    private enum Direction {
        none  = 0x0,
        back  = 0x1, /* Camera facing */
        front = 0x2,
        left  = 0x4,
        right = 0x8
    };

    /** Currently facing direction */
    private Direction facing = Direction.back;
    /** Tracks whether we are already running a coroutine */
    private bool isMoving;

    /** Keep track of collisions on the object's surroundings */
    private int[] collisionTracker;
    /** Reference to the object's rigid body */
    private UnityEngine.Rigidbody rb;

    /** How long to delay movement after a turn */
    public float TurnDelay = 0.3f;
    /** How long move a square takes */
    public float MoveDelay = 0.6f;

    // Start is called before the first frame update
    void Start() {
        this.facing = Direction.back;
        this.transform.eulerAngles = new UnityEngine.Vector3(0f, 0f, 0f);

        this.rb = this.GetComponent<UnityEngine.Rigidbody>();
        if (this.rb == null)
            this.rb = this.gameObject.AddComponent<UnityEngine.Rigidbody>();
        this.rb.isKinematic = true;
        this.rb.useGravity = false;
        this.rb.constraints = UnityEngine.RigidbodyConstraints.FreezeRotationX | UnityEngine.RigidbodyConstraints.FreezeRotationZ;

        RelPos p = 0;
        this.collisionTracker = new int[p.count()];

        this.isMoving = false;
    }

    /**
     * Retrieve the current input direction, if any.
     */
    private Direction getInputDirection() {
        float tmp = UnityEngine.Input.GetAxisRaw("Horizontal");
        if (tmp > 0.5)
            return Direction.right;
        else if (tmp < -0.5)
            return Direction.left;
        tmp = UnityEngine.Input.GetAxisRaw("Vertical");
        if (tmp > 0.5)
            return Direction.front;
        else if (tmp < -0.5)
            return Direction.back;
        return Direction.none;
    }

    /**
     * Move the player to a new position.
     */
    private System.Collections.IEnumerator move(UnityEngine.Vector3 tgtPosition) {
        this.isMoving = true;

        int steps = (int)(this.MoveDelay / UnityEngine.Time.fixedDeltaTime);
        UnityEngine.Vector3 dtMovement = tgtPosition / (float)steps;
        UnityEngine.Vector3 finalPosition = this.transform.localPosition + tgtPosition;

        for (int i = 0; i < steps; i++) {
            this.transform.localPosition = this.transform.localPosition + dtMovement;
            //this.transform.Rotate(axis, dtAngle * (i / (float)steps) * 2f);
            yield return new UnityEngine.WaitForFixedUpdate();
        }
        this.transform.localPosition = finalPosition;

        this.isMoving = false;
    }

    private void tryMoveForward() {
        UnityEngine.Vector3 tgtPosition;

        switch (this.facing) {
        case Direction.back: /* Camera facing */
            tgtPosition = new UnityEngine.Vector3(0f, 0f, -1f);
            break;
        case Direction.front:
            tgtPosition = new UnityEngine.Vector3(0f, 0f, 1f);
            break;
        case Direction.left:
            tgtPosition = new UnityEngine.Vector3(-1f, 0f, 0f);
            break;
        case Direction.right:
            tgtPosition = new UnityEngine.Vector3(1f, 0f, 0f);
            break;
        default:
            tgtPosition = new UnityEngine.Vector3(0f, 0f, 0f);
            break;
        }

        /* Compound the movement by looking at the surroundings */
        if (this.collisionTracker[RelPos.Front.toIdx()] > 0) {
            /* Something ahead; Try to jump up */
            if (this.collisionTracker[RelPos.TopFront.toIdx()] == 0) {
                /* There's a floor above; Jump toward it */
                tgtPosition.y = 1f;
                this.StartCoroutine(this.move(tgtPosition));
            }
        }
        else {
            if (this.collisionTracker[RelPos.BottomFront.toIdx()] > 0)
                /* Front is clear and there's footing; Just move forward */
                this.StartCoroutine(this.move(tgtPosition));
            else if (this.collisionTracker[RelPos.BottomBottomFront.toIdx()] > 0) {
                /* There's a floor bellow; Jump toward it */
                tgtPosition.y = -1f;
                this.StartCoroutine(this.move(tgtPosition));
            }
        }
    }

    /**
     * Retrieve the next vertical position aligned to the grid.
     */
    private float getGridAlignedY() {
        return (float)System.Math.Floor(this.transform.localPosition.y);
    }

    /**
     * Animate falling until there's a block bellow
     */
    private System.Collections.IEnumerator fall() {
        UnityEngine.Vector3 tmp;
        float newY;

        this.isMoving = true;
        this.rb.isKinematic = false;
        this.rb.useGravity = true;

        while (this.collisionTracker[RelPos.Bottom.toIdx()] == 0)
            yield return new UnityEngine.WaitForFixedUpdate();

        /* Align to the grid */
        newY = this.getGridAlignedY();
        while (this.transform.localPosition.y > newY)
            yield return new UnityEngine.WaitForFixedUpdate();

        tmp = this.transform.localPosition;
        this.transform.localPosition = new UnityEngine.Vector3(tmp.x, newY, tmp.z);

        this.rb.isKinematic = true;
        this.rb.useGravity = false;
        this.isMoving = false;
    }

    /**
     * Animate the rotation from the current orientation to 'd'.
     */
    private System.Collections.IEnumerator turn(Direction d) {
        float tgtAngle, dtAngle;
        int steps;

        this.isMoving = true;

        switch ((int)this.facing | ((int)d << 4)) {
        case (int)Direction.back | ((int)Direction.front << 4):
            tgtAngle = 180f;
            dtAngle = 180f;
            break;
        case (int)Direction.back | ((int)Direction.left << 4):
            tgtAngle = 90f;
            dtAngle = 90f;
            break;
        case (int)Direction.back | ((int)Direction.right << 4):
            tgtAngle = -90f;
            dtAngle = -90f;
            break;
        case (int)Direction.front | ((int)Direction.back << 4):
            tgtAngle = 0f;
            dtAngle = 180f;
            break;
        case (int)Direction.front | ((int)Direction.left << 4):
            tgtAngle = 90f;
            dtAngle = -90f;
            break;
        case (int)Direction.front | ((int)Direction.right << 4):
            tgtAngle = -90f;
            dtAngle = 90f;
            break;
        case (int)Direction.left | ((int)Direction.front << 4):
            tgtAngle = 180f;
            dtAngle = 90f;
            break;
        case (int)Direction.left | ((int)Direction.back << 4):
            tgtAngle = 0f;
            dtAngle = -90f;
            break;
        case (int)Direction.left | ((int)Direction.right << 4):
            tgtAngle = -90f;
            dtAngle = 180f;
            break;
        case (int)Direction.right | ((int)Direction.front << 4):
            tgtAngle = 180f;
            dtAngle = -90f;
            break;
        case (int)Direction.right | ((int)Direction.back << 4):
            tgtAngle = 0f;
            dtAngle = 90f;
            break;
        case (int)Direction.right | ((int)Direction.left << 4):
            tgtAngle = 90f;
            dtAngle = 180f;
            break;
        default:
            tgtAngle = this.transform.eulerAngles.y;
            dtAngle = 0f;
            break;
        }
        steps = (int)(this.TurnDelay / UnityEngine.Time.fixedDeltaTime);
        dtAngle /= (float)steps;

        UnityEngine.Vector3 axis = new UnityEngine.Vector3(0, 1, 0);
        for (int i = 0; i < steps; i++) {
            this.transform.Rotate(axis, dtAngle * (i / (float)steps) * 2f);
            yield return new UnityEngine.WaitForFixedUpdate();
        }

        UnityEngine.Vector3 tmp = this.transform.eulerAngles;
        this.transform.eulerAngles = new UnityEngine.Vector3(tmp.x, tgtAngle, tmp.z);
        this.facing = d;
        /* If still holding on the same direction, buffer a movement */
        if (d == this.getInputDirection())
            this.tryMoveForward();
        else
            this.isMoving = false;
    }

    // Update is called once per frame
    void Update() {
        if (this.isMoving)
            /* Ignore inputs unless stopped */
            return;
        else if (this.collisionTracker[RelPos.Bottom.toIdx()] == 0)
            /* Start falling if there's nothing bellow */
            this.StartCoroutine(this.fall());

        Direction newDir = this.getInputDirection();
        if (newDir != Direction.none)
            if (this.facing != newDir)
                this.StartCoroutine(this.turn(newDir));
            else
                this.tryMoveForward();
    }

    public void OnEnterRelativeCollision(RelPos p, UnityEngine.Collider c) {
        this.collisionTracker[p.toIdx()]++;
    }

    public void OnExitRelativeCollision(RelPos p, UnityEngine.Collider c) {
        this.collisionTracker[p.toIdx()]--;
    }
}
