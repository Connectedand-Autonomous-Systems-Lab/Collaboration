using UnityEngine;
using UnityEngine.Perception.Randomization;
using UnityEngine.Perception.Randomization.Scenarios;

public class MyFixedLengthScenario : FixedLengthScenario
{
    // Set the number of iterations in the scenario
    public int totalIterations = 3;

    // Set the initial seed value
    public int initialSeed = 123;

    private int currentIteration = 0;

    protected override void OnAwake()
    {
        // Set the initial seed value
        Random.InitState(initialSeed);
    }

    protected override void OnIterationStart()
    {
        // Set the seed value for the current iteration
        Random.InitState(initialSeed + currentIteration);

        // Rest of your iteration logic...

        // Increment the current iteration index
        currentIteration++;
    }
}

