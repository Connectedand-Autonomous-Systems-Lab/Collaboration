using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAvatarBehaviourHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FoundVictim();
    }


    void FoundVictim(){
        if(Input.GetKeyDown(KeyCode.C)){
            Vector2 _gazePointer = EyeGazeTracker.gazePosition;
            // Debug.Log(_gazePointer);
            GetHitPositionGaze(_gazePointer);
        }
    }

    public Vector3 GetHitPositionGaze(Vector2 gazePosition)
    {
        Debug.Log(gazePosition);
        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(gazePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null && hit.collider.transform.gameObject.tag == "Human")
            {
                GameObject victim = hit.collider.transform.gameObject;
                Debug.Log(victim.name);
                victim.SetActive(false);
                return hit.collider.transform.position;
            }else{
                Debug.Log("Hard to detect Victims, Move closer: "+ hit.collider.gameObject.name);
                // hit.collider.gameObject.SetActive(false);
            }
        }
        return Vector3.negativeInfinity;
    }
}
