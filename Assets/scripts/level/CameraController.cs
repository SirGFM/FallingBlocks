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

    private bool wasUsingMouse;
    private Vec3 mouse;

    void Start() {
        this.cam = this.transform;
        this.lastPos = new Vec3();
        this.wasUsingMouse = false;
    }

    void Update() {
        Vec3 pos;
        /* Cosine of the angle on the X-Z ("horizontal") plane */
        float xCosTeta;
        /* Sine of the angle on the Z-Y ("vertical") plane */
        float ySinPhi;

        if (this.player == null) {
            GO pl = null;
            this.rootEvent<GetPlayer>( (x,y) => x.Get(out pl) );
            if (pl != null)
                this.player = pl.transform;
            return;
        }

        if (UnityEngine.Input.GetAxisRaw("MouseCamera") == 0) {
            /* Try to manipulate the camera using a gamepad */
            xCosTeta = Global.camX * -1.0f * UnityEngine.Input.GetAxis("CameraX");
            ySinPhi = Global.camY * UnityEngine.Input.GetAxis("CameraY");
            this.wasUsingMouse = false;
        }
        else {
            if (this.wasUsingMouse) {
                /* Move the camera, using a 50px (?) circle around the mouse */
                Vec3 mouseDelta = UnityEngine.Input.mousePosition - this.mouse;
                xCosTeta = Global.camX * -1.0f * mouseDelta.x * 0.02f;
                ySinPhi = Global.camY * -1.0f * mouseDelta.y * 0.02f;
                ySinPhi = Math.Clamp(ySinPhi, -1.0f, 1.0f);
            }
            else {
                /* Use the current position as the mouse's origin */
                this.mouse = UnityEngine.Input.mousePosition;
                this.wasUsingMouse = true;
                xCosTeta = 0.0f;
                ySinPhi = 0.0f;
            }
        }
        xCosTeta = Math.Clamp(xCosTeta, -0.8f, 0.8f);

        float dist = Math.Sqrt(xCosTeta * xCosTeta + ySinPhi * ySinPhi);
        if (!this.wasUsingMouse && dist < 0.5f) {
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
