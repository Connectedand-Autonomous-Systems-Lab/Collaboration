using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class RandomMazeSceneManager : MonoBehaviour
{
    public static bool isPaused = false;
    public int mazeSize = 11;
    public int numPeople = 2;
    public int numObjects = 10;
    public int lightingLevel = 1;

    public bool autoIdentifyHumans = true;


    private MazeGenerator mazeGenerator;

    private ROSConnection ros;
    private string reset_signal;
    private string pause_signal;
    private string unpause_signal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitiateROSConnection();
        InitiateMazeGenerator();
        UpdateCameraCapture();
    }   

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitiateROSConnection(){
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<StringMsg>("reset_world", ReceiveReset);
        ros.Subscribe<StringMsg>("pause_world", ReceivePause);
    }

    void PausePhysics(){
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
    }

    void LoadVariablesFromPlayerPrefs(){
        mazeSize = PlayerPrefs.GetInt("MazeSize");
        lightingLevel = PlayerPrefs.GetInt("Lighting");
        int val = PlayerPrefs.GetInt("DetectHumans");
        if(val == 0){
            autoIdentifyHumans = false;
        }else{
            autoIdentifyHumans = true;
        }
    }

    void InitiateMazeGenerator(){
        MazeGenerator mazeGenerator = FindObjectOfType<MazeGenerator>();
        if(mazeGenerator != null){
            mazeGenerator.lightingLevel = lightingLevel;
            mazeGenerator.mazeHeight = mazeSize;
            mazeGenerator.mazeWidth = mazeSize;
            mazeGenerator.pointCount = numPeople;

            Debug.Log(mazeGenerator.pointCount);
            

            // Instantiate(MazeGenerator);

            mazeGenerator.GenerateMaze();
        }
        
        

    }

    void UpdateCameraCapture(){
        WebSocketCameraCapture cameraCapture = FindObjectOfType<WebSocketCameraCapture>();
        Debug.Log(cameraCapture);
        if(cameraCapture != null){
            Debug.Log("Camera capture: "+ autoIdentifyHumans);
            cameraCapture.autoIdentifyHumans = autoIdentifyHumans;
        }
    }

    void ReceiveReset(StringMsg signal)
    {
        reset_signal = signal.data;
        if(reset_signal == "reset"){
            ResetScene();
        }
    }


    void ResetScene(){
        SceneManager.LoadSceneAsync("RandomMaze");
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