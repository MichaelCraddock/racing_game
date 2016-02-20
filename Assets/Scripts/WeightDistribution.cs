using UnityEngine;
using System.Collections;

public class WeightDistribution : MonoBehaviour {

  public float rearAxleWeight;
  public float frontAxleWeight;

  private Rigidbody rigidbody;

  private float mass;

  private float rearAxleDistance;
  private float frontAxleDistance;
  private float wheelBase;

	// Use this for initialization
	void Start () {
    rigidbody = GetComponent<Rigidbody>();
    mass = rigidbody.mass;
    rearAxleDistance = gameObject.transform.Find("Rear_right").GetComponent<WheelController>().distanceToCM;
    frontAxleDistance = gameObject.transform.Find("Front_right").GetComponent<WheelController>().distanceToCM;
    wheelBase = rearAxleDistance + frontAxleDistance;
  }
	
	// Update is called once per frame
	void Update () {
    // If not accelerating.
    rearAxleWeight = (frontAxleDistance / wheelBase) * mass;
    frontAxleWeight = (rearAxleDistance / wheelBase) * mass;
	}

  void FixedUpdate() {

  }

  // Calculate weight on axles.
  void calculateAxleWeight() {

  }
}
