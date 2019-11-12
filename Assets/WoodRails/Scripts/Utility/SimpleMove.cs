using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMove : MonoBehaviour
{
    public float ForwardSpeed = 0.3f;
    public float TurnSpeed = 150.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.up * TurnSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.down * TurnSpeed * Time.deltaTime);
        }

        transform.Translate(Vector3.forward * ForwardSpeed * Time.deltaTime * Input.GetAxis("Vertical"));
    }
}
