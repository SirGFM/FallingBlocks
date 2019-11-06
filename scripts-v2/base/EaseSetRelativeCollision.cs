using GO = UnityEngine.GameObject;
using Obj = UnityEngine.Object;
using Quat = UnityEngine.Quaternion;
using RB = UnityEngine.Rigidbody;
using RelCol = RelativeCollision;
using RelPos = RelativeCollision.RelativePosition;
using SC = UnityEngine.SphereCollider;
using Vec3 = UnityEngine.Vector3;

[UnityEngine.ExecuteInEditMode]
public class EaseSetRelativeCollision : UnityEngine.MonoBehaviour {
    public UnityEngine.Color gizmoColor = UnityEngine.Color.white;
    public float gizmoRadius = 0.5f;
    public bool showGizmo = false;

    public bool UpdateColliders = false;

    public bool Top = false;
    public bool Left = false;
    public bool Right = false;
    public bool Bottom = false;
    public bool TopLeft = false;
    public bool TopRight = false;
    public bool BottomLeft = false;
    public bool BottomRight = false;
    public bool Back = false;
    public bool BackTop = false;
    public bool BackLeft = false;
    public bool BackRight = false;
    public bool BackBottom = false;
    public bool BackTopLeft = false;
    public bool BackTopRight = false;
    public bool BackBottomLeft = false;
    public bool BackBottomRight = false;
    public bool Front = false;
    public bool FrontTop = false;
    public bool FrontLeft = false;
    public bool FrontRight = false;
    public bool FrontBottom = false;
    public bool FrontTopLeft = false;
    public bool FrontTopRight = false;
    public bool FrontBottomLeft = false;
    public bool FrontBottomRight = false;
    public bool BottomBottomFront = false;
    public bool FrontFront = false;
    public bool BackBack = false;
    public bool LeftLeft = false;
    public bool RightRight = false;
    public bool FrontTopFrontTop = false;
    public bool FrontFrontTop = false;
    public bool FrontFrontBottom = false;
    public bool FrontBottomFrontBottom = false;
    public bool Center = false;

    private GO dummy;

    private Vec3 getRelPos(RelPos p) {
        Vec3 pos = new Vec3();
        for (RelPos tmp = p; tmp != 0; tmp = tmp.shift()) {
            pos += tmp.masked().toPosition();
        }
        return pos;
    }

    private void createDetector(RelPos p) {
        UnityEngine.Transform t = this.transform;
        Vec3 newPos;
        GO clone;

        newPos = this.getRelPos(p);

        /* XXX: This was the only way I found to instantiate the clone
         * in local space... */
        clone = Obj.Instantiate(this.dummy, t.position, Quat.identity, t);
        clone.name = p.ToString();
        clone.transform.Translate(newPos, UnityEngine.Space.World);
        RelCol rc = clone.GetComponent<RelCol>();
        rc.pos = p;
    }

    private void forEachPos(System.Action<RelPos> cb) {
        var t = this.GetType();
        foreach (RelPos p in System.Enum.GetValues(typeof(RelPos))) {
            bool hasPos = false;

            try {
                var v = t.GetField(p.ToString());
                if ((bool)v.GetValue(this))
                    hasPos = true;
            } catch (System.Exception e) {
            }

            if (hasPos)
                cb(p);
        }
    }

    private void createEveryDetectors() {
        this.dummy = new GO();
        this.dummy.layer = this.gameObject.layer;
        RB rb = this.dummy.AddComponent<RB>();
        rb.useGravity = false;
        rb.isKinematic = true;
        SC sc = this.dummy.AddComponent<SC>();
        sc.radius = 0.125f;
        sc.isTrigger = true;
        this.dummy.AddComponent<RelCol>();

        this.forEachPos( (x) => this.createDetector(x) );

        Obj.DestroyImmediate(this.dummy);
    }

    void Update() {
        if (this.UpdateColliders) {
            foreach (RelCol rc in this.GetComponentsInChildren<RelCol>())
                Obj.DestroyImmediate(rc.gameObject);
            this.createEveryDetectors();
            this.UpdateColliders = false;
        }
    }

    private void drawDetector(RelPos p) {
        Vec3 pos = this.getRelPos(p);
        UnityEngine.Gizmos.DrawWireSphere(this.transform.position + pos, this.gizmoRadius);
    }

    void OnDrawGizmosSelected() {
        if (!this.showGizmo)
            return;

        UnityEngine.Gizmos.color = this.gizmoColor;
        this.forEachPos( (x) => this.drawDetector(x) );
    }
}
