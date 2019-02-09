using UnityEngine;

namespace Majordomo
{
    public interface ITaiwuWindow
    {
        /// <summary>
        /// 获取窗口的主 GameObject
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// 在显示窗口之前，检查并创建及注册资源
        /// </summary>
        void TryRegisterResources();

        /// <summary>
        /// 打开窗口，会修改激活状态
        /// </summary>
        void Open();

        /// <summary>
        /// 更新显示数据，不修改激活状态
        /// 若窗口已激活则会在屏幕更新展示内容，若未激活则无变化
        /// </summary>
        void Update();

        /// <summary>
        /// 关闭窗口，会修改激活状态
        /// </summary>
        void Close();
    }
}
