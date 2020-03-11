using GO = UnityEngine.GameObject;
using Math = UnityEngine.Mathf;
using Transform = UnityEngine.Transform;
using Vec3 = UnityEngine.Vector3;

public class CameraController : BaseRemoteAction {

    public Transform cam;
    public Transform player;

    private float distance = 6.0f;
    private float baseDX = -1.5f;
    private float baseDY = 6.0f;
    private float baseDZ = -17.0f;

    private Vec3 lastPos;

    void Start() {
        this.cam = this.transform;
        this.lastPos = new Vec3();
    }

    void Update() {
        Vec3 pos;

        if (this.player == null) {
            GO pl = null;
            this.rootEvent<GetPlayer>( (x,y) => x.Get(out pl) );
            if (pl != null)
                this.player = pl.transform;
            return;
        }

        /* Cosine of the angle on the X-Z ("horizontal") plane */
        float xCosTeta = UnityEngine.Input.GetAxis("CameraX");
        xCosTeta = Math.Clamp(xCosTeta, -0.8f, 0.8f);
        /* Sine of the angle on the Z-Y ("vertical") plane */
        float ySinPhi = -1.0f * UnityEngine.Input.GetAxis("CameraY");

        float dist = Math.Sqrt(xCosTeta * xCosTeta + ySinPhi * ySinPhi);
        if (dist < 0.5f) {
            pos = new Vec3(this.baseDX, this.baseDY, this.baseDZ);
        }
        else {
            float zSinTeta = -1.0f * Math.Sqrt(1.0f - xCosTeta * xCosTeta);
            float zCosPhi = -1.0f * Math.Sqrt(1.0f - ySinPhi * ySinPhi);

            pos = new Vec3(xCosTeta, ySinPhi, (zSinTeta + zCosPhi) * 0.5f);
        }
        pos = pos.normalized * this.distance;
        this.lastPos = 0.75f * this.lastPos + pos * 0.25f;

        this.cam.position = this.player.position + this.lastPos;
        this.cam.LookAt(this.player);
    }
}
