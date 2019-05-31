using UnityEngine;
using UnityEngine.UI;

namespace GuiHandle
{
    public class Test : MonoBehaviour
    {
        public void Start()
        {
            int i = 0;
            foreach (Transform item in transform)
            {
                Button btn = item.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(delegate { Debug.Log(btn); });
                }
            }
        }


        private int Xbox_One_Controller = 0;
        private int PS4_Controller = 0;
        private float time;
        void Update()
        {
            if (time > Time.time)
                return;

            time = Time.time + 1;

            string[] names = Input.GetJoystickNames();
            for (int x = 0; x < names.Length; x++)
            {
                //print(names[x].Length); if (names[x].Length == 19)
                {
                    //print("PS4 CONTROLLER IS CONNECTED");
                    PS4_Controller = 1;
                    Xbox_One_Controller = 0;
                }
                if (names[x].Length == 33)
                {
                    //print("XBOX ONE CONTROLLER IS CONNECTED");
                    //set a controller bool to true    
                    PS4_Controller = 0;
                    Xbox_One_Controller = 1;
                }
            }
            if (Xbox_One_Controller == 1)

            {

                //do something
                if (Control.controlEnum == ControlEnum.App)
                    Control.controlEnum = ControlEnum.CommonHand;

            }
            else if (PS4_Controller == 1)

            {

                //do something
                if (Control.controlEnum == ControlEnum.App)
                    Control.controlEnum = ControlEnum.CommonHand;

            }
            else

            {

                // there is no controllers
                if (Control.controlEnum == ControlEnum.CommonHand)
                    Control.controlEnum = ControlEnum.App;

            }


            //Debug.Log(Input.GetAxis("Horizontal") + "\t Horizontal");
            //Debug.Log(Input.GetAxis("Vertical") + "\t Vertical");
            //Debug.Log(Input.GetAxis("Fire1") + "\t Fire1");
            //Debug.Log(Input.GetAxis("Fire2") + "\t Fire2");
            //Debug.Log(Input.GetAxis("Fire3") + "\t Fire3");
            //Debug.Log(Input.GetAxis("Jump") + "\t Jump");
            //Debug.Log(Input.GetAxis("Mouse X") + "\t Mouse X");
            //Debug.Log(Input.GetAxis("Mouse Y") + "\t Mouse Y");
            //Debug.Log(Input.GetAxis("Mouse ScrollWheel") + "\t Mouse ScrollWheel");
        }
    }
}