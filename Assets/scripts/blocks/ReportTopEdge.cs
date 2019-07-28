public class ReportTopEdge : EdgeBase {

    /** Maps a SphereCollider's ID to a Direction */
    private System.Collections.Generic.Dictionary<int, Direction> id2Direction;

    /**
     * Start is called before the first frame update.
     */
    void Start() {
        float _scale = 0.5f;

        this.id2Direction = new System.Collections.Generic.Dictionary<int, Direction>((int)Direction.max);

        this.setupRigidbody();

        /* Create every collider used for edge detection */
        for (Direction d = Direction.min; d < Direction.max; d++) {
            UnityEngine.SphereCollider sc;
            UnityEngine.Vector3 v3;

            v3 = this.directionToV3(d);
            sc = this.gameObject.AddComponent<UnityEngine.SphereCollider>();
            sc.center = v3 * _scale;
            sc.isTrigger = true;
            sc.radius = 0.25f;
            id2Direction.Add(sc.GetInstanceID(), d);
        }
    }

    /**
     * Check if a collider is a block and retrieve its colliding edge.
     */
    virtual public EdgeBase.Direction getDirection(UnityEngine.Collider c) {
        Direction d;

        if (!this.id2Direction.TryGetValue(c.GetInstanceID(), out d))
            return Direction.max;
        return d;
    }
}
