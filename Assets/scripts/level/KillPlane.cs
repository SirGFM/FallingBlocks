using GO = UnityEngine.GameObject;

public class KillPlane : ReportTopEdge {
    private GO player;

    private void getPlayer() {
        /* XXX: This assumes ther's only a single player in each scene */
        foreach (GO go in GO.FindGameObjectsWithTag("Player"))
            this.player = go;
    }

    void Start() {
        this.setupRigidbody();
        UnityEngine.BoxCollider bc;
        bc = this.gameObject.AddComponent<UnityEngine.BoxCollider>();
        bc.size = new UnityEngine.Vector3(100.0f, 1.0f, 100.0f);
        bc.isTrigger = true;

        this.player = null;
        this.getPlayer();
    }

    void Update() {
        /* XXX: Using 'OnTriggerEnter' would normally be easier, but the way I
         * set the layers up make this quite difficult... Manually checking is
         * way easier D: */
        if (this.player == null)
            this.getPlayer();
        else if (this.player.transform.position.y < this.transform.position.y) {
            /* TODO: Message that the player died */
            UnityEngine.Debug.Log("Game Over");
        }
    }

    override public EdgeBase.Direction getDirection(UnityEngine.Collider c) {
        return EdgeBase.Direction.min;
    }
}
