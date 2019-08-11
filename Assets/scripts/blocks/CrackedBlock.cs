﻿using Model = UnityEngine.MeshFilter;

public class CrackedBlock : UnityEngine.MonoBehaviour, ActivateOnTop {
    private enum State{
        untouched = 0,
        touched,
        broken,
        brokenAnim,
        maxState
    }

    private State state;
    private UnityEngine.Coroutine bgFunc;
    private Model curModel;

    public UnityEngine.Mesh defaultModel;
    public UnityEngine.Mesh breakingModel;

    private void updateAsset() {
        /* TODO: Update the asset based on the state */
        switch (this.state) {
        case State.untouched:
            this.curModel.mesh = this.defaultModel;
            break;
        case State.touched:
            this.curModel.mesh = this.breakingModel;
            break;
        case State.broken:
        case State.brokenAnim:
            break;
        }
    }

    void Start() {
        this.curModel = this.gameObject.GetComponentInChildren<Model>();
        this.state = State.untouched;
        this.updateAsset();
    }

    private void playChangeStateAnim() {
        /* TODO: Play some effects */
    }

    public void OnEnterTop(UnityEngine.GameObject other) {
        playChangeStateAnim();
    }

    private System.Collections.IEnumerator Break() {
        this.state++;

        /* TODO Play the breaking animation */
        yield return new UnityEngine.WaitForFixedUpdate();

        /* XXX: Forcefully move the entity away from any close entity before
         * destroying it, to avoid glitching the physics. */
        this.transform.position = new UnityEngine.Vector3(0.0f, -10.0f, 0.0f);
        yield return new UnityEngine.WaitForFixedUpdate();
        UnityEngine.GameObject.Destroy(this.gameObject);
    }

    public void OnLeaveTop(UnityEngine.GameObject other) {
        this.state++;
        this.updateAsset();
        if (state == State.broken)
            this.bgFunc = this.StartCoroutine(this.Break());
    }
}