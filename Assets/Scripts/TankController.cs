using UnityEngine;
using System.Collections;

public class TankController : MonoBehaviour {


    TankMotor Motor;

    void OnEnable() {
        Motor = GetComponent<TankMotor>();


    }

    void Update() {
        Motor.In_LeftMv = Mathf.Lerp(Motor.In_LeftMv, (Input.GetKey(KeyCode.Q) ? 1.0f : 0) - (Input.GetKey(KeyCode.A) ? 1.0f : 0), 20.0f * Time.deltaTime);
        Motor.In_RightMv = Mathf.Lerp(Motor.In_RightMv, (Input.GetKey(KeyCode.W) ? 1.0f : 0) - (Input.GetKey(KeyCode.S) ? 1.0f : 0), 20.0f * Time.deltaTime);
        Motor.In_TurretRot = Mathf.Lerp(Motor.In_TurretRot, (Input.GetKey(KeyCode.Z) ? 1.0f : 0) - (Input.GetKey(KeyCode.X) ? 1.0f : 0), 20.0f * Time.deltaTime);
        Motor.In_Fire = Input.GetKey(KeyCode.Space) ? 1.0f : -1;
    }
    //[SerializeField]
   // private float speed = 15f;



    /*   ?????
    public enum Directions {
        Forward,
        Reverse,
        Left,
        Right

    }

    Directions tankDirection;

    // Use this for initialization
    void Start() {


    }

    // Update is called once per frame
    void Update() {
        switch(tankDirection) {
            case Directions.Forward:
                transform.position += ForwardMotor(speed);
                break;

            case Directions.Reverse:
                transform.position -= ReverseMotor(speed);
                break;

            case Directions.Left:
                transform.position += LeftMotor(speed);
                break;

            case Directions.Right:
                transform.position += RightMotor(speed);
                break;

        }
    }

    public Vector3 ForwardMotor(float speed) {
        Vector3 pos = Vector3.zero;

        return pos;
    }

    public Vector3 ReverseMotor(float speed) {
        Vector3 pos = Vector3.zero;

        return pos;
    }

    public Vector3 LeftMotor(float speed) {
        Vector3 pos = Vector3.zero;

        return pos;
    }
    public Vector3 RightMotor(float speed) {
        Vector3 pos = Vector3.zero;

        return pos;
    } */
}
