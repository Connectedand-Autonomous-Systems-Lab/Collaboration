## Version Compatibilities

We are using 2021.3.9f1 because of the following reasons.
1. UNITY_ROS_RL_SIMULATOR which was on 2020.3.48f1 did not support Perception pkg.
2. In future, we can use ML Agent in this project as it was used in PeopleSansPeople in the same Unity version.

## Starting Beam Eye tracker

Run the Beam Eye tracker and enable the gaming extensions and eye tracking overlay. 

Activate an environment which contains beam pkg installed in it.

Run Original_Beam_Python_Server\Beam.py

## Play Game

FullscreenGameView.cs is responsible for viewing the Game in full screen view. The keyboard shortcut to toggle the fullscreen is F11.

First play the game. 
Then toggle fullscreen --> F11