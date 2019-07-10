using UnityEngine;
using System.Collections;

public class StarField : MonoBehaviour {

	public float Speed = 0.01f;
	float xOffset = 0;

	void Start()
	{
		GetComponent<Renderer>().sortingLayerName = "Stars";
		GetComponent<Renderer>().sortingOrder = -1000;
	}
	
	// Update is called once per frame
	void Update ()
	{
		xOffset -= Speed;
		GetComponent<Renderer>().material.mainTextureOffset = new Vector2(xOffset, 0);
	}
}