using UnityEngine;
using System.Collections;

public class Hoodcollision : MonoBehaviour {
    public float frontbumphealth = 100;
    public Collider FBCollider1;

	// Use this for initialization
	void Start () {
	
	}

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Enviroment")
        {
            Debug.Log("something hit");
            frontbumphealth -= 5;

        }

    }

    // Update is called once per frame
    void Update () {
	
	}
}
