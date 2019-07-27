﻿using EvSys = UnityEngine.EventSystems;
using GO = UnityEngine.GameObject;
using Vec3 = UnityEngine.Vector3;

public interface iSignalFall : EvSys.IEventSystemHandler {
    /**
     * Start falling.
     */
    void Fall(GO caller);

    /**
     * Signal the entity to stop falling, aligned to the grid.
     */
    void Halt(GO caller);
}

public interface iDetectFall : EvSys.IEventSystemHandler {
    /**
     * Signal that the entity started falling.
     */
    void OnStartFalling(GO callee);

    /**
     * Signal that the entity has finished falling.
     */
    void OnFinishFalling(GO callee);
}

public class Faller : BaseRemoteAction, iSignalFall {
    /** Whether the object is currently falling. */
    private bool isFalling = false;
    /** Whether the object should start aligning itself. */
    private bool startAligning = false;
    /** Aligned after the object stops falling. */
    private float newAlignedY;
    /** Reference to the object's rigid body */
    private UnityEngine.Rigidbody rb;

    /** Maximum allowed fall speed */
    public float MaxFallSpeed = -4.5f;

    void Start() {
        this.rb = this.GetComponent<UnityEngine.Rigidbody>();
        if (this.rb == null)
            this.rb = this.gameObject.AddComponent<UnityEngine.Rigidbody>();
        this.rb.isKinematic = true;
        this.rb.useGravity = false;
        this.rb.constraints = UnityEngine.RigidbodyConstraints.FreezeRotationX | UnityEngine.RigidbodyConstraints.FreezeRotationZ;
    }

    /**
     * Retrieve the next vertical position aligned to the grid.
     */
    private float getGridAlignedY() {
        return (float)System.Math.Floor(this.transform.localPosition.y);
    }

    /**
     * Fall until signaled, and then until the object becomes aligned.
     */
    private System.Collections.IEnumerator fall() {
        this.isFalling = true;
        this.issueEvent<iDetectFall>((x,y)=>x.OnStartFalling(this.gameObject));

        this.rb.isKinematic = false;
        this.rb.useGravity = true;

        while (!this.startAligning)
            yield return new UnityEngine.WaitForFixedUpdate();

        /* Align to the grid */
        while (this.transform.localPosition.y > this.newAlignedY)
            yield return new UnityEngine.WaitForFixedUpdate();

        this.rb.isKinematic = true;
        this.rb.useGravity = false;

        Vec3 tmp = this.transform.localPosition;
        this.transform.localPosition = new Vec3(tmp.x, this.newAlignedY, tmp.z);

        this.isFalling = false;
        this.issueEvent<iDetectFall>((x,y)=>x.OnFinishFalling(this.gameObject));
    }

    void FixedUpdate() {
        Vec3 v = this.rb.velocity;
        if (v.y < this.MaxFallSpeed)
            this.rb.velocity = new Vec3(v.x, this.MaxFallSpeed, v.z);
    }

    public void Fall(GO caller) {
        if (this.isFalling)
            return;

        this.startAligning = false;
        this.caller = caller;
        this.StartCoroutine(this.fall());
    }

    public void Halt(GO caller) {
        if (!this.isFalling)
            return;

        this.startAligning = true;
        this.newAlignedY = this.getGridAlignedY();
    }
}
