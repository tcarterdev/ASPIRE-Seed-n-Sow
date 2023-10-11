using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace GoMap
{
    public class GOAlert : MonoBehaviour
    {

        //Implementation example, use this to pop the alert from everywhere.

        //	GoAlert.LoadAlert("Test Alert!","Wow it really works! \n\n Even with line breaks",new string[] {"Button 1", "Button 2", "Button 3"},GoAlert.TransitionStyle.BottomToBottom, 0.5f, delegate(int buttonIndex, string buttontext) {
        //
        //			Debug.Log ("Callback " + buttontext + " " + buttonIndex);
        //
        //			switch (buttonIndex) {
        //			case 0 : 
        //				//Do stuff here
        //				break;
        //			case 1 : 
        //				//Do stuff here
        //				break;
        //			case 2 : 
        //				//Do stuff here
        //				break;
        //
        //			}
        //		});

        public enum TransitionStyle
        {
            TopToBottom,
            TopToTop,
            BottomToTop,
            BottomToBottom,
            Fade
        }

        public float duration;
        public Text titleLabel;
        public Text bodyText;
        string[] buttonTexts;
        Button[] buttons;
        public TransitionStyle transitionStyle;
        public GameObject buttonsLayout;
        public GameObject alertPanel;
        public Action<int, string> buttonClicked;
        public Vector2 destinationPosition;


        //Title, Body Text, Customizable Button Action
        public static void LoadAlert(string titleLabel, string bodyText, Action<int, string> buttonClicked)
        {
            LoadAlert(titleLabel, bodyText, new string[] { "Ok" }, GOAlert.TransitionStyle.BottomToBottom, 0.5f, buttonClicked);
        }


        //Title, Body Text
        public static void LoadAlert(string titleLabel, string bodyText)
        {
            LoadAlert(titleLabel, bodyText, new string[] { "Ok" }, GOAlert.TransitionStyle.BottomToBottom, 0.5f, delegate (int buttonIndex, string buttontext) { });
        }


        //Title, Body Text, Buttons, Buttons' actions
        public static void LoadAlert(string titleLabel, string bodyText, string[] buttonTexts, Action<int, string> buttonClicked)
        {
            LoadAlert(titleLabel, bodyText, buttonTexts, GOAlert.TransitionStyle.BottomToBottom, 0.5f, buttonClicked);
        }


        //Title, Body Text, Buttons, Animation, Animation's duration, Buttons' actions
        public static void LoadAlert(string titleLabel, string bodyText, string[] buttonTexts, TransitionStyle transitionStyle, float duration, Action<int, string> buttonClicked)
        {

            GameObject alertCanvas = Instantiate(Resources.Load("GOAlert/AlertCanvas") as GameObject);
            GOAlert alert = alertCanvas.GetComponent<GOAlert>();
            alert.titleLabel.text = titleLabel;
            alert.bodyText.text = bodyText;
            alert.buttonTexts = buttonTexts;
            alert.buttonClicked = buttonClicked;
            alert.duration = duration;
            alert.CreateButtons();
            alert.transitionStyle = transitionStyle;
            RectTransform bannerRect = alert.alertPanel.GetComponent<RectTransform>();
            switch (transitionStyle)
            {
                case TransitionStyle.BottomToBottom:
                    alert.destinationPosition = new Vector2(bannerRect.anchoredPosition.x, -Screen.height);
                    bannerRect.anchoredPosition = new Vector2(bannerRect.anchoredPosition.x, -Screen.height);
                    break;
                case TransitionStyle.BottomToTop:
                    alert.destinationPosition = new Vector2(bannerRect.anchoredPosition.x, Screen.height);
                    bannerRect.anchoredPosition = new Vector2(bannerRect.anchoredPosition.x, -Screen.height);
                    break;
                case TransitionStyle.TopToBottom:
                    alert.destinationPosition = new Vector2(bannerRect.anchoredPosition.x, -Screen.height);
                    bannerRect.anchoredPosition = new Vector2(bannerRect.anchoredPosition.x, Screen.height);
                    break;
                case TransitionStyle.TopToTop:
                    alert.destinationPosition = new Vector2(bannerRect.anchoredPosition.x, Screen.height);
                    bannerRect.anchoredPosition = new Vector2(bannerRect.anchoredPosition.x, Screen.height);
                    break;
                case TransitionStyle.Fade:
                    break;
                default:
                    break;
            }
        }

        void Start()
        {
            if (transitionStyle != TransitionStyle.Fade)
            {
                StartCoroutine(Slide(true, duration));
            }
            else
            {
                StartCoroutine(Fade(true, duration));
            }
        }

        //Instantiate button template for each button
        public void CreateButtons()
        {

            GameObject buttonTemplate = null;
            foreach (Transform child in buttonsLayout.transform)
            {
                if (buttonTemplate == null)
                {
                    buttonTemplate = Instantiate(child.gameObject);
                }

                GameObject.Destroy(child.gameObject);
            }


            //create a button for each string in buttonTexts[], with each string as text
            for (int i = 0; i < buttonTexts.Length; i++)
            {

                string buttonName = buttonTexts[i];
                GameObject button = Instantiate(buttonTemplate);
                button.name = i.ToString();
                Text text = button.GetComponentInChildren<Text>();
                text.text = buttonName;
                button.transform.SetParent(buttonsLayout.transform);
                //			button.transform.parent = buttonsLayout.transform;
                button.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                Button btn = button.GetComponent<Button>();
                btn.onClick.AddListener(() => { OnButtonClick(btn); });

            }

            Destroy(buttonTemplate);
        }


        public void OnButtonClick(Button clicked)
        {

            Text text = clicked.gameObject.GetComponentInChildren<Text>();
            buttonClicked(int.Parse(clicked.name), text.text);

            if (transitionStyle != TransitionStyle.Fade)
            {
                StartCoroutine(Slide(false, duration));
            }
            else
            {
                StartCoroutine(Fade(false, duration));
            }
            gameObject.GetComponent<Image>().enabled = false;
        }



        //Animations
        private IEnumerator Slide(bool show, float time)
        {

            Vector2 newPosition;
            RectTransform bannerRect = alertPanel.GetComponent<RectTransform>();

            if (show)
            {//Open
                newPosition = new Vector2(bannerRect.anchoredPosition.x, 0);
            }
            else
            { //Close
                newPosition = destinationPosition;
            }

            float elapsedTime = 0;
            while (elapsedTime < time)
            {
                bannerRect.anchoredPosition = Vector2.Lerp(bannerRect.anchoredPosition, newPosition, (elapsedTime / time));
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }



            if (!show)
            {

                GameObject.Destroy(gameObject);

            }

        }


        private IEnumerator Fade(bool show, float time)
        {

            Vector2 alphaValues;
            if (show)
            {//Open
                alphaValues = new Vector2(0, 1);
            }
            else
            { //Close
                alphaValues = new Vector2(1, 0);
            }

            float elapsedTime = 0;
            while (elapsedTime < time)
            {
                alertPanel.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(alphaValues.x, alphaValues.y, (elapsedTime / time));
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            if (!show)
            {
                gameObject.GetComponent<Image>().enabled = false;
                GameObject.Destroy(gameObject);

            }

        }


    }
}