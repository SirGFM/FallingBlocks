using Animator = UnityEngine.Animator;
using Coll = System.Collections.Generic;
using Dir = Movement.Direction;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using Layer = UnityEngine.LayerMask;
using Math = UnityEngine.Mathf;
using Phy = UnityEngine.Physics;
using RelPos = ReportRelativeCollision.RelativePosition;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;
using Vec3 = UnityEngine.Vector3;

public class PlayerController : BaseController, iTiledMoved, OnEntityDone {
    private const string moveAnim = "isWalking";
    private const string fallAnim = "isFalling";
    private const string forceAnim = "stopIdle";
    private const string climbAnim = "isClimbing";

    /** The animation handler */
    private Animator unityAnimator;
    /** Whether we are currently holding onto an ledge */
    private bool onLedge;
    /** Block right in front of the player (in local space), that may be moved */
    private UnityEngine.GameObject frontBlock;
    /** Number of blocks currently being pushed */
    private int pushing;
    /** Layer hit by raycasting, used while detecting all adjacent blocks */
    private int rayLayer;
    /** Maximum distance for raycasting for adjacent blocks */
    private const float maxPushDistance = Math.Infinity;
    private const string blockTag = "Block";

    private void getAnimator() {
        if (this.unityAnimator == null)
            this.unityAnimator = this.gameObject.GetComponentInChildren<Animator>();
    }

    private void resetAnimation() {
        this.getAnimator();
        this.unityAnimator.SetTrigger(PlayerController.forceAnim);
    }

    // Start is called before the first frame update
    void Start() {
        this.facing = Dir.back;
        this.transform.eulerAngles = new UnityEngine.Vector3(0f, 0f, 0f);

        this.onLedge = false;
        this.pushing = 0;
        this.allowLedgeMovement = true;

        this.rayLayer = Layer.GetMask("Game Model");

        this.getAnimator();

        this.commonInit();

        EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                this.gameObject, null, (x,y)=>x.Fall(this.gameObject));
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
        if (list.Count == 0)
            return new GO[0];
        else if (this.frontBlock == list.Values[0]) {
            off = 0;
            if (list.Values[0].tag != PlayerController.blockTag)
                return new GO[0];
        }
        else {
            off = list.Count;
            if (list.Values[off - 1].tag != PlayerController.blockTag)
                return new GO[0];
        }

        for (int i = 1; i < list.Count; i++) {
            int idx = Math.Abs(off - i);
            float v1 = list.Values[idx].transform.position[vec];
            float v2 = list.Values[idx - 1].transform.position[vec];
            if (list.Values[idx].tag != PlayerController.blockTag ||
                    list.Values[idx - 1].tag != PlayerController.blockTag)
                return new GO[0];
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

            if (turnDir != Dir.none)
                this.turn(turnDir);
            return;
        }

        Dir back = this.facing.rotateClockWise().rotateClockWise();
        if (movingDir == this.facing) {
            /* Push the block, using the slowest speed */
            GO[] list = getSortedBlocksInFront();
            if (list.Length == 0)
                return;

            float speed = 0.0f;
            foreach (GO go in list) {
                BlockMovement bm = go.GetComponent<BlockMovement>();
                if (bm.MoveDelay == 0.0f)
                    return;
                speed = Math.Max(speed, bm.MoveDelay);
            }

            this.anim |= Animation.Push;
            foreach (GO go in list) {
                EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                        go, null, (x,y)=>x.Move(this.facing, this.gameObject, speed));
            }
        }
        else if (this.collisionTracker[RelPos.Back.toIdx()] == 0 &&
                movingDir == back) {
            /* Pull the block */
            BlockMovement bm = this.frontBlock.GetComponent<BlockMovement>();
            if (bm == null)
                return;
            else if (bm.MoveDelay == 0.0f)
                return;
            this.anim |= Animation.Push;
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
            if (this.collisionTracker[RelPos.FrontTop.toIdx()] == 0) {
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
                    (this.collisionTracker[(RelPos.FrontSomething | dir).toIdx()] > 0);
            isOuter = (this.collisionTracker[dir.toIdx()] == 0) &&
                    (this.collisionTracker[(RelPos.FrontSomething | dir).toIdx()] == 0);
            isInner = (this.collisionTracker[dir.toIdx()] > 0);

            if (isWall &&
                    this.collisionTracker[(RelPos.TopSomething | dir).toIdx()] == 0) {
                Dir d = moveDir.toLocal(this.facing);
                EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                        this.gameObject, null, (x,y)=>x.Move(d, this.gameObject, this.MoveDelay));
            }
            else if (isOuter &&
                    this.collisionTracker[(RelPos.FrontTopSomething | dir).toIdx()] == 0 &&
                    this.collisionTracker[(RelPos.TopSomething | dir).toIdx()] == 0) {
                Dir d = moveDir.toLocal(this.facing) | Dir.front.toLocal(this.facing);
                EvSys.ExecuteEvents.ExecuteHierarchy<iTiledMovement>(
                        this.gameObject, null, (x,y)=>x.Move(d, this.gameObject, this.MoveDelay));

                this.turn(outerTurn);
            }
            else if (isInner)
                this.turn(innerTurn);
        } break;
        }
    }

    override protected void setOnLedge() {
        this.onLedge = true;
    }

    override public bool isOnLedge() {
        return this.onLedge;
    }

    // Update is called once per frame
    void Update() {
        this.updateAnimationState();

        if (this.anim != Animation.None)
            /* Ignore inputs unless stopped */
            return;

        Dir newDir = this.getInputDirection();
        if (this.onLedge)
            if (this.collisionTracker[RelPos.Front.toIdx()] == 0)
                /* The block in front of the player just disappeared! */
                this.onLedge = false;
            else if (this.collisionTracker[RelPos.Bottom.toIdx()] != 0 &&
                    this.shouldHoldBlock())
                this.tryPushBlock(newDir);
            else
                this.tryMoveLedge(newDir);
        else if (this.shouldHoldBlock())
            this.tryPushBlock(newDir);
        else if (newDir != Dir.none)
            if (this.facing != newDir)
                this.turn(newDir);
            else
                this.tryMoveForward();
    }

    override protected bool canFall() {
        return this.anim == Animation.None && !this.onLedge;
    }

    override protected void _onEnterRelativeCollision(RelPos p,
            UnityEngine.Collider c) {
        if (p == RelPos.Front)
            this.frontBlock = c.gameObject;
    }

    override protected void _onExitRelativeCollision(RelPos p,
            UnityEngine.Collider c) {
        if (p == RelPos.Front && this.collisionTracker[p.toIdx()] == 0)
            frontBlock = null;
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

    public void OnGoal() {
        this.anim |= Animation.Goal;
    }

    override public void onDeath() {
        /* XXX: Ugly way of reloading the current level... urk */
        int scene = SceneMng.GetActiveScene().buildIndex;
        SceneMng.LoadSceneAsync(scene, SceneMode.Single);
    }

    private bool checkState(Animation target) {
        return (this.anim & target) == target;
    }

    private void updateAnimationState() {
        this.getAnimator();

        if (this.anim != Animation.None)
            this.resetAnimation();
        this.unityAnimator.SetBool(PlayerController.moveAnim,
                this.checkState(Animation.Move));
        this.unityAnimator.SetBool(PlayerController.fallAnim,
                this.checkState(Animation.Fall));
        this.unityAnimator.SetBool(PlayerController.climbAnim,
                (this.collisionTracker[RelPos.Front.toIdx()] > 0));

        if (this.checkState(Animation.Move) &&
                (this.collisionTracker[RelPos.FrontBottom.toIdx()] == 0) &&
                (this.collisionTracker[RelPos.BottomBottomFront.toIdx()] > 0)) {
            /* Set the falling animation for moving down a floor */
            this.unityAnimator.SetBool(PlayerController.fallAnim, true);
        }
    }
}
