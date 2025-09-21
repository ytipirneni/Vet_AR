using System.Collections.Generic;
using UnityEngine;

public static class PolygonUtils
{
    // Shoelace formula, returns m² if points are in meters
    public static float PolygonArea(List<Vector2> poly)
    {
        if (poly == null || poly.Count < 3) return 0f;
        double area = 0;
        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 a = poly[i];
            Vector2 b = poly[(i + 1) % poly.Count];
            area += (double)a.x * b.y - (double)b.x * a.y;
        }
        return Mathf.Abs((float)(area * 0.5f));
    }

    // Close polygon if first/last not same
    public static void EnsureClosed(List<Vector2> poly, float closeEps)
    {
        if (poly == null || poly.Count < 3) return;
        if (Vector2.Distance(poly[0], poly[poly.Count - 1]) > closeEps)
            poly.Add(poly[0]);
    }

    /// <summary>
    /// Rasterize polygon to a mask (true=inside).
    /// More accurate: samples at pixel centers and supports supersampling (ss=1/2/4).
    /// Units: 'poly' in meters, origin in meters, mmPerPixel in millimeters/pixel.
    /// </summary>
    public static bool[,] RasterizePolygon(
        List<Vector2> poly,
        Vector2 origin,
        float mmPerPixel,
        int widthPx,
        int heightPx,
        int ss = 1)
    {
        if (poly == null || poly.Count < 3 || widthPx <= 0 || heightPx <= 0)
            return new bool[Mathf.Max(1, widthPx), Mathf.Max(1, heightPx)];

        ss = Mathf.Max(1, ss);
        float mmPerPixelEff = mmPerPixel / ss;

        // m -> mm, then / (mm/px_eff)
        float scale = 1000f / mmPerPixelEff;
        int w = widthPx * ss;
        int h = heightPx * ss;

        bool[,] maskHi = new bool[w, h];

        // Project to pixel space once
        var pix = new List<Vector2>(poly.Count);
        foreach (var p in poly)
            pix.Add(new Vector2((p.x - origin.x) * scale, (p.y - origin.y) * scale));

        var intersections = new List<float>(64);

        // Scanline fill with center sampling and half-open rule
        for (int y = 0; y < h; y++)
        {
            float scanY = y + 0.5f; // center of pixel row
            intersections.Clear();

            for (int i = 0; i < pix.Count - 1; i++)
            {
                Vector2 a = pix[i];
                Vector2 b = pix[i + 1];

                bool crosses = (a.y <= scanY && b.y > scanY) || (b.y <= scanY && a.y > scanY);
                if (!crosses) continue;

                float t = (scanY - a.y) / (b.y - a.y);
                intersections.Add(a.x + t * (b.x - a.x));
            }

            intersections.Sort();

            for (int k = 0; k + 1 < intersections.Count; k += 2)
            {
                // Round to nearest pixel center
                int x0 = Mathf.Clamp(Mathf.FloorToInt(intersections[k] + 0.5f), 0, w - 1);
                int x1 = Mathf.Clamp(Mathf.FloorToInt(intersections[k + 1] + 0.5f), 0, w - 1);
                for (int x = x0; x <= x1; x++)
                    maskHi[x, y] = true;
            }
        }

        if (ss == 1) return maskHi;

        // Downsample (logical OR over ss×ss block)
        bool[,] mask = new bool[widthPx, heightPx];
        for (int Y = 0; Y < heightPx; Y++)
        {
            for (int X = 0; X < widthPx; X++)
            {
                bool any = false;
                int baseX = X * ss;
                int baseY = Y * ss;
                for (int dy = 0; dy < ss && !any; dy++)
                    for (int dx = 0; dx < ss && !any; dx++)
                        any = maskHi[baseX + dx, baseY + dy];
                mask[X, Y] = any;
            }
        }
        return mask;
    }
}

/// <summary>
/// Helpers to make a noisy freehand stroke become a clean, closed polygon.
/// Distances are in meters.
/// </summary>
public static class PolyFix
{
    // Remove near-duplicates (keeps order)
    public static void DedupInPlace(List<Vector2> pts, float eps = 1e-5f)
    {
        if (pts == null || pts.Count < 2) return;
        int w = 1;
        for (int i = 1; i < pts.Count; i++)
            if ((pts[i] - pts[w - 1]).sqrMagnitude > eps * eps)
                pts[w++] = pts[i];
        if (w < pts.Count) pts.RemoveRange(w, pts.Count - w);
    }

    // If user ends near start, snap there (closes tiny gap)
    public static void SnapCloseToStart(List<Vector2> pts, float closeEps)
    {
        if (pts == null || pts.Count < 3) return;
        if (Vector2.Distance(pts[pts.Count - 1], pts[0]) <= closeEps)
            pts[pts.Count - 1] = pts[0];
    }

    // If the last point returns near an earlier vertex, close at that vertex
    public static void AutoCloseOnNearRevisit(List<Vector2> pts, float visitEps)
    {
        if (pts == null || pts.Count < 4) return;
        Vector2 last = pts[^1];
        for (int i = 0; i < pts.Count - 3; i++) // keep at least 2 edges
        {
            if (Vector2.Distance(last, pts[i]) <= visitEps)
            {
                pts[^1] = pts[i]; // snap
                pts.RemoveRange(i + 1, pts.Count - (i + 1)); // truncate tail
                break;
            }
        }
    }

    // Optional: uniform resample for even spacing (helps raster/area stability)
    public static List<Vector2> ResampleUniform(List<Vector2> src, float spacing)
    {
        if (src == null || src.Count < 2) return new List<Vector2>(src ?? new List<Vector2>());
        var outPts = new List<Vector2>();
        float leftover = 0f;
        outPts.Add(src[0]);
        for (int i = 1; i < src.Count; i++)
        {
            Vector2 a = src[i - 1];
            Vector2 b = src[i];
            float seg = Vector2.Distance(a, b);
            if (seg <= Mathf.Epsilon) continue;

            float t = leftover / seg;
            while (t < 1f)
            {
                Vector2 p = Vector2.Lerp(a, b, t);
                if ((p - outPts[^1]).sqrMagnitude >= spacing * spacing)
                    outPts.Add(p);
                t += spacing / seg;
            }
            // carry leftover distance for next segment
            float consumed = (Mathf.Floor((seg - leftover) / spacing) * spacing) + leftover;
            leftover = (seg - consumed);
            if (leftover < 1e-6f) leftover = 0f;
        }
        // ensure last point included
        if ((src[^1] - outPts[^1]).sqrMagnitude > (spacing * spacing * 0.25f))
            outPts.Add(src[^1]);

        return outPts;
    }
}
