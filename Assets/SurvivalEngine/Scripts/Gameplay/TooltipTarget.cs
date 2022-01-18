using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalEngine
{
    public enum TooltipTargetType
    {
        Automatic=0,
        Custom=10,
    }

    [RequireComponent(typeof(Selectable))]
    public class TooltipTarget : MonoBehaviour
    {
        public TooltipTargetType type;

        [Header("Custom")]
        public string title;
        public Sprite icon;
        [TextArea(3,5)]
        public string text;

        private Selectable select;
        private Construction construct;
        private Plant plant;
        private Item item;
        private Character character;

        void Awake()
        {
            select = GetComponent<Selectable>();

            construct = GetComponent<Construction>();
            plant = GetComponent<Plant>();
            item = GetComponent<Item>();
            character = GetComponent<Character>();
        }

        void Update()
        {
            if (TooltipUI.Get() != null) {

                PlayerControlsMouse mouse = PlayerControlsMouse.Get();
                if (select.IsHovered() && !mouse.IsMovingMouse(0.25f) && TooltipUI.Get().GetTarget() != this) {

                    if (type == TooltipTargetType.Custom)
                    {
                        TooltipUI.Get().Set(select, title, text, icon);
                    }
                    else
                    {
                        if (construct != null)
                            TooltipUI.Get().Set(select, construct.data);
                        else if (plant != null)
                            TooltipUI.Get().Set(select, plant.data);
                        else if (item != null)
                            TooltipUI.Get().Set(select, item.data);
                        else if (character != null)
                            TooltipUI.Get().Set(select, character.data);
                        else
                            TooltipUI.Get().Set(select, title, text, icon);
                    }
                }
            }
        }
    }

}