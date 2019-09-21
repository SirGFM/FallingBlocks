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

    public static bool isShivering(this MinionController.State s) {
        switch (s) {
        case MinionController.State.Shiver:
        case MinionController.State.WanderAround:
            return true;
        default:
            return false;
        }
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

    /** Whether any of the minions on screen is a leader */
    private static bool hasLeader = false;

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
    private GO leader;
    private GO goal;
    private GO lastMinion;

    private State nextState;
    private State state;
    private int closeMinion;
    /** How many colliders are detecting the leader */
    private int closeLeader;

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
        } /* switch (st) */
        if (this.state == State.Leader &&
                this.nextState != this.state &&
                this.nextState != State.None)
            MinionController.hasLeader = false;
        this.state = st;
        if (this.nextState == st)
            this.nextState = State.None;
    }

    private void updateState() {
        if (this.goal != null) {
            this.nextState = State.EnterChest;
            this.target = this.goal;
        }
        else if (this.closeLeader > 0 && !MinionController.hasLeader) {
            MinionController.hasLeader = true;
            this.nextState = State.Leader;
            this.target = this.leader;
        }
        else if (this.closeMinion > 0 && this.state.canFollowLeader()) {
            MinionController other;
            other = this.lastMinion.GetComponent<MinionController>();
            if (other.state.shouldFollow() || other.nextState.shouldFollow()) {
                this.nextState = State.Follow;
                this.target = this.lastMinion;
            }
            else {
                this.nextState = State.PseudoLeader;
            }
        }
        else if (this.closeLeader == 0 && this.closeMinion == 0 &&
                !this.state.isShivering()) {
            this.nextState = State.Shiver;
        }
    }

    void Update() {
        if (this.anim != Animation.None)
            return;

        updateState();

        if (this.nextState != State.None)
            doState(this.nextState);
        else if (this.state != State.None)
            doState(this.state);
    }

    override protected void _onEnterRelativeCollision(RelPos p, UEColl c) {
        if (c.gameObject.tag == this.gameObject.tag) {
            MinionController other;

            other = c.gameObject.GetComponent<MinionController>();
            if (other != this) {
                this.lastMinion = c.gameObject;
                this.closeMinion++;
            }
        }
        else if (c.gameObject.tag == MinionController.LeaderTag) {
            /* Found the player */
            this.closeLeader++;
            this.leader = c.gameObject;
        }
        else if (c.gameObject.tag == MinionController.GoalTag) {
            /* Found the end-of-level goal */
            this.goal = c.gameObject;
        }
    }

    override protected void _onExitRelativeCollision(RelPos p, UEColl c) {
        if (c.gameObject.tag == this.gameObject.tag) {
            MinionController other;
            other = c.gameObject.GetComponent<MinionController>();
            if (other != this)
                this.closeMinion--;
        }
        else if (c.gameObject.tag == MinionController.LeaderTag) {
            /* Lost track of the player */
            this.closeLeader--;
            if (this.closeLeader == 0)
                this.leader = null;
        }
        else if (c.gameObject.tag == MinionController.GoalTag) {
            /* (shouldn't happen) Lost track of the end-of-level goal */
            this.goal = null;
        }
    }
}
