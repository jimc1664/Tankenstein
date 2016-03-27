using UnityEngine;
using System.Collections;

public class AiTankController : MonoBehaviour {

    [HideInInspector] public TankMotor Motor;

    [HideInInspector]
    public NeuralNetwork.Network NN;


    float[] Input;

    void OnEnable() {
        Motor = GetComponent<TankMotor>();

        Input = new float[2 + Motor.Scanner.Count];
        if(!Test.IsTesting)
            init();
        Sys.get().add(this);
    }
    void OnDisable() {
        if(Sys.get())
            Sys.get().Ais.Remove(this);
    }

    public void init() {

        int[] layers = { Input.Length, 8, 4, 2 };
        try {
            var r = Random.value; //lazy..
            NN = new NeuralNetwork.Network(layers, true, Random.seed);
        } catch(System.Exception e) {
            Debug.LogError("NN err: " + e.Message);
        }
    }
    public void init(NeuralNetwork.Network nn) {
        //NN = nn;
        NN = new NeuralNetwork.Network(nn);
        
    }

    public void aFixedUpdate() {
        int inI = 0;
        Input[inI++] = Motor.Out_Vel.x;
        Input[inI++] = Motor.Out_Vel.y;
        foreach(var r in Motor.Scanner) {
            Input[inI++] = r.Out_Dis;
        }

        try {
            var output = NN.Compute(Input);
            Motor.In_LeftMv = output[0];
            Motor.In_RightMv = output[1];
        } catch(System.Exception e) {
            Debug.LogError("NN err: " + e.Message);
        }
    }


  /*  void aUpdate() {
        Motor.In_LeftMv = Mathf.Lerp(Motor.In_LeftMv, (Input.GetKey(KeyCode.Q) ? 1.0f : 0) - (Input.GetKey(KeyCode.A) ? 1.0f : 0), 20.0f * Time.deltaTime);
        Motor.In_RightMv = Mathf.Lerp(Motor.In_RightMv, (Input.GetKey(KeyCode.W) ? 1.0f : 0) - (Input.GetKey(KeyCode.S) ? 1.0f : 0), 20.0f * Time.deltaTime);
        Motor.In_TurretRot = Mathf.Lerp(Motor.In_TurretRot, (Input.GetKey(KeyCode.Z) ? 1.0f : 0) - (Input.GetKey(KeyCode.X) ? 1.0f : 0), 20.0f * Time.deltaTime);
        Motor.In_Fire = Input.GetKey(KeyCode.Space) ? 1.0f : -1;
    } */
}
