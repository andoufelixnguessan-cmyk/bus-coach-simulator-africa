using System.Collections.Generic;
using UnityEngine;

/// <summary>Generated low-poly first-person cockpit and live animated instruments.</summary>
public sealed class CoachCockpit3D : MonoBehaviour
{
    private CoachVehicleController vehicle;
    private Transform wheel, speedNeedle, rpmNeedle, fuelNeedle, tempNeedle;
    private GameObject seatbeltVisual;
    private readonly List<Renderer> lamps = new List<Renderer>();
    private readonly List<Color> lampOn = new List<Color>();
    private TextMesh display;
    private TextMesh gearDisplay;
    private Material dark, trim, face, needleMat, screenMat, white;

    public void Configure(Transform root, CoachVehicleController target, CoachDashboardProfile profile)
    {
        vehicle=target;
        dark=Mat(new Color(.025f,.035f,.045f)); trim=Mat(new Color(.10f,.13f,.15f));
        face=Mat(new Color(.008f,.014f,.02f)); needleMat=Mat(new Color(.95f,.12f,.06f));
        screenMat=Mat(new Color(.015f,.10f,.12f)); white=Mat(new Color(.82f,.86f,.88f));
        Part(root,"Padded dashboard",new Vector3(0,1.34f,4.43f),new Vector3(2.35f,.62f,.70f),dark);
        Part(root,"Dashboard lower trim",new Vector3(0,1.03f,4.38f),new Vector3(2.25f,.12f,.62f),trim);
        Part(root,"Instrument bezel",new Vector3(-.18f,1.48f,4.08f),new Vector3(1.42f,.58f,.13f),trim);
        if(profile.HasDigitalCluster)
        {
            Part(root,"Digital cluster screen",new Vector3(-.18f,1.49f,3.995f),new Vector3(1.27f,.43f,.035f),screenMat);
            GameObject t=new GameObject("Digital dashboard readout"); t.transform.SetParent(root,false);
            t.transform.localPosition=new Vector3(-.18f,1.50f,3.966f); t.transform.localRotation=Quaternion.Euler(0,180,0);
            display=t.AddComponent<TextMesh>(); display.anchor=TextAnchor.MiddleCenter; display.alignment=TextAlignment.Center;
            display.fontSize=48; display.characterSize=.012f; display.color=new Color(.3f,.95f,.85f);
        }
        else
        {
            speedNeedle=Gauge(root,"Speedometer",new Vector3(-.55f,1.5f,3.98f),.22f);
            rpmNeedle=Gauge(root,"Tachometer",new Vector3(.08f,1.5f,3.98f),.22f);
            fuelNeedle=Gauge(root,"Fuel",new Vector3(.57f,1.55f,3.98f),.105f);
            tempNeedle=Gauge(root,"Coolant",new Vector3(.57f,1.38f,3.98f),.105f);
        }
        BuildLamps(root);
        Part(root, "Gear indicator screen", new Vector3(.60f, 1.23f, 3.98f), new Vector3(.30f, .16f, .035f), screenMat);
        GameObject gearObject = new GameObject("Transmission gear readout");
        gearObject.transform.SetParent(root, false);
        gearObject.transform.localPosition = new Vector3(.60f, 1.24f, 3.95f);
        gearObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
        gearDisplay = gearObject.AddComponent<TextMesh>();
        gearDisplay.anchor = TextAnchor.MiddleCenter;
        gearDisplay.alignment = TextAlignment.Center;
        gearDisplay.fontSize = 52;
        gearDisplay.characterSize = .012f;
        gearDisplay.color = new Color(.3f, .95f, .85f);
        Part(root,"Passenger glovebox",new Vector3(.78f,1.16f,4.12f),new Vector3(.82f,.3f,.42f),trim);
        Part(root,"Left air conditioning vent",new Vector3(-.82f,1.62f,4.02f),new Vector3(.24f,.08f,.035f),trim);
        Part(root,"Right air conditioning vent",new Vector3(.82f,1.62f,4.02f),new Vector3(.24f,.08f,.035f),trim);
        for(int vent=0;vent<3;vent++)
        {
            float ventY=1.60f+vent*.025f;
            Part(root,"Left vent slat",new Vector3(-.82f,ventY,3.99f),new Vector3(.18f,.008f,.012f),white);
            Part(root,"Right vent slat",new Vector3(.82f,ventY,3.99f),new Vector3(.18f,.008f,.012f),white);
        }
        Part(root,"Center console",new Vector3(.65f,.92f,3.72f),new Vector3(.5f,.16f,.65f),dark);
        Part(root,"Radio screen",new Vector3(.65f,1.12f,3.94f),new Vector3(.28f,.12f,.04f),screenMat);
        BuildWheel(root);
        BuildDriverSeat(root);
        BuildStalk(root);
    }

    private Transform Gauge(Transform root,string name,Vector3 pos,float radius)
    {
        GameObject f=GameObject.CreatePrimitive(PrimitiveType.Cylinder); f.name=name+" dial";
        f.transform.SetParent(root,false); f.transform.localPosition=pos; f.transform.localRotation=Quaternion.Euler(90,0,0);
        f.transform.localScale=new Vector3(radius*2,.035f,radius*2); Object.Destroy(f.GetComponent<Collider>()); f.GetComponent<Renderer>().sharedMaterial=face;
        GameObject pivot=new GameObject(name+" needle pivot");
        pivot.transform.SetParent(root,false); pivot.transform.localPosition=pos+new Vector3(0,0,-.045f);
        GameObject n=Part(pivot.transform,name+" needle",new Vector3(0,radius*.34f,0),new Vector3(.012f,radius*.72f,.012f),needleMat);
        for(int i=0;i<=10;i++)
        {
            float a=Mathf.Lerp(-135f,135f,i/10f)*Mathf.Deg2Rad;
            Vector3 tickPos=pos+new Vector3(Mathf.Sin(a)*radius*.82f,Mathf.Cos(a)*radius*.82f,-.045f);
            GameObject tick=Part(root,name+" scale mark",tickPos,new Vector3(.012f,.035f,.012f),white);
            tick.transform.localRotation=Quaternion.Euler(0,0,-a*Mathf.Rad2Deg);
        }
        GameObject h=GameObject.CreatePrimitive(PrimitiveType.Sphere); h.transform.SetParent(root,false); h.transform.localPosition=pos+new Vector3(0,0,-.05f);
        h.transform.localScale=Vector3.one*.04f; Object.Destroy(h.GetComponent<Collider>()); h.GetComponent<Renderer>().sharedMaterial=trim;
        return pivot.transform;
    }

    private void BuildLamps(Transform root)
    {
        Vector3[] p={new Vector3(-.90f,1.18f,4.02f),new Vector3(-.68f,1.18f,4.02f),new Vector3(-.46f,1.18f,4.02f),new Vector3(-.24f,1.18f,4.02f),new Vector3(0,1.18f,4.02f),new Vector3(.24f,1.18f,4.02f),new Vector3(.46f,1.18f,4.02f),new Vector3(.68f,1.18f,4.02f),new Vector3(.90f,1.18f,4.02f)};
        Color[] c={Color.green,Color.green,Color.blue,Color.red,Color.red,new Color(1,.55f,0),Color.red,new Color(1,.55f,0),Color.red};
        for(int i=0;i<p.Length;i++)
        {
            GameObject g=GameObject.CreatePrimitive(PrimitiveType.Sphere); g.name="Dashboard warning light "+i;
            g.transform.SetParent(root,false); g.transform.localPosition=p[i]; g.transform.localScale=Vector3.one*.055f;
            Object.Destroy(g.GetComponent<Collider>()); Renderer r=g.GetComponent<Renderer>(); r.sharedMaterial=Mat(new Color(.06f,.07f,.08f));
            lamps.Add(r); lampOn.Add(c[i]);
        }
    }

    private void BuildWheel(Transform root)
    {
        GameObject w=new GameObject("Steering wheel"); w.transform.SetParent(root,false);
        w.transform.localPosition=new Vector3(-.57f,1.13f,3.86f); wheel=w.transform;
        const int count=14; float radius=.34f;
        for(int i=0;i<count;i++)
        {
            float a=i*Mathf.PI*2/count, b=(i+1)*Mathf.PI*2/count;
            Vector3 p1=new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0), p2=new Vector3(Mathf.Cos(b)*radius,Mathf.Sin(b)*radius,0);
            GameObject seg=GameObject.CreatePrimitive(PrimitiveType.Cylinder); seg.transform.SetParent(w.transform,false);
            seg.transform.localPosition=(p1+p2)*.5f; seg.transform.localRotation=Quaternion.FromToRotation(Vector3.up,p2-p1);
            seg.transform.localScale=new Vector3(.045f,Vector3.Distance(p1,p2)*.58f,.045f);
            Object.Destroy(seg.GetComponent<Collider>()); seg.GetComponent<Renderer>().sharedMaterial=dark;
        }
        Part(w.transform,"Steering hub",Vector3.zero,new Vector3(.16f,.16f,.09f),trim);
        for(int i=0;i<3;i++)
        {
            float a=i*Mathf.PI*2/3; GameObject spoke=Part(w.transform,"Steering spoke",new Vector3(Mathf.Cos(a)*.085f,Mathf.Sin(a)*.085f,0),new Vector3(.035f,.19f,.035f),trim);
            spoke.transform.localRotation=Quaternion.Euler(0,0,-a*Mathf.Rad2Deg+90);
        }
    }


    private void BuildDriverSeat(Transform root)
    {
        Part(root, "Driver seat cushion", new Vector3(-.58f, .63f, 2.70f),
            new Vector3(.72f, .20f, .78f), dark);
        Part(root, "Driver seat backrest", new Vector3(-.58f, 1.08f, 3.02f),
            new Vector3(.72f, .86f, .20f), dark);
        Part(root, "Driver seat headrest", new Vector3(-.58f, 1.58f, 3.04f),
            new Vector3(.38f, .26f, .18f), trim);

        seatbeltVisual = new GameObject("Driver three-point seatbelt");
        seatbeltVisual.transform.SetParent(root, false);
        GameObject strap = Part(seatbeltVisual.transform, "Seatbelt webbing",
            new Vector3(-.36f, 1.08f, 2.82f), new Vector3(.055f, .92f, .045f), white);
        strap.transform.localRotation = Quaternion.Euler(0f, 0f, -34f);
        Part(seatbeltVisual.transform, "Seatbelt buckle",
            new Vector3(-.12f, .70f, 2.48f), new Vector3(.12f, .10f, .07f), trim);
        Part(seatbeltVisual.transform, "Seatbelt anchor",
            new Vector3(-.84f, .78f, 2.98f), new Vector3(.09f, .12f, .08f), trim);
    }

    private void BuildStalk(Transform root)
    {
        GameObject stalk=GameObject.CreatePrimitive(PrimitiveType.Cylinder); stalk.name="Turn signal stalk";
        stalk.transform.SetParent(root,false); stalk.transform.localPosition=new Vector3(-.88f,1.15f,3.83f);
        stalk.transform.localRotation=Quaternion.Euler(90,0,25); stalk.transform.localScale=new Vector3(.035f,.22f,.035f);
        Object.Destroy(stalk.GetComponent<Collider>()); stalk.GetComponent<Renderer>().sharedMaterial=trim;
    }

    private void Update()
    {
        if(vehicle==null)return;
        if(wheel!=null) wheel.localRotation=Quaternion.Lerp(wheel.localRotation,Quaternion.Euler(0,0,-vehicle.SteeringInput*95f),Time.deltaTime*8f);
        if(seatbeltVisual!=null) seatbeltVisual.SetActive(vehicle.SeatbeltFastened);
        Needle(speedNeedle,Mathf.InverseLerp(0,140,vehicle.SpeedKph));
        Needle(rpmNeedle,vehicle.EngineRpmNormalized);
        Needle(fuelNeedle,vehicle.FuelPercent/100f);
        Needle(tempNeedle,Mathf.InverseLerp(60,120,vehicle.CoolantTemperatureC));
        if(display!=null) display.text=Mathf.RoundToInt(vehicle.SpeedKph)+" km/h\n"+Mathf.RoundToInt(vehicle.EngineRpm)+" rpm   FUEL "+Mathf.RoundToInt(vehicle.FuelPercent)+"%";
        if(gearDisplay!=null) gearDisplay.text=vehicle.CurrentGearLabel;
        bool flash=Mathf.Repeat(Time.time,.8f)<.4f;
        bool[] active={(vehicle.LeftIndicatorOn||vehicle.HazardLightsOn)&&flash,(vehicle.RightIndicatorOn||vehicle.HazardLightsOn)&&flash,vehicle.HighBeamOn&&vehicle.HeadlightsOn,vehicle.ParkingBrakeApplied,!vehicle.SeatbeltFastened,vehicle.AbsWarning,vehicle.EngineWarning,vehicle.DoorWarning,vehicle.AirPressureWarning||vehicle.FuelWarning};
        for(int i=0;i<lamps.Count;i++) lamps[i].sharedMaterial.color=active[i]?lampOn[i]:new Color(.06f,.07f,.08f);
    }

    private static void Needle(Transform n,float v){if(n!=null)n.localRotation=Quaternion.Euler(0,0,-Mathf.Lerp(-125,125,Mathf.Clamp01(v)));}
    private static GameObject Part(Transform parent,string name,Vector3 pos,Vector3 scale,Material mat)
    {
        GameObject g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);
        g.transform.localPosition=pos;g.transform.localScale=scale;Object.Destroy(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=mat;return g;
    }
    private static Material Mat(Color c){Shader s=Shader.Find("Universal Render Pipeline/Lit");if(s==null)s=Shader.Find("Standard");Material m=new Material(s);m.color=c;return m;}
}
