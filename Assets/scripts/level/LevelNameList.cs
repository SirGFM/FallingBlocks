using Scene = UnityEngine.SceneManagement.Scene;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneUtil = UnityEngine.SceneManagement.SceneUtility;

static public class LevelNameList {
    static private string[] _list = null;

    static private string ProcessName(string baseName) {
        int start = baseName.LastIndexOf("/");
        int end = baseName.LastIndexOf(".");

        baseName = baseName.Substring(start + 1, end - start - 1);
        for (int i = 0; i < baseName.Length; i++) {
            if (baseName[i] >= 'A' && baseName[i] <= 'Z') {
                baseName = baseName.Substring(i);
                break;
            }
        }

        return baseName;
    }

    static private void UpdateList() {
        int i;
        int max = SceneMng.sceneCountInBuildSettings;

        for (i = 1; i < max; i++) {
            string name = SceneUtil.GetScenePathByBuildIndex(i);
            int pos = name.LastIndexOf("/");
            char first = name[pos + 1];

            /* Every level start with a number, so use this to find the
             * number of levels */
            if (first < '0' && first > '9')
                break;
        }

        max = i;
        _list = new string[max];
        for (i = 1; i < max; i++)
            _list[i] = ProcessName(SceneUtil.GetScenePathByBuildIndex(i));
    }

    static public string[] Get() {
        if (_list == null)
            UpdateList();
        return _list;
    }

    /**
     * Retrieve the name of a given level (starting at 1).
     */
    static public string GetLevel(int i) {
        if (_list == null)
            UpdateList();
        if (i < _list.Length)
            return _list[i];
        return "Unknown";
    }
}
