# Ultra Violet: Designing a Game Using Biofeedback to Augment Traditional Controls and Player Enjoyment
> Ultra Violet is a 2D side-scrolling platform game developed using facial expression and eye tracking to explore player enjoyment with non-traditional controls. 

This project studied how developing a game with biofeedback input could augment traditional controls and increase player enjoyment. This game was developed in Unity using C#. The various biofeedback signals used were facial expression/emotion tracking via the Affectiva Affdex Unity SDK (https://knowledge.affectiva.com/v3.2/docs/getting-started-with-the-emotion-sdk-for-unity) and eye tracking via Tobii Unity SDK (https://developer.tobii.com/tobii-unity-sdk/). Facial expression tracking was used to examine player physiological state during game play. To conduct the study, the 2D platform game compared game play with various biofeedback inputs to game play without biofeedback. The results of the study showed that participants preferred playing the game with physiological controls. This has a few implications not just on designing games with physiological input in mind, but on game design in general: (1) Physiological input should be designed in an intuitive manner, combining the physiological input with a traditional input, if necessary, if it gives the player more control over the mechanic; (2) Tracking the player's emotional state during game play can provide immediate responses, but should not be overdone as to make the difficulty of the game too easy.

## Compatibility
Windows only. (Until the Tobii developers provide MacOSX & Linux support)

To use facial expression tracking any web cam will work, but for eye tracking only Tobii eye trackers will work with the Unity SDK. This project only contains the scripts and assets for the game as github has a filesize limit of 100 MB. To be able to play the game, the Affectiva Affdex Unity SDK and Tobii Unity SDK must be downloaded from the links above.

## Installation

To install:
* Create a new Unity 2D project.
* Go to the Assets tab at the top.
    * Click Import Package, then on Custom Package...
    * Select this package.
* Do the same for the Affdex Unity SDK and Tobii Unity SDK and you're done!

## Contributions
Drew Noma - djknoma@gmail.com

[https://github.com/dknoma](https://github.com/dknoma)

# Revision History
Change log below:

Version 1.0.0
* First version of the game with Affdex facial expression tracking (joy and anger/frustration) and Tobii eye tracking.
