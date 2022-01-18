using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapMinimap.Demo;

#if IN_CONTROL
using InControl;
#endif

namespace MapMinimap
{
    /// <summary>
    /// Wrapper class for integrating InControl
    /// </summary>

    public class InControlWrap : MonoBehaviour
    {
#if IN_CONTROL

        public InputControlType accept = InputControlType.Action1;
        public InputControlType cancel = InputControlType.Action2;
        public InputControlType map = InputControlType.Action4;

        public InputControlType zoom_out = InputControlType.LeftTrigger;
        public InputControlType zoom_in = InputControlType.RightTrigger;

        [Header("Load InControl Manager Prefab")]
        public GameObject in_control_manager;

        private InputDevice active_device;

        private void Awake()
        {
            //Add InControl Manager to scene
            if (!FindObjectOfType<InControlManager>())
            {
                if (in_control_manager != null)
                {
                    Instantiate(in_control_manager);
                }
                else
                {
                    GameObject incontrol = new GameObject("InControl");
                    incontrol.AddComponent<InControlManager>();
                }
            }
        }

        void Start()
        {
            active_device = InputManager.ActiveDevice;

            MapControls controls = MapControls.Get();
            if (controls != null)
            {
                controls.gamepad_linked = true;
                controls.gamepad_map = () => { return WasPressed(active_device, map); };
                controls.gamepad_accept = () => { return WasPressed(active_device, accept); };
                controls.gamepad_cancel = () => { return WasPressed(active_device, cancel); };
                controls.gamepad_move = () => { return GetTwoAxis(active_device, InputControlType.LeftStickX, InputControlType.LeftStickY); };
                controls.gamepad_zoom = () => { return new Vector2(0f, -GetAxis(active_device, zoom_out) + GetAxis(active_device, zoom_in))
                    + new Vector2(0f, GetAxis(active_device, InputControlType.RightStickY)); };
            }

            PlayerControlsDemo controls_demo = PlayerControlsDemo.Get();
            if (controls_demo != null)
            {
                controls_demo.gamepad_linked = true;
                controls_demo.gamepad_action = () => { return WasPressed(active_device, accept); };
                controls_demo.gamepad_move = () => { return GetTwoAxis(active_device, InputControlType.LeftStickX, InputControlType.LeftStickY); };
            }
        }

        void Update()
        {
            active_device = InputManager.ActiveDevice;
        }

        private bool WasPressed(InputDevice device, InputControlType type)
        {
            if(device != null)
                return device.GetControl(type).WasPressed;
            return false;
        }

        private float GetAxis(InputDevice device, InputControlType type)
        {
            if (device != null)
                return device.GetControl(type).Value;
            return 0f;
        }

        private Vector2 GetTwoAxis(InputDevice device, InputControlType typeX, InputControlType typeY)
        {
            return new Vector2(GetAxis(device, typeX), GetAxis(device, typeY));
        }

        private float GetAxisPress(InputDevice device, InputControlType type)
        {
            if (device != null)
            {
                InputControl control = device.GetControl(type);
                return control.WasPressed ? control.Value : 0f;
            }
            return 0f;
        }

        private Vector2 GetTwoAxisPress(InputDevice device, InputControlType typeX, InputControlType typeY)
        {
            return new Vector2(GetAxisPress(device, typeX), GetAxisPress(device, typeY));
        }


        private float GetAxisThreshold(InputDevice device, InputControlType type)
        {
            if (device != null)
            {
                InputControl control = device.GetControl(type);
                return Mathf.Abs(control.LastValue) < 0.5f && Mathf.Abs(control.Value) >= 0.5f ? Mathf.Sign(control.Value) : 0f;
            }
            return 0f;
        }

        private Vector2 GetTwoAxisThreshold(InputDevice device, InputControlType typeX, InputControlType typeY)
        {
            return new Vector2(GetAxisThreshold(device, typeX), GetAxisThreshold(device, typeY));
        }
#endif
    }
}