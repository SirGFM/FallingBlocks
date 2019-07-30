﻿using UEColl = UnityEngine.Collider;
using Dir = Movement.Direction;
using EvSys = UnityEngine.EventSystems;
using RelPos = ReportRelativeCollision.RelativePosition;

public class BaseController : UnityEngine.MonoBehaviour, OnRelativeCollisionEvent {
    /** Previously started coroutine */
    private UnityEngine.Coroutine bgFunc;
    /** Keep track of collisions on the object's surroundings */
    protected int[] collisionTracker;
    /** Whether this entity can fall to a ledge */
    protected bool allowLedgeMovement;
    /** Currently facing direction */
    protected Dir facing = Dir.back;

    /** How fast (in seconds) the entity walks over a block */
    public float MoveDelay = 0.4f;

    /**
     * Initialize everything common to all controllers. This must be
     * manually called because 'Start()' can't be overriden.
     */
    protected void commonInit() {
        RelPos p = 0;
        this.collisionTracker = new int[p.count()];
    }

    /**
     * Whether the entity can move right now.
     */
    virtual protected bool isMoving() {
        return true;
    }

    /**
     * Mark this entity as being on the ledge.
     */
    virtual protected void setOnLedge() { }

    /**
     * Try to move the entity one block forward, depending on the colliders
     * around it.
     */
    protected void tryMoveForward() {
        /* Avoid corner cases by checking before doing anything */
        if (this.isMoving())
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
            else if (this.allowLedgeMovement) {
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
                this.setOnLedge();
            }
        }
    }

    /**
     * Whether the entity is currently falling.
     */
    virtual protected bool isFalling() {
        return false;
    }

    /**
     * Whether the entity can fall right now.
     */
    virtual protected bool canFall() {
        return true;
    }

    /**
     * Called on response of an 'OnEnterRelativeCollision()' event.
     */
    virtual protected void _onEnterRelativeCollision(RelPos p, UEColl c) { }

    /**
     * Called on response of an 'OnExitRelativeCollision()' event.
     */
    virtual protected void _onExitRelativeCollision(RelPos p, UEColl c) { }

    public void OnEnterRelativeCollision(RelPos p, UEColl c) {
        int idx = p.toIdx();
        this.collisionTracker[idx]++;
        if (p == RelPos.Bottom) {
            EvSys.ExecuteEvents.ExecuteHierarchy<ActivateOnTop>(
                    c.gameObject, null, (x,y)=>x.OnEnterTop(this.gameObject));
            /* Stops falling if there's anything bellow the entity */
            if (this.collisionTracker[idx] == 1 && this.isFalling())
                EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                        this.gameObject, null, (x,y)=>x.Halt(this.gameObject));
        }
        this._onEnterRelativeCollision(p, c);
    }

    private System.Collections.IEnumerator tryFall() {
        while (!this.canFall()) {
            if (this.collisionTracker[RelPos.Bottom.toIdx()] > 0 ||
                    this.isFalling()) {
                this.bgFunc = null;
                yield break;
            }
            yield return new UnityEngine.WaitForFixedUpdate();
        }
        EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                this.gameObject, null, (x,y)=>x.Fall(this.gameObject));
        this.bgFunc = null;
    }

    public void OnExitRelativeCollision(RelPos p, UEColl c) {
        this.collisionTracker[p.toIdx()]--;
        if (p == RelPos.Bottom) {
            EvSys.ExecuteEvents.ExecuteHierarchy<ActivateOnTop>(
                    c.gameObject, null, (x,y)=>x.OnLeaveTop(this.gameObject));
            if (this.collisionTracker[p.toIdx()] == 0 && this.bgFunc == null)
                this.bgFunc = this.StartCoroutine(this.tryFall());
        }
        this._onExitRelativeCollision(p, c);
    }
}
