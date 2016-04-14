using UnityEngine;
using System.IO;

public class AiTankController : MonoBehaviour {

    [HideInInspector] public TankMotor Motor;

    [HideInInspector]
    public NeuralNetwork.Network NN;
    public bool loadWeight = false;

    float[] Input;

    void OnEnable() {
        Motor = GetComponent<TankMotor>();

        Input = new float[4 + Motor.Scanner.Count*2];
        if(!Test.IsTesting) {
            init( GetComponents<AIConfig>() );
        }
        Sys.get().add(this);
    }
    void OnDisable() {
        if(Sys.get())
            Sys.get().Ais.Remove(this);
    }

    public void init( AIConfig[] aic  ) {

        if( aic ==null || aic.Length == 0 ) { //old 
            int[] layers = { Input.Length, 8, 4 };
            try {
                var r = Random.value; //lazy..
                NN = new NeuralNetwork.Network(layers, true, Random.seed);
                if(loadWeight) {
                    NN.Weights = LoadWeights();
                }
            } catch(System.Exception e) {
                Debug.LogError("NN err: " + e.Message);
            }
        } else {

            int mxLayer = -1;
            foreach(var c in aic) {
                mxLayer = Mathf.Max(mxLayer, c.Layers.Count + c.StartLayer);
            }

            int[] layers = new int[mxLayer +1];

            layers[0] = Input.Length;
            layers[mxLayer] = 4;


            foreach(var c in aic) {
                if(c.enabled)
                    for(int i = c.Layers.Count; i-- > 0;) {
                        c.Layers[i].Ni1 = layers[c.StartLayer + i];
                        layers[c.StartLayer + i] += c.Layers[i].Cnt;
                    }
            }

            Debug.Log("lc  " + layers.Length);

            foreach(int i in layers) {
                Debug.Log("  " + i);
            }

          //  try {
                var r = Random.value; //lazy..
                NN = new NeuralNetwork.Network(layers, Random.seed);


                foreach(var c in aic) {
                    if( c.enabled )
                        c.init(NN, Motor.Scanner.Count);
                }

                var output = NN.Compute(Input);
                /*if(loadWeight) {
                    NN.Weights = LoadWeights();
                } */
            //} catch(System.Exception e) {

             //   Debug.LogError("NN err 1: " + e.Message);
           // }
        }
    }

    float[] LoadWeights() {

        TextReader read = new StreamReader("weights.txt");
        string[] str = read.ReadToEnd().Split(',');
        read.Close();
        float[] weights = new float[str.Length];
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = float.Parse(str[i]);
        }

        return weights;
    }

    public void init(NeuralNetwork.Network nn) {
        //NN = nn;
        NN = new NeuralNetwork.Network(nn);
    }



    public void aFixedUpdate() {
        int inI = 0;
        Input[inI++] = Motor.Out_Vel.x;
        Input[inI++] = Motor.Out_Vel.y;
        Input[inI++] = Motor.Out_TurretDir.x;
        Input[inI++] = Motor.Out_TurretDir.y;
        foreach(var r in Motor.Scanner) {
            Input[inI + Motor.Scanner.Count] = r.Out_Opponent;
            Input[inI++] = r.Out_Dis;
        }

        try {
            var output = NN.Compute(Input);
            Motor.In_LeftMv = output[0];
            Motor.In_RightMv = output[1];
            Motor.In_Fire = output[2];
            Motor.In_TurretRot = output[3];
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
