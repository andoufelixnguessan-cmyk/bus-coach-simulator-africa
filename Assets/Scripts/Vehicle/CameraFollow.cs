using UnityEngine;
public sealed class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset=new Vector3(0f,4.2f,-10.5f);
    [SerializeField,Range(1f,20f)] private float followSpeed=7f;
    [SerializeField,Range(1f,20f)] private float lookSpeed=8f;
    [SerializeField] private bool cockpitView=true;
    private Transform target;
    public void SetTarget(Transform value)=>target=value;
    public bool IsCockpitView=>cockpitView;
    public void SetCockpitView(bool enabled){cockpitView=enabled;}
    private void LateUpdate()
    {
        if(target==null)return;
        if(cockpitView)
        {
            Vector3 p=target.TransformPoint(new Vector3(-.52f,1.92f,3.62f));
            Vector3 look=target.TransformPoint(new Vector3(-.52f,1.88f,15f));
            transform.position=p;transform.rotation=Quaternion.LookRotation(look-p,target.up);return;
        }
        Vector3 desired=target.TransformPoint(offset);
        transform.position=Vector3.Lerp(transform.position,desired,followSpeed*Time.deltaTime);
        Vector3 point=target.position+Vector3.up*1.55f+target.forward*3f;
        transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(point-transform.position,Vector3.up),lookSpeed*Time.deltaTime);
    }
}