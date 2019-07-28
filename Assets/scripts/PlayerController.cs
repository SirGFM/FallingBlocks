using Coll = System.Collections.Generic;
using Dir = Movement.Direction;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using Layer = UnityEngine.LayerMask;
using Math = UnityEngine.Mathf;
using Phy = UnityEngine.Physics;
using RelPos = ReportRelativeCollision.RelativePosition;
using Vec3 = UnityEngine.Vector3;

public class PlayerController : UnityEngine.MonoBehaviour, OnRelativeCollisionEvent, iTiledMoved, iTurned, iDetectFall {
    private enum Animation {
        None  = 0x00,
        Stand = 0x01,
        Turn  = 0x02,
        Move  = 0x04,
        Fall  = 0x08,
        Push  = 0x10,
    };

    /** Currently facing direction */
    private Dir facing = Dir.back;
    /** Tracks whether we are already running a coroutine */
    private Animation anim;
    /** Whether we are currently holding onto an ledge */
    private bool onLedge;
    /** Keep track of collisions on the object's surroundings */
    private int[] collisionTracker;
    /** Block right in front of the player (in local space), that may be moved */
    private UnityEngine.GameObject frontBlock;
    /** Number of blocks currently being pushed */
    private int pushing;
    /** Layer hit by raycasting, used while detecting all adjacent blocks */
    private int rayLayer;
    /** Maximum distance for raycasting for adjacent blocks */
    private const float maxPushDistance = Math.Infinity;

    public float MoveDelay = 0.4f;

    // Start is called before the first frame update
    void Start() {
        this.facing = Dir.back;
        this.transform.eulerAngles = new UnityEngine.Vector3(0f, 0f, 0f);

        RelPos p = 0;
        this.collisionTracker = new int[p.count()];

        this.anim = Animation.None;
        this.onLedge = false;
        this.pushing = 0;

        this.rayLayer = Layer.GetMask("Game Model");
    }

    private bool shouldHoldBlock() {
        return UnityEngine.Input.GetAxisRaw("Action") > 0.5;
    }

    private GO[] getSortedBlocksInFront() {
        UnityEngine.RaycastHit[] objs;
        Coll.SortedList<float, GO> list;
        Vec3 origin, direction;
        GO[] ret;
        /** Access a Vec3 x, y, z component using 0, 1, 2 respectively */
        int vec;
        /** Offset when iterating through the list */
        int off;
        /** Number of sequential items in the list */
        int len;

        list = new Coll.SortedList<float, GO>();
        origin = this.transform.position;
        direction = new Vec3();

        switch (this.facing) {
        case Dir.back:
            vec = 2;
            direction.z = -1.0f;
            break;
        case Dir.left:
            vec = 0;
            direction.x = -1.0f;
            break;
        case Dir.front:
            vec = 2;
            direction.z = 1.0f;
            break;
        case Dir.right:
            vec = 0;
            direction.x = 1.0f;
            break;
        default:
            throw new System.Exception("Not facing any direction: *PANIC*");
        }

        /* At the very least, there must be one object in front of the player.
         * Retrieve and sort every object found. */
        objs = Phy.RaycastAll(origin, direction,
                PlayerController.maxPushDistance, this.rayLayer);
        foreach (UnityEngine.RaycastHit obj in objs)
            list.Add(obj.transform.position[vec], obj.transform.gameObject);

        /* Count how many objects are linned sequentially (from either side of
         * the list) */
        len = 1;
        if (this.frontBlock == list.Values[0])
            off = 0;
        else
            off = list.Count;

        for (int i = 1; i < list.Count; i++) {
            int idx = Math.Abs(off - i);
            float v1 = list.Values[idx].transform.position[vec];
            float v2 = list.Values[idx - 1].transform.position[vec];
            if (1.0f != Math.Abs(v1 - v2))
                break;
            len++;
        }


        /* Retrieve every object (in sequence) */
        if (this.frontBlock != list.Values[0])
            off = list.Count - 1;

        ret = new GO[len];
        for (int i = 0; i < len; i++)
            ret[i] = list.Values[Math.Abs(off - i)];

        return ret;
    }

    private void tryPushBlock(Dir movingDir) {
        /* Check get the block in front of the player, or any adjacent */
        if (this.frontBlock == null) {
            /* If there's no block adjacent to the player, try to rotate to the
             * closest one and stop */
            Dir turnDir = Dir.none;
            if (this.collisionTracker[RelPos.Front.toIdx()] > 0)
                turnDir = Dir.front.toLocal(this.facing);
            else if (this.collisionTracker[RelPos.Right.toIdx()] > 0)
                turnDir = Dir.right.toLocal(this.facing);
            else if (this.collisionTracker[RelPos.Left.toIdx()] > 0)
                turnDir = Dir.left.toLocal(this.facing);
            else if (this.collisionTracker[RelPos.Back.toIdx()] > 0)
                turnDir = Dir.back.toLocal(this.facing);

            if (turnDir != Dir.none) {
                float newAngle = 0.0f;
                switch (turnDir) {
                case Dir.front:
                    newAngle = 180.0f;
                    break;
                case Dir.right:
                    newAngle = -90.0f;
                    break;
                case Dir.left:
                    newAngle = 90.0f;
                    break;
                case Dir.back:
                    newAngle = 0.0f;
                    break;
                }
                Vec3 tmp = this.transform.eulerAngles;
                this.transform.eulerAngles = new Vec3(tmp.x, newAngle, tmp.z);
                this.facing = turnDir;
            }

            return;
        }

        Dir back = this.facing.rotateClockWise().rotateClockWise();
        if (movingDir == this.facing) {
            /* Push the block, using the slowest speed */
            this.anim |= Animation.Push;
            GO[] list = getSortedBlocksInFront();
            float speed = 0.0f;
            foreach (GO go in list) {
                BlockMovement bm = go.GetComponent<BlockMovement>();
                speed = Math.Max(speed, bm.MoveDelay);
            }

            foreach (GO go in list) {
                EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                        go, null, (x,y)=>x.Move(this.facing, this.gameObject, speed));
            }
        }
        else if (this.collisionTracker[RelPos.Back.toIdx()] == 0 &&
                movingDir == back) {
            /* Pull the block */
            this.anim |= Animation.Push;
            BlockMovement bm = this.frontBlock.GetComponent<BlockMovement>();
            EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                    this.gameObject, null, (x,y)=>x.Move(back, this.gameObject, bm.MoveDelay));
            EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                    this.frontBlock, null, (x,y)=>x.Move(back, this.gameObject, bm.MoveDelay));
        }
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
                        this.gameObject, null, (x,y)=>x.Move(d, this.gameObject, this.MoveDelay));
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
                        this.gameObject, null, (x,y)=>x.Move(d, this.gameObject, this.MoveDelay));
            }
            else if (isOuter &&
                    this.collisionTracker[(RelPos.FrontTopSomething | dir).toIdx()] == 0 &&
                    this.collisionTracker[(RelPos.Top | dir).toIdx()] == 0) {
                Dir d = moveDir.toLocal(this.facing) | Dir.front.toLocal(this.facing);
                EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                        this.gameObject, null, (x,y)=>x.Move(d, this.gameObject, this.MoveDelay));

                EvSys.ExecuteEvents.ExecuteHierarchy<iTurning>(
                        this.gameObject, null, (x,y)=>x.Turn(this.facing, outerTurn, this.gameObject));
            }
            else if (isInner)
                EvSys.ExecuteEvents.ExecuteHierarchy<iTurning>(
                        this.gameObject, null, (x,y)=>x.Turn(this.facing, innerTurn, this.gameObject));
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
                        this.gameObject, null, (x,y)=>x.Move(d, this.gameObject, this.MoveDelay));
            }
        }
        else {
            if (this.collisionTracker[RelPos.BottomFront.toIdx()] > 0)
                /* Front is clear and there's footing; Just move forward */
                EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                        this.gameObject, null, (x,y)=>x.Move(this.facing, this.gameObject, this.MoveDelay));
            else if (this.collisionTracker[RelPos.BottomBottomFront.toIdx()] > 0) {
                /* There's a floor bellow; Jump toward it */
                Dir d = this.facing | Dir.bottom;
                EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                        this.gameObject, null, (x,y)=>x.Move(d, this.gameObject, this.MoveDelay));
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
                        this.gameObject, null, (x,y)=>x.Move(d, this.gameObject, this.MoveDelay));
                EvSys.ExecuteEvents.ExecuteHierarchy<iTurning>(
                        this.gameObject, null, (x,y)=>x.Turn(this.facing, newDir, this.gameObject));
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
            if (this.collisionTracker[RelPos.Bottom.toIdx()] != 0 &&
                    this.shouldHoldBlock())
                this.tryPushBlock(newDir);
            else
                this.tryMoveLedge(newDir);
        else if (this.collisionTracker[RelPos.Bottom.toIdx()] == 0)
            /* Start falling if there's nothing bellow */
            EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                    this.gameObject, null, (x,y)=>x.Fall(this.gameObject));
        else if (this.shouldHoldBlock())
            this.tryPushBlock(newDir);
        else if (newDir != Dir.none)
            if (this.facing != newDir)
                EvSys.ExecuteEvents.ExecuteHierarchy<iTurning>(
                        this.gameObject, null, (x,y)=>x.Turn(this.facing, newDir, this.gameObject));
            else
                this.tryMoveForward();
    }

    public void OnEnterRelativeCollision(RelPos p, UnityEngine.Collider c) {
        int idx = p.toIdx();
        this.collisionTracker[idx]++;
        if (p == RelPos.Bottom) {
            EvSys.ExecuteEvents.ExecuteHierarchy<ActivateOnTop>(
                    c.gameObject, null, (x,y)=>x.OnEnterTop(this.gameObject));
            /* Stops falling if there's anything bellow the player */
            if (this.collisionTracker[idx] == 1 &&
                    (this.anim & Animation.Fall) == Animation.Fall)
                EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                        this.gameObject, null, (x,y)=>x.Halt(this.gameObject));
        }
        else if (p == RelPos.Front)
            this.frontBlock = c.gameObject;
    }

    public void OnExitRelativeCollision(RelPos p, UnityEngine.Collider c) {
        this.collisionTracker[p.toIdx()]--;
        if (p == RelPos.Front && this.collisionTracker[p.toIdx()] == 0)
            frontBlock = null;
        else if (p == RelPos.Bottom)
            EvSys.ExecuteEvents.ExecuteHierarchy<ActivateOnTop>(
                    c.gameObject, null, (x,y)=>x.OnLeaveTop(this.gameObject));
    }

    public void OnStartMovement(Dir d, GO callee) {
        if (callee == this.gameObject)
            this.anim |= Animation.Move;
        else
            this.pushing++;
    }

    public void checkLedgeOnBack() {
        if ((this.anim & Animation.Move) != 0 || this.onLedge ||
                this.collisionTracker[RelPos.Bottom.toIdx()] > 0)
            return;

        GO self = this.gameObject;
        this.onLedge = true;
        EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                self, null, (x,y)=>x.Move(Dir.bottom, self, this.MoveDelay));
    }

    public void OnFinishMovement(Dir d, GO callee) {
        if (callee == this.gameObject) {
            this.anim &= ~Animation.Move;
            this.checkLedgeOnBack();
        }
        else {
            this.pushing--;
            if (this.pushing == 0)
                this.anim &= ~Animation.Push;
        }
    }

    public void OnStartTurning(Dir d, GO callee) {
        this.anim |= Animation.Turn;
    }

    public void OnFinishTurning(Dir d, GO callee) {
        this.anim &= ~Animation.Turn;
        this.facing = d;
    }

    public void OnStartFalling(GO callee) {
        this.anim |= Animation.Fall;
    }

    public void OnFinishFalling(GO callee) {
        this.anim &= ~Animation.Fall;
    }
}
