using AudioListener = UnityEngine.AudioListener;
using PlayerPrefs = UnityEngine.PlayerPrefs;

static public class Config {
    static private bool getBool(string key, bool def) {
        if (!PlayerPrefs.HasKey(key))
            return def;
        return PlayerPrefs.GetInt(key) == 1;
    }

    static private void setBool(string key, bool v) {
        PlayerPrefs.SetInt(key, (v ? 1 : 0));
    }

    static public void load() {
        float globalVolume, musicVolume, sfxVolume;
        bool invertCamX, invertCamY;
        int particleQuality, playerModel;

        globalVolume = PlayerPrefs.GetFloat("GlobalVolume", 1.0f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.6f);
        sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 0.4f);
        invertCamX = getBool("InvertCamX", false);
        invertCamY = getBool("InvertCamY", false);
        particleQuality = PlayerPrefs.GetInt("ParticleQuality",
                                             (int)Global.ParticleQuality.High);
        playerModel = PlayerPrefs.GetInt("PlayerModel", 0);

        setGlobalVolume(globalVolume);
        setMusicVolume(musicVolume);
        setSfxVolume(sfxVolume);
        setHorCamInverted(invertCamX);
        setVerCamInverted(invertCamY);
        setParticlesQual((Global.ParticleQuality)particleQuality);
        setPlayerModel(playerModel);

        for (int i = 0; i < 3; i++)
            loadInput(i);
    }

    static public void forceSave() {
        PlayerPrefs.Save();
    }

    static public void setGlobalVolume(float v) {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat("GlobalVolume", v);
    }

    static public float getGlobalVolume() {
        return AudioListener.volume;
    }

    static public void setMusicVolume(float v) {
        Global.Sfx.setMusicVolume(v);
        PlayerPrefs.SetFloat("MusicVolume", v);
    }

    static public float getMusicVolume() {
        return Global.Sfx.getMusicVolume();
    }

    static public void setSfxVolume(float v) {
        Global.Sfx.sfxVolume = v;
        PlayerPrefs.SetFloat("SfxVolume", v);
    }

    static public float getSfxVolume() {
        return Global.Sfx.sfxVolume;
    }

    static public void setHorCamInverted(bool v) {
        if (v)
            Global.camX = -1.0f;
        else
            Global.camX = 1.0f;
        setBool("InvertCamX", v);
    }

    static public bool getHorCamInverted() {
        return (Global.camX == -1.0f);
    }

    static public void setVerCamInverted(bool v) {
        if (v)
            Global.camY = -1.0f;
        else
            Global.camY = 1.0f;
        setBool("InvertCamY", v);
    }

    static public bool getVerCamInverted() {
        return (Global.camY == -1.0f);
    }

    static public void setParticlesQual(Global.ParticleQuality gpq) {
       Global.particleQuality = gpq;
       PlayerPrefs.SetInt("ParticleQuality", (int)gpq);
    }

    static public Global.ParticleQuality getParticlesQual() {
       return Global.particleQuality;
    }

    static private void saveInput(int column) {
        string key = $"Input-{column}";

        try {
            string json = Input.axisToJson(column);
            PlayerPrefs.SetString(key, json);
        } catch (System.Exception) {
        }
    }

    static private void loadInput(int column) {
        string key = $"Input-{column}";

        if (!PlayerPrefs.HasKey(key))
            return;
        string json = PlayerPrefs.GetString(key, "");

        try {
            Input.axisFromJson(column, json);
        } catch (System.Exception) {
            Input.RevertMap(column);
        }
    }

    static public void saveInputA() {
        saveInput(0);
    }

    static public void saveInputB() {
        saveInput(1);
    }

    static public void saveInputC() {
        saveInput(2);
    }

    static public void setPlayerModel(int v) {
        PlayerModel.active = v;
        PlayerPrefs.SetInt("PlayerModel", v);
    }

    static public int getPlayerModel() {
        return PlayerModel.active;
    }
}
