using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
    
    public int GenerationCounter = 0;


    public GameObject TankFab;

    public int Sample = 5;
    public float MaxTime = 10;

    public bool Vs = false;

    public Transform Start1 , Start2;

    public bool Pause = false, Accel = false;
    public float AccelFactor = 20;


    public MovementScorer[] Tanks;
    public float Timer = 0;


    //make base 'Scorer' class out of this...
    public class MovementScorer : MonoBehaviour {


        [HideInInspector]
        public TankMotor Motor;
        [HideInInspector]
        public AiTankController Ctrl;


        public float getFullScore( ) {
             return Score - Mathf.Abs(AV) * 0.00001f;
        }
        public float Score = 0;
        public float AV = 0;

        Vector2[] PosList = new Vector2[20];
        int PLi = -1;
        int Skip = 0;
        void OnEnable() {
            Ctrl = GetComponent<AiTankController>();
            Motor = Ctrl.Motor;
        }
        void FixedUpdate() {


            AV += Motor.Body.angularVelocity;
            if(Skip-- > 0) return;
            Skip = 5;
            var np = Motor.Body.position;

            float dirFactor = 0.3f;
            Score += (np - PosList[PLi] ).magnitude
                * (1.0f - dirFactor + Mathf.Sign(Vector2.Dot(Motor.Body.velocity, Motor.Forward)) * dirFactor);
            PosList[PLi] = np;
            PLi = (PLi + 1) % PosList.Length;
        }

        public void reset(Transform t) {
            Motor.reset(t);
            for(int i = PosList.Length; i-- > 0; ) {
                PosList[i] = Motor.Body.position;
            }
            PLi = 0;
            //LastPos = Motor.Body.position;
            AV= Score = 0;
        }
    };

    void OnEnable() {
        foreach( var t in FindObjectsOfType<AiTankController>() ) 
            Destroy( t.gameObject );

        Sample = Mathf.Max(Sample, 3);
        Tanks = new MovementScorer[Sample];
        for(int i = Sample; i-- >0; ) {
            Tanks[i] = Instantiate(TankFab).AddComponent<MovementScorer>();
        }
        reset();
    }

    void reset() {

        var ot = Tanks;

        Sample = Mathf.Max(Sample, 3);
        if(Sample != Tanks.Length) {            
            Tanks = new MovementScorer[Sample];
            if(ot.Length > Sample) {
                for(int i = Sample; i-- > 0; ) Tanks[i] = ot[i];

                for(int i = ot.Length; i-- > Sample; )
                    Destroy(ot[i].gameObject);
            } else {
                for(int i = ot.Length; i-- > 0; ) Tanks[i] = ot[i];
                for(int i = Sample; i-- > ot.Length; )
                    Tanks[i] = Instantiate(TankFab).AddComponent<MovementScorer>();
            }
        }

        foreach(var t in Tanks) 
            t.reset(Start1);

        Timer = MaxTime;
    }


    void Update() {

        if(Pause)
            Time.timeScale = 0;
        else if(Accel)
            Time.timeScale = 10;
        else
            Time.timeScale = 1;
    }

    MovementScorer LastBest = null;
    void FixedUpdate() {
        if((Timer -= Time.deltaTime) > 0) return;

        GenerationCounter++;

        if (LastBest != null) {
            LastBest.Score *= 0.5f;
        }

        MovementScorer best = Tanks[Tanks.Length - 1];
        float scr = best.getFullScore();
        for( int i = Tanks.Length-1; i-- >0; ) {
            var t = Tanks[i];
            if(t.getFullScore() > scr )
                best = t;
        }
        Debug.Log("best " + best.Score);
        reset();

        if( LastBest== null ) LastBest = best;

        foreach(var t in Tanks) {
            if(t == best || t == LastBest) continue;
            geneticOptimisation( t,  (Random.value > 0.5f) ? best : LastBest  );
        }
        LastBest = best;

    }

    //todo - class-ify optimiser so it can haz parameters and not look shit  -- also, ya know, actually do it properly
    void geneticOptimisation(MovementScorer t1, MovementScorer t2) {
        NeuralNetwork.Network n1 = t1.Ctrl.NN, n2 = t2.Ctrl.NN;
       // if( n1 != n2 ) 
            for(int layer = n1.Neurons.Length; --layer > 0; ) {
                int nl = n1.Neurons[layer].Length;
                for(int ri = Mathf.CeilToInt((float)nl * Random.Range(0.5f, 0.8f)  ); ri-- > 0; ) {
                    int ni = Random.Range(0, nl);  //chance of duplicate - i suspect tracking this would be more cost than worth
                    n1.Neurons[layer][ni].bias = n2.Neurons[layer][ni].bias;
                }
                var s1 = n1.Synapsis[layer - 1];
                var s2 = n2.Synapsis[layer - 1];
                int sl = s1.Length;
                for(int ri = Mathf.CeilToInt((float)s1.Length * Random.Range(0.5f, 0.8f) ); ri-- > 0; ) {
                    int si = Random.Range(0, s1.Length);  //chance of duplicate - i suspect tracking this would be more cost than worth
                    s1[si].weight = s2[si].weight;
                }
            }
       
        
        for(int layer = n1.Neurons.Length; --layer > 0; ) {
            int nl = n1.Neurons[layer].Length;
            for(int ri = Mathf.CeilToInt((float)nl * Random.Range(0.0f, 0.1f)); ri-- > 0; ) {
                int ni = Random.Range(0, nl);  //chance of duplicate - i suspect tracking this would be more cost than worth
                var m = Random.Range(-1.0f, 1.0f);
                if(Random.value > 0.95f) {
                    n1.Neurons[layer][ni].bias = m;
                } else {
                    m *= Mathf.Abs(m);
                    n1.Neurons[layer][ni].bias = NeuralNetwork.ActivationMethods.LogisticSigmoid(n1.Neurons[layer][ni].bias + m *0.05f);
                }
            }
            var s1 = n1.Synapsis[layer - 1];
            int sl = s1.Length;
            for(int ri = Mathf.CeilToInt((float)s1.Length * Random.Range(0.0f, 0.1f)); ri-- > 0; ) {
                int si = Random.Range(0, s1.Length);  //chance of duplicate - i suspect tracking this would be more cost than worth
                var m = Random.Range(-1.0f,1.0f);
                if(Random.value > 0.95f) {
                    s1[si].weight = m;
                } else {
                    m *= Mathf.Abs(m);
                    s1[si].weight = NeuralNetwork.ActivationMethods.LogisticSigmoid(s1[si].weight + m * 0.05f);
                }
            }
        }
    }
}
