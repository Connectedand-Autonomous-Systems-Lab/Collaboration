using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class HumanBehaviourCheckSceneManager : MonoBehaviour
{
    // public MazeGenHuman mazeGen;
    public List<GameObject> mazeList;
    private GameObject activeMaze = null;
    private Vector3 posMaze = new Vector3(0f,3.45f,0f); 
    private ROSConnection ros;
    private string reset_signal;
    private string pause_signal;
    private string unpause_signal;

    // Start is called before the first frame update
    void Start()
    {
        // mazeGen.GenerateMaze();
        InitiateROSConnection();
        activeMaze = Instantiate(mazeList[0], posMaze, Quaternion.identity);

        GameObject m = GameObject.Find("Maze");
        Destroy(m);
    }

    // Update is called once per frame
    void Update()
    {       
        if(Input.GetKeyDown(KeyCode.Q)){
            ResetMaze(Random.Range(1, 5));
        }
    }

    void InitiateROSConnection(){
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<StringMsg>("reset_world", ReceiveReset);
        ros.Subscribe<StringMsg>("pause_world", ReceivePause);
    }

    void ReceiveReset(StringMsg signal)
    {   
        int maze_number = int.Parse(signal.data);
        ResetMaze(maze_number);
    }

    void ResetMaze(int i)
    {
        // Debug.Log(reset_signal.GetType());
        Debug.Log(i);
        activeMaze.gameObject.SetActive(false);
        Destroy(activeMaze);
        activeMaze = null;
        // GameObject m = GameObject.Find("Maze");
        // Destroy(m);
        activeMaze = Instantiate(mazeList[i], posMaze, Quaternion.identity);
    }

    void ReceivePause(StringMsg signal)
    {
        pause_signal = signal.data;
        if(pause_signal == "pause"){
            Time.timeScale = 0;
        }
        else if (pause_signal == "unpause"){
            Time.timeScale = 1;
        }
    }

}
