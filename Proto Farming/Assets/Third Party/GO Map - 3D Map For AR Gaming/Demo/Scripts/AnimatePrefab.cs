using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AnimatePrefab : MonoBehaviour {

	public float speed = 500;
	public bool continousRotation;
	bool isAnimating = false;


	void OnCollisionEnter(Collision collision) {

		print("Animate prefab - on collision enter");
		if (!isAnimating && !continousRotation)
			StartCoroutine (rotate(1));
	}

	void OnCollisionStay(Collision collision){
		if (continousRotation)
			transform.Rotate(transform.eulerAngles.x,speed*Time.deltaTime,transform.eulerAngles.z);
	}

	private IEnumerator rotate(float time) {

		print("Animate prefab - rotate");
		isAnimating = true;
		float elapsedTime = 0;

		while (elapsedTime < time)
		{
			float value = Mathf.Lerp (0, 360, elapsedTime);
			transform.eulerAngles = new Vector3 (transform.eulerAngles.x, value, transform.eulerAngles.z);
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		isAnimating = false;
		transform.eulerAngles = new Vector3 (transform.eulerAngles.x, 0, transform.eulerAngles.z);

	}

}
