- Custom Input Manager using Unity default Input Manager -
----------------------------------------------------------

About:

This static script uses default values entered into Unity's InputManager for a more versatile system that offers various controller support and custom key-mapping.
I originally wanted to write a whole editor script to pair with this, but for now it is just the cs file. 

----------------------------------------------------------

Setup:

Make sure your Unity project is using the original Input Manager (a new package came out sometime in 2019). 
Follow the image 'UnityInputManager-AllMaps.png' for creating each input. 
The values of each input should be as followed: 				(*if not specified, leave blank OR default for gravity/dead/sensitivity)

-ALL-
Joy Num: Get Motion from all Joysticks 		- allow any controllers detected/keyboard mouse input at all times (*currently only allows for one player)

-ControllerHorizontal-
Type: Joystick Axis
Axis: X Axis

-ControllerVertical-
Type: Joystick Axis
Axis: Y Axis

-ControllerAxis3-
Type: Joystick Axis
Axis: 3rd axis (Joysticks and Scrollwheel)

-ControllerAxis4-
Type: Joystick Axis
Axis: 4th axis (Joysticks)

-ControllerAxis5-
Type: Joystick Axis
Axis: 5th axis (Joysticks)

-ControllerAxis6-
Type: Joystick Axis
Axis: 6th axis (Joysticks)

-ControllerAxis7-
Type: Joystick Axis
Axis: 7th axis (Joysticks)

-ControllerAxis8-
Type: Joystick Axis
Axis: 8th axis (Joysticks)


-ControllerButton0-
Positive Button: joystick button 0
Type: Key or Mouse Button

-ControllerButton1-
Positive Button: joystick button 1
Type: Key or Mouse Button

-ControllerButton2-
Positive Button: joystick button 2
Type: Key or Mouse Button

-ControllerButton3-
Positive Button: joystick button 3
Type: Key or Mouse Button

-ControllerButton4-
Positive Button: joystick button 4
Type: Key or Mouse Button

-ControllerButton5-
Positive Button: joystick button 5
Type: Key or Mouse Button

-ControllerButton6-
Positive Button: joystick button 6
Type: Key or Mouse Button

-ControllerButton7-
Positive Button: joystick button 7
Type: Key or Mouse Button

-ControllerButton8-
Positive Button: joystick button 8
Type: Key or Mouse Button

-ControllerButton9-
Positive Button: joystick button 9
Type: Key or Mouse Button

-ControllerButton10-
Positive Button: joystick button 10
Type: Key or Mouse Button

-ControllerButton11-
Positive Button: joystick button 11
Type: Key or Mouse Button

-ControllerButton12-
Positive Button: joystick button 12
Type: Key or Mouse Button

-ControllerButton13-
Positive Button: joystick button 13
Type: Key or Mouse Button


Any button pushed on a controller can now be referenced in code, so we can move on to editing the InputManager.cs

----------------------------------------------------------

InputManager.cs Editing:

Each input needs to be added to a number of arrays in InputManager.cs. 

**For each value in the array, the index value will match all inputs to a single name, for example the first element of each array will line up with its inputs.
  Because of this, all arrays need to have the same amount of elements (there should be a default 'none' equivalent for each array).

To add an input, write the name of the input in keyNames (see examples "Input1", Input2). This is the string name that will be referenced in other scripts.
**Creating an axis:
  I have some special wording for axes, which allows for any button to behave as an axis. (see example "AxisPos", "AxisNeg")
  The main thing is there needs to be two inputs, "---pos" and "---neg". If the --- name is not identical, it will not function properly.

Next, add its KeyCode value in keys. This is the input using a keyboard.

There is also an optional altKeys array, if not needed use KeyCode.None for the element. (do not skip it!)

Now to add ps4Keys for the input. This is a string value that matches the inputs we wrote in the Unity Editor earlier. 
  (To know which button to map, use Google Images to find a controller mapping)

**Using an axis as a button
  For example, pushing a joystick down to crouch or using the back bumpers (considered joysticks)
  Any axis can be used as a button, but needs to have a '-' or '+' attached to the end. (Example: "ControllerAxis3+")

The rest is similar to the ps4Keys; there are xBoxKeys and alternates for both to fill in.
  (for none, write "none" in any of these four arrays)

----------------------------------------------------------

Calling from InputManager:

This is the easiest part! Instead of using 'Input.Get___' call 'InputManager.Get___' as long as the InputManager.cs is somewhere in the Assets folder.
The string parameter is for the name of the input created in the first array keyNames.

Here are the methods that can be called:

bool InputManager.GetKeyDown(string) - returns true the frame input is pressed

bool InputManager.GetKey(string) - returns true if input is pressed (constant check)

bool InputManager.GetKeyUp(string) - returns true the fram input is released

float InputManager.GetAxis(string) - returns -1, 0, or 1 depending on current axis value
      **for this string, write the keyName string WITHOUT the Pos or Neg at the end!
      **this works as a GetAxisRaw, even though that is not what it is called

void InputManager.DetectControllerType() - method that searches for controller types available
      **this needs to be called on start to enable controllers
        it can also be used whenever controllers need to be calibrated (if someone plugs in a controller after the game has launched)


Other Methods:

The other methods within this script are more experiemental and unfinished, or just not immediately useful.
They were features I saw potential for, but never got around to needing in my project. I would like to come back and finish these.

void InputManager.SetNewKey(string, KeyCode) - allows keymapping changes for given input with keyboard (does not save on close)

void InputManager.SetNewButton(string, string) - allow keymapping changes of a given input for controller, currently this only changes ps4 on runtime (does not save on close)

string InputManager.GetKeyFromIndex(int) - get the input name from an index value

string InputManager.GetKeyValue(string) - get the keyboard key name from input name

----------------------------------------------------------

Hopefully I wil come back to this and clean it up better, but even if I don't I hope someone finds it useful!

Created: May 2020
- Leah Blasczyk