using UnityEngine;
using System.Collections;
using System.Collections.Generic;


using UnityEditor;

public class ScannerHlpr : MonoBehaviour {

    void OnEnable() {
        Motor = GetComponent<TankMotor>();

        if(Area != null) init();
    }
    public Arena Area;

    public class ObstacleEntry {
        public Arena.Obstacle Obs;
        public float Dis;
        public int Ri;
    }
    List<ObstacleEntry> Obs;

    /*
    public Vector2 ClosestIntersection(float cx, float cy, float radius,
           Vector2 lineStart, Vector2 lineEnd) {
        Vector2 intersection1;
        Vector2 intersection2;
        int intersections = FindLineCircleIntersections(cx, cy, radius, lineStart, lineEnd, out intersection1, out intersection2);

        if(intersections == 1)
            return intersection1;//one intersection

        if(intersections == 2) {
            double dist1 = Distance(intersection1, lineStart);
            double dist2 = Distance(intersection2, lineStart);

            if(dist1 < dist2)
                return intersection1;
            else
                return intersection2;
        }

        return Vector2.Empty;// no intersections at all
    }

    private double Distance(Vector2 p1, Vector2 p2) {
        return Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
    }

    // Find the points of intersection.
    private int FindLineCircleIntersections(float cx, float cy, float radius,
        Vector2 point1, Vector2 point2, out Vector2 intersection1, out Vector2 intersection2) {
        float dx, dy, A, B, C, det, t;

        dx = point2.x - point1.x;
        dy = point2.y - point1.y;

        A = dx * dx + dy * dy;
        B = 2 * (dx * (point1.x - cx) + dy * (point1.y - cy));
        C = (point1.x - cx) * (point1.x - cx) + (point1.y - cy) * (point1.y - cy) - radius * radius;

        det = B * B - 4 * A * C;
        if((A <= 0.0000001) || (det < 0)) {
            // No real solutions.
            intersection1 = new Vector2(float.NaN, float.NaN);
            intersection2 = new Vector2(float.NaN, float.NaN);
            return 0;
        } else if(det == 0) {
            // One solution.
            t = -B / (2 * A);
            intersection1 = new Vector2(point1.x + t * dx, point1.y + t * dy);
            intersection2 = new Vector2(float.NaN, float.NaN);
            return 1;
        } else {
            // Two solutions.
            t = (float)((-B + Mathf.Sqrt(det)) / (2 * A));
            intersection1 = new Vector2(point1.x + t * dx, point1.y + t * dy);
            t = (float)((-B - Mathf.Sqrt(det)) / (2 * A));
            intersection2 = new Vector2(point1.x + t * dx, point1.y + t * dy);
            return 2;
        }
    }*/


    TankMotor Motor;
    public void init() {
        Obs = new List<ObstacleEntry>();
        foreach( var o in Area.Obs ) {

            var vec = o.Pos - Motor.Body.position;
            float mag = vec.magnitude;
            var dir = vec / mag;

            int bestRi = -1; float bestDt = float.MinValue;
            for( int ri = Motor.Scanner.Count; ri-- >0; ) {
                var r = Motor.Scanner[ri];
                float dt = Vector2.Dot(r.Dir, dir);
                if( dt > bestDt ) {
                    bestDt = dt;
                    bestRi = ri;
                }

            }

            Obs.Add( new ObstacleEntry {
                Obs = o,
                Dis = mag - o.Rad,
                Ri = bestRi,
            } );
        }
    }


    public static void FindIntersection(
          Vector2 _p1, Vector2 d1, Vector2 _p2, Vector2 d2,
          out Vector2 intersection,
          out float t1, out float t2 ) {

        Vector2 p1 = _p1, p2 = _p1 + d1, p3 = _p2, p4 = _p2+d2;

        // Get the segments' parameters.
        float dx12 = d1.x;
        float dy12 = d1.y;
        float dx34 = d2.x;
        float dy34 = d2.y;

        // Solve for t1 and t2
        float denominator = (dy12 * dx34 - dx12 * dy34);

        t1 =  ((p1.x - p3.x) * dy34 + (p3.y - p1.y) * dx34)
                / denominator;


        t2 =  ((p3.x - p1.x) * dy12 + (p1.y - p3.y) * dx12)
                / -denominator;

        // Find the point of intersection.
        intersection = new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);

    }

    void fixRi( Vector2 dir, ref int ri ) {
        float dt = Vector2.Dot(Motor.Scanner[ri].Dir, dir);
        //todo - if dt == neg then flip right round... ?

        int tRi = (ri + 1) % Motor.Scanner.Count;

        float tDt = Vector2.Dot(Motor.Scanner[tRi].Dir, dir);
        if( tDt > dt ) {

            for(int maxIter = Motor.Scanner.Count; ; ) {
                dt = tDt;
                ri = tRi;

                tRi = (ri + 1) % Motor.Scanner.Count;
                tDt = Vector2.Dot(Motor.Scanner[tRi].Dir, dir);
                if(tDt <= dt) return;

                if(maxIter-- < 0) {
                    Debug.LogError("stuck");
                    return;
                }
            }

        } else {
            for(int maxIter = Motor.Scanner.Count; ;) {
                tRi = (ri + Motor.Scanner.Count - 1) % Motor.Scanner.Count;
                tDt = Vector2.Dot(Motor.Scanner[tRi].Dir, dir);

                if(tDt <= dt) return;

                dt = tDt;
                ri = tRi;

                if(maxIter-- < 0) {
                    Debug.LogError("stuck");
                    return;
                }
            }
        }

    }

    bool cast( Arena.Obstacle obs, TankMotor.Ray r ) {
        Vector2 tan = new Vector2(r.Dir.y, -r.Dir.x);


        Vector2 intersection; float t1, t2;
        FindIntersection(Motor.Body.position, r.Dir, obs.Pos, tan,
            out intersection,
            out t1, out t2);
        if(t1 < 0 || Mathf.Abs(t2) > obs.Rad) return false;
        if( t1 - obs.Rad > r.RangeMod * Motor.BaseRange *r.Out_Dis ) return true;

        float pen = Mathf.Sqrt(obs.Rad * obs.Rad - t2 * t2);
        if(t1 - pen > r.RangeMod * Motor.BaseRange * r.Out_Dis ) return true;

        r.Out_Dis = (t1 - pen) / (r.RangeMod * Motor.BaseRange);

        return true;
    }

    public void proc() {

        foreach(var r in Motor.Scanner) {
            r.Out_Dis = 1;
        }
        for(int i = 0; i < Obs.Count; i++ ) {
            var o = Obs[i];
            var vec = o.Obs.Pos - Motor.Body.position;
            float mag = vec.magnitude;
            var dir = vec / mag;
            o.Dis = mag - o.Obs.Rad;
            if( i > 0 && Obs[i-1].Dis > o.Dis ) {  //soft sorting
                Obs[i] = Obs[i - 1];
                Obs[i - 1] = o;
            }
         
            fixRi(dir, ref o.Ri);

            if( mag < o.Obs.Rad ) {

                Motor.Scanner[o.Ri].Out_Dis = 0;
                for(int ri = o.Ri, maxIter = Motor.Scanner.Count; ;) {
                    ri = (ri + 1) % Motor.Scanner.Count;
                    float dt = Vector2.Dot(Motor.Scanner[ri].Dir, dir);
                    if(dt > 0.3f)
                        Motor.Scanner[ri].Out_Dis = 0;
                    else
                        break;

                    if(maxIter-- < 0) {
                        Debug.LogError("stuck");
                        return;
                    }
                }
                for(int ri = o.Ri, maxIter = Motor.Scanner.Count; ;) {
                    ri = (ri + Motor.Scanner.Count - 1) % Motor.Scanner.Count;
                    float dt = Vector2.Dot(Motor.Scanner[ri].Dir, dir);
                    if(dt > 0.3f)
                        Motor.Scanner[ri].Out_Dis = 0;
                    else
                        break;
                    if(maxIter-- < 0) {
                        Debug.LogError("stuck");
                        return;
                    }
                }

                continue;
            }

            // Gizmos.DrawLine(Motor.Body.position, Motor.Body.position + dir);
            //Gizmos.color = Color.black;
            //Gizmos.DrawLine(Motor.Body.position, Motor.Body.position + Motor.Scanner[o.Ri].Dir *4.0f );

            cast(o.Obs, Motor.Scanner[o.Ri]);
            for(int ri = o.Ri, maxIter = Motor.Scanner.Count; ;) {
                ri = (ri + 1) % Motor.Scanner.Count;
                if(!cast(o.Obs, Motor.Scanner[ri])) break;
                if(maxIter-- < 0) {
                    Debug.LogError("stuck");
                    return;
                }
            }
            for(int ri = o.Ri, maxIter = Motor.Scanner.Count; ;) {
                ri = (ri + Motor.Scanner.Count - 1) % Motor.Scanner.Count;
                if(!cast(o.Obs, Motor.Scanner[ri])) break;

                if(maxIter-- < 0) {
                    Debug.LogError("stuck");
                    return;
                }
            }



            /* foreach( var r in Motor.Scanner) {
                 Vector2 tan = new Vector2(r.Dir.y, -r.Dir.x);


                 Vector2 intersection; float t1, t2;
                 FindIntersection(Motor.Body.position, r.Dir, obs.Pos, tan,
                     out  intersection,
                     out  t1, out  t2);
                 if( t1 < 0 || Mathf.Abs(t2) > obs.Rad || t1-obs.Rad > r.RangeMod * Motor.BaseRange) continue;

                 float pen = Mathf.Sqrt(obs.Rad * obs.Rad - t2 * t2);
                 if(t1 - pen > r.RangeMod * Motor.BaseRange) continue;
                 //|| t1 > r.RangeMod * Motor.BaseRange
 //                Debug.Log("t1 " + t1 + "  t2 " + t2 + "   obs.Rad " + obs.Rad + "  r.RangeMod * Motor.BaseRange " + (r.RangeMod * Motor.BaseRange));
                 Gizmos.color = Color.blue;
                 Gizmos.DrawLine(obs.Pos, intersection);

                 intersection = Motor.Body.position + r.Dir * (t1 - pen);
                 Gizmos.color = Color.red;
                 Gizmos.DrawLine(Motor.Body.position, intersection);

                 Gizmos.color = Color.black;
                 Gizmos.DrawLine(Motor.Body.position, Motor.Body.position + r.Dir*4);
             } */
        }
    }
    void OnDrawGizmos () {

        if(Application.isPlaying  || Area == null || Area.Obs == null ) return;
        if( Obs == null ) {
            Motor = GetComponent<TankMotor>();
            Motor.Trnsfrm = transform;
            Motor.Body = GetComponent<Rigidbody2D>();
            init();
        }


        foreach(var o in Obs) {
            if(o.Ri >= Motor.Scanner.Count) {
                Obs = null;
                return;
            }
        }

        proc();
        
    }
}
