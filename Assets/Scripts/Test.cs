using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Test : MonoBehaviour {
    
    public int GenerationCounter = 0;


    public GameObject TankFab;

    public int ArenaCnt = 1;
    public int StrainCnt  = 1;
    public int Sample = 5;
    public float MaxTime = 10;

    public bool Vs = false;

    public List<Transform> Start1 , Start2;

    public bool Pause = false, Accel = false;
    public float AccelFactor = 20;

    public NeuralNetwork.Network[] Nets;
    public MovementScorer[,,] Tanks;
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
        public float Spacing = 0;
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
            Vel += (np - PosList[PLi]).magnitude
                * (1.0f - dirFactor + Mathf.Sign(Vector2.Dot(Motor.Pos, Motor.Forward)) * dirFactor);
            PosList[PLi] = np;

            foreach(var r in Motor.Scanner) {
                float m = Mathf.Min(0.2f, r.Out_Dis * r.RangeMod );
                Spacing += m / r.RangeMod;
            }
            PLi = (PLi + 1) % PosList.Length;

           // Score = Spacing + Vel - Mathf.Abs(AV) * 0.0025f;
            //Score = Spacing;
        }

        public void reset(Transform t) {
            Motor.reset(t);
            for(int i = PosList.Length; i-- > 0; ) {
                PosList[i] = Motor.Pos;
            }
            PLi = 0;
            //LastPos = Motor.Pos;
            Spacing = Vel = AV = Score = 0;
        }
    };

    void initTank( int i, int j, int k ) {

        Tanks[i, j, k] = Instantiate(TankFab).AddComponent<MovementScorer>();
        if(k == 0)
            Tanks[i, j, k].Ctrl.init();
        else
            Tanks[i, j, k].Ctrl.init(Tanks[i, j, 0].Ctrl.NN);

        var scn = Tanks[i, j, k].GetComponent<ScannerHlpr>();

        scn.Area = Start1[k].GetComponentInParent<Arena>();
        scn.init();
    }
    void OnEnable() {
        foreach( var t in FindObjectsOfType<AiTankController>() ) 
            Destroy( t.gameObject );

        Sample = Mathf.Clamp(Sample, 1, 999);
        ArenaCnt = Mathf.Clamp(ArenaCnt, 1, Start1.Count);
        StrainCnt = Mathf.Clamp(StrainCnt, 1, Sample-2 );
        Tanks = new MovementScorer[Sample, StrainCnt, ArenaCnt ];
        for(int k = 0; k < ArenaCnt; k++)
            for(int j = 0; j < StrainCnt; j++)
                for(int i = Sample; i-- >0; ) {
                initTank(i, j, k);
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
        ///this got over complicated when i wasn't looking at it...

        int ac = Tanks.GetLength(2), sc = Tanks.GetLength(1);
        Sample = Mathf.Max(Sample, 3);
        if(Sample != Tanks.GetLength(0)) {
            Debug.Log("change sample " + Sample + "__" + Tanks.GetLength(0));
            var ot = Tanks;
            Tanks = new MovementScorer[Sample,sc, ac];
            for(int k = 0; k < ac; k++)
                for(int j = 0; j < sc; j++) {
                    if(ot.GetLength(0) > Sample) {

                        for(int i = Sample; i-- > 0;) Tanks[i, j, k] = ot[i, j, k];

                        for(int i = ot.GetLength(0); i-- > Sample;)
                            Destroy(ot[i, j, k].gameObject);
                    } else {
                        for(int i = ot.GetLength(0); i-- > 0;) Tanks[i, j, k] = ot[i, j, k];
                        for(int i = Sample; i-- > ot.GetLength(0);) {
                            initTank(i, j, k);
                        }
                    }
            }
        }

        ArenaCnt = Mathf.Clamp(ArenaCnt, 1, Start1.Count);
        if(ac != ArenaCnt ) {
            Debug.Log("change ArenaCnt " + ArenaCnt + "__" + Tanks.GetLength(2));
            var ot = Tanks;
            Tanks = new MovementScorer[Sample, sc, ArenaCnt];       
            if(ac > ArenaCnt) {

                for(int k = ArenaCnt; k-- > 0;)
                    for(int j = sc; j-- > 0;)
                        for(int i = Sample; i-- > 0;)
                            Tanks[i, j, k] = ot[i, j, k];
                for(int k = ac; k-- > ArenaCnt;)
                    for(int j = sc; j-- > 0;)
                        for(int i = Sample; i-- > 0;) 
                            Destroy(ot[i, j, k].gameObject);
            } else {
                for(int k = ac; k-- > 0;)
                    for(int j = sc; j-- > 0;)
                        for(int i = Sample; i-- > 0;)
                            Tanks[i, j,k] = ot[i, j, k];


                for(int k = ArenaCnt; k-- > ac;)
                    for(int j = sc; j-- > 0;)
                        for(int i = Sample; i-- > 0;) {
                            initTank(i, j,k);
                        }
             }            
        }

        StrainCnt = Mathf.Clamp(StrainCnt, 1, Sample -2 );
        if(sc != StrainCnt) {
            Debug.Log("change StrainCnt " + StrainCnt + "__" + Tanks.GetLength(1));
            var ot = Tanks;
            Tanks = new MovementScorer[Sample, StrainCnt, ArenaCnt];
            if(sc > StrainCnt) {

                for(int k = 0; k < ArenaCnt; k++)
                    for(int j = StrainCnt; j-- > 0;)
                        for(int i = Sample; i-- > 0;)
                            Tanks[i, j, k] = ot[i, j, k];
                for(int k = 0; k < ArenaCnt; k++)
                    for(int j = sc; j-- > StrainCnt;)
                        for(int i = Sample; i-- > 0;)
                            Destroy(ot[i, j, k].gameObject);
            } else {
                for(int k = 0; k < ArenaCnt; k++)
                    for(int j = sc; j-- > 0;)
                        for(int i = Sample; i-- > 0;)
                            Tanks[i, j, k] = ot[i, j, k];


                for(int k = 0; k < ArenaCnt; k++)
                    for(int j = StrainCnt; j-- > sc;)
                        for(int i = Sample; i-- > 0;) {
                            initTank(i, j, k);
                        }
            }
        }


        for(int k = ArenaCnt; k-- > 0;)
            for(int j = StrainCnt; j-- > 0;)
                for(int i = Sample; i-- > 0;)
                    Tanks[i, j,k].reset(Start1[k]);

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


    //  MovementScorer LastBest = null;
    public void aFixedUpdate() {



        if((Timer -= Time.deltaTime) > 0) return;

        GenerationCounter++;



        for(int j = Tanks.GetLength(1); j-- > 0;) {
            MovementScorer best = Tanks[Tanks.GetLength(0) - 1, j, 0];

            for(int i = Tanks.GetLength(0) - 1; i-- > 0;) {
                for(int k = Tanks.GetLength(2); --k > 0;)
                    Tanks[i, j, 0].Score += Tanks[i, j, k].Score;
                if(Tanks[i, j, 0].Score > best.Score)
                    best = Tanks[i, j, 0];
            }
            /*
            if(LastBest != null) {
                LastBest.Score *= 0.5f;
                for(int i = Tanks.GetLength(0) - 1; i-- > 0;) {
                    var t = Tanks[i, 0];
                    if(t.Score > best.Score)
                        best = t;
                }
            } else {
                LastBest = Tanks[Tanks.GetLength(0) - 2, 0];
                if(LastBest.Score > best.Score) {
                    LastBest = Tanks[Tanks.GetLength(0) - 1, 0];
                    best = Tanks[Tanks.GetLength(0) - 2, 0];
                }

                for(int i = Tanks.GetLength(0) - 1; i-- > 0;) {
                    var t = Tanks[i, 0];
                    if(t.Score > LastBest.Score)
                        if(t.Score > best.Score) {
                            LastBest = best;
                            best = t;
                        } else
                            LastBest = t;

                }

            } */
            
            Debug.Log("best " + best.Score);

            for(int i = Tanks.GetLength(0) - 1; i-- > 0;) {
                var t = Tanks[i,j, 0];
                if(t == best ) continue;
               
                geneticOptimisation(t, best );
            }
        }
        reset();
    }

    //todo - class-ify optimiser so it can haz parameters and not look shit  -- also, ya know, actually do it properly
    void geneticOptimisation(MovementScorer t1, MovementScorer t2) {
        NeuralNetwork.Network n1 = t1.Ctrl.NN, n2 = t2.Ctrl.NN;

        if(n1 != n2)
            for(int layer = n1.Neurons.Length; --layer > 0;) { 
                n2.Neurons[layer].CopyTo(n1.Neurons[layer], 0);
                n2.Synapsis[layer-1].CopyTo(n1.Synapsis[layer-1], 0);
            }

              /*      
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
            } */
       
        
        for(int layer = n1.Neurons.Length; --layer > 0; ) {
            int nl = n1.Neurons[layer].Length;
            for(int ri = Mathf.CeilToInt((float)nl * Random.Range(0.0f, 0.1f)); ri-- > 0; ) {
                int ni = Random.Range(0, nl);  //chance of duplicate - i suspect tracking this would be more cost than worth
                var m = Random.Range(-1.0f, 1.0f);
                if(Random.value > 0.975f) {
                    n1.Neurons[layer][ni].bias = m;
                } else {
                    m *= Mathf.Abs(m);
                    n1.Neurons[layer][ni].bias = NeuralNetwork.ActivationMethods.LogisticSigmoid(n1.Neurons[layer][ni].bias + m *0.01f);
                }
            }
            var s1 = n1.Synapsis[layer - 1];
            int sl = s1.Length;
            for(int ri = Mathf.CeilToInt((float)s1.Length * Random.Range(0.0f, 0.1f)); ri-- > 0; ) {
                int si = Random.Range(0, s1.Length);  //chance of duplicate - i suspect tracking this would be more cost than worth
                var m = Random.Range(-1.0f,1.0f);
                if(Random.value > 0.975f) {
                    s1[si].weight = m;
                } else {
                    m *= Mathf.Abs(m);
                    s1[si].weight = NeuralNetwork.ActivationMethods.LogisticSigmoid(s1[si].weight + m * 0.01f);
                }
            }
        }
    }
}
