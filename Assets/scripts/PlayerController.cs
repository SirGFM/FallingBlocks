using Dir = Movement.Direction;
using EvSys = UnityEngine.EventSystems;
using RelPos = ReportRelativeCollision.RelativePosition;

public class PlayerController : UnityEngine.MonoBehaviour, OnRelativeCollisionEvent, iTiledMoved, iTurned, iDetectFall {
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

    // Start is called before the first frame update
    void Start() {
        this.facing = Dir.back;
        this.transform.eulerAngles = new UnityEngine.Vector3(0f, 0f, 0f);

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

                EvSys.ExecuteEvents.ExecuteHierarchy<iTurning>(
                        this.gameObject, null, (x,y)=>x.Turn(this.facing, outerTurn));
            }
            else if (isInner)
                EvSys.ExecuteEvents.ExecuteHierarchy<iTurning>(
                        this.gameObject, null, (x,y)=>x.Turn(this.facing, innerTurn));
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
                EvSys.ExecuteEvents.ExecuteHierarchy<iTurning>(
                        this.gameObject, null, (x,y)=>x.Turn(this.facing, newDir));
                this.onLedge = true;
            }
        }
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
            EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                    this.gameObject, null, (x,y)=>x.Fall());
        else if (newDir != Dir.none)
            if (this.facing != newDir)
                EvSys.ExecuteEvents.ExecuteHierarchy<iTurning>(
                        this.gameObject, null, (x,y)=>x.Turn(this.facing, newDir));
            else
                this.tryMoveForward();
    }

    public void OnEnterRelativeCollision(RelPos p, UnityEngine.Collider c) {
        int idx = p.toIdx();
        this.collisionTracker[idx]++;
        if (p == RelPos.Bottom && this.collisionTracker[idx] == 1 &&
                (this.anim & Animation.Fall) == Animation.Fall)
            EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                    this.gameObject, null, (x,y)=>x.Halt());
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

    public void OnStartTurning(Dir d) {
        this.anim |= Animation.Turn;
    }

    public void OnFinishTurning(Dir d) {
        this.anim &= ~Animation.Turn;
        this.facing = d;
    }

    public void OnStartFalling() {
        this.anim |= Animation.Fall;
    }

    public void OnFinishFalling() {
        this.anim &= ~Animation.Fall;
    }
}
