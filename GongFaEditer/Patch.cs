using Harmony12;
using Ju.GongFaEditer;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[HarmonyPatch(typeof(MainMenu), "PageReady")]
public static class MainMenu_PageReady_Path
{
    public static void Postfix()
    {
        Debug.Log("开始注入");
        bool enabled = Main.Enabled;
        if (enabled)
        {
            Transform transform = GameObject.Find("MainMenu").transform;
            CreateButton(transform);
            GameObject.Find("MianMenuBack").AddComponent<MakeGui>();
            MakeGui.Instance = GameObject.Find("MianMenuBack").GetComponent<MakeGui>();
            MakeGui.Instance.parentInstance = MainMenu.instance;
            RectTransform component = GameObject.Find("GoEditGongFaButton").GetComponent<RectTransform>();
            RectTransform component2 = GameObject.Find("GameSettingButton").GetComponent<RectTransform>();
            component.localPosition = new Vector3(component2.transform.localPosition.x, component2.transform.localPosition.y + component2.rect.height + 5f, component2.transform.localPosition.z);
            component.localScale = new Vector3(1f, 1f, 1f);
            component.rotation = new Quaternion(0f, 0f, 0f, 0f);
            Main.LoadChangedGongFa();
        }
        Main.LoadAllGongFaAttributes();
    }

    public static void CreateButton(Transform transform)
    {
        Debug.Log("开始创建按钮");
        RectTransform component = GameObject.Find("GameSettingButton").GetComponent<RectTransform>();
        GameObject gameObject = Object.Instantiate<GameObject>(component.gameObject, new Vector3(component.transform.position.x, component.transform.position.y, component.transform.position.z), Quaternion.identity);
        gameObject.name = "GoEditGongFaButton";
        Button component2 = gameObject.GetComponent<Button>();
        RectTransform component3 = gameObject.GetComponent<RectTransform>();
        Transform transform2 = gameObject.transform.Find("GameSettingButtonText");
        transform2.name = "GoEditGongFaButtonText";
        transform2.GetComponent<Text>().text = "编辑功法";
        component2.onClick = new Button.ButtonClickedEvent();
        component2.onClick.AddListener(delegate ()
        {
            MakeGui.Instance.ShowOrHide(true);
        });
        gameObject.transform.SetParent(transform);
    }
}

//[HarmonyPatch(typeof(MainMenu), "PageReady")]
//public static class MainMenu_PageReady_Path
//{
//    public static void Postfix()
//    {
//        RectTransform component = GameObject.Find("GoEditGongFaButton").GetComponent<RectTransform>();
//        RectTransform component2 = GameObject.Find("GameSettingButton").GetComponent<RectTransform>();
//        component.localPosition = new Vector3(component2.transform.localPosition.x, component2.transform.localPosition.y + component2.rect.height + 5f, component2.transform.localPosition.z);
//        component.localScale = new Vector3(1f, 1f, 1f);
//        component.rotation = new Quaternion(0f, 0f, 0f, 0f);
//        Main.LoadChangedGongFa();
//    }
//}
