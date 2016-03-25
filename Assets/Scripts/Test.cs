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

    [System.Serializable]
    public class ArenaData {
        public Transform S1, S2;
        public float ScoreMod = 1;

        public float LastScore = 0;
    }; 

    public List<ArenaData> ArenaDat;

    public bool Pause = false, Accel = false;
    public float AccelFactor = 20;

    public NeuralNetwork.Network[] Nets;
    public Scorer[,,] Tanks;
    public float Timer = 0;

    public bool UseJimCast = true;

    public static bool IsTesting = false;


    void initTank( int i, int j, int k ) {

        Tanks[i, j, k] = Instantiate(TankFab).AddComponent(ScoreType ) as Scorer;
        if(k == 0)
            Tanks[i, j, k].Ctrl.init();
        else
            Tanks[i, j, k].Ctrl.init(Tanks[i, j, 0].Ctrl.NN);

        var scn = Tanks[i, j, k].GetComponent<ScannerHlpr>();

        scn.Area = ArenaDat[k].S1.GetComponentInParent<Arena>();
        scn.init();
    }
    System.Type ScoreType;
    Scorer Scrr;

    void OnEnable() {

        ScoreType = (Scrr=GetComponent<Scorer>()).GetType();        

        foreach( var t in FindObjectsOfType<AiTankController>() ) 
            Destroy( t.gameObject );

        Sample = Mathf.Clamp(Sample, 1, 999);
        ArenaCnt = Mathf.Clamp(ArenaCnt, 1, ArenaDat.Count);
        StrainCnt = Mathf.Clamp(StrainCnt, 1, Sample-2 );
        Tanks = new Scorer[Sample, StrainCnt, ArenaCnt ];
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
            Tanks = new Scorer[Sample,sc, ac];
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

        ArenaCnt = Mathf.Clamp(ArenaCnt, 1, ArenaDat.Count);
        if(ac != ArenaCnt ) {
            Debug.Log("change ArenaCnt " + ArenaCnt + "__" + Tanks.GetLength(2));
            var ot = Tanks;
            Tanks = new Scorer[Sample, sc, ArenaCnt];       
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
            Tanks = new Scorer[Sample, StrainCnt, ArenaCnt];
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
                    Tanks[i, j,k].reset(ArenaDat[k].S1, Scrr);

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


    //  Scorer LastBest = null;
    public void aFixedUpdate() {



        if((Timer -= Time.deltaTime) > 0) return;

        GenerationCounter++;



        for(int j = Tanks.GetLength(1); j-- > 0;) {
            //  Scorer best = Tanks[Tanks.GetLength(0) - 1, j, 0];

            float tm = 0;
            int bestI = Tanks.GetLength(0)-1;
            float[] arenaScores = new float[Tanks.GetLength(0)];
            for(int i = Tanks.GetLength(0); i-- > 0;) {
                for(int k = Tanks.GetLength(2); k-- > 0;) {
                    arenaScores[i] += Tanks[i, j, k].Score * ArenaDat[k].ScoreMod;
                    tm += ArenaDat[k].ScoreMod;
                }

                if(arenaScores[i] > arenaScores[bestI])
                    bestI = i;
            }

            /*
            for(int i = Tanks.GetLength(0) - 1; i-- > 0;) {
                for(int k = Tanks.GetLength(2); --k > 0;)
                    Tanks[i, j, 0].Score += Tanks[i, j, k].Score;
                if(Tanks[i, j, 0].Score > best.Score)
                    best = Tanks[i, j, 0];
            }
            
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

            var best = Tanks[bestI, j, 0];
            float avg = arenaScores[bestI] / tm;
            Debug.Log("best " + avg );

            int bestK = 0;
            ArenaDat[0].ScoreMod = 1;

            for(int k = Tanks.GetLength(2); --k > 0;) {
                ArenaDat[k].ScoreMod = 1;
                if(Tanks[bestI, j, k].Score > Tanks[bestI, j, bestK].Score)
                    bestK = k;
            }
            ArenaDat[bestK].ScoreMod = 0.1f;
            for(int i = Tanks.GetLength(0); i-- > 0;) {
                var t = Tanks[i,j, 0];
                if(t == best ) continue;
               
                geneticOptimisation(t, best );
            }
        }
        reset();
    }

    //todo - class-ify optimiser so it can haz parameters and not look shit  -- also, ya know, actually do it properly
    void geneticOptimisation(Scorer t1, Scorer t2) {
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
                    n1.Neurons[layer][ni].bias = NeuralNetwork.ActivationMethods.HyperbolidTangent(n1.Neurons[layer][ni].bias + m *0.01f);
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
                    s1[si].weight = NeuralNetwork.ActivationMethods.HyperbolidTangent(s1[si].weight + m * 0.01f);
                }
            }
        }
    }
}
