using UnityEngine;
using System.Collections.Generic;

public static class Bresenhams3D {

    public static Vector3Int[] GetLine(Vector3Int coords, Vector3Int endCoords) {
        List<Vector3Int> points = new List<Vector3Int>();
        points.Add(coords);

        int dx = Mathf.Abs(endCoords.x - coords.x);
        int dy = Mathf.Abs(endCoords.y - coords.y);
        int dz = Mathf.Abs(endCoords.z - coords.z);

        int xs = endCoords.x > coords.x ? 1 : -1;
        int ys = endCoords.y > coords.y ? 1 : -1;
        int zs = endCoords.z > coords.z ? 1 : -1;

        if(dx >= dy && dx >= dz) {
            int p1 = 2 * dy - dx;
            int p2 = 2 * dz - dx;

            while(coords.x != endCoords.x) {
                coords.x += xs;

                if(p1 >= 0) {
                    coords.y += ys;
                    p1 -= 2 * dx;
                }

                if(p2 >= 0) {
                    coords.z += zs;
                    p2 -= 2 * dx;
                }

                p1 += 2 * dy;
                p2 += 2 * dz;

                points.Add(coords);
            }

            return points.ToArray();
        }
        else if(dy >= dx && dy >= dz) {
            int p1 = 2 * dx - dy;
            int p2 = 2 * dz - dy;

            while(coords.y != endCoords.y) {
                coords.y += ys;

                if(p1 >= 0) {
                    coords.x += xs;
                    p1 -= 2 * dy;
                }

                if(p2 >= 0) {
                    coords.z += zs;
                    p2 -= 2 * dy;
                }

                p1 += 2 * dx;
                p2 += 2 * dz;

                points.Add(coords);
            }

            return points.ToArray();
        }
        else {
            int p1 = 2 * dy - dz;
            int p2 = 2 * dx - dz;

            while(coords.z != endCoords.z) {
                coords.z += zs;

                if(p1 >= 0) {
                    coords.y += ys;
                    p1 -= 2 * dz;
                }

                if(p2 >= 0) {
                    coords.x += xs;
                    p2 -= 2 * dz;
                }

                p1 += 2 * dy;
                p2 += 2 * dx;

                points.Add(coords);
            }

            return points.ToArray();
        }
    }
}
