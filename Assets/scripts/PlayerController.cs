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
    /** Function handling the turn animation (and delay). */
    private UnityEngine.Coroutine turnDelay = null;

    /** How long to delay movement after a turn */
    public float TurnDelay = 0.3f;

    // Start is called before the first frame update
    void Start() {
        this.facing = Direction.back;
        this.transform.eulerAngles = new UnityEngine.Vector3(0f, 0f, 0f);
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

    private void moveForward() {
    }

    /**
     * Animate the rotation from the current orientation to 'd'.
     */
    private System.Collections.IEnumerator turn(Direction d) {
        float tgtAngle, dtAngle;
        int steps;

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
        this.turnDelay = null;
        /* If still holding on the same direction, buffer a movement */
        if (d == this.getInputDirection())
            this.moveForward();
    }

    // Update is called once per frame
    void Update() {
        Direction newDir = this.getInputDirection();
        if (newDir == Direction.none)
            return;

        if (this.facing != newDir)
            if (this.turnDelay == null)
                this.turnDelay = this.StartCoroutine(this.turn(newDir));
            else {
                /* Can't do anything D: */
            }
        else
            this.moveForward();
    }

    public void OnEnterRelativeCollision(RelativePosition p, UnityEngine.Collider c) {
        /* TODO: Do something */
    }

    public void OnExitRelativeCollision(RelativePosition p, UnityEngine.Collider c) {
        /* TODO: Do something */
    }
}
