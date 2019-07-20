using RelPos = ReportRelativeCollision.RelativePosition;

public class PlayerController : UnityEngine.MonoBehaviour, OnRelativeCollisionEvent {
    /** List of directions the player may face (in camera space),
     * sorted clock-wise. */
    private enum Direction {
        none  = 0x0,
        back  = 0x1, /* Camera facing */
        left  = 0x2,
        front = 0x4,
        right = 0x8
    };
    private enum Animation {
        None  = 0x0,
        Stand = 0x1,
        Turn  = 0x2,
        Move  = 0x4,
        Fall  = 0x8,
    };

    /** Currently facing direction */
    private Direction facing = Direction.back;
    /** Tracks whether we are already running a coroutine */
    private Animation anim;
    /** Whether we are currently holding onto an ledge */
    private bool onLedge;

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

        this.anim = Animation.None;
        this.onLedge = false;
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
        this.anim |= Animation.Move;

        int steps = (int)(this.MoveDelay / UnityEngine.Time.fixedDeltaTime);
        UnityEngine.Vector3 dtMovement = tgtPosition / (float)steps;
        UnityEngine.Vector3 finalPosition = this.transform.localPosition + tgtPosition;

        for (int i = 0; i < steps; i++) {
            this.transform.localPosition = this.transform.localPosition + dtMovement;
            //this.transform.Rotate(axis, dtAngle * (i / (float)steps) * 2f);
            yield return new UnityEngine.WaitForFixedUpdate();
        }
        this.transform.localPosition = finalPosition;

        this.anim &= ~Animation.Move;
    }

    private UnityEngine.Vector3 getForwardVector(Direction dir) {
        switch (dir) {
        case Direction.back: /* Camera facing */
            return new UnityEngine.Vector3(0f, 0f, -1f);
        case Direction.front:
            return new UnityEngine.Vector3(0f, 0f, 1f);
        case Direction.left:
            return new UnityEngine.Vector3(-1f, 0f, 0f);
        case Direction.right:
            return new UnityEngine.Vector3(1f, 0f, 0f);
        default:
            return new UnityEngine.Vector3(0f, 0f, 0f);
        }
    }

    private Direction localDirToGlobal(Direction dir) {
        switch (this.facing) {
        case Direction.front:
            return dir;
        case Direction.back:
            return rotateCwDirection(rotateCwDirection(dir));
        case Direction.left:
            return rotateCcwDirection(dir);
        case Direction.right:
            return rotateCwDirection(dir);
        }
        return dir;
    }

    private Direction rotateCwDirection(Direction dir) {
        int idir = (int)dir;
        if ((idir << 1) > 0xf)
            return (Direction)0x1;
        return (Direction)(idir << 1);
    }

    private Direction getCwDirection() {
        return rotateCwDirection(this.facing);
    }

    private Direction rotateCcwDirection(Direction dir) {
        int idir = (int)dir;
        if ((idir >> 1) == 0x0)
            return (Direction)0x8;
        return (Direction)(idir >> 1);
    }

    private Direction getCcwDirection() {
        return rotateCcwDirection(this.facing);
    }

    private void tryMoveLedge(Direction moveDir) {
        UnityEngine.Vector3 tgtPosition;

        switch (moveDir) {
        case Direction.front:
            /* Move up, if there's enough room */
            if (this.collisionTracker[RelPos.TopFront.toIdx()] == 0) {
                tgtPosition = this.getForwardVector(this.facing);
                tgtPosition.y = 1f;
                this.StartCoroutine(this.move(tgtPosition));
                this.onLedge = false;
            }
            break;
        case Direction.back:
            /* Simply start to fall */
            this.onLedge = false;
            break;
        case Direction.right:
        case Direction.left: {
            RelPos dir;
            Direction innerTurn, outerTurn;
            bool isWall, isOuter, isInner;

            /* XXX: C# doesn't allow fallthrough if there's code between the
             * two case (unless you use a goto)... */
            if (moveDir == Direction.right) {
                dir = RelPos.Right;
                innerTurn = this.getCwDirection();
                outerTurn = this.getCcwDirection();
            }
            else {
                dir = RelPos.Left;
                innerTurn = this.getCcwDirection();
                outerTurn = this.getCwDirection();
            }

            isWall = (this.collisionTracker[dir.toIdx()] == 0) &&
                    (this.collisionTracker[(RelPos.Front | dir).toIdx()] > 0);
            isOuter = (this.collisionTracker[dir.toIdx()] == 0) &&
                    (this.collisionTracker[(RelPos.Front | dir).toIdx()] == 0);
            isInner = (this.collisionTracker[dir.toIdx()] > 0);

            if (isWall &&
                    this.collisionTracker[(RelPos.Top | dir).toIdx()] == 0) {
                tgtPosition = this.getForwardVector(localDirToGlobal(moveDir));
                this.StartCoroutine(this.move(tgtPosition));
            }
            else if (isOuter &&
                    this.collisionTracker[(RelPos.FrontTopSomething | dir).toIdx()] == 0 &&
                    this.collisionTracker[(RelPos.Top | dir).toIdx()] == 0) {
                tgtPosition = this.getForwardVector(localDirToGlobal(moveDir));
                tgtPosition += this.getForwardVector(localDirToGlobal(Direction.front));
                this.StartCoroutine(this.move(tgtPosition));
                this.StartCoroutine(this.turn(outerTurn));
            }
            else if (isInner) {
                this.StartCoroutine(this.turn(innerTurn));
            }
        } break;
        }
    }

    private void tryMoveForward() {
        /* Avoid corner cases by checking before doing anything */
        if ((this.anim & Animation.Move) == Animation.Move)
            return;

        UnityEngine.Vector3 tgtPosition = this.getForwardVector(this.facing);

        /* Compound the movement by looking at the surroundings */
        if (this.collisionTracker[RelPos.Front.toIdx()] > 0) {
            /* Something ahead; Try to jump up */
            if (this.collisionTracker[RelPos.TopFront.toIdx()] == 0 &&
                    this.collisionTracker[RelPos.Top.toIdx()] == 0) {
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
            else {
                Direction newDir;

                /* Fall to the ledge! */
                tgtPosition.y = -1f;
                switch (this.facing) {
                case Direction.back:
                    newDir = Direction.front;
                    break;
                case Direction.front:
                    newDir = Direction.back;
                    break;
                case Direction.left:
                    newDir = Direction.right;
                    break;
                case Direction.right:
                    newDir = Direction.left;
                    break;
                default:
                    newDir = Direction.none;
                    break;
                }
                this.StartCoroutine(this.turn(newDir));
                this.StartCoroutine(this.move(tgtPosition));
                this.onLedge = true;
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

        this.anim |= Animation.Fall;
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
        this.anim &= ~Animation.Fall;
    }

    /**
     * Animate the rotation from the current orientation to 'd'.
     */
    private System.Collections.IEnumerator turn(Direction d) {
        float tgtAngle, dtAngle;
        int steps;

        this.anim |= Animation.Turn;

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
        /* XXX: Wait some extra time to update the collision, otherwise next
         * frame's movement may break */
        yield return new UnityEngine.WaitForFixedUpdate();
        this.anim &= ~Animation.Turn;
    }

    // Update is called once per frame
    void Update() {
        if (this.anim != Animation.None)
            /* Ignore inputs unless stopped */
            return;

        Direction newDir = this.getInputDirection();
        if (this.onLedge)
            this.tryMoveLedge(newDir);
        else if (this.collisionTracker[RelPos.Bottom.toIdx()] == 0)
            /* Start falling if there's nothing bellow */
            this.StartCoroutine(this.fall());
        else if (newDir != Direction.none)
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
