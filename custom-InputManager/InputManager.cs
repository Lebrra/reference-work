using System.Collections.Generic;
using UnityEngine;

public static class InputManager
{
    /// My InputManger that will replaces Unity's 
    /// (This class does not need an instance in the scene to be used/referenced)
    /// InputManager.ASSET is a mapping of all controller keys referenced here via strings

    // Inputs count 
    const int inputsCount = 4;

    // Dictionaries only this script references
    static Dictionary<string, KeyCode> keyMappings;
    static Dictionary<string, KeyCode> keyAltMappings;
    static Dictionary<string, string> ps4Mappings;
    static Dictionary<string, string> xBoxMappings;
    static Dictionary<string, string> ps4AltMappings;
    static Dictionary<string, string> xBoxAltMappings;

    // INPUT NAMES
    /// index MUST match KeyCode index
    /// axes get two keys; positive and negative ** must ALWAYS end with Pos and Neg
    static string[] keyNames = new string[inputsCount]
    {
        "Input1",
        "Input2",
        "AxisPos",
        "AxisNeg"
    };

    // PRIMARY KEYS
    /// index MUST match Name index
    static KeyCode[] keys = new KeyCode[inputsCount]
    {
        KeyCode.Key,
        KeyCode.Key,
        KeyCode.Up,
        KeyCode.Down
    };

    // ALTERNATE KEYS
    /// index MUST match Name index
    /// None = no alternate key
    static KeyCode[] keysAlt = new KeyCode[inputsCount]
    {
        KeyCode.Key,
        KeyCode.None,
        KeyCode.None,
        KeyCode.None
    };

    // PS4 PRIMARY BUTTONS
    /// see InputManager.ASSET
    /// axes calls end with '-' or '+' accordingly
    static string[] ps4Keys = new string[inputsCount]
    {
        "ControllerButton",
        "ControllerButton",
        "ControllerAxis3+",
        "ControllerAxis3-"
    };
    // PS4 ALTERNATE BUTTONS
    /// none = no alternate button
    static string[] ps4AltKeys = new string[inputsCount]
    {
        "ControllerAxis7+",
        "none",
        "none",
        "none"
    };

    // XBOX PRIMARY BUTTONS
    /// see InputManager.ASSET
    /// axes calls end with '-' or '+' accordingly
    static string[] xBoxKeys = new string[inputsCount]
    {
        "ControllerHorizontal+",
        "ControllerButton",
        "ControllerAxis3+",
        "ControllerAxis3-"
    };
    // XBOX ALTERNATE BUTTONS
    /// none = no alternate button
    static string[] xBoxAltKeys = new string[inputsCount]
    {
        "ControllerAxis6+",
        "none",
        "none",
        "none"
    };

    // Buttons pressed dictionary (bools)
    /// For controller GetKeyDown and GetKeyUp
    static Dictionary<string, bool> activeKeys;

    // bools for active controller types
    static bool usingKeyboard;
    static bool usingPS4;
    static bool usingXbox;

    // constructor, calls once on game startup
    static InputManager()
    {
        setDefaultDictionaries();   // for saving later
        DetectControllerType();
    }

    static void setDefaultDictionaries()
    {
        keyMappings = new Dictionary<string, KeyCode>();
        keyAltMappings = new Dictionary<string, KeyCode>();
        ps4Mappings = new Dictionary<string, string>();
        xBoxMappings = new Dictionary<string, string>();
        ps4AltMappings = new Dictionary<string, string>();
        xBoxAltMappings = new Dictionary<string, string>();
        activeKeys = new Dictionary<string, bool>();

        for (int i = 0; i < inputsCount; i++)
        {
            keyMappings.Add(keyNames[i], keys[i]);
            keyAltMappings.Add(keyNames[i], keysAlt[i]);
            ps4Mappings.Add(keyNames[i], ps4Keys[i]);
            ps4AltMappings.Add(keyNames[i], ps4AltKeys[i]);
            xBoxMappings.Add(keyNames[i], xBoxKeys[i]);
            xBoxAltMappings.Add(keyNames[i], xBoxAltKeys[i]);
            activeKeys.Add(keyNames[i], false);
        }
    }

    #region Button Inputs

    // Public standard methods:
    public static bool GetKeyDown(string str)
    {
        if (keyMappings.ContainsKey(str))
        {
            bool value = false;

            if (usingPS4)
            {
                if (GetKeyDownController(str, 0)) value = true;
                else value = GetKeyDownController(str, 2);

                if (value) activeKeys[str] = true;
            }

            if (usingXbox)
            {
                value = GetKeyDownController(str, 1);
                if (!value) value = GetKeyDownController(str, 3);
                if (value) activeKeys[str] = true;
            }

            if (usingKeyboard)
            {
                if (!value) value = GetKeyDownKeyboard(str);
                if (!value) value = GetKeyDownAltKeyboard(str);
            }

            if (usingPS4 || usingXbox) GetKey(str); // reset activeKeys[]

            return value;
        }
        else
        {
            Debug.LogError("Input name not found.");
            return false;
        }
    }

    public static bool GetKey(string str)
    {
        if (keyMappings.ContainsKey(str))
        {
            bool value = false;

            if (usingPS4)
            {
                if (GetKeyController(str, 0)) value = true;
                else value = GetKeyController(str, 2);
                activeKeys[str] = value;
            }

            if (usingXbox)
            {
                value = GetKeyController(str, 1);
                if (!value) value = GetKeyController(str, 3);
                activeKeys[str] = value;
            }

            if (usingKeyboard)
            {
                if (!value) value = GetKeyKeyboard(str);
                if (!value) value = GetKeyAltKeyboard(str); //alternate key
            }

            return value;
        }
        else
        {
            Debug.LogError("Input name not found.");
            return false;
        }
    }

    public static bool GetKeyUp(string str)
    {
        if (keyMappings.ContainsKey(str))
        {
            bool value = false;

            if (usingPS4)
            {
                if (GetKeyUpController(str, 0)) value = true;
                else value = GetKeyUpController(str, 2);

                if (value && activeKeys[str])
                {
                    activeKeys[str] = false; 
                }
            }

            if (usingXbox)
            {
                value = GetKeyUpController(str, 1);
                if (!value) value = GetKeyUpController(str, 3);

                if (value) activeKeys[str] = false;
            }

            if (usingKeyboard)
            {
                if (!value) value = GetKeyUpKeyboard(str);
                if (!value) value = GetKeyUpAltKeyboard(str);
            }

            if (usingPS4 || usingXbox) GetKey(str); // reset activeKeys[]

            return value;
        }
        else
        {
            Debug.LogError("Input name not found.");
            return false;
        }
    }

    public static float GetAxis(string str)
    {
        string axis = str.Remove(str.Length - 3, 3);

        if (keyMappings.ContainsKey(str))   // found string
        {
            float value = 0;

            if (usingKeyboard)
            {
                if (Input.GetKey(keyMappings[axis + "Pos"])) return 1;
                else if (Input.GetKey(keyMappings[axis + "Neg"])) return -1;
                else value = 0;

                if (keyAltMappings[str] != KeyCode.None && value == 0)
                {
                    if (Input.GetKey(keyAltMappings[axis + "Pos"])) return 1;
                    else if (Input.GetKey(keyAltMappings[axis + "Neg"])) return -1;
                    else value = 0;
                }
            }

            if (usingPS4) //GetKeyController(axis + "Pos", 0)
            {
                if (GetKeyController(axis + "Pos", 0)) return 1;
                else if (GetKeyController(axis + "Neg", 0)) return -1;
                else value = 0;

                if(ps4AltMappings[str] != "none")
                {
                    if (GetKeyController(axis + "Pos", 2)) return 1;
                    else if (GetKeyController(axis + "Neg", 2)) return -1;
                    else value = 0;
                }
            }

            if (usingXbox)
            {
                if (GetKeyController(axis + "Pos", 1)) return 1;
                else if (GetKeyController(axis + "Neg", 1)) return -1;
                else value = 0;

                if (xBoxAltMappings[str] != "none")
                {
                    if (GetKeyController(axis + "Pos", 3)) return 1;
                    else if (GetKeyController(axis + "Neg", 3)) return -1;
                    else value = 0;
                }
            }

            return value;
        }
        else
        {
            Debug.LogError("Input name not found.");
            return 0;
        }
    }



    // Private methods for keyboards:
    private static bool GetKeyDownKeyboard(string str)
    {
        return Input.GetKeyDown(keyMappings[str]);
    }
    private static bool GetKeyDownAltKeyboard(string str)
    {
        if (keyAltMappings[str] != KeyCode.None)
            return Input.GetKeyDown(keyAltMappings[str]);
        else return false;
    }

    private static bool GetKeyKeyboard(string str)
    {
        return Input.GetKey(keyMappings[str]);
    }
    private static bool GetKeyAltKeyboard(string str)
    {
        if (keyAltMappings[str] != KeyCode.None)
            return Input.GetKey(keyAltMappings[str]);
        else return false;
    }

    private static bool GetKeyUpKeyboard(string str)
    {
        return Input.GetKeyUp(keyMappings[str]);
    }
    private static bool GetKeyUpAltKeyboard(string str)
    {
        if (keyAltMappings[str] != KeyCode.None)
            return Input.GetKeyUp(keyAltMappings[str]);
        else return false;
    }


    //Private methods for controllers:
    private static bool GetKeyController(string str, int controllerType)
    {
        string myInput;
        if (controllerType == 0) myInput = ps4Mappings[str];    // 0 = ps4, 1 = xbox, 2 = ps4Alt, 3 = xBoxAlt
        else if(controllerType == 1) myInput = xBoxMappings[str];
        else if(controllerType == 2) myInput = ps4AltMappings[str];
        else myInput = xBoxAltMappings[str];

        if (myInput == "none") return false;

        char value = myInput[myInput.Length - 1];
        myInput = myInput.Remove(myInput.Length - 1);

        if (value == '+')
        {
            if (Input.GetAxisRaw(myInput) > 0.99F) return true;     // will use input when joystick is over half
            else return false;
        }
        else if (value == '-')
        {
            if (Input.GetAxisRaw(myInput) < -0.99F) return true;
            else return false;
        }

        if (controllerType == 0) return Input.GetButton(ps4Mappings[str]);
        else if(controllerType == 1) return Input.GetButton(xBoxMappings[str]);
        else if(controllerType == 2) return Input.GetButton(ps4AltMappings[str]);
        else return Input.GetButton(xBoxAltMappings[str]);
    }
    private static bool GetKeyDownController(string str, int controllerType)
    {
        string myInput;
        if (controllerType == 0) myInput = ps4Mappings[str];
        else if (controllerType == 1) myInput = xBoxMappings[str];
        else if (controllerType == 2) myInput = ps4AltMappings[str];
        else myInput = xBoxAltMappings[str];

        if (myInput == "none") return false;

        char value = myInput[myInput.Length - 1];
        myInput = myInput.Remove(myInput.Length - 1);

        if (value == '+')
        {
            if (Input.GetAxisRaw(myInput) > 0.99F && !activeKeys[str]) return true;
            else return false;
        }
        else if (value == '-')
        {
            if (Input.GetAxisRaw(myInput) < -0.99F && !activeKeys[str]) return true;
            else return false;
        }

        if (controllerType == 0) return Input.GetButtonDown(ps4Mappings[str]);
        else if (controllerType == 1) return Input.GetButtonDown(xBoxMappings[str]);
        else if (controllerType == 2) return Input.GetButtonDown(ps4AltMappings[str]);
        else return Input.GetButtonDown(xBoxAltMappings[str]);
    }
    private static bool GetKeyUpController(string str, int controllerType)
    {
        string myInput;
        if (controllerType == 0) myInput = ps4Mappings[str];
        else if (controllerType == 1) myInput = xBoxMappings[str];
        else if (controllerType == 2) myInput = ps4AltMappings[str];
        else myInput = xBoxAltMappings[str];

        if (myInput == "none") return false;

        char value = myInput[myInput.Length - 1];
        myInput = myInput.Remove(myInput.Length - 1);

        if (value == '+')
        {
            float input = Input.GetAxisRaw(myInput);
            if (input < 0.99F && activeKeys[str]) return true;
            else return false;
        }
        else if (value == '-')
        {
            float input = Input.GetAxisRaw(myInput);
            if (input > -0.99F && activeKeys[str]) return true;
            else return false;
        }

        if (controllerType == 0) return Input.GetButtonUp(ps4Mappings[str]);
        else if (controllerType == 1) return Input.GetButtonUp(xBoxMappings[str]);
        else if (controllerType == 2) return Input.GetButtonUp(ps4AltMappings[str]);
        else return Input.GetButtonUp(xBoxAltMappings[str]);
    }

    #endregion

    // Setting new input values:
    public static void SetNewKey(string str, KeyCode key)
    {
        if (keyMappings.ContainsKey(str))
        {
            keyMappings[str] = key;
        }
    }

    public static void SetNewButton(string str, string key) // right now only ps4 changes
    {
        if (ps4Mappings.ContainsKey(str))
        {
            ps4Mappings[str] = key;
        }
    }

    public static string GetKeyFromIndex(int i)
    {
        return keyNames[i];
    }

    public static string GetKeyValue(string inputName)
    {
        if (keyMappings.ContainsKey(inputName))
        {
            return keyMappings[inputName].ToString();
        }
        else
        {
            Debug.LogError("Input name not found.");
            return null;
        }
    }


    // Finding controller type
    public static void DetectControllerType()
    {
        // find active controller types and set bools
        usingKeyboard = true;
        usingXbox = usingPS4 = false;

        // is a controller plugged in?
        if(Input.GetJoystickNames().Length > 0)
        {
            // comparing controllers by length of name:
            // PS4 = 19
            // xbox = 33

            foreach(string c in Input.GetJoystickNames())
            {
                if (c.Length == 19) // ps4 controller
                {
                    Debug.Log("PS4 controller found, PS4 controls enabled.");
                    usingPS4 = true;
                }
                if(c.Length == 33)  // xbox controller
                {
                    Debug.Log("Xbox controller found, Xbox controls enabled.");
                    usingXbox = true;
                }
            }
        }
    }
}
