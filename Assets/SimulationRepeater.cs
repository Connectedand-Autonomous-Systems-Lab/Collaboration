using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulationRepeater : MonoBehaviour
{
    public string sceneName;
    public int iterationCount = 5;

    private int currentIteration = 0;

    private void Start()
    {
        RepeatSimulation();
    }

    private void RepeatSimulation()
    {
        if (currentIteration < iterationCount)
        {
            SceneManager.LoadScene(sceneName);
            currentIteration++;
        }
        else
        {
            // End of iterations, do any cleanup or finalization here.
            Debug.Log("Simulation iterations completed.");
        }
    }
}

