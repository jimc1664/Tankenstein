using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class Sys : MonoBehaviour {
    static Sys Singleton;
    public static Sys get() {
        if(Singleton == null) Singleton = FindObjectOfType<Sys>();
        return Singleton;
    }
    [System.NonSerialized]
    public List<TankMotor> Tanks = new List<TankMotor>();
    [System.NonSerialized]
    public List<AiTankController> Ais = new List<AiTankController>();
    [System.NonSerialized]
    public List<Scorer> Scorers = new List<Scorer>();


    public void add(TankMotor tm) {
        if(tm.GetComponent<AiTankController>()) return;
        Tanks.Add(tm);
    }
    public void add(AiTankController a) {
        if(a.GetComponent<Scorer>()) return;
        Ais.Add(a);
    }
    public void add(Scorer s) {
        Ais.Remove(s.GetComponent<AiTankController>());
        Scorers.Add(s);
    }

    public int ThreadCount = 8;

    Test Tst;

    Thread[] Threads;

    void OnEnable() {

        Tst = FindObjectOfType<Test>();

        Flag = true;
        Shutdown = false;
        Threads = new Thread[ThreadCount];
        for( int i = ThreadCount; i-- >0;) {
            Threads[i] = new Thread(new ThreadStart( threadFunc ));
            Threads[i].Start();
            
        }
       
    }
    void OnDisable() {
        Shutdown = true;
        for(int i = ThreadCount; i-- > 0;) {
            Threads[i].Join();
        }
    }
    volatile bool Shutdown, Flag;

    volatile int Pending =0, Working, TankI, AiI, ScorerI;
    
    void threadFunc() {

        for(;;) {
            Interlocked.Increment(ref Pending);
            while(Flag)
                if(Shutdown) return;
                else Thread.Sleep(1);

            for(; ;) {
                var si = Interlocked.Decrement(ref ScorerI);
                if(si < 0) break;
                var s = Scorers[si];
                // s.Motor.aFixedUpdate();
                s.Motor.ScanH.proc();
                s.Ctrl.aFixedUpdate();
                s.aFixedUpdate();
            }
            for(; ;) {
                var aii = Interlocked.Decrement(ref AiI);
                if(aii < 0) break;
                var a = Ais[aii];
                a.Motor.ScanH.proc();
                a.aFixedUpdate();
            }
            for(; ;) {
                var ti = Interlocked.Decrement(ref TankI);
                if(ti < 0) break;
                var t = Tanks[ti];
                t.ScanH.proc();
            }
            Interlocked.Decrement(ref Working);
            while(!Flag)
                if(Shutdown) return;
                else Thread.Sleep(1);
        }
    }



    void FixedUpdate () {

        while(Working != 0) {
          //  Thread.Sleep(1);
            //  if(iter-- < 0) break;
        }
        Flag = true;


        if(Tst)
            Tst.aFixedUpdate();
        else
            return;

        //Debug.Log(" about to start iter " + Pending);

        foreach(var t in Tanks) t.aFixedUpdate();
        foreach(var a in Ais) a.Motor.aFixedUpdate();
        foreach(var s in Scorers) s.Motor.aFixedUpdate();

        while(Pending != Threads.Length) {
            //Thread.Sleep(1);
        }

        TankI = Tanks.Count;
        ScorerI = Scorers.Count;
        AiI = Ais.Count;

        Pending = 0;
        Working = Threads.Length;
        Flag = false;
       // Debug.Log(" started iter " + Working );
      //  int iter = 100;



        /*

        foreach(var t in Tanks)
            t.aFixedUpdate();
        foreach(var t in Ais)
            t.aFixedUpdate();
        foreach(var t in Scorers)
            t.aFixedUpdate();
            */
    }
}
