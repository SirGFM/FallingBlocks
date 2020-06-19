using GO = UnityEngine.GameObject;
using Obj = UnityEngine.Object;
using Quat = UnityEngine.Quaternion;
using RB = UnityEngine.Rigidbody;
using RelCol = RelativeCollision;
using SC = UnityEngine.SphereCollider;
using Tr = UnityEngine.Transform;
using Vec3 = UnityEngine.Vector3;

public class Shadow : UnityEngine.MonoBehaviour, OnRelativeCollisionEvent {
    public int MaxDepth = 5;
    public Tr shadowModel;
    public Tr self;

    private class List {
        public Tr block;
        public int y;

        public List next;

        public List() {
            this.block = null;
            this.y = -1;
            this.next = null;
        }
    };

    private List touchingList;
    private List emptyList;

    void Start() {
        GO tracker = new GO();
        tracker.layer = this.gameObject.layer;
        RB rb = tracker.AddComponent<RB>();
        rb.useGravity = false;
        rb.isKinematic = true;
        SC sc = tracker.AddComponent<SC>();
        sc.radius = 0.125f;
        sc.isTrigger = true;
        tracker.AddComponent<RelCol>();

        /* Generate as many detectors as requested */
        Tr t = this.transform;
        for (int i = 1; i <= this.MaxDepth; i++) {
            /* XXX: (again) This was the only way I found to instantiate
             * the clone in local space... */
            GO obj = Obj.Instantiate(tracker, t.position, Quat.identity, t);
            obj.name = $"ShadowTracker_{i}";
            Vec3 pos = new Vec3(0.0f, i * -1.0f, 0.0f);
            obj.transform.Translate(pos, UnityEngine.Space.World);
        }

        Obj.DestroyImmediate(tracker);

        for (int i = 0; i < this.MaxDepth * 2; i++) {
            List node = new List();
            node.next = this.emptyList;
            this.emptyList = node;
        }

        this.touchingList = null;
        this.self = this.transform;
    }

    private bool isBlock(GO other) {
        return (other.GetComponent<BaseBlock>() != null);
    }

    void Update() {
        if (this.touchingList == null) {
            this.shadowModel.gameObject.SetActive(false);
        }
        else {
            this.shadowModel.gameObject.SetActive(true);
            Vec3 pos = self.position;

            Tr block = this.touchingList.block;
            if (block == null)
                return;
            float scale = pos.y - block.position.y;
            shadowModel.localScale = new Vec3(1.0f / scale, 1.0f, 1.0f / scale);

            shadowModel.position = new Vec3(pos.x, pos.y - scale + 1.01f, pos.z);
        }
    }

    public void OnEnterRelativeCollision(RelCol rc, UnityEngine.Collider c) {
        GO other = c.gameObject;
        if (!isBlock(other) || this.emptyList == null)
            return;

        List node = this.emptyList;
        this.emptyList = this.emptyList.next;
        int y = (int)other.transform.position.y;

        if (this.touchingList == null || y > this.touchingList.y) {
            node.next = this.touchingList;
            this.touchingList = node;
        }
        else if (this.touchingList.next == null) {
            this.touchingList.next = node;
            node.next = null;
        }
        else {
            List prev = this.touchingList;
            while (prev.next != null && y <= prev.next.y)
                prev = prev.next;
            node.next = prev.next;
            prev.next = node;
        }

        node.y = y;
        node.block = other.transform;
    }

    public void OnExitRelativeCollision(RelCol rc, UnityEngine.Collider c) {
        GO other = c.gameObject;
        if (!isBlock(other))
            return;
        Tr trOther = other.transform;

        List node = null;
        if (this.touchingList.block == trOther) {
            node = this.touchingList;
            this.touchingList = this.touchingList.next;
        }
        else {
            List prev = this.touchingList;
            while (prev.next.block != trOther)
                prev = prev.next;

            node = prev.next;
            prev.next = prev.next.next;
        }
        node.next = this.emptyList;
        this.emptyList = node;
    }
}
