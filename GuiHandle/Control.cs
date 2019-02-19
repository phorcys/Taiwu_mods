using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GuiHandle
{
    public class Control : MonoBehaviour
    {

        //public bool btn_up;// 0                   暂停（空格）
        //public bool btn_down;// 1                   关闭（esc）
        //public bool btn_left;// 2                   向前选择组件
        //public bool btn_right;// 3                   向后选择组件

        //public bool btn_l1;// 4                   战斗后退大
        //public bool btn_r1;// 5                   战斗前进大
        //public bool btn_l2;// 6                   战斗后退小
        //public bool btn_r2;// 7                   战斗前进小

        //public bool btn_square;//□    8              左键
        //public bool btn_circle;//○     9             右键
        //public bool btn_triangle;//△  10              上滚
        //public bool btn_x;//×          11           下滚
        /*
         △
       □  ○
         ×
        */
        //public bool btn_pad;// 0        12           xx
        //public bool btn_options;// 0    13               呼出/关闭mod管理器
        //public bool btn_share;// 0      14             xx


        public static int last_boolvalue;

        public static Control instance;
        public static float left_x;
        public static float left_y;
        public static float right_x;
        public static float right_y;
        public static int boolvalue;
        public static byte num;

        public static ControlEnum controlEnum = ControlEnum.App;

        private void Awake()
        {
            instance = this;
        }

        public void MyUpdate()
        {

            Debug.Log(Control.controlEnum);


            switch (controlEnum)
            {
                case ControlEnum.App:
                    left_x = ModReceivePage.left_x;
                    left_y = ModReceivePage.left_y;
                    right_x = ModReceivePage.right_x;
                    right_y = ModReceivePage.right_y;
                    boolvalue = ModReceivePage.boolvalue;
                    break;
                default:
                    if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                    {
                        left_x = Input.GetAxis("Horizontal");
                    }
                    if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.X))
                    {
                        left_y = Input.GetAxis("Vertical");
                    }

                    boolvalue = 0;
                    if (!Input.GetKey(KeyCode.Space))// 暂停 △ 0
                    {
                        if (Input.GetAxis("Jump") > 0)
                        {
                            // 暂停
                            Debug.Log("暂停" + (1 << 0));
                            boolvalue |= 1 << 0;
                        }
                    }
                    if (!Input.GetMouseButton(0))// 左键 □ 8
                    {
                        if (Input.GetAxis("Fire1") > 0)
                        {
                            // 左键
                            Debug.Log("左键" + (1 << 8));
                            boolvalue |= 1 << 8;
                        }
                    }
                    if (!Input.GetMouseButton(1))// 关闭 × 1
                    {
                        if (Input.GetAxis("Fire2") > 0)
                        {
                            // 关闭
                            Debug.Log("关闭" + (1 << 1));
                            boolvalue |= 1 << 1;
                        }
                    }
                    if (!Input.GetMouseButton(2))// 选择UI ○ 3
                    {
                        if (Input.GetAxis("Fire3") > 0)
                        {
                            // 选择
                            Debug.Log("选择" + (1 << 3));
                            boolvalue |= 1 << 3;
                        }
                    }
                    if (boolvalue > 0)
                        Debug.Log(last_boolvalue + " == " + boolvalue);



                    //left_x = ModReceivePage.left_x;
                    //left_y = ModReceivePage.left_y;
                    //right_x = ModReceivePage.right_x;
                    //right_y = ModReceivePage.right_y;
                    //boolvalue = ModReceivePage.boolvalue;


                    break;
            }
        }

        public void MyLateUpdate()
        {
            switch (controlEnum)
            {
                case ControlEnum.App:
                    last_boolvalue = ModReceivePage.boolvalue;
                    break;
                default:
                    last_boolvalue = boolvalue;


                    //left_x = ModReceivePage.left_x;
                    //left_y = ModReceivePage.left_y;
                    //right_x = ModReceivePage.right_x;
                    //right_y = ModReceivePage.right_y;
                    //boolvalue = ModReceivePage.boolvalue;


                    break;
            }
        }


        /// <summary>
        /// 是否触发选择控件
        /// </summary>
        /// <returns></returns>
        public bool IsJudgment()
        {
            int keyA = 1 << ControlKey.SelectBack;
            int keyB = 1 << ControlKey.SelectForward;
            //Debug.Log(last_boolvalue + " " + boolvalue + " " + keyA + " " + keyB + " " + ((last_boolvalue & keyA) != keyA && (boolvalue & keyA) == keyA));
            //if (((last_boolvalue & keyA) != keyA && (boolvalue & keyA) == keyA) || ((last_boolvalue & keyB) != keyB && (boolvalue & keyB) == keyB))
            //    Debug.Log("触发选择");
            return ((last_boolvalue & keyA) != keyA && (boolvalue & keyA) == keyA) || ((last_boolvalue & keyB) != keyB && (boolvalue & keyB) == keyB);
        }
        /// <summary>
        /// 是否触发选择上一个控件
        /// </summary>
        /// <returns></returns>
        public bool IsLast()
        {
            int keyA = 1 << ControlKey.SelectBack;
            return (last_boolvalue & keyA) != keyA && (boolvalue & keyA) == 2;
        }
        /// <summary>
        /// 是否触发鼠标左右滑动
        /// </summary>
        /// <returns></returns>
        public float IsHorizontal()
        {
            return left_x;
        }
        /// <summary>
        /// 是否触发鼠标上下滑动
        /// </summary>
        /// <returns></returns>
        public float IsVertical()
        {
            return left_y;
        }
        /// <summary>
        /// 是否触发鼠标左键按下
        /// </summary>
        /// <returns></returns>
        public bool IsMouseLeftDoun()
        {
            int key = 1 << ControlKey.MouseLeft;
            //Debug.Log("左键按下" + ((last_boolvalue & key) != key && (boolvalue & key) == key));
            //if ((last_boolvalue & key) != key && (boolvalue & key) == key)
            //    Debug.Log("触发鼠标左键按下");
            return (last_boolvalue & key) != key && (boolvalue & key) == key;
        }
        /// <summary>
        /// 是否触发鼠标左键弹起
        /// </summary>
        /// <returns></returns>
        public bool IsMouseLeftUp()
        {
            int key = 1 << ControlKey.MouseLeft;
            return (last_boolvalue & key) == key && (boolvalue & key) != key;
        }
        /// <summary>
        /// 是否触发鼠标右键按下
        /// </summary>
        /// <returns></returns>
        public bool IsMouseRightDoun()
        {
            int key = 1 << ControlKey.MouseRight;
            return (last_boolvalue & key) != key && (boolvalue & key) == key;
        }
        /// <summary>
        /// 是否触发鼠标右键弹起
        /// </summary>
        /// <returns></returns>
        public bool IsMouseRightUp()
        {
            int key = 1 << ControlKey.MouseRight;
            return (last_boolvalue & key) == key && (boolvalue & key) != key;
        }
        /// <summary>
        /// 是否触发点击esc
        /// </summary>
        /// <returns></returns>
        public bool IsClickEsc()
        {
            int key = 1 << ControlKey.Esc;
            return (last_boolvalue & key) != key && (boolvalue & key) == key;
        }
        /// <summary>
        /// 是否触发点击空格
        /// </summary>
        /// <returns></returns>
        public bool IsClickSpace()
        {
            int key = 1 << ControlKey.Space;
            return (last_boolvalue & key) != key && (boolvalue & key) == key;
        }
        /// <summary>
        /// 是否触发战斗大步前进
        /// </summary>
        /// <returns></returns>
        public bool IsForwardBig()
        {
            int key = 1 << ControlKey.BattleForwardBig;
            return (last_boolvalue & key) != key && (boolvalue & key) == key;
        }
        /// <summary>
        /// 是否触发战斗大步后退
        /// </summary>
        /// <returns></returns>
        public bool IsBackBig()
        {
            int key = 1 << ControlKey.BattleBackBig;
            return (last_boolvalue & key) != key && (boolvalue & key) == key;
        }
        /// <summary>
        /// 是否触发战斗小步前进
        /// </summary>
        /// <returns></returns>
        public bool IsForwardSmall()
        {
            int key = 1 << ControlKey.BattleForwardSmall;
            return (last_boolvalue & key) != key && (boolvalue & key) == key;
        }
        /// <summary>
        /// 是否触发战斗小步后退
        /// </summary>
        /// <returns></returns>
        public bool IsBackSmall()
        {
            int key = 1 << ControlKey.BattleBackSmall;
            return (last_boolvalue & key) != key && (boolvalue & key) == key;
        }
        /// <summary>
        /// 是否触发了滚轮滑动
        /// </summary>
        /// <returns></returns>
        public float IsScroll()
        {
            int keyA = 1 << ControlKey.MouseScrollUp;
            int keyB = 1 << ControlKey.MouseScrollDown;
            if ((boolvalue & keyA) == keyA && (boolvalue & keyB) == keyB)
            {
                return ((boolvalue & keyA) == keyA ? 0.5f : 0) + ((last_boolvalue & keyA) == keyA ? 0.5f : 0) + ((boolvalue & keyB) == keyB ? -0.5f : 0) + ((last_boolvalue & keyB) == keyB ? -0.5f : 0);
            }
            return 0;
        }

    }

    public enum ControlEnum
    {
        App = 0,
        CommonHand = 1,
    }

    public static class ControlKey
    {
        public static int Space = 0;
        public static int Esc = 1;
        public static int SelectBack = 2;
        public static int SelectForward = 3;

        public static int BattleBackBig = 4;
        public static int BattleForwardBig = 5;
        public static int BattleBackSmall = 6;
        public static int BattleForwardSmall = 7;

        public static int MouseLeft = 8;
        public static int MouseRight = 9;
        public static int MouseScrollUp = 10;
        public static int MouseScrollDown = 11;

        public static int xx1 = 12;
        public static int SetMod = 13;
        public static int xx3 = 14;











        //public static int Space { get; set; }
        //public static int Esc { get; set; }
        //public static int SelectBack { get; set; }
        //public static int SelectForward { get; set; }

        //public static int BattleBackBig { get; set; }
        //public static int BattleForwardBig { get; set; }
        //public static int BattleBackSmall { get; set; }
        //public static int BattleForwardSmall { get; set; }

        //public static int MouseLeft { get; set; }
        //public static int MouseRight { get; set; }
        //public static int MouseScrollUp { get; set; }
        //public static int MouseScrollDown { get; set; }

        //public static int xx1 { get; set; }
        //public static int SetMod { get; set; }
        //public static int xx3 { get; set; }
    }
}