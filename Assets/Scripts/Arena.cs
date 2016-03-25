using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Arena : MonoBehaviour {

    //http://csharphelper.com/blog/2014/08/find-a-minimal-bounding-circle-of-a-set-of-points-in-c/
    //was broken!! -- fixed..

    static public void FindIntersection(
        Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4,
        out bool lines_intersect, out bool segments_intersect,
        out Vector2 intersection,
        out Vector2 close_p1, out Vector2 close_p2) {
        // Get the segments' parameters.
        float dx12 = p2.x - p1.x;
        float dy12 = p2.y - p1.y;
        float dx34 = p4.x - p3.x;
        float dy34 = p4.y - p3.y;

        // Solve for t1 and t2
        float denominator = (dy12 * dx34 - dx12 * dy34);

        float t1 =
            ((p1.x - p3.x) * dy34 + (p3.y - p1.y) * dx34)
                / denominator;
        if(float.IsInfinity(t1)) {
            // The lines are parallel (or close enough to it).
            lines_intersect = false;
            segments_intersect = false;
            intersection = new Vector2(float.NaN, float.NaN);
            close_p1 = new Vector2(float.NaN, float.NaN);
            close_p2 = new Vector2(float.NaN, float.NaN);
            return;
        }
        lines_intersect = true;

        float t2 =
            ((p3.x - p1.x) * dy12 + (p1.y - p3.y) * dx12)
                / -denominator;

        // Find the point of intersection.
        intersection = new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);

        // The segments intersect if t1 and t2 are between 0 and 1.
        segments_intersect =
            ((t1 >= 0) && (t1 <= 1) &&
             (t2 >= 0) && (t2 <= 1));

        // Find the closest points on the segments.
        if(t1 < 0) {
            t1 = 0;
        } else if(t1 > 1) {
            t1 = 1;
        }

        if(t2 < 0) {
            t2 = 0;
        } else if(t2 > 1) {
            t2 = 1;
        }

        close_p1 = new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);
        close_p2 = new Vector2(p3.x + dx34 * t2, p3.y + dy34 * t2);
    }

    static void FindCircle(Vector2 a, Vector2 b, Vector2 c, out Vector2 center, out float radius2) {
        // Get the perpendicular bisector of (x1, y1) and (x2, y2).
        float x1 = (b.x + a.x) / 2;
        float y1 = (b.y + a.y) / 2;
        float dy1 = b.x - a.x;
        float dx1 = -(b.y - a.y);

        // Get the perpendicular bisector of (x2, y2) and (x3, y3).
        float x2 = (c.x + b.x) / 2;
        float y2 = (c.y + b.y) / 2;
        float dy2 = c.x - b.x;
        float dx2 = -(c.y - b.y);

        // See where the lines intersect.
        bool lines_intersect, segments_intersect;
        Vector2 intersection, close1, close2;
        FindIntersection(
            new Vector2(x1, y1), new Vector2(x1 + dx1, y1 + dy1),
            new Vector2(x2, y2), new Vector2(x2 + dx2, y2 + dy2),
            out lines_intersect, out segments_intersect,
            out intersection, out close1, out close2);
        if(!lines_intersect) {
            Debug.LogError("The points are colinear");
            center = new Vector2(0, 0);
            radius2 = 0;
        } else {
            center = intersection;
            float dx = center.x - a.x;
            float dy = center.y - a.y;
            radius2 = dx * dx + dy * dy;
        }
    }

    // Return true if the indicated circle encloses all of the points.
    private static bool CircleEnclosesPoints(Vector2 center,
        float radius2, List<Vector2> points,
        int skip1, int skip2, int skip3) {
        for(int i = 0; i < points.Count; i++) {
            if((i != skip1) && (i != skip2) && (i != skip3)) {
                Vector2 point = points[i];
                float dx = center.x - point.x;
                float dy = center.y - point.y;
                float test_radius2 = dx * dx + dy * dy;
                if(test_radius2 > radius2) return false;
            }
        }
        return true;
    }

    public static void FindMinimalBoundingCircle(List<Vector2> points, out Vector2 center, out float radius) {
        // Find the convex hull.
        // List<Vector2> hull = MakeConvexHull(points);
        //already hull
        var hull = points;

        // The best solution so far.
        Vector2 best_center = hull[0];
        float best_radius2 = float.MaxValue;

        // Look at pairs of hull points.
        for(int i = 0; i < hull.Count - 1; i++) {
            for(int j = i + 1; j < hull.Count; j++) {
                // Find the circle through these two points.
                Vector2 test_center = new Vector2(
                    (hull[i].x + hull[j].x) / 2f,
                    (hull[i].y + hull[j].y) / 2f);
                float dx = test_center.x - hull[i].x;
                float dy = test_center.y - hull[i].y;
                float test_radius2 = dx * dx + dy * dy;

                // See if this circle would be an improvement.
                if(test_radius2 < best_radius2) {
                    // See if this circle encloses all of the points.
                    if(CircleEnclosesPoints(test_center,
                        test_radius2, hull, i, j, -1)) {
                        // Save this solution.
                        best_center = test_center;
                        best_radius2 = test_radius2;
                    }
                }
            } // for i
        } // for j

        // Look at triples of hull points.
        for(int i = 0; i < hull.Count - 2; i++) {
            for(int j = i + 1; j < hull.Count - 1; j++) {
                for(int k = j + 1; k < hull.Count; k++) {
                    // Find the circle through these three points.
                    Vector2 test_center;
                    float test_radius2;
                    FindCircle(hull[i], hull[j], hull[k],
                        out test_center, out test_radius2);

                    // See if this circle would be an improvement.
                    if(test_radius2 < best_radius2) {
                        // See if this circle encloses all the points.
                        if(CircleEnclosesPoints(test_center,
                            test_radius2, hull, i, j, k)) {
                            // Save this solution.
                            best_center = test_center;
                            best_radius2 = test_radius2;
                        }
                    }
                } // for k
            } // for i
        } // for j

        center = best_center;
        if(best_radius2 == float.MaxValue)
            radius = 0;
        else
            radius = (float)Mathf.Sqrt(best_radius2);
    }

    public class Obstacle {

        public Vector2 Pos;
        public float Rad;
        //  public float Dis2;

        public struct Segment {
            public Vector2 Pnt, Tan;
        }
        public Segment[] Segments;
        public PolygonCollider2D Col;

        static bool FindIntersection(
             Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out float t2) {
            // Get the segments' parameters.
            float dx12 = p2.x - p1.x;
            float dy12 = p2.y - p1.y;
            float dx34 = p4.x - p3.x;
            float dy34 = p4.y - p3.y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 = ((p1.x - p3.x) * dy34 + (p3.y - p1.y) * dx34)
                    / denominator;
            if(float.IsInfinity(t1)) {
                t2 = 0;
                return false;
            }

            t2 = ((p3.x - p1.x) * dy12 + (p1.y - p3.y) * dx12)
                    / -denominator;
            // The segments intersect if t1 and t2 are between 0 and 1.
            return
                (t1 >= 0) && (t1 <= 1) &&
                 (t2 <= 1);


        }
        public void cast( TankMotor mtr, TankMotor.Ray r  ) {    
            //if( c! ) r.Out_Dis = (t1 - pen) / (r.RangeMod * Motor.BaseRange);
            for(int i = 0, j = Segments.Length - 1; i < Segments.Length; j = i, i++) { //todo we could cache index of hit and start at that point next time

                //   var m = (c.points[i] + c.points[j]) * 0.5f;
                //   Gizmos.color = Color.black;
                //  Gizmos.DrawLine(m, m + obs.Tangents[i] * 4);
                if(Vector2.Dot(Segments[i].Tan, r.Dir) > 0.0f) continue;

                float d;
                if(FindIntersection(Segments[i].Pnt, Segments[j].Pnt, mtr.Body.position, mtr.Body.position + r.Dir * (r.RangeMod * mtr.BaseRange * r.Out_Dis),
                    out d)) {
                    r.Out_Dis *= d;
                    break;
                }
            }
        }
    };

    [System.NonSerialized]
    public List<Obstacle> Obs;

    void OnEnable() {

        gen();

    }
    void gen() {     
        Obs = new List<Obstacle>();
        var cols = GetComponentsInChildren<PolygonCollider2D>();

        var wrknList = new List<Vector2>();
        foreach(var c in cols) {
            Vector2 centre; float rad;
            var t = c.transform;
            var tans = new Obstacle.Segment[c.points.Length];
            for( int i = 0, j = c.points.Length-1; i < c.points.Length; j = i, i++ ) {
                var p = c.points[i];
                wrknList.Add(t.TransformPoint(p));
                var n = (p - c.points[j]).normalized;
                tans[i].Pnt = p;
                tans[i].Tan = new Vector2(n.y, -n.x);
            }
            FindMinimalBoundingCircle(wrknList, out centre, out rad);
            wrknList.Clear();

            var o = new Obstacle() {
                Pos = centre,
                Rad = rad,
                //Dis2 = (Motor.Body.position - centre).magnitude,
                Col = c,
                Segments = tans,
            };
            Obs.Add(o);
           // Debug.Log("add " + centre + "  r " + rad);
        }
    }

    void OnDrawGizmos() {

        if(Obs == null) {
            gen();
            if(Obs == null) return;
        }


        foreach(var o in Obs) {
            Gizmos.DrawWireSphere(o.Pos, o.Rad);
        }

    }
}
