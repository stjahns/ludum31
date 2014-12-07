using UnityEngine;
using System.Collections;

public class StarField : MonoBehaviour {

	public float Speed = 0.01f;
	float xOffset = 0;

	void Start()
	{
		renderer.sortingLayerName = "Stars";
		renderer.sortingOrder = -1000;
	}
	
	// Update is called once per frame
	void Update ()
	{
		xOffset -= Speed;
		renderer.material.mainTextureOffset = new Vector2(xOffset, 0);
	}
}