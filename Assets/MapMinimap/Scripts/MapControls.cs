using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapMinimap
{

    /// <summary>
    /// Keyboard controls manager
    /// </summary>

    public class MapControls : MonoBehaviour
    {
        public KeyCode accept_key = KeyCode.Return;
        public KeyCode cancel_key = KeyCode.Backspace;
        public KeyCode map_key = KeyCode.M;

        public KeyCode zoom_in_key = KeyCode.E;
        public KeyCode zoom_out_key = KeyCode.Q;

        public delegate Vector2 MoveAction();
        public delegate bool PressAction();

        [HideInInspector]
        public bool gamepad_linked = false;
        public MoveAction gamepad_move;
        public MoveAction gamepad_zoom; //Triggers
        public PressAction gamepad_accept; //A
        public PressAction gamepad_cancel; //B
        public PressAction gamepad_map; //Y
        public System.Action gamepad_update;

        private Vector3 move;
        private float zoom_val;
        private bool press_accept;
        private bool press_cancel;
        private bool press_map;
        private bool paused = false;

        private bool is_zoom_mode = false;
        private float zoom_val_touch;
        private Vector2 prev_touch1;
        private Vector2 prev_touch2;

        private float use_mouse_timer = 0f;
        private float use_key_timer = 0f;
        private Vector3 mouse_pos;

        private static MapControls _instance;

        void Awake()
        {
            _instance = this;
        }

        private void Start()
        {

        }

        void Update()
        {
            move = Vector3.zero;
            zoom_val = 0f;
            zoom_val_touch = 0f;
            press_accept = false;
            press_cancel = false;
            press_map = false;

            if (paused)
                return;

            if (Input.GetKey(KeyCode.A))
                move += Vector3.left;
            if (Input.GetKey(KeyCode.D))
                move += Vector3.right;
            if (Input.GetKey(KeyCode.W))
                move += Vector3.up;
            if (Input.GetKey(KeyCode.S))
                move += Vector3.down;

            if (Input.GetKey(KeyCode.LeftArrow))
                move += Vector3.left;
            if (Input.GetKey(KeyCode.RightArrow))
                move += Vector3.right;
            if (Input.GetKey(KeyCode.UpArrow))
                move += Vector3.up;
            if (Input.GetKey(KeyCode.DownArrow))
                move += Vector3.down;

            if (Input.GetKey(zoom_in_key))
                zoom_val += 1f;
            if (Input.GetKey(zoom_out_key))
                zoom_val += -1f;

            if (Input.GetKeyDown(accept_key))
                press_accept = true;
            if (Input.GetKeyDown(cancel_key))
                press_cancel = true;
            if (Input.GetKeyDown(map_key))
                press_map = true;

            if (gamepad_linked)
            {
                Vector2 gmove = gamepad_move.Invoke();
                move += new Vector3(gmove.x, gmove.y, 0f);
                zoom_val += gamepad_zoom.Invoke().y;
                press_accept = press_accept || gamepad_accept.Invoke();
                press_cancel = press_cancel || gamepad_cancel.Invoke();
                press_map = press_map || gamepad_map.Invoke();
                gamepad_update?.Invoke();
            }

            move = move.normalized * Mathf.Min(move.magnitude, 1f);

            //Check if use mouse
            use_mouse_timer += Time.deltaTime;
            use_key_timer += Time.deltaTime;
            if((mouse_pos - Input.mousePosition).magnitude > 0.1f)
                use_mouse_timer = 0f;
            if(Input.GetMouseButton(0))
                use_mouse_timer = 0f;
            if (move.magnitude > 0.1f || press_accept)
                use_key_timer = 0f;
            mouse_pos = Input.mousePosition;

            //Touch zoom with 2 fingers
            if (Input.touchCount == 2)
            {
                Vector2 pos1 = Input.GetTouch(0).position;
                Vector2 pos2 = Input.GetTouch(1).position;
                if (is_zoom_mode)
                {
                    float distance = Vector2.Distance(pos1, pos2);
                    float prev_distance = Vector2.Distance(prev_touch1, prev_touch2);
                    zoom_val_touch = (distance - prev_distance) / (float)Screen.height;
                }
                prev_touch1 = pos1;
                prev_touch2 = pos2;
                is_zoom_mode = true; //Wait one frame to make sure distance has been calculated once
            }
            else
            {
                is_zoom_mode = false;
            }
        }

        public bool IsMoving()
        {
            return move.magnitude > 0.1f;
        }

        public bool IsPressAccept()
        {
            return press_accept;
        }

        public bool IsPressCancel()
        {
            return press_cancel;
        }

        public bool IsPressMap()
        {
            return press_map;
        }

        public Vector3 GetMove()
        {
            return move;
        }

        public float GetZoomValue()
        {
            return zoom_val;
        }

        public float GetZoomTouchValue()
        {
            return zoom_val_touch;
        }

        public bool IsPaused()
        {
            return paused;
        }

        public bool IsUseMouse()
        {
            return Input.mousePresent && use_mouse_timer < use_key_timer + 1f;
        }

        public static MapControls Get()
        {
            return _instance;
        }
    }

}