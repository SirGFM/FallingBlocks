using Dir = Movement.Direction;
using EvSys = UnityEngine.EventSystems;
using RelPos = ReportRelativeCollision.RelativePosition;

public class PlayerController : UnityEngine.MonoBehaviour, OnRelativeCollisionEvent, iTiledMoved {
    private enum Animation {
        None  = 0x0,
        Stand = 0x1,
        Turn  = 0x2,
        Move  = 0x4,
        Fall  = 0x8,
    };

    /** Currently facing direction */
    private Dir facing = Dir.back;
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
        this.facing = Dir.back;
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
    private Dir getInputDirection() {
        float tmp = UnityEngine.Input.GetAxisRaw("Horizontal");
        if (tmp > 0.5)
            return Dir.right;
        else if (tmp < -0.5)
            return Dir.left;
        tmp = UnityEngine.Input.GetAxisRaw("Vertical");
        if (tmp > 0.5)
            return Dir.front;
        else if (tmp < -0.5)
            return Dir.back;
        return Dir.none;
    }

    private void tryMoveLedge(Dir moveDir) {
        switch (moveDir) {
        case Dir.front:
            /* Move up, if there's enough room */
            if (this.collisionTracker[RelPos.TopFront.toIdx()] == 0) {
                Dir d = this.facing | Dir.top;
                EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                        this.gameObject, null, (x,y)=>x.Move(d));
                this.onLedge = false;
            }
            break;
        case Dir.back:
            /* Simply start to fall */
            this.onLedge = false;
            break;
        case Dir.right:
        case Dir.left: {
            RelPos dir;
            Dir innerTurn, outerTurn;
            bool isWall, isOuter, isInner;

            /* XXX: C# doesn't allow fallthrough if there's code between the
             * two case (unless you use a goto)... */
            if (moveDir == Dir.right) {
                dir = RelPos.Right;
                innerTurn = this.facing.rotateClockWise();
                outerTurn = this.facing.rotateCounterClockWise();
            }
            else {
                dir = RelPos.Left;
                innerTurn = this.facing.rotateCounterClockWise();
                outerTurn = this.facing.rotateClockWise();
            }

            isWall = (this.collisionTracker[dir.toIdx()] == 0) &&
                    (this.collisionTracker[(RelPos.Front | dir).toIdx()] > 0);
            isOuter = (this.collisionTracker[dir.toIdx()] == 0) &&
                    (this.collisionTracker[(RelPos.Front | dir).toIdx()] == 0);
            isInner = (this.collisionTracker[dir.toIdx()] > 0);

            if (isWall &&
                    this.collisionTracker[(RelPos.Top | dir).toIdx()] == 0) {
                Dir d = moveDir.toLocal(this.facing);
                EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                        this.gameObject, null, (x,y)=>x.Move(d));
            }
            else if (isOuter &&
                    this.collisionTracker[(RelPos.FrontTopSomething | dir).toIdx()] == 0 &&
                    this.collisionTracker[(RelPos.Top | dir).toIdx()] == 0) {
                Dir d = moveDir.toLocal(this.facing) | Dir.front.toLocal(this.facing);
                EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                        this.gameObject, null, (x,y)=>x.Move(d));

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

        /* Compound the movement by looking at the surroundings */
        if (this.collisionTracker[RelPos.Front.toIdx()] > 0) {
            /* Something ahead; Try to jump up */
            if (this.collisionTracker[RelPos.TopFront.toIdx()] == 0 &&
                    this.collisionTracker[RelPos.Top.toIdx()] == 0) {
                /* There's a floor above; Jump toward it */
                Dir d = this.facing | Dir.top;
                EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                        this.gameObject, null, (x,y)=>x.Move(d));
            }
        }
        else {
            if (this.collisionTracker[RelPos.BottomFront.toIdx()] > 0)
                /* Front is clear and there's footing; Just move forward */
                EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                        this.gameObject, null, (x,y)=>x.Move(this.facing));
            else if (this.collisionTracker[RelPos.BottomBottomFront.toIdx()] > 0) {
                /* There's a floor bellow; Jump toward it */
                Dir d = this.facing | Dir.bottom;
                EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                        this.gameObject, null, (x,y)=>x.Move(d));
            }
            else {
                Dir newDir;

                /* Fall to the ledge! */
                switch (this.facing) {
                case Dir.back:
                    newDir = Dir.front;
                    break;
                case Dir.front:
                    newDir = Dir.back;
                    break;
                case Dir.left:
                    newDir = Dir.right;
                    break;
                case Dir.right:
                    newDir = Dir.left;
                    break;
                default:
                    newDir = Dir.none;
                    break;
                }

                Dir d = this.facing | Dir.bottom;
                EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                        this.gameObject, null, (x,y)=>x.Move(d));
                this.StartCoroutine(this.turn(newDir));
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
    private System.Collections.IEnumerator turn(Dir d) {
        float tgtAngle, dtAngle;
        int steps;

        this.anim |= Animation.Turn;

        switch ((int)this.facing | ((int)d << 4)) {
        case (int)Dir.back | ((int)Dir.front << 4):
            tgtAngle = 180f;
            dtAngle = 180f;
            break;
        case (int)Dir.back | ((int)Dir.left << 4):
            tgtAngle = 90f;
            dtAngle = 90f;
            break;
        case (int)Dir.back | ((int)Dir.right << 4):
            tgtAngle = -90f;
            dtAngle = -90f;
            break;
        case (int)Dir.front | ((int)Dir.back << 4):
            tgtAngle = 0f;
            dtAngle = 180f;
            break;
        case (int)Dir.front | ((int)Dir.left << 4):
            tgtAngle = 90f;
            dtAngle = -90f;
            break;
        case (int)Dir.front | ((int)Dir.right << 4):
            tgtAngle = -90f;
            dtAngle = 90f;
            break;
        case (int)Dir.left | ((int)Dir.front << 4):
            tgtAngle = 180f;
            dtAngle = 90f;
            break;
        case (int)Dir.left | ((int)Dir.back << 4):
            tgtAngle = 0f;
            dtAngle = -90f;
            break;
        case (int)Dir.left | ((int)Dir.right << 4):
            tgtAngle = -90f;
            dtAngle = 180f;
            break;
        case (int)Dir.right | ((int)Dir.front << 4):
            tgtAngle = 180f;
            dtAngle = -90f;
            break;
        case (int)Dir.right | ((int)Dir.back << 4):
            tgtAngle = 0f;
            dtAngle = 90f;
            break;
        case (int)Dir.right | ((int)Dir.left << 4):
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

        Dir newDir = this.getInputDirection();
        if (this.onLedge)
            this.tryMoveLedge(newDir);
        else if (this.collisionTracker[RelPos.Bottom.toIdx()] == 0)
            /* Start falling if there's nothing bellow */
            this.StartCoroutine(this.fall());
        else if (newDir != Dir.none)
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

    public void OnStartMovement(Dir d) {
        this.anim |= Animation.Move;
    }

    public void OnFinishMovement(Dir d) {
        this.anim &= ~Animation.Move;
    }
}
