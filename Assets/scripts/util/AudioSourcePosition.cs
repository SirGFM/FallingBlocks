using GO = UnityEngine.GameObject;

public class AudioSourcePosition : FollowObject {
    private GO followed = null;

    private bool forceReset = false;
    public void onLoadLevel(int idx) {
        forceReset = true;
    }

    private System.Collections.IEnumerator recachePlayer() {
        float wait = 1.0f;
        do {
            float curWait = 0.0f;
            this.followed = GO.FindWithTag("Player");

            if (this.followed != null) {
                while (curWait < wait && !this.forceReset) {
                    yield return new UnityEngine.WaitForSeconds(0.1f);
                    curWait += 0.1f;
                }
                if (wait < 10.0f)
                    wait += 1.0f;
            }
        } while (this.followed != null && !this.forceReset);
        this.forceReset = false;
    }

    private UnityEngine.Coroutine bgFunc = null;
    private System.Collections.IEnumerator recacheFollowedObject() {
        Loader.addOnLoadLevel(this.onLoadLevel);
        while (true) {
            yield return this.recachePlayer();
            yield return new UnityEngine.WaitForSeconds(0.5f);
        }
    }

    override protected GO getFollowed() {
        if (this.bgFunc == null)
            this.bgFunc = this.StartCoroutine(this.recacheFollowedObject());

        return this.followed;
    }
}
