using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GuiHandle
{
    public class GuiHand : MonoBehaviour
    {
        public Transform image;
        public Camera uiCamera;
        private EventSystem eventSystem;
        Selectable select = null;
        bool getCom = false;
        Button btn = null;
        CalculationScreenPosition calculationScreenPosition;
        ModPublicizeIp modUDPPublicizeIp;
        private void Awake()
        {
            calculationScreenPosition = gameObject.AddComponent<CalculationScreenPosition>();
            modUDPPublicizeIp = new ModPublicizeIp();
            modUDPPublicizeIp.Start();
        }
        void Start()
        {
            this.eventSystem = EventSystem.current;
        }

        /// <summary>
        /// 触发鼠标点击
        /// </summary>
        void MouseClick()
        {
            if (Control.instance.IsMouseLeftDoun())
            {
                GuiMouse.MouseLeftDown();
            }
            if (Control.instance.IsMouseLeftUp())
            {
                GuiMouse.MouseLeftUp();
            }
        }

        /// <summary>
        /// 选择按钮
        /// </summary>
        void SelectBtn()
        {
            if (Control.instance.IsJudgment())
            {
                //Debug.Log("触发了选择");
                Selectable next = null;
                Selectable current = null;

                // 找出我们是否有一个有效的当前选择的游戏对象
                if (eventSystem.currentSelectedGameObject != null)
                {
                    // Unity似乎没有“取消选择”一个不活动的对象
                    if (eventSystem.currentSelectedGameObject.activeInHierarchy)
                    {
                        current = eventSystem.currentSelectedGameObject.GetComponent<Selectable>();
                    }
                }
                if (current != null)
                {
                    // 当SHIFT与标签一起被按住时，向后移动而不是向前移动
                    if (Control.instance.IsLast())
                    {
                        //Debug.Log("触发了向前选择");
                        next = current.FindSelectableOnLeft();
                        if (next == null)
                        {
                            next = current.FindSelectableOnUp();
                        }
                    }
                    else
                    {
                        //Debug.Log("触发了向后选择");
                        next = current.FindSelectableOnRight();
                        if (next == null)
                        {
                            next = current.FindSelectableOnDown();
                            if (next == null)
                            {
                                current = eventSystem.firstSelectedGameObject.GetComponent<Selectable>();
                                next = current;
                            }
                        }
                    }
                }
                else
                {
                    // 如果没有当前选择的游戏对象，选择第一个
                    if (Selectable.allSelectables.Count > 0)
                    {
                        next = Selectable.allSelectables[0];
                        getCom = false;
                    }
                }

                if (next != null)
                {
                    next.Select();
                    select = next;
                    image.SetParent(next.transform, false);



                    Vector2 winPos = uiCamera.WorldToScreenPoint(image.transform.position);
                    var screenPos = calculationScreenPosition.CalculationScreenPos(winPos);
                    GuiMouse.SetMousePos(screenPos);
                }
            }
        }

        /// <summary>
        /// 指针移动
        /// </summary>
        void MouseMove()
        {
            float h = Control.instance.IsHorizontal();
            float v = Control.instance.IsVertical();
            if (v != 0 || h != 0)
            {
                GuiMouse.POINT pt = GuiMouse.GetMousePos();
                pt.X += (int)(h * 10);
                pt.Y -= (int)(v * 10);
                GuiMouse.SetMousePos(pt);
            }
        }

        /// <summary>
        /// 指针滑动
        /// </summary>
        void MouseScroll()
        {
            float value = Control.instance.IsScroll();
            if (value != 0)
            {
                value = value * 100;
                GuiMouse.MouseScroll((int)value);
            }
        }

        void KeyboardClick()
        {
            if (Control.instance.IsClickEsc())
                GuiKeyboard.KeyboardClick(27);

            if (Control.instance.IsClickSpace())
                GuiKeyboard.KeyboardClick(32);

            if (Control.instance.IsForwardBig())
                GuiKeyboard.KeyboardClick(101);

            if (Control.instance.IsBackBig())
                GuiKeyboard.KeyboardClick(113);

            if (Control.instance.IsForwardSmall())
                GuiKeyboard.KeyboardClick(100);

            if (Control.instance.IsBackSmall())
                GuiKeyboard.KeyboardClick(97);
        }

        void Update()
        {
            ModUDP.instance.MyUpdate();
            Control.instance.MyUpdate();

            SelectBtn();
            MouseMove();
            MouseClick();
            MouseScroll();
            KeyboardClick();

            //if (Input.anyKeyDown)
            //{

            //    Debug.Log(Event.current.keyCode);
            //    //Event e = Event.current;
            //    //if (e.isKey)
            //    //{
            //    //    KeyCode currentKey = e.keyCode;
            //    //    int i = (int)currentKey;
            //    //    Debug.LogWarning("Current Key is : " + currentKey.ToString() + "   " + i);
            //    //}
            //}

            Control.instance.MyLateUpdate();
        }

        //private void OnGUI()
        //{
        //    if (Input.anyKeyDown)
        //    {

        //        Debug.LogWarning(Event.current.keyCode);
        //        //Event e = Event.current;
        //        //if (e.isKey)
        //        //{
        //        //    KeyCode currentKey = e.keyCode;
        //        //    int i = (int)currentKey;
        //        //    Debug.LogWarning("Current Key is : " + currentKey.ToString() + "   " + i);
        //        //}
        //    }
        //}
    }

}