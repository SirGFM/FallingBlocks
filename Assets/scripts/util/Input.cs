using CoroutineRet = System.Collections.IEnumerator;
using DefInput = UnityEngine.Input;
using KeyCode = UnityEngine.KeyCode;
using GO = UnityEngine.GameObject;

using Int32 = System.Int32;
using GroupCollection = System.Text.RegularExpressions.GroupCollection;
using MatchCollection = System.Text.RegularExpressions.MatchCollection;
using Regex = System.Text.RegularExpressions.Regex;

public static class ActionsMethods {
    public static int idx(this Input.Actions a) {
        return (int)a;
    }
}

static public class Input {
    private const int gamepadNum = 9;
    private const int gamepadAxisNum = 10;
    private const int gamepadButtonNum = 20;

    public enum Actions {
        Left = 0,
        Right,
        Up,
        Down,
        Action,
        Reset,
        Pause,
        MouseCamera,
        CameraLeft,
        CameraRight,
        CameraUp,
        CameraDown,
        DropLedge,
        NumActions,
    };

    private enum axisType {
        positiveAxis = 0,
        negativeAxis,
        none,
    };

    private class axis {
        string input;
        KeyCode key;
        axisType type;
        bool isKey;
        string name;

        private axis() {
            /* Empty constructor */
        }

        static private Regex jsonParser = null;
        static public axis fromJson(string json) {
            if (jsonParser == null)
                jsonParser = new Regex("\"input\": \"(.*)\",.*" +
                                       "\"key\": \"(.*)\",.*" +
                                       "\"type\": \"(.*)\",.*" +
                                       "\"isKey\": \"(.*)\",.*" +
                                       "\"name\": \"(.*)\"");
            MatchCollection matches = jsonParser.Matches(json);
            GroupCollection groups = matches[0].Groups;

            string input = groups[1].Value;
            int ikey = Int32.Parse(groups[2].Value);
            KeyCode key = (KeyCode)ikey;
            int itype = Int32.Parse(groups[3].Value);
            axisType type = (axisType)itype;
            int iIsKey = Int32.Parse(groups[4].Value);
            bool isKey = (iIsKey != 0);
            string name = groups[5].Value;

            axis a = new axis();
            a.input = input;
            a.key = key;
            a.type = type;
            a.isKey = isKey;
            a.name = name;
            return a;
        }

        public string toJson() {
            int ikey = (int)this.key;
            int itype = (int)this.type;
            int iIsKey = this.isKey ? 1 : 0;

            return $"\"input\": \"{this.input}\"," +
                   $"\"key\": \"{ikey}\"," +
                   $"\"type\": \"{itype}\"," +
                   $"\"isKey\": \"{iIsKey}\"," +
                   $"\"name\": \"{this.name}\"";
        }

        public axis(string input, axisType type) {
            this.type = type;
            this.input = input;
            this.isKey = false;

            this.key = KeyCode.None;

            if (this.type == axisType.positiveAxis)
                this.name = $"{this.input} +";
            else if (this.type == axisType.negativeAxis)
                this.name = $"{this.input} -";
            else
                this.name = this.input;
        }

        public axis(KeyCode key) {
            this.key = key;
            this.isKey = true;

            this.type = axisType.none;
            this.input = "";

            this.name = $"Key: {this.key}";
        }

        override public string ToString() {
            return this.name;
        }

        private float key2axis() {
            if (DefInput.GetKey(this.key))
                return 1.0f;
            else
                return 0.0f;
        }

        private float axisVal2axis(float val) {
            if (val < -0.5f && this.type == axisType.negativeAxis)
                return -1.0f * val;
            else if (val > 0.5f && this.type != axisType.negativeAxis)
                /* Used for positive and none */
                return val;
            else
                return 0.0f;
        }

        public float GetAxis() {
            if (this.isKey)
                return this.key2axis();
            else
                return this.axisVal2axis(DefInput.GetAxis(this.input));
        }

        public float GetAxisRaw() {
            if (this.isKey)
                return this.key2axis();
            else
                return this.axisVal2axis(DefInput.GetAxisRaw(this.input));
        }

        public bool GetButton() {
            return this.GetAxisRaw() > 0.5f;
        }

        public bool GetButtonJustPressed() {
            if (this.isKey)
                return DefInput.GetKeyDown(this.key);
            else
                return DefInput.GetButtonDown(this.input);
        }
    };

    static private axis[] axis0 = {
        new axis(KeyCode.A) /* Left */,
        new axis(KeyCode.D) /* Right */,
        new axis(KeyCode.W) /* Up */,
        new axis(KeyCode.S) /* Down */,
        new axis(KeyCode.Space) /* Action */,
        new axis(KeyCode.R) /* Reset */,
        new axis(KeyCode.Escape) /* Pause */,
        new axis(KeyCode.Mouse1) /* MouseCamera */,
        new axis(KeyCode.H) /* CameraLeft */,
        new axis(KeyCode.K) /* CameraRight */,
        new axis(KeyCode.U) /* CameraUp */,
        new axis(KeyCode.J) /* CameraDown */,
        new axis(KeyCode.LeftShift) /* DropLedge */,
    };

    static private axis[] axis1 = {
        new axis("joystick 0 axis 0", axisType.negativeAxis) /* Left */,
        new axis("joystick 0 axis 0", axisType.positiveAxis) /* Right */,
        new axis("joystick 0 axis 1", axisType.negativeAxis) /* Up */,
        new axis("joystick 0 axis 1", axisType.positiveAxis) /* Down */,
        new axis("joystick 0 button 0", axisType.none) /* Action */,
        new axis("joystick 0 button 3", axisType.none) /* Reset */,
        new axis("joystick 0 button 7", axisType.none) /* Pause */,
        null /* MouseCamera */,
        new axis("joystick 0 axis 3", axisType.negativeAxis) /* CameraLeft */,
        new axis("joystick 0 axis 3", axisType.positiveAxis) /* CameraRight */,
        new axis("joystick 0 axis 4", axisType.negativeAxis) /* CameraUp */,
        new axis("joystick 0 axis 4", axisType.positiveAxis) /* CameraDown */,
        new axis("joystick 0 button 1", axisType.none) /* DropLedge */,
    };

    static private axis[] axis2 = {
        new axis("joystick 0 axis 6", axisType.negativeAxis) /* Left */,
        new axis("joystick 0 axis 6", axisType.positiveAxis) /* Right */,
        new axis("joystick 0 axis 7", axisType.negativeAxis) /* Up */,
        new axis("joystick 0 axis 7", axisType.positiveAxis) /* Down */,
        null /* Action */,
        null /* Reset */,
        null /* Pause */,
        null /* MouseCamera */,
        null /* CameraLeft */,
        null /* CameraRight */,
        null /* CameraUp */,
        null /* CameraDown */,
        null /* DropLedge */,
    };

    /* =======================================================================
     *   Remapper accessors
     * =======================================================================*/

    static private float _combineAxis(axis[] arr, int pos, int neg) {
        if (pos >= arr.Length || neg >= arr.Length ||
                arr[pos] == null || arr[neg] == null)
            return 0.0f;
        return arr[pos].GetAxis() - arr[neg].GetAxis();
    }

    static private float combineAxis(Actions pos, Actions neg) {
        float val;
        val = _combineAxis(axis0, pos.idx(), neg.idx());
        if (val == 0.0f)
            val = _combineAxis(axis1, pos.idx(), neg.idx());
        if (val == 0.0f)
            val = _combineAxis(axis2, pos.idx(), neg.idx());
        return val;
    }

    static private float _combineAxisRaw(axis[] arr, int pos, int neg) {
        if (pos >= arr.Length || neg >= arr.Length ||
                arr[pos] == null || arr[neg] == null)
            return 0.0f;
        return arr[pos].GetAxisRaw() - arr[neg].GetAxisRaw();
    }

    static private float combineAxisRaw(Actions pos, Actions neg) {
        float val;
        val = _combineAxisRaw(axis0, pos.idx(), neg.idx());
        if (val == 0.0f)
            val = _combineAxisRaw(axis1, pos.idx(), neg.idx());
        if (val == 0.0f)
            val = _combineAxisRaw(axis2, pos.idx(), neg.idx());
        return val;
    }

    static private bool _getButton(axis[] arr, int bt) {
        if (bt >= arr.Length || arr[bt] == null)
            return false;
        return arr[bt].GetButton();
    }

    static private bool combineButton(Actions bt) {
        int idx = bt.idx();
        return _getButton(axis0, idx) || _getButton(axis1, idx) ||
                _getButton(axis2, idx);
    }

    static private bool _getButtonJP(axis[] arr, int bt) {
        if (bt >= arr.Length || arr[bt] == null)
            return false;
        return arr[bt].GetButtonJustPressed();
    }

    static private bool combineButtonJustPressed(Actions bt) {
        int idx = bt.idx();
        return _getButtonJP(axis0, idx) || _getButtonJP(axis1, idx) ||
                _getButtonJP(axis2, idx);
    }

    /* =======================================================================
     *   Remapping helpers
     * =======================================================================*/

    static private axis[] getArr(int column) {
        switch (column) {
        case 0:
            return axis0;
        case 1:
            return axis1;
        case 2:
            return axis2;
        default:
            throw new System.Exception($"Invalid input column ({column})");
        }
    }

    static public bool CheckAnyKeyDown() {
        return UnityEngine.Input.anyKey;
    }

    static public bool CheckAnyKeyJustPressed() {
        return UnityEngine.Input.anyKeyDown;
    }

    static private UnityEngine.Coroutine waitFunc = null;
    static private KeyLogger waitCaller = null;

    static private CoroutineRet _waitInput(axis[] arr, Actions action) {
        int idx = action.idx();
        bool done = false;

        while (!done) {
            /* Wait until the end of the next frame */
            yield return null;

            if (waitCaller.lastKey != KeyCode.None) {
                arr[idx] = new axis(waitCaller.lastKey);
                done = true;
                break;
            }
            else {
                /* Test every option in every gamepad :grimacing: */
                for (int gpIdx = 1; !done && gpIdx < gamepadNum; gpIdx++) {
                    for (int gpAxis = 0; gpAxis < gamepadAxisNum; gpAxis++) {
                        string name = $"joystick {gpIdx} axis {gpAxis}";
                        if (DefInput.GetAxisRaw(name) > 0.8f) {
                            arr[idx] = new axis(name, axisType.positiveAxis);
                            done = true;
                            break;
                        }
                        else if (DefInput.GetAxisRaw(name) < -0.8f) {
                            arr[idx] = new axis(name, axisType.negativeAxis);
                            done = true;
                            break;
                        }
                    }
                    for (int gpBt = 0; gpBt < gamepadButtonNum; gpBt++) {
                        string name = $"joystick {gpIdx} button {gpBt}";
                        if (DefInput.GetButton(name)) {
                            arr[idx] = new axis(name, axisType.none);
                            done = true;
                            break;
                        }
                    }
                }
            }
        }

        waitFunc = null;
        waitCaller.GetComponentInChildren<KeyLogger>().enabled = false;
        waitCaller = null;
    }

    static public void WaitInput(GO caller, int column, Actions action) {
        if (waitFunc != null || waitCaller != null)
            return;

        axis[] arr = getArr(column);

        KeyLogger kl = caller.GetComponentInChildren<KeyLogger>();
        if (kl == null)
            kl = caller.AddComponent<KeyLogger>();
        kl.enabled = true;

        waitCaller = kl;
        waitFunc = kl.StartCoroutine(_waitInput(arr, action));
    }

    static public void CancelWaitInput() {
        if (waitFunc == null || waitCaller == null)
            return;

        waitCaller.StopCoroutine(waitFunc);
        waitFunc = null;
        waitCaller.GetComponentInChildren<KeyLogger>().enabled = false;
        waitCaller = null;
    }

    static public bool IsWaitingInput() {
        return (waitFunc != null);
    }

    static public void ClearAxis(Actions action, int column) {
        axis[] arr = getArr(column);

        if (action.idx() < arr.Length && arr[action.idx()] != null)
            arr[action.idx()] = null;
    }

    static public string AxisName(Actions action, int column) {
        axis[] arr = getArr(column);

        if (action.idx() < arr.Length && arr[action.idx()] != null)
            return arr[action.idx()].ToString();
        return "";
    }

    static public void RevertMap(int column) {
        switch (column) {
        case 0:
            axis[] _axis0 = {
                new axis(KeyCode.A) /* Left */,
                new axis(KeyCode.D) /* Right */,
                new axis(KeyCode.W) /* Up */,
                new axis(KeyCode.S) /* Down */,
                new axis(KeyCode.Space) /* Action */,
                new axis(KeyCode.R) /* Reset */,
                new axis(KeyCode.Escape) /* Pause */,
                new axis(KeyCode.Mouse1) /* MouseCamera */,
                new axis(KeyCode.H) /* CameraLeft */,
                new axis(KeyCode.K) /* CameraRight */,
                new axis(KeyCode.U) /* CameraUp */,
                new axis(KeyCode.J) /* CameraDown */,
                new axis(KeyCode.LeftShift) /* DropLedge */,
            };
            axis0 = _axis0;
            break;
        case 1:
            axis[] _axis1 = {
                new axis("joystick 0 axis 0", axisType.negativeAxis) /* Left */,
                new axis("joystick 0 axis 0", axisType.positiveAxis) /* Right */,
                new axis("joystick 0 axis 1", axisType.negativeAxis) /* Up */,
                new axis("joystick 0 axis 1", axisType.positiveAxis) /* Down */,
                new axis("joystick 0 button 0", axisType.none) /* Action */,
                new axis("joystick 0 button 3", axisType.none) /* Reset */,
                new axis("joystick 0 button 7", axisType.none) /* Pause */,
                null /* MouseCamera */,
                new axis("joystick 0 axis 3", axisType.negativeAxis) /* CameraLeft */,
                new axis("joystick 0 axis 3", axisType.positiveAxis) /* CameraRight */,
                new axis("joystick 0 axis 4", axisType.negativeAxis) /* CameraUp */,
                new axis("joystick 0 axis 4", axisType.positiveAxis) /* CameraDown */,
                new axis("joystick 0 button 1", axisType.none) /* DropLedge */,
            };
            axis1 = _axis1;
            break;
        case 2:
            axis[] _axis2 = {
                new axis("joystick 0 axis 6", axisType.negativeAxis) /* Left */,
                new axis("joystick 0 axis 6", axisType.positiveAxis) /* Right */,
                new axis("joystick 0 axis 7", axisType.negativeAxis) /* Up */,
                new axis("joystick 0 axis 7", axisType.positiveAxis) /* Down */,
                null /* Action */,
                null /* Reset */,
                null /* Pause */,
                null /* MouseCamera */,
                null /* CameraLeft */,
                null /* CameraRight */,
                null /* CameraUp */,
                null /* CameraDown */,
                null /* DropLedge */,
            };
            axis2 = _axis2;
            break;
        default:
            throw new System.Exception("Invalid input map");
        }
    }

    static private string _axisToJson(axis[] _axis) {
        string json = "[";
        for (int i = 0; i < _axis.Length; i++) {
            json += "{";
            if (_axis[i] != null)
                json += _axis[i].toJson();
            json += "},";
        }
        json += "]";

        return json;
    }

    static public string axisToJson(int column) {
        switch (column) {
        case 0:
            return _axisToJson(axis0);
        case 1:
            return _axisToJson(axis1);
        case 2:
            return _axisToJson(axis2);
        default:
            throw new System.Exception("Invalid input map");
        }
    }

    static private Regex jsonParser = null;
    static public void axisFromJson(int column, string json) {
        if (jsonParser == null)
            jsonParser = new Regex("{([^}]*)},+");
        MatchCollection matches = jsonParser.Matches(json);
        if (matches.Count != (int)Actions.NumActions)
            throw new System.Exception("Invalid number of actions in the configuration file");

        axis[] _axis = new axis[matches.Count];
        for (int i = 0; i < matches.Count; i++) {
            int len = matches[i].Value.Length - 3;
            string subJson = matches[i].Value.Substring(1, len);

            if (subJson.Length > 0)
                _axis[i] = axis.fromJson(subJson);
            else
                _axis[i] = null;
        }

        switch (column) {
        case 0:
            axis0 = _axis;
            break;
        case 1:
            axis1 = _axis;
            break;
        case 2:
            axis2 = _axis;
            break;
        default:
            throw new System.Exception("Invalid input map");
        }
    }

    /* =======================================================================
     *   Controller getters
     * =======================================================================*/

    static public float GetHorizontalAxis() {
        return combineAxis(Actions.Right, Actions.Left);
    }

    static public float GetVerticalAxis() {
        return combineAxis(Actions.Up, Actions.Down);
    }

    static public bool GetActionButton() {
        return combineButton(Actions.Action);
    }

    static public bool GetResetButton() {
        return combineButtonJustPressed(Actions.Reset);
    }

    static public bool GetPauseDown() {
        return combineButton(Actions.Pause);
    }

    static public bool GetPauseJustPressed() {
        return combineButtonJustPressed(Actions.Pause);
    }

    static public bool GetMouseCameraEnabled() {
        return combineButton(Actions.MouseCamera);
    }

    static public float GetCameraX() {
        return combineAxis(Actions.CameraRight, Actions.CameraLeft);
    }

    static public float GetCameraY() {
        return combineAxis(Actions.CameraUp, Actions.CameraDown);
    }

    static public UnityEngine.Vector3 GetMousePosition() {
        return UnityEngine.Input.mousePosition;
    }

    static public bool GetDropLedgeButton() {
        return combineButtonJustPressed(Actions.DropLedge);
    }

    /* =======================================================================
     *   Menu getters
     * =======================================================================*/

    static public bool MenuLeft() {
        return DefInput.GetAxisRaw("joystick 0 axis 0") < -0.7f ||
                DefInput.GetAxisRaw("joystick 0 axis 6") < -0.7f ||
                DefInput.GetKey("left") ||
                Input.GetHorizontalAxis() < -0.7f;
    }

    static public bool MenuRight() {
        return DefInput.GetAxisRaw("joystick 0 axis 0") > 0.7f ||
                DefInput.GetAxisRaw("joystick 0 axis 6") > 0.7f ||
                DefInput.GetKey("right") ||
                Input.GetHorizontalAxis() > 0.7f;
    }

    static public bool MenuUp() {
        return DefInput.GetAxisRaw("joystick 0 axis 1") < -0.7f ||
                DefInput.GetAxisRaw("joystick 0 axis 7") < -0.7f ||
                DefInput.GetKey("up") ||
                Input.GetVerticalAxis() > 0.7f;
    }

    static public bool MenuDown() {
        return DefInput.GetAxisRaw("joystick 0 axis 1") > 0.7f ||
                DefInput.GetAxisRaw("joystick 0 axis 7") > 0.7f ||
                DefInput.GetKey("down") ||
                Input.GetVerticalAxis() < -0.7f;
    }

    static public bool MenuSelect() {
        return DefInput.GetAxisRaw("joystick 0 button 0") > 0.7f ||
                DefInput.GetKey("return");
    }

    static public bool MenuCancel() {
        return DefInput.GetAxisRaw("joystick 0 button 1") > 0.7f ||
                DefInput.GetKey("escape");
    }
}
