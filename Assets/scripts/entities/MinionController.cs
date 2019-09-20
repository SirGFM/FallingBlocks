using Dir = Movement.Direction;
using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using Math = UnityEngine.Mathf;
using RelPos = ReportRelativeCollision.RelativePosition;
using UEColl = UnityEngine.Collider;
using Vec3 = UnityEngine.Vector3;

public static class StateMethods {
    /** Whether this state makes the entity a leader. */
    public static bool isLeader(this MinionController.State s) {
        switch (s) {
        case MinionController.State.PseudoLeader:
        case MinionController.State.Leader:
            return true;
        default:
            return false;
        }
    }

    /** Whether this state makes the entity a desireable leader. */
    public static bool shouldFollow(this MinionController.State s) {
        switch (s) {
        case MinionController.State.PseudoLeader:
        case MinionController.State.Leader:
        case MinionController.State.EnterChest:
            return true;
        default:
            return false;
        }
    }

    /** Whether this state allows the entity to follow a leader. */
    public static bool canFollowLeader(this MinionController.State s) {
        switch (s) {
        case MinionController.State.PseudoLeader:
        case MinionController.State.Leader:
        case MinionController.State.EnterChest:
            return false;
        default:
            return true;
        }
    }

    public static bool isEnterChest(this MinionController.State s) {
        return s == MinionController.State.EnterChest;
    }
}

public class MinionController : BaseController, iDetectFall {
    public enum State {
        None = 0,
        Shiver,       /* Shivers in place if no other entity is around */
        WanderAround, /* WanderAround, trying to look another entity */
        Follow,       /* Following a leader of sorts */
        PseudoLeader, /* Ramdonly elected as a leader, in place of the player */
        Leader,       /* Minion closest to the player, and following them */
        EnterChest,   /* Entering a nearby chest */
    };

    /** Tag of the object that should be used as the real leader */
    private const string LeaderTag = "Player";
    /** Tag of the object that should be used as the goal */
    private const string GoalTag = "Finish";
    /** Minimum duration of the shiver animation */
    private const float minShiverTime = 0.5f;
    /** Maximum duration of the shiver animation */
    private const float maxShiverTime = 1.5f;

    /** The entity leading this one */
    private GO target;
    private State nextState;
    private State state;
    private int closeMinion;

    // Start is called before the first frame update
    void Start() {
        this.commonInit();

        EvSys.ExecuteEvents.ExecuteHierarchy<iSignalFall>(
                this.gameObject, null, (x,y)=>x.Fall(this.gameObject));
    }

    static private float v3dist(Vec3 a, Vec3 b, int idx) {
        return Math.Abs(a[idx] - b[idx]);
    }

    private void followEntity(State st) {
        Vec3 self, other;
        float yDist;
        Dir otherDir = Dir.none;

        other = this.target.transform.position;
        do {
            BaseController b = this.target.GetComponent<BaseController>();
            if (b != null) {
                otherDir = b.getFacing();
                other = b.getLastPosition();
            }
        } while (false);

        self = this.transform.position;
        yDist = v3dist(self, other, 1/*y*/);

        Dir d = this.getRelativeDirection(other);
        if (d == Dir.none)
            { /* Do nothing */ }
        else if (d != this.facing)
            this.turn(d);
        else
            this.tryMoveForward(yDist < 1.0f);
    }

    private void doState(State st) {
        switch (st) {
        case State.Shiver:
            this.shiver(minShiverTime, maxShiverTime);
            this.nextState = State.WanderAround;
            break;
        case State.WanderAround:
            bool front = this.collisionTracker[RelPos.Front.toIdx()] > 0;
            bool floor = this.collisionTracker[RelPos.FrontBottom.toIdx()] > 0;
            if (!front && floor) {
                this.tryMoveForward();
                this.nextState = State.WanderAround;
            }
            else {
                this.turn(Dir.right.toLocal(this.facing));
                this.nextState = State.Shiver;
            }
            break;
        case State.Follow:
        case State.Leader:
        case State.EnterChest:
            this.followEntity(st);
            break;
        case State.PseudoLeader:
            /* Just try moving to a "random" direction */
            if (this.collisionTracker[RelPos.Front.toIdx()] == 0 &&
                    this.collisionTracker[RelPos.FrontBottom.toIdx()] != 0) {
                this.tryMoveForward();
            }
            else
                this.turn(Dir.left.toLocal(this.facing));
            break;
        }
        this.state = st;
        this.nextState = State.None;
    }

    void Update() {
        if (this.anim != Animation.None)
            return;

        if (this.nextState != State.None)
            doState(this.nextState);
        else if (this.state != State.None)
            doState(this.state);
    }

    private void modCloseMinions(int num, MinionController other) {
        this.closeMinion += num;

        /* Do nothing if already targeting the chest */
        if (this.state.isEnterChest() || this.nextState.isEnterChest())
            return;

        switch (this.closeMinion) {
        case 0:
            /* Just got separated of the last minion */
            if (this.state != State.Leader) {
                this.nextState = State.Shiver;
                this.target = null;
            }
            break;
        case 1:
            if (this.state.isLeader() && this.nextState.isLeader())
                { /* Do nothing: this entity is already following the player */ }
            else if (other.state.shouldFollow() ||
                    other.nextState.shouldFollow()) {
                /* Just found another minion: follow it */
                this.nextState = State.Follow;
                this.target = other.gameObject;
            }
            else
                /* Just *was* found by another minion: make it follow this */
                this.nextState = State.PseudoLeader;
            break;
        }
    }

    override protected void _onEnterRelativeCollision(RelPos p, UEColl c) {
        if (c.gameObject.tag == this.gameObject.tag) {
            MinionController other;

            other = c.gameObject.GetComponent<MinionController>();
            this.modCloseMinions(1/*num*/, other);
        }
        else if (c.gameObject.tag == MinionController.LeaderTag) {
            /* Found the player */
            this.nextState = State.Leader;
            this.target = c.gameObject;
        }
        else if (c.gameObject.tag == MinionController.GoalTag) {
            /* Found the end-of-level goal */
            this.nextState = State.EnterChest;
            this.target = c.gameObject;
        }
    }

    override protected void _onExitRelativeCollision(RelPos p, UEColl c) {
        if (c.gameObject.tag == this.gameObject.tag) {
            MinionController other;

            other = c.gameObject.GetComponent<MinionController>();
            this.modCloseMinions(-1/*num*/, other);
        }
        else if (c.gameObject.tag == MinionController.LeaderTag) {
            /* Lost track of the player */
            this.nextState = State.Shiver;
            this.target = null;
        }
        else if (c.gameObject.tag == MinionController.GoalTag) {
            /* (shouldn't happen) Lost track of the end-of-level goal */
            this.nextState = State.Shiver;
            this.target = null;
        }
    }
}
