using System;
using UnityEngine;

namespace ModsCollection
{
    public static class UiStyle
    {
        public static GUIStyle Label()
        {
            GUIStyle uiStyle = new GUIStyle();
            uiStyle.normal.textColor = Color.yellow;
            uiStyle.richText = true;
            uiStyle.alignment = TextAnchor.MiddleRight;
            uiStyle.padding = new RectOffset(0,40,0, 0);
            uiStyle.fontSize = 20;
            return uiStyle;
        }

        public static GUIStyle Toggle()
        {
            GUIStyle uiStyle = new GUIStyle();

            Texture2D unChecked = new Texture2D(1, 1);
            unChecked.LoadImage(
            //            Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAA+gAAAAyCAMAAAD4MAECAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRFqqqqqampAAAAWfh9mAAAAAN0Uk5T//8A18oNQQAAAO9JREFUeNrs3cENwyAQRFHG/RdtlALM3pJx3isB6WtBHHZdwOstRwBCB4QOVIWeZD2Js4L60Hflz6FLHepDXzlmHKVDd+iZNLyHuvOC3tAzSzhCh+rQL6HD60Mfvr690qE49OnjW+ggdEDogNABoQNCB4QOQhc6CF3oIHRA6IDQAaEDQgeEDggdhC50EDogdEDogNABoQNCB4QOQhc6CF3oIHTgl0Of7l6zZRH+YKILHXpDH+5D1jk0hz68u9uPDtWhZzKsdQ7doe9b+SnjxM0dykPfpZ/4WoP60D8P9QemObwjdEDogNABoQNfdAswAH3NhLdDg0JNAAAAAElFTkSuQmCC")); //400
            //Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAfQAAAAyCAMAAACdztmlAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRFqqqqqamp////WUZbigAAAAN0Uk5T//8A18oNQQAAAMdJREFUeNrs3MERwyAMRUGJ/ou2kwJAh1zy2VcCO1jjg6il6ypHAF3QFY3e3bWrnVUc+iu+R8ceh159JG3qWeg98Xwvu/PKQe8ZZ0OPQl/Qr0MfTmtTPQh9OqyhQxd0QRd0QRd0QRd0QRd0QRd0QRd06NChQ4cOHbqgC7qgC7qgC7qgC7p+hT7dZbPBeOFNh56DPtxBZp6EPvy+20+PQh89RcE8C3193pY6kLevexj6qmN+1+LQv4N9k1ueiS7ogi7o+uMeAQYAT8PBWO0+280AAAAASUVORK5CYII=")); //200
            Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAASwAAAAyCAMAAADRCBYYAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRFqqqqqamp////WUZbigAAAAN0Uk5T//8A18oNQQAAAK9JREFUeNrs2zEOwzAMA0DK/3902yDwmLBDgQY4zp4OliYqS+oEASxYf4M1M7nKsNpYb6lrLFwbK3NLMbRyzmDzNIF1zGDzdmD1CrDqKTw2G6zVLiNYsGDBggULFixYsGDBggULFixYsGDBggULFixYsGDBggUL1u+w2q5DYPU/C1bdJVJm+2IZ6WedDGHVYq1PV/mGagJr76NodrdYx+K6iEuM5RwFFixYz8pLgAEAcDtzOGtSo2EAAAAASUVORK5CYII="));//120

            Texture2D isChecked = new Texture2D(1, 1);
            isChecked.LoadImage(
            //            Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAA+gAAAAyCAMAAAD4MAECAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRF/////v7+AAAAwHRenAAAAAN0Uk5T//8A18oNQQAAAVxJREFUeNrs3cuOgzAQRUE3///RI6LM8AgTvEv6qmqbHdKJDcLNWIB4wyUAoQNCB1qFXlXjjSrXCtqHXmvLb6ytu1rQO/SJFbuUDr1Dn9qYKx1ahz6ZcHlyB61DX4QO8aFP7snt3aFx6GMIHYQudBA6IHRA6MA3hP73UF7oEBv69jqN0CE29C1voUNq6Lu3aYQOoaHvfxM6ZIZ+OO8idIgM/XiuTeiQGPrp/KrQISL0OnV+/AcQOmSEfmj5fHxV6JCxdd+X/jJoQugQco++lf46YEroEBL68jvX+WKQnNAhJfRn4VcDI4UOMaE/Gr8cDCt0yAl9rfyyaaFDUOj/DXoXOiSFvlwXLXSICn0ROggdEDogdOATofv2GuSHXlZ0EPqTzyZD59Anl+qyoEPn0GumdAs69A59Lb3ulvMhdOgd+joe7oZ9O7QP/aZ1t+cQEjogdEDogNCBD/oRYADXL4RsqP3JCAAAAABJRU5ErkJggg=="));
            //Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAfQAAAAyCAMAAACdztmlAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRF/////v7+AAAAwHRenAAAAAN0Uk5T//8A18oNQQAAATFJREFUeNrs3EmSwyAQBECa/z/aVjhmtBjLXF1kXXUjoxGIFq3LcmmGALpAl2j0qmo3qTJWcei1ud5kczdaWegTlVzUs9CnJm/qUeiTnGXVF4XeoS+HPjlvm9+D0FuDDh06dOjQBbokov8v7qEvg75/uoG+DPpODX0V9MOXG+iLoB+fQV8D/XQWA30J9PP5G/QV0C9nrtAj0eti3jr0fPST6/XIFXrm9H5Uf2uagB76Tt/V35uooKcu5P56nQeNc9BjV+8v7VGzJPTcLdvmPWyQhR68T3+KD32hB6N/aoSHnozex7rQo9E7dOjQoQt0+UV0/7Kth14qHfqH+FU5CX2yhF02FIU+dRWFQs9C39TrW5k36Fno/fYSuddNcgYrDf2Lu9d5KLpAF+gCXX44DwEGAP6XwQ0RCY4iAAAAAElFTkSuQmCC"));
            Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAASwAAAAyCAMAAADRCBYYAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRF/////v7+////wMp4jgAAAAN0Uk5T//8A18oNQQAAARFJREFUeNrs28sSgzAIQFHI/390m2rzpIAzXdjpZeNCV2dIRIhSiHQIBGCBdRssVRUnVLFqWFo9nKheYJ2XOHMULTnXYOJRtOQCgwpYaQWw0qvwtbOBVUTAAgsssMD6R6z2sgQrxOolK1ghVicCK8IaKlawAqzxHlg+1vSNDZaLNfcjwPKwlt4NWBOWLlZCUepgTR5r6waseRmOWluzD6xlz+pae7MZrHWDf8+8jMY8WNvb8FCyhhhg7aVDdTIHPmAZddZTynQByypKPwwSwTIreFsFrEJbGSywwPoxLM465LGUzPo6FkeOLqSMcvLPqdhJLAurammUVgJWS5soONldph4p/wyksQiwwALr9vEQYACnrHLtf8IR/AAAAABJRU5ErkJggg==")); //120

            uiStyle.normal.background = unChecked;
            uiStyle.normal.textColor = Color.gray;

            uiStyle.onNormal.background = isChecked;
            uiStyle.onNormal.textColor = Color.white;

            uiStyle.richText = true;
            uiStyle.stretchWidth = true;
            uiStyle.alignment = TextAnchor.UpperLeft;
            uiStyle.fixedWidth = 150;
            uiStyle.fixedHeight = 22;
            uiStyle.imagePosition = ImagePosition.ImageLeft;
            uiStyle.fontSize = 20;
            uiStyle.border = new RectOffset(3, 0, 0, 0);
            uiStyle.contentOffset = new Vector2(30, 0);

            return uiStyle;
        }
    }
}
