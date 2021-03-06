using GO = UnityEngine.GameObject;
using Mesh = UnityEngine.Mesh;
using Model = UnityEngine.MeshFilter;
using RelPos = RelativeCollision.RelativePosition;
using Type = GetType.Type;
using Vec3 = UnityEngine.Vector3;

public class CrackedBlock : BaseBlock, LedgeTracker {
    public enum State {
        Intact = 0,
        PreCracked,
        Cracked,
        PreBreaking,
        Breaking,
        Broken,
    }

    public State state = State.Intact;

    public Mesh IntactModel;
    public Mesh PreCrackedModel;
    public Mesh CrackedModel;
    public Mesh PreBreakingModel;
    public Mesh BreakingModel;
    public Mesh BrokenModel;

    private Model model;

    override protected void start() {
        System.Action<bool, RelPos, GO> cb;

        base.start();

        cb = (x, y, z) => this.onCollisionUp(x, y, z);
        this.setCollisionCb(RelPos.Top, cb);

        this.model = this.gameObject.GetComponentInChildren<Model>();
        this.updateCrackedState();
    }

    private System.Collections.IEnumerator Break() {
        /* TODO Play the breaking animation */
        yield return new UnityEngine.WaitForFixedUpdate();
        this.state++;

        /* XXX: Forcefully move the entity away from any close entity before
         * destroying it, to avoid glitching the physics. */
        this.transform.position = new Vec3(0.0f, -10.0f, 0.0f);
        yield return new UnityEngine.WaitForFixedUpdate();
        GO.Destroy(this.gameObject);
    }

    private void updateCrackedState() {
        Mesh newModel;

        switch (this.state) {
        case State.Intact:
            newModel = this.IntactModel;
            break;
        case State.PreCracked:
            Global.Sfx.playEnterCrackedBlock(this.fastGetTr());
            newModel = this.PreCrackedModel;
            break;
        case State.Cracked:
            Global.Sfx.playExitCrackedBlock(this.fastGetTr());
            newModel = this.CrackedModel;
            break;
        case State.PreBreaking:
            Global.Sfx.playEnterCrackedBlock(this.fastGetTr());
            newModel = this.PreBreakingModel;
            break;
        case State.Breaking:
            Global.Sfx.playBreakCrackedBlock(this.fastGetTr());
            newModel = this.BreakingModel;
            this.StartCoroutine(this.Break());
            break;
        case State.Broken:
            newModel = this.BrokenModel;
            break;
        default:
            newModel = null;
            break;
        }

        if (newModel != null)
            this.model.mesh = newModel;
    }

    private void updateCrackedStateExit() {
        State last = this.state;

        switch (this.state) {
        case State.PreCracked:
        case State.PreBreaking:
            this.state++;
            break;
        }

        if (last != this.state)
            this.updateCrackedState();
    }

    private void updateCrackedStateEnter() {
        State last = this.state;

        switch (this.state) {
        case State.Intact:
        case State.Cracked:
            this.state++;
            break;
        }

        if (last != this.state)
            this.updateCrackedState();
    }

    public void JustDropped(GO other) {
        this.updateCrackedStateEnter();
    }

    private void onCollisionUp(bool enter, RelPos p, GO other) {
        Type objType = Type.Error;
        bool onLedge = false;

        this.issueEvent<RemoteGetType>( (x,y) => x.Get(out objType), other);
        if (objType != Type.Minion && objType != Type.Player)
            return;
        this.issueEvent<OnLedgeDetector>( (x,y) => x.Check(out onLedge), other);

        if (enter && !onLedge)
            this.updateCrackedStateEnter();
        else if (!enter)
            this.updateCrackedStateExit();
    }
}
