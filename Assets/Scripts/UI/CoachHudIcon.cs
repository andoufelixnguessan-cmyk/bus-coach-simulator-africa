using UnityEngine;
using UnityEngine.UI;

/// <summary>Resolution-independent vector icons for the mobile coach HUD; no font glyph dependency.</summary>
public sealed class CoachHudIcon : Graphic
{
    public enum IconType
    {
        ArrowLeft, ArrowRight, Warning, Headlights, HighBeam, Camera, Power,
        SeatbeltOn, SeatbeltOff, DoorClosed, DoorOpen, PedalUp, PedalDown, Parking, Wipers, CabinLight, AirConditioning, RouteMap, FuelPump
    }

    [SerializeField] private IconType iconType;
    [SerializeField, Range(1f, 12f)] private float stroke = 4f;

    public void SetIcon(IconType value)
    {
        if (iconType == value) return;
        iconType = value;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        float unit = Mathf.Max(1f, Mathf.Min(rectTransform.rect.width, rectTransform.rect.height));
        float thickness = unit * stroke / 100f;
        switch (iconType)
        {
            case IconType.ArrowLeft:
                Line(vh, P(.65f, 0), P(-.55f, 0), thickness);
                Line(vh, P(-.55f, 0), P(-.12f, .42f), thickness);
                Line(vh, P(-.55f, 0), P(-.12f, -.42f), thickness);
                break;
            case IconType.ArrowRight:
                Line(vh, P(-.65f, 0), P(.55f, 0), thickness);
                Line(vh, P(.55f, 0), P(.12f, .42f), thickness);
                Line(vh, P(.55f, 0), P(.12f, -.42f), thickness);
                break;
            case IconType.Warning:
                Polyline(vh, new[] { P(0,.62f), P(-.62f,-.48f), P(.62f,-.48f), P(0,.62f) }, thickness);
                Line(vh, P(0,.20f), P(0,-.12f), thickness * 1.2f);
                Disc(vh, P(0,-.30f), thickness * .72f, 12);
                break;
            case IconType.Headlights:
                Polyline(vh, new[] { P(-.52f,.32f), P(-.12f,.32f), P(.05f,.18f), P(.05f,-.18f), P(-.12f,-.32f), P(-.52f,-.32f), P(-.52f,.32f) }, thickness);
                for (int i=-1;i<=1;i++) Line(vh, P(.18f,i*.28f), P(.62f,i*.42f), thickness*.8f);
                break;
            case IconType.HighBeam:
                Polyline(vh, new[] { P(-.55f,.32f), P(-.15f,.32f), P(.02f,.18f), P(.02f,-.18f), P(-.15f,-.32f), P(-.55f,-.32f), P(-.55f,.32f) }, thickness);
                for (int i=-1;i<=1;i++) Line(vh, P(.18f,i*.25f), P(.65f,i*.25f), thickness*.75f);
                break;
            case IconType.Camera:
                Polyline(vh, new[] { P(-.58f,-.36f),P(-.58f,.30f),P(-.22f,.30f),P(-.08f,.48f),P(.28f,.48f),P(.40f,.30f),P(.58f,.30f),P(.58f,-.36f),P(-.58f,-.36f) }, thickness);
                Circle(vh, P(0,-.02f), unit*.19f, thickness, 24);
                break;
            case IconType.Power:
                CircleArc(vh, P(0,0), unit*.34f, thickness, 35f, 325f, 24);
                Line(vh, P(0,.12f), P(0,.68f), thickness*1.2f);
                break;
            case IconType.SeatbeltOn:
            case IconType.SeatbeltOff:
                Line(vh, P(-.42f,.55f), P(-.42f,-.48f), thickness);
                Line(vh, P(-.42f,.10f), P(.42f,-.50f), thickness*1.35f);
                Line(vh, P(-.42f,-.48f), P(.42f,-.48f), thickness);
                Polyline(vh, new[] { P(.22f,-.34f),P(.48f,-.34f),P(.48f,-.08f),P(.22f,-.08f),P(.22f,-.34f) }, thickness);
                if (iconType == IconType.SeatbeltOff) Line(vh, P(-.65f,.62f), P(.65f,-.62f), thickness);
                break;
            case IconType.DoorClosed:
            case IconType.DoorOpen:
                Polyline(vh, new[] { P(-.48f,-.58f),P(-.48f,.58f),P(.38f,.58f),P(.38f,-.58f),P(-.48f,-.58f) }, thickness);
                Line(vh, P(-.28f,-.48f), P(-.28f,.48f), thickness*.65f);
                if (iconType == IconType.DoorOpen)
                {
                    Line(vh, P(-.28f,.48f), P(.62f,.22f), thickness);
                    Line(vh, P(-.28f,-.48f), P(.62f,-.22f), thickness);
                }
                else Line(vh, P(.16f,0), P(.24f,0), thickness);
                break;
            case IconType.PedalUp:
                Polygon(vh, new[] { P(0,.62f),P(-.55f,-.38f),P(.55f,-.38f) });
                break;
            case IconType.PedalDown:
                Polygon(vh, new[] { P(-.55f,.38f),P(.55f,.38f),P(0,-.62f) });
                break;
            case IconType.Parking:
                Line(vh, P(-.38f,-.58f), P(-.38f,.58f), thickness*1.25f);
                Polyline(vh, new[] { P(-.38f,.58f),P(.20f,.58f),P(.48f,.40f),P(.48f,.12f),P(.20f,-.02f),P(-.38f,-.02f) }, thickness);
                break;
            case IconType.Wipers:
                Polyline(vh, new[] { P(-.62f,-.42f),P(-.48f,.48f),P(.48f,.48f),P(.62f,-.42f) }, thickness);
                Line(vh, P(-.42f,-.28f), P(.28f,.22f), thickness*1.15f);
                Line(vh, P(-.48f,-.38f), P(.48f,-.38f), thickness*.75f);
                break;
            case IconType.CabinLight:
                Circle(vh, P(0,.18f), unit*.22f, thickness, 20);
                Line(vh, P(-.13f,-.12f), P(.13f,-.12f), thickness);
                Line(vh, P(-.11f,-.26f), P(.11f,-.26f), thickness);
                Line(vh, P(0,.66f), P(0,.49f), thickness*.8f);
                Line(vh, P(-.55f,.42f), P(-.40f,.32f), thickness*.8f);
                Line(vh, P(.55f,.42f), P(.40f,.32f), thickness*.8f);
                Line(vh, P(-.62f,-.02f), P(-.44f,-.02f), thickness*.8f);
                Line(vh, P(.62f,-.02f), P(.44f,-.02f), thickness*.8f);
                break;
            case IconType.AirConditioning:
                Line(vh, P(0,.66f), P(0,-.66f), thickness);
                Line(vh, P(-.58f,.33f), P(.58f,-.33f), thickness);
                Line(vh, P(-.58f,-.33f), P(.58f,.33f), thickness);
                Line(vh, P(0,.66f), P(-.16f,.48f), thickness*.7f);
                Line(vh, P(0,.66f), P(.16f,.48f), thickness*.7f);
                Line(vh, P(0,-.66f), P(-.16f,-.48f), thickness*.7f);
                Line(vh, P(0,-.66f), P(.16f,-.48f), thickness*.7f);
                break;
            case IconType.RouteMap:
                Polyline(vh, new[] { P(-.58f,-.48f), P(-.20f,-.62f), P(.18f,-.42f), P(.58f,-.56f) }, thickness);
                Polyline(vh, new[] { P(-.58f,.48f), P(-.20f,.62f), P(.18f,.42f), P(.58f,.56f) }, thickness);
                Line(vh, P(-.20f,-.62f), P(-.20f,.62f), thickness*.8f);
                Line(vh, P(.18f,-.42f), P(.18f,.42f), thickness*.8f);
                Circle(vh, P(-.38f,.08f), unit*.11f, thickness, 14);
                Circle(vh, P(.38f,-.06f), unit*.11f, thickness, 14);
                break;
            case IconType.FuelPump:
                Polyline(vh, new[] { P(-.55f,-.48f), P(-.55f,.48f), P(-.10f,.48f), P(-.10f,-.48f), P(-.55f,-.48f) }, thickness);
                Polyline(vh, new[] { P(-.46f,.30f), P(-.20f,.30f), P(-.20f,.12f), P(-.46f,.12f), P(-.46f,.30f) }, thickness*.7f);
                Line(vh, P(-.12f,.26f), P(.24f,.26f), thickness);
                Line(vh, P(.24f,.26f), P(.36f,.08f), thickness);
                Line(vh, P(.36f,.08f), P(.36f,-.34f), thickness);
                Line(vh, P(.36f,-.34f), P(.52f,-.34f), thickness);
                Line(vh, P(-.55f,-.48f), P(-.12f,-.48f), thickness);
                break;
        }
    }

    private Vector2 P(float x, float y)
    {
        float s = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * .38f;
        return new Vector2(rectTransform.rect.center.x + x*s, rectTransform.rect.center.y + y*s);
    }

    private void Vertex(VertexHelper vh, Vector2 p)
    {
        UIVertex v = UIVertex.simpleVert;
        v.color = color;
        v.position = p;
        vh.AddVert(v);
    }

    private void Line(VertexHelper vh, Vector2 a, Vector2 b, float width)
    {
        Vector2 d = (b-a).normalized;
        if (d.sqrMagnitude < .001f) return;
        Vector2 n = new Vector2(-d.y,d.x) * width * .5f;
        int start=vh.currentVertCount;
        Vertex(vh,a+n); Vertex(vh,b+n); Vertex(vh,b-n); Vertex(vh,a-n);
        vh.AddTriangle(start,start+1,start+2);
        vh.AddTriangle(start,start+2,start+3);
    }

    private void Polyline(VertexHelper vh, Vector2[] points, float width)
    {
        for(int i=1;i<points.Length;i++) Line(vh,points[i-1],points[i],width);
    }

    private void Polygon(VertexHelper vh, Vector2[] points)
    {
        int start=vh.currentVertCount;
        for(int i=0;i<points.Length;i++) Vertex(vh,points[i]);
        for(int i=1;i<points.Length-1;i++) vh.AddTriangle(start,start+i,start+i+1);
    }

    private void Disc(VertexHelper vh, Vector2 center, float radius, int segments)
    {
        int start=vh.currentVertCount;
        Vertex(vh,center);
        for(int i=0;i<=segments;i++)
        {
            float a=i*Mathf.PI*2f/segments;
            Vertex(vh,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius);
            if(i>0) vh.AddTriangle(start,start+i,start+i+1);
        }
    }

    private void Circle(VertexHelper vh, Vector2 center, float radius, float width, int segments)
    {
        Vector2 previous=center+new Vector2(radius,0);
        for(int i=1;i<=segments;i++)
        {
            float a=i*Mathf.PI*2f/segments;
            Vector2 next=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;
            Line(vh,previous,next,width);
            previous=next;
        }
    }

    private void CircleArc(VertexHelper vh, Vector2 center, float radius, float width, float startDeg, float endDeg, int segments)
    {
        Vector2 previous=center+new Vector2(Mathf.Cos(startDeg*Mathf.Deg2Rad),Mathf.Sin(startDeg*Mathf.Deg2Rad))*radius;
        for(int i=1;i<=segments;i++)
        {
            float a=Mathf.Lerp(startDeg,endDeg,i/(float)segments)*Mathf.Deg2Rad;
            Vector2 next=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;
            Line(vh,previous,next,width);
            previous=next;
        }
    }
}