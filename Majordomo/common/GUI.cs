using System;
using UnityEngine;

namespace Majordomo
{
    /// <summary>
    /// 适用于 mod 的、使用注册方式创建的功能性窗口的接口
    /// </summary>
    public interface ITaiwuWindow
    {
        /// <summary>
        /// 获取窗口的主 GameObject
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// 在显示窗口之前，检查并创建及注册资源
        /// </summary>
        /// <param name="parent"></param>
        void TryRegisterResources(GameObject parent);

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


    /// <summary>
    /// 用于 Unity OnGUI 的浮点数输入框
    /// </summary>
    public class FloatField
    {
        private readonly float defaultValue;
        private readonly string format;
        private readonly Func<float, bool> validator;
        private float currValue;
        private string currText;
        private string prevText;


        public FloatField(float defaultValue, string format = "0.00")
            : this(defaultValue, format, value => true) { }


        public FloatField(float defaultValue, string format, Func<float, bool> validator)
        {
            this.defaultValue = defaultValue;
            this.format = format;
            this.validator = validator;
            this.Init();
        }


        private void Init()
        {
            this.currValue = defaultValue;
            this.currText = defaultValue.ToString();
            this.prevText = defaultValue.ToString(format);
        }


        public float GetFloat(int maxLength, params GUILayoutOption[] options)
        {
            this.currText = GUILayout.TextField(this.currText, maxLength, options);

            if (float.TryParse(this.currText, out float parsedValue) && validator(parsedValue))
            {
                this.prevText = this.currText;
                this.currValue = parsedValue;
                return this.currValue;
            }
            else
            {
                if (!string.IsNullOrEmpty(this.currText))
                    this.currText = this.prevText;
                else
                    this.Init();

                return this.currValue;
            }
        }
    }
}
