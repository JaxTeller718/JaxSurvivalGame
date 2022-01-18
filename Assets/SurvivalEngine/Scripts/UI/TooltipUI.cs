using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivalEngine {

    [RequireComponent(typeof(CanvasGroup))]
    public class TooltipUI : UIPanel
    {
        public GameObject icon_group;
        public GameObject text_only_group;

        public Text title;
        public Image icon;
        public Text desc;

        public Text title2;
        public Text desc2;

        private RectTransform rect;
        private Selectable target = null;

        private static TooltipUI _instance;

        protected override void Awake()
        {
            base.Awake();
            _instance = this;
            rect = GetComponent<RectTransform>();
        }

        protected override void Start()
        {
            base.Start();

        }

        protected override void Update()
        {
            base.Update();

            RefreshTooltip();

            if (target == null)
                Hide();
        }

        void RefreshTooltip()
        {
            if (target != null)
            {
                rect.anchoredPosition = TheUI.Get().ScreenPointToCanvasPos(PlayerControlsMouse.Get().GetMousePosition());
                //transform.position = PlayerControlsMouse.Get().GetPointingPos();
                //transform.rotation = Quaternion.LookRotation(TheCamera.Get().transform.forward, Vector3.up);

                PlayerControlsMouse mouse = PlayerControlsMouse.Get();
                if (!target.IsHovered() || mouse.IsMovingMouse())
                    Hide();
            }
        }

        public void Set(Selectable target, CraftData data) {

            this.target = target;

            if (title != null)
                title.text = data.title;
            if (icon != null)
                icon.sprite = data.icon;
            if (desc != null)
                desc.text = data.desc;

            if(title2 != null)
                title2.text = data.title;
            if (desc2 != null)
                desc2.text = data.desc;

            if (text_only_group != null)
                text_only_group.SetActive(data.icon == null);
            if (icon_group != null)
                icon_group.SetActive(data.icon != null);

            Show();
            RefreshTooltip();
        }

        public void Set(Selectable target, string atitle, string adesc, Sprite aicon)
        {
            this.target = target;

            if (title != null)
                title.text = atitle;
            if (icon != null)
                icon.sprite = aicon;
            if (desc != null)
                desc.text = adesc;

            if (title2 != null)
                title2.text = atitle;
            if (desc2 != null)
                desc2.text = adesc;

            if (text_only_group != null)
                text_only_group.SetActive(aicon == null);
            if (icon_group != null)
                icon_group.SetActive(aicon != null);

            Show();
            RefreshTooltip();
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
            target = null;
        }

        public Selectable GetTarget()
        {
            return target;
        }

        public static TooltipUI Get()
        {
            return _instance;
        }
    }

}
