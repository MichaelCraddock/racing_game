using UnityEngine;
using System.Collections;

public class Deform : MonoBehaviour
{
    private Material material;
    public CarController car;
    public AudioSource crashAudio;
    public GameObject CarshellN, CarshellB, CarshellVB, CarshellW, FBnorm, FBDam, FBCOL;
    public float carhealth, frontbumphealth = 100;
    public Texture Normal;
    public Texture Damaged;

    private float destructionLevel = 0.0f;

    void Start()
    {
        car = transform.root.GetComponent<CarController>();
        material = GetComponent<Renderer>().material;

        crashAudio = (AudioSource)GetComponent(typeof(AudioSource));
        if (crashAudio == null)
        {
            Debug.Log("No audio source, please add one crash sound to the car");
        }
       
    }

    void Update()
    {
   
        if (carhealth <= 100)
        {
            CarshellN.SetActive(true);
            CarshellB.SetActive(false);
            CarshellVB.SetActive(false);
        }

        if (carhealth <= 75)
        {
            Debug.Log("Health is at 75%");
            CarshellN.SetActive(false);
            CarshellB.SetActive(true);
            CarshellVB.SetActive(false);
        }

        if (carhealth <= 50)
        {
            Debug.Log("Health is at 50%");
            CarshellB.SetActive(false);
            CarshellVB.SetActive(true);
            CarshellN.SetActive(false);
        }
        

        if (carhealth <= 25)
        {
            Debug.Log("health is at 25%");

            }
        
    
        
        if (Input.GetKey(KeyCode.R))
        {
            Debug.Log("100 Health");
            carhealth = 100;
        }

    }
    void OnCollisionEnter(Collision col)
    {
       
        if (col.gameObject.tag == "Enviroment")
        {
            Debug.Log("something hit"+ gameObject.name + col.collider.name + col.contacts.Length);
            frontbumphealth -= 5;

        }

    }


}
