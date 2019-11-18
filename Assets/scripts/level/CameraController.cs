using GO = UnityEngine.GameObject;
using Math = UnityEngine.Mathf;
using Transform = UnityEngine.Transform;
using Vec3 = UnityEngine.Vector3;

public class CameraController : BaseRemoteAction {

    public Transform cam;
    public Transform player;

    private Vec3 globalOffset;

    private float distance = 7.0f;
    private float baseDY = 6.0f;
    private float baseDZ = -17.0f;

    void Start() {
        this.cam = this.transform;
    }

    void Update() {
        if (this.player == null) {
            GO pl = null;
            this.rootEvent<GetPlayer>( (x,y) => x.Get(out pl) );
            if (pl != null)
                this.player = pl.transform;
            return;
        }

        Vec3 v = new Vec3();
        v.x = 0f;
        v.y = this.baseDY;
        v.z = this.baseDZ;
        v = v.normalized * this.distance;

        if (this.player.position.y >= this.globalOffset.y)
            this.globalOffset = this.player.position;
        else {
            this.globalOffset.x = this.player.position.x;
            this.globalOffset.z = this.player.position.z;
        }
        this.globalOffset.x *= 1.25f;

        Vec3 localOffset = new Vec3();
        float offY = UnityEngine.Input.GetAxis("CameraY");
        float absOffY = Math.Abs(offY);
        if (offY < 0) {
            /* Moving camera up, looking down */
            localOffset.z = this.baseDZ * absOffY * -0.3f;
            localOffset.y = absOffY * 4.0f;
        }
        else {
            /* Moving camera down, looking up */
            localOffset.z = this.baseDZ * absOffY * -0.35f;
            localOffset.z += absOffY * -5.0f;
            localOffset.y = this.baseDY * -absOffY;
        }
        localOffset.x = UnityEngine.Input.GetAxis("CameraX") * 2.0f;

        this.cam.position = v + this.globalOffset + localOffset;
        this.cam.LookAt(this.player);
    }
}
