using GO = UnityEngine.GameObject;
using Obj = UnityEngine.Object;
using Quat = UnityEngine.Quaternion;
using RelCol = ReportRelativeCollision;
using RelPos = ReportRelativeCollision.RelativePosition;
using Vec3 = UnityEngine.Vector3;

public class SpawnRelativeCollision : UnityEngine.MonoBehaviour {

    /** Basic prefab that detects the surroundings */
    public GO prefab;
    /** List of positions that should be detected by this entity */
    public RelPos[] nodes;

    void Start() {
        UnityEngine.Transform t = this.transform;

        if (this.nodes != null) {
            foreach (RelPos pos in this.nodes) {
                Vec3 newPos;
                GO clone;

                newPos = new Vec3();
                for (RelPos tmp = pos; tmp != 0; tmp = tmp.shift()) {
                    newPos += tmp.masked().toPosition();
                }

                /* XXX: This was the only way I found to instantiate the clone
                 * in local space... */
                clone = Obj.Instantiate(prefab, t.position, Quat.identity, t);
                clone.name = pos.ToString();
                clone.transform.Translate(newPos, UnityEngine.Space.World);
                RelCol rc = clone.GetComponent<RelCol>();
                rc.pos = pos;
            }
        }
    }
}
