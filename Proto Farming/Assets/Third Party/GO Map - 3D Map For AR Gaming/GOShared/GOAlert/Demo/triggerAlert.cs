using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using GoMap;

public class triggerAlert : MonoBehaviour {

	public Button button1;
	public Button button2;
	public Button button3;
	public Material skyboxMaterial;

	public 	void simpleAlert(){
		GOAlert.LoadAlert ("Simple Alert", "This is a simple alert.");	
	}

	public void customAlert(){
		GOAlert.LoadAlert ("Customized Alert", "This is a customized alert, you can choose button's action! ", delegate(int buttonIndex, string buttontext) {
			RenderSettings.skybox = skyboxMaterial;

		});
	}

	public void buttonsAlert(){
	
		GOAlert.LoadAlert ("Multiple Buttons", "Do you want to open another Alert?", new string[]{ "No", "Yes" }, delegate(int buttonIndex, string buttontext) {
			switch (buttonIndex) {
			case 0:
				break;
			case 1:
				buttonsAlert ();
				break;
			}

		}); 
	
	
	}
	public void fullCustomAlert(){
		GOAlert.LoadAlert("Fully Customized Alert", "This is a fully customized alert, you can set buttons' number, and even transition animation!",
						 new string[]{"Set Blue Color","Set Red Color"},GOAlert.TransitionStyle.TopToTop, 0.6f, delegate(int buttonIndex, string buttonText){
				switch (buttonIndex){
				case 0:
					button3.image.color = new Color(122/255f,151/255f,180/255f);
					break;
				case 1:
					button3.image.color = new Color(238/255f,30/255f,0);
					break;

				}


			});
			
	}
}
