# ConCord: Human-in-the-Loop, Cooperative Robot Exploration

This repository contains the codebase to deploy the Unity side of this project. Use the Unity Hub to install this from the disk after cloning it. 

## Quick Guide

### Installation

1. Clone this repository to your local disk.
2. Navigate to Unity Hub and use the option "Add project from disk" to select the cloned folder. 

### Known errors

1. **Perception package not found:** Add Perception pkg by the package manager from Unity. The required version of it is in Perception folder. 

### Version Compatibilities

We are using 2021.3.9f1 because of the following reasons.
1. UNITY_ROS_RL_SIMULATOR which was on 2020.3.48f1 did not support Perception pkg.
2. In future, we can use ML Agent in this project as it was used in PeopleSansPeople in the same Unity version.

**With this being setup, you are ready to test out the results from ConCord!**


## Starting Beam Eye tracker

Run the Beam Eye tracker and enable the gaming extensions and eye tracking overlay. 

Activate an environment which contains beam pkg installed in it.

Run Original_Beam_Python_Server\Beam.py

## Play Game

FullscreenGameView.cs is responsible for viewing the Game in full screen view. The keyboard shortcut to toggle the fullscreen is F11.

First play the game. 
Then toggle fullscreen --> F11
