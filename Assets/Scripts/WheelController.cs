﻿using UnityEngine;
using System.Collections;

public class WheelController : MonoBehaviour { 
    public Rigidbody rigidbody;
    // In Kilograms
    public float mass = 30.0f;
    // In meters
    public float radius = 0.5f;
    public float tractionCoeff = 8.0f;
    public float maxTractionAmt = 100.0f;
    public float sideTraction;

    // In degrees per second
    public float angularVelocityDegSec;

    // In radians per second
    public float AngularVelocity
    {
        get { return angularVelocityDegSec * 0.017453292519968f; }
        set { angularVelocityDegSec = value; }
    }

    public float rpm
    {
        get { return AngularVelocity / ((2.0f * Mathf.PI)/60.0f); }
    }

    public float driveTorque;
    public float brakeTorque;

    public float tractionTorque;
    public float totalTorque;
    public float linearVel;
    public float slipRatio;

    public Vector3 localVel;

    public float slipAngle;

    public AnimationCurve frictionCurve;
    public AnimationCurve sideCurve;

    public GameObject wheelGeometry;
    public float tractionForce;
    public float maxSideForce = 500.0f;

    public float distanceToCM;
    public LayerMask raycastIgnore;
    public float steeringAngle;
    public Vector3 fwd;

    public bool overrideSlipRatio;
    public float overridenSlipRatio;
    float prevSteringAngle;
	// Update is called once per frame
	void Update () 
    {
        rigidbody = GetComponent<Rigidbody>();
        //wheelGeometry.transform.localRotation *= Quaternion.Euler(0.0f, steeringAngle - prevSteringAngle, 0.0f);
        wheelGeometry.transform.Rotate(rigidbody.transform.up, steeringAngle - prevSteringAngle, Space.World);

        wheelGeometry.transform.Rotate(Vector3.right, angularVelocityDegSec * Time.deltaTime, Space.Self);
        wheelGeometry.transform.localPosition = transform.localPosition;
        prevSteringAngle = steeringAngle;
	}
    void Awake()
    {
        GetComponent<Rigidbody>().centerOfMass = (Vector3.zero);
    }
    RaycastHit groundInfo;
    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -rigidbody.transform.up, out groundInfo, radius, raycastIgnore))
        {
            SimulateTraction();
        }
        //else
           // Debug.Log("Not touching ground");
    }

    public Vector3 prevPos;
    public float w;

    public float fwdForce;
    public float sideForce;
    public float atanValue;


    Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 v )
    {
        planeNormal.Normalize();
        var distance = -Vector3.Dot(planeNormal.normalized, v);
        return v + planeNormal * distance;
    }    


    void SimulateTraction()
    {
        fwd = rigidbody.transform.forward;
        fwd = Quaternion.Euler(0, steeringAngle, 0) * fwd;
        fwd.Normalize();
        
        Vector3 right = rigidbody.transform.right;
        right = Vector3.Cross(transform.up, fwd).normalized;
        right = ProjectVectorOnPlane(groundInfo.normal, right);

        //localVel = Vector3.Lerp(localVel, transform.InverseTransformDirection(rigidbody.GetPointVelocity(transform.position)), Time.fixedDeltaTime * 1.5f);
        localVel = transform.InverseTransformDirection(rigidbody.GetPointVelocity(transform.position));
        Debug.DrawLine(transform.position, transform.position + rigidbody.GetPointVelocity(transform.position), Color.green);
        //totalTorque = driveTorque + tractionTorque + brakeTorque;

        float wheelInertia = mass * radius * radius * 0.5f;    // Mass is 70.0kg
        //totalTorque = (-1.0f * Mathf.Sign(driveTorque) * tractionTorque + driveTorque - brakeTorque);
        //Mathf.Clamp(brakeTorque, -driveTorque, driveTorque);
        if (angularVelocityDegSec == 0.0f)
            brakeTorque = 0.0f;
        brakeTorque = -brakeTorque;
       // totalTorque = driveTorque - brakeTorque;
        totalTorque = driveTorque + brakeTorque;

        float wheelAngularAccel = (totalTorque) / wheelInertia;

        // If the wheel is driven by the engine or braking
        if (totalTorque != 0.0f)
        {
            angularVelocityDegSec += wheelAngularAccel * Time.fixedDeltaTime;
            linearVel = angularVelocityDegSec * 0.017453292519968f * radius;
        }
        // If the wheel is spinning free
        else
        {
            angularVelocityDegSec = (localVel.z) * (1.0f / 0.017453292519968f) * (1.0f / radius);
            //linearVel = Mathf.Lerp (linearVel, localVel.z, Time.fixedDeltaTime * 300.0f);
            linearVel = localVel.z;
        }

        //linearVel = angularVelocity * 0.017453292519968f * radius;

        slipRatio = (linearVel - localVel.z) / Mathf.Abs(localVel.z);
        slipRatio = Mathf.Clamp(slipRatio, -6f, 6f);
        // If it's NaN, then the car and the wheel are stopped (0 / 0 division)
        if (float.IsNaN(slipRatio))
        {
            slipRatio = 0.0f;
        }
        // If it's infinity, then the wheel is spinning but the car is stopped (x / 0) division
        else if (float.IsInfinity(slipRatio))
        {
            slipRatio = 1.0f * Mathf.Sign(slipRatio);
        }

        if (overrideSlipRatio)
            slipRatio = overridenSlipRatio;

        // Calculate the slip angle: Angle between forward direction vector of the wheel and velocity vector of the wheel
        //Vector3 v = localVel;
        //v.y = 0.0f;
        //slipAngle = Vector3.Angle(Vector3.forward, v);
        //Vector3 cross = Vector3.Cross(Vector3.forward, v);
        slipAngle = Vector3.Angle(Mathf.Sign(linearVel) * fwd, rigidbody.velocity.normalized);
        Vector3 cross = Vector3.Cross(Mathf.Sign(linearVel) * fwd, rigidbody.velocity.normalized);

        //  if(linearVel >= 0.0f)
            if (cross.y < 0) slipAngle = -slipAngle;

        slipAngle *= Mathf.Sign(linearVel);
        //else
        //    if (cross.y > 0) slipAngle = -slipAngle;

       // slipAngle %= 90.0f;

        tractionForce = frictionCurve.Evaluate(Mathf.Abs(slipRatio)) * tractionCoeff * Mathf.Sign(slipRatio);
        tractionForce = Mathf.Clamp(tractionForce, -maxTractionAmt, maxTractionAmt);
        tractionTorque = tractionForce / radius;

        Vector3 tractionForceV = fwd * tractionForce;
        //Debug.DrawLine(transform.position, 0.01f * tractionForceV + transform.position, Color.red);

        //if(Mathf.Abs(slipRatio) > 0.01f)
        if(totalTorque != 0.0f)
            rigidbody.AddForceAtPosition(tractionForceV, transform.position);
        fwdForce = tractionForceV.magnitude;


        Vector3 sideForce = -right * sideCurve.Evaluate(Mathf.Abs(slipAngle / 90.0f)) * Mathf.Sign(slipAngle) * sideTraction * Mathf.Clamp((Mathf.Abs(localVel.x) / 0.010f), 0.0f, 1.0f);


        this.sideForce = sideForce.magnitude;
        sideForce = sideForce.magnitude > maxSideForce ? sideForce.normalized * maxSideForce : sideForce;
        //sideForce *= Mathf.Clamp(rigidbody.velocity.magnitude / 0.50f , - 1.0f, 1.0f);
        //sideForce = transform.TransformDirection(sideForce);


        Debug.DrawLine(transform.position, 0.01f * sideForce + transform.position, Color.yellow);

        prevPos = transform.position;

        Debug.DrawLine(transform.position, transform.position + fwd * 5.0f, Color.blue);
        //Debug.DrawLine(transform.position, transform.position + right * 5.0f, Color.red);
        if (Mathf.Abs(linearVel) < 1.2f && totalTorque == 0.0f)
            rigidbody.drag = 100.0f;
        else
        {
            rigidbody.drag = 0.0f;
            rigidbody.AddForceAtPosition(sideForce, transform.position);

        }

    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(GetComponent<Rigidbody>().worldCenterOfMass, 0.1f);
    }
}
