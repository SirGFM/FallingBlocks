using AudioListener = UnityEngine.AudioListener;

static public class Config {
    static public void setGlobalVolume(float v) {
        AudioListener.volume = v;
    }

    static public float getGlobalVolume() {
        return AudioListener.volume;
    }

    static public void setMusicVolume(float v) {
        Global.Sfx.setMusicVolume(v);
    }

    static public float getMusicVolume() {
        return Global.Sfx.getMusicVolume();
    }

    static public void setSfxVolume(float v) {
        Global.Sfx.sfxVolume = v;
    }

    static public float getSfxVolume() {
        return Global.Sfx.sfxVolume;
    }

    static public void setHorCamInverted(bool v) {
        if (v)
            Global.camX = -1.0f;
        else
            Global.camX = 1.0f;
    }

    static public bool getHorCamInverted() {
        return (Global.camX == -1.0f);
    }

    static public void setVerCamInverted(bool v) {
        if (v)
            Global.camY = -1.0f;
        else
            Global.camY = 1.0f;
    }

    static public bool getVerCamInverted() {
        return (Global.camY == -1.0f);
    }

    static public void setParticlesQual(Global.ParticleQuality gpq) {
       Global.particleQuality = gpq;
    }

    static public Global.ParticleQuality getParticlesQual() {
       return Global.particleQuality;
    }

    static private void saveInput(int column) {
    }

    static private void loadInput(int column) {
    }

    static public void saveInputA() {
        saveInput(0);
    }

    static public void loadInputA() {
        loadInput(0);
    }

    static public void saveInputB() {
        saveInput(1);
    }

    static public void loadInputB() {
        loadInput(1);
    }

    static public void saveInputC() {
        saveInput(2);
    }

    static public void loadInputC() {
        loadInput(2);
    }

    static public void setPlayerModel(int v) {
        PlayerModel.active = v;
    }

    static public int getPlayerModel() {
        return PlayerModel.active;
    }
}
