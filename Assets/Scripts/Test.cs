using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Test : MonoBehaviour {
    
    public int GenerationCounter = 0;


    public GameObject TankFab;

    public int ArenaCnt = 1;
    public int Sample = 5;
    public float MaxTime = 10;

    public bool Vs = false;

    public List<Transform> Start1 , Start2;

    public bool Pause = false, Accel = false;
    public float AccelFactor = 20;

    public NeuralNetwork.Network[] Nets;
    public MovementScorer[,] Tanks;
    public float Timer = 0;

    public bool UseJimCast = true;

    public static bool IsTesting = false;

    //make base 'Scorer' class out of this...
    public class MovementScorer : MonoBehaviour {


        [HideInInspector]
        public TankMotor Motor;
        [HideInInspector]
        public AiTankController Ctrl;


        public float Vel = 0;
        public float AV = 0;
        public float Score = 0;

        Vector2[] PosList = new Vector2[20];
        int PLi = -1;
        int Skip = 0;
        void OnEnable() {
            Ctrl = GetComponent<AiTankController>();
            Motor = Ctrl.Motor;
            Sys.get().add(this);
        }
        void OnDisable() {
            if(Sys.get())
                Sys.get().Scorers.Remove(this);
        }

        public void aFixedUpdate() {


            AV += Motor.AngVel;
            if(Skip-- > 0) return;
            Skip = 5;
            var np = Motor.Pos;

            float dirFactor = 0.7f;
            Vel += (np - PosList[PLi] ).magnitude
                * (1.0f - dirFactor + Mathf.Sign(Vector2.Dot(Motor.Pos, Motor.Forward)) * dirFactor);
            PosList[PLi] = np;


            PLi = (PLi + 1) % PosList.Length;

            Score = Vel - Mathf.Abs(AV) * 0.01f;
        }

        public void reset(Transform t) {
            Motor.reset(t);
            for(int i = PosList.Length; i-- > 0; ) {
                PosList[i] = Motor.Pos;
            }
            PLi = 0;
            //LastPos = Motor.Pos;
            AV= Score = 0;
        }
    };

    void initTank( int i, int j ) {

        Tanks[i, j] = Instantiate(TankFab).AddComponent<MovementScorer>();
        if(j == 0)
            Tanks[i, j].Ctrl.init();
        else
            Tanks[i, j].Ctrl.init(Tanks[i, 0].Ctrl.NN);

        var scn = Tanks[i, j].GetComponent<ScannerHlpr>();

        scn.Area = Start1[j].GetComponentInParent<Arena>();
        scn.init();
    }
    void OnEnable() {
        foreach( var t in FindObjectsOfType<AiTankController>() ) 
            Destroy( t.gameObject );

        Sample = Mathf.Max(Sample, 3);
        ArenaCnt = Mathf.Clamp(ArenaCnt, 1, Start1.Count);
        Tanks = new MovementScorer[Sample, ArenaCnt ];
        for(int j =0; j < ArenaCnt; j++) 
            for(int i = Sample; i-- >0; ) {
                initTank(i, j);
            }
        reset();
        IsTesting = true;

    }
    void Awake() {
        IsTesting = true;
    }
    
    void OnDisable() {

        IsTesting = false;

    }

    void reset() {


        int ac = Tanks.GetLength(1);
        Sample = Mathf.Max(Sample, 3);
        if(Sample != Tanks.GetLength(0)) {
            Debug.Log("change sample " + Sample + "__" + Tanks.GetLength(0));
            var ot = Tanks;
            Tanks = new MovementScorer[Sample, ac];
            for(int j = 0; j < ac; j++) {
                if(ot.GetLength(0) > Sample) {

                    for(int i = Sample; i-- > 0;) Tanks[i, j] = ot[i, j];

                    for(int i = ot.GetLength(0); i-- > Sample;)
                        Destroy(ot[i, j].gameObject);
                } else {
                    for(int i = ot.GetLength(0); i-- > 0;) Tanks[i, j] = ot[i, j];
                    for(int i = Sample; i-- > ot.GetLength(0);) {
                        initTank(i, j);
                    }
                }
            }
        }

        ArenaCnt = Mathf.Clamp(ArenaCnt, 1, Start1.Count);
        if(ac != ArenaCnt ) {
            Debug.Log("change ArenaCnt " + ArenaCnt + "__" + Tanks.GetLength(1));
            var ot = Tanks;
            Tanks = new MovementScorer[Sample, ArenaCnt];


            
            if(ac > ArenaCnt) {

                for(int j = ArenaCnt; j-- > 0;)
                    for(int i = Sample; i-- > 0;)
                        Tanks[i, j] = ot[i, j];
                for(int j = ac; j-- > ArenaCnt;)
                    for(int i = Sample; i-- > 0;) 
                        Destroy(ot[i, j].gameObject);
            } else {
                for(int j = ac; j-- > 0;)
                    for(int i = Sample; i-- > 0;)
                        Tanks[i, j] = ot[i, j];


                for(int j = ArenaCnt; j-- > ac;)
                    for(int i = Sample; i-- > 0;) {
                        initTank(i, j);
                    }
            }
            
        }


        for(int j = ArenaCnt; j-- > 0;)
            for(int i = Sample; i-- > 0;)
                Tanks[i, j].reset(Start1[j]);

        Timer = MaxTime;
    }


    void Update() {

        if(Pause)
            Time.timeScale = 0;
        else if(Accel)
            Time.timeScale = AccelFactor;
        else
            Time.timeScale = 1;
    }

    MovementScorer LastBest = null;
    public void aFixedUpdate() {
        if((Timer -= Time.deltaTime) > 0) return;

        GenerationCounter++;

        MovementScorer best = Tanks[Tanks.GetLength(0) - 1,0];

        for( int j = Tanks.GetLength(1); --j >0; ) 
            for( int i = Tanks.GetLength(0); i-- > 0;) {
                Tanks[i, 0].Score += Tanks[i, j].Score;
            }

        if (LastBest != null) {
            LastBest.Score *= 0.5f;
            for (int i = Tanks.GetLength(0) - 1; i-- > 0;) {
                var t = Tanks[i, 0];
                if (t.Score > best.Score)
                    best = t;
            }
        } else {
            LastBest = Tanks[Tanks.GetLength(0) - 2, 0];
            if( LastBest.Score > best.Score ) {
                LastBest = Tanks[Tanks.GetLength(0) - 1, 0];
                best = Tanks[Tanks.GetLength(0) - 2, 0];
            }

            for (int i = Tanks.GetLength(0) - 1; i-- > 0;) {
                var t = Tanks[i, 0];
                if (t.Score > LastBest.Score)
                    if (t.Score > best.Score) {
                        LastBest = best;
                        best = t;
                    }  else
                        LastBest = t;

            }

        }

        Debug.Log("best " + best.Score);
        reset();

        for(int i = Tanks.GetLength(0) - 1; i-- > 0;) {
            var t = Tanks[i, 0];
            if(t == best || t == LastBest) continue;
            var mix = (Random.value > 0.5f) ? best : LastBest;
          //  if(t == best) mix = LastBest;
           // else if(t == LastBest) mix = best;
            geneticOptimisation( t,  mix );
        }
        LastBest = best;

    }

    //todo - class-ify optimiser so it can haz parameters and not look shit  -- also, ya know, actually do it properly
    void geneticOptimisation(MovementScorer t1, MovementScorer t2) {
        NeuralNetwork.Network n1 = t1.Ctrl.NN, n2 = t2.Ctrl.NN;
       // if( n1 != n2 ) 
            for(int layer = n1.Neurons.Length; --layer > 0; ) {
                int nl = n1.Neurons[layer].Length;
                for(int ri = Mathf.CeilToInt((float)nl * Random.Range(0.7f, 0.9f)  ); ri-- > 0; ) {
                    int ni = Random.Range(0, nl);  //chance of duplicate - i suspect tracking this would be more cost than worth
                    n1.Neurons[layer][ni].bias = n2.Neurons[layer][ni].bias;
                }
                var s1 = n1.Synapsis[layer - 1];
                var s2 = n2.Synapsis[layer - 1];
                int sl = s1.Length;
                for(int ri = Mathf.CeilToInt((float)s1.Length * Random.Range(0.7f, 0.9f) ); ri-- > 0; ) {
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
