using UnityEngine;

namespace GuiHandle
{
    public class CalculationScreenPosition : MonoBehaviour
    {
        // 屏幕坐标与窗口坐标偏移量
        Vector2 offect;
        /// <summary>
        /// 计算屏幕位置
        /// </summary>
        private void Start()
        {
            // 设置鼠标位置到游戏窗口中心
            Cursor.lockState = CursorLockMode.Locked;
            Invoke("CalibrationPointerPosition", 0.2f);
        }

        /// <summary>
        /// 校准指针位置
        /// </summary>
        public void CalibrationPointerPosition()
        {
            //Cursor.lockState = CursorLockMode.None;
            // 获取鼠标的窗口坐标
            Vector2 winPos = Input.mousePosition;
            Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);

            //winPos = winPos - (winPos - center) * 2;

            //Debug.Log("窗口位置" + winPos + " " + center);
            // 获取指针的屏幕坐标
            GuiMouse.POINT screenPos = GuiMouse.GetMousePos();
            //Debug.Log("屏幕位置" + screenPos);
            //然后经过一系列的计算 嘿嘿
            offect = new Vector2(winPos.x - screenPos.X, winPos.y - screenPos.Y);
            //Debug.Log("校准指针偏移" + offect);
            Invoke("Xxxx", 0.2f);
        }
        void Xxxx()
        {
            Cursor.lockState = CursorLockMode.None;
        }
        public GuiMouse.POINT CalculationScreenPos(Vector2 winPos)
        {
            return new GuiMouse.POINT((int)(winPos.x - offect.x), (int)(Screen.height - winPos.y - offect.y));
        }
    }
}