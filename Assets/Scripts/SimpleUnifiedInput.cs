using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum JumpingState { NotJumping, LeftShoulder, RightShoulder, KeyboardSpace };

public static class SimpleUnifiedInput
{    
    public static JumpingState Jump {     
        get
        {
            if (Gamepad.current != null)
            {
                if (Gamepad.current.rightShoulder.isPressed) return JumpingState.RightShoulder;
                if (Gamepad.current.leftShoulder.isPressed) return JumpingState.LeftShoulder;
            }

            if (Keyboard.current?.spaceKey.isPressed ?? false)
            {
                return JumpingState.KeyboardSpace;
            }
            return JumpingState.NotJumping;
        }
    }

    public static bool CheckJump(JumpingState state)
    {
        switch (state)
        {
            case JumpingState.NotJumping:
                return Jump == JumpingState.NotJumping;
            case JumpingState.KeyboardSpace:
                return Keyboard.current?.spaceKey.isPressed ?? false;
            case JumpingState.LeftShoulder:
                return Gamepad.current?.leftShoulder.isPressed ?? false;
            case JumpingState.RightShoulder:
                return Gamepad.current?.rightShoulder.isPressed ?? false;
            default:
                return false;
        }
    }

    private static Vector2 KeyBoardStick(Key left, Key up, Key right, Key down)
    {
        if (Keyboard.current == null) return Vector2.zero;
        return new Vector2(
            (Keyboard.current[left].isPressed ? -1 : 0) + (Keyboard.current[right].isPressed ? 1 : 0),
            (Keyboard.current[up].isPressed ? 1 : 0) + (Keyboard.current[down].isPressed ? -1 : 0)
        );
    }

    public static Vector2 VirtualPrimaryStick(float threshold, out bool fromKeyboard)
    {
        Vector2 gamepad = Gamepad.current?.leftStick.ReadValue() ?? Vector2.zero;
        if (gamepad.magnitude > threshold)
        {
            fromKeyboard = false;
            return gamepad;
        }
        Vector2 keyboard = KeyBoardStick(Key.A, Key.W, Key.D, Key.S);
        if (keyboard.magnitude > threshold)
        {
            fromKeyboard = true;
            return keyboard;
        }
        fromKeyboard = false;
        return Vector2.zero;
    }

    public static Vector2 VirtualSecondaryStick(float threshold, out bool fromKeyboard)
    {
        Vector2 gamepad = Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;
        if (gamepad.magnitude > threshold)
        {
            fromKeyboard = false;
            return gamepad;
        }
        Vector2 keyboard = KeyBoardStick(Key.J, Key.I, Key.L, Key.K);
        if (keyboard.magnitude > threshold)
        {
            fromKeyboard = true;
            return keyboard;
        }
        fromKeyboard = false;
        return Vector2.zero;
    }
}
