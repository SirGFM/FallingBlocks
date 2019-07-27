using Container = System.Collections.Generic.HashSet<int>;

public class DetectEdge : EdgeBase {

    /** List of colliders directly above this one */
    private Container colliders;

    /**
     * Start is called before the first frame update.
     */
    void Start() {
        this.colliders = new Container();

        this.setupRigidbody();

        /* Create a box on the bottom of the object */
        UnityEngine.BoxCollider bc = this.gameObject.GetComponent<UnityEngine.BoxCollider>();
        if (bc == null)
            bc = this.gameObject.AddComponent<UnityEngine.BoxCollider>();
        bc.center = new UnityEngine.Vector3(0f, -0.35f, 0f);
        bc.size = new UnityEngine.Vector3(0.9f, 0.2f, 0.9f);
        bc.isTrigger = true;
    }

    void OnTriggerEnter(UnityEngine.Collider c) {
        /* XXX: Layer should ensure c is a ReportTopEdge */

        /* Ignore if already in the list */
        if (colliders.Contains(c.GetInstanceID()))
            return;
        colliders.Add(c.GetInstanceID());

        ReportTopEdge rte = c.gameObject.GetComponent<ReportTopEdge>();
        EdgeBase.Direction d = rte.getDirection(c);

        /** Send a message upward */
        UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy<OnBlockEdge>(
                this.gameObject, null, (x,y)=>x.OnTouchEdge(d));
    }

    void OnTriggerExit(UnityEngine.Collider c) {
        /* XXX: Layer should ensure c is a ReportTopEdge */

        /* Ignore if already removed from the list */
        if (!colliders.Contains(c.GetInstanceID()))
            return;
        colliders.Remove(c.GetInstanceID());

        ReportTopEdge rte = c.gameObject.GetComponent<ReportTopEdge>();
        EdgeBase.Direction d = rte.getDirection(c);

        /** Send a message upward */
        UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy<OnBlockEdge>(
                this.gameObject, null, (x,y)=>x.OnReleaseEdge(d));
    }
}
