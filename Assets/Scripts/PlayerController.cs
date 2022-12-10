using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Ball ball;
    [SerializeField] GameObject arrow;
    [SerializeField] LayerMask ballLayer;
    [SerializeField] LayerMask rayLayer;
    [SerializeField] Transform cameraPivot;
    [SerializeField] Camera cam;
    [SerializeField] Vector2 camSensitivity;
    [SerializeField] float shootForce;

    Vector3 lastMousePosition;
    float ballDistance;
    bool isShooting;
    float forceFactor;
    Vector3 forceDir;

    Renderer[] arrowRends;

    private void Start()
    {
        ballDistance = Vector3.Distance(
                cam.transform.position, ball.Position) + 1;
        arrowRends = arrow.GetComponentsInChildren<Renderer>();
        arrow.SetActive(false);
    }


    void Update()
    {
        if(ball.IsMoving || ball.IsTeleporting)
            return;
         
        if(this.transform.position != ball.Position)
            this.transform.position = ball.Position;

        if(Input.GetMouseButtonDown(0))
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray,ballDistance,ballLayer))
            {
                isShooting=true;
                arrow.SetActive(true);
            }
        }


        // Shooting mode
        if(Input.GetMouseButton(0) && isShooting == true)
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, ballDistance*2, rayLayer))
             {
                Debug.DrawLine(ball.Position, hit.point);
                
                var forceVector = ball.Position - hit.point;
                forceDir = forceVector.normalized;
                var forceMagnitude = forceVector.magnitude;
                Debug.Log(forceMagnitude);
                forceMagnitude = Mathf.Clamp(forceMagnitude,0,5);
                forceFactor = forceMagnitude/5;
             }   

            // arr0w
            this.transform.LookAt(this.transform.position+forceDir);
            arrow.transform.localScale = new Vector3(1+0.5f*forceFactor,1+0.5f*forceFactor,1+2*forceFactor);

            foreach(var rend in arrowRends)
            {
                rend.material.color = Color.Lerp(Color.white,Color.red,forceFactor);
            }
        }

        // camera mode
        if(Input.GetMouseButton(0) && isShooting == false)
        {
            var current = cam.ScreenToViewportPoint(Input.mousePosition);
            var last = cam.ScreenToViewportPoint(lastMousePosition);
            var delta = current - last  ;

            // rotate horizontal
            cameraPivot.transform.RotateAround(
                ball.Position, 
                Vector3.up,
                delta.x*camSensitivity.x);
            
            // rotate vertical
            cameraPivot.transform.RotateAround(
                ball.Position, 
                cam.transform.right,
                -delta.y*camSensitivity.y);

            var angle = Vector3.SignedAngle(
                Vector3.up, cam.transform.up, cam.transform.right);

            // kalau melewati batas putar balik
            if(angle < 3)
                cameraPivot.transform.RotateAround(
                ball.Position, 
                cam.transform.right,
                3 - angle);
            else if(angle > 65)
                cameraPivot.transform.RotateAround(
                ball.Position, 
                cam.transform.right,
                65 - angle);

        }
        if(Input.GetMouseButtonUp(0))
        {
            ball.AddForce(forceDir*shootForce*forceFactor);
            forceFactor=0;
            forceDir=Vector3.zero;
            isShooting=false;
            arrow.SetActive(false);
        }
         
        lastMousePosition = Input.mousePosition;
    }
}

