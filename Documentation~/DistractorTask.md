# Documentation

## Overview

## Package contents

Scripts inside the runtime folder:
- Scripts
  - Core (interfaces etc.)
  - InputHandler (input handler stub, not working yet)
  - SceneManagement (bootstrapper and scene management logic)
  - Transport (Client/Server implementation and data transmission)
  - UserStudy (User study stages)

## Installation instructions

This package requires the Mixed Reality Toolkit 3.
Please follow the steps here:

https://learn.microsoft.com/de-de/windows/mixed-reality/mrtk-unity/mrtk3-overview/getting-started/setting-up/setup-new-project 

or download the setup here:
https://www.microsoft.com/en-us/download/details.aspx?id=102778

Afterward, install the package using the package manager:

https://docs.unity3d.com/Manual/upm-ui-giturl.html

## Requirements

The package is currently using Unity 2022.3.16f1. Technically, there shouldn't be any reason why a previous version should not work as well, but I have not tested it.

## Limitations

Currently, the system does not provide a dedicated logging system.
- Not all features have been tested on the Hololens itself (mainly the gaze controller and the clicker input need more testing).
- There seems to be a bug regarding the distractor movement if the user moves away from their original position. 

## Workflows

### Client / Server Communication

To allow for the communication between the hololens and a PC, the Unity Transport package is used to create a server on the PC that the hololens can connect to.
To simplify the connection, the server contains functionality to send its IP-address to a client as long as it has access to the client's IP-address. 
Therefore, the hololens will display its IP-address in a "hand menu" (https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk2/features/ux-building-blocks/hand-menu?view=mrtkunity-2021-05).

The IP-address can be entered into the "Ip transmission settings" of the "Server"-script. Afterward, click on the Connect button, to inform the client and allow the client to connect to the server.

To transmit any type of data, create a struct that implements the "DistractorTask.Core.ISerializer" interface.
The struct implementing that interface is required to handle its own serialization and deserialization.
For examples and an overview of all currently available implementations of this interface, check "Runtime/Scripts/Transport/DataContainer"

Both server and client offer a "TransmitNetworkMessage"-method that expects input of the type "ISerializer".
Before sending the serialized data, a byte indicating the type of data is sent. This is done by the "ConnectionDataWriter", which contains a list of each type mapping it to the correct byte.
This class also contains a helper function to send strings over the network using the smallest amount of data necessary. 

Additionally, client and server allow other components to register callbacks for certain "ISerializer"-types. 

### Bootstrapper

To ensure that the correct scenes and modules will be loaded, a bootstrapper script automatically loads a bootstrapper scene during startup, which then will additively load additional scenes.

In a build, the package will automatically use the correct bootstrapper scene.
For android builds, it loads "Android_Bootstrapper", for windows builds  "Win_Bootstrapper".

In theory, the package also provides the option to load the bootstrapper automatically inside the editor, but currently this is disabled. 
Instead, to test both client and server at the same time, start the "Editor_Bootstrapper" scene. The windows scene will only start the server and the android scene only the client.
All three bootstrapper scenes are located at "Runtime/Scenes/Bootstrapper". 

The client-side is currently contained inside the "UserStudyScene". The server-side inside the "ServerScreen" scene. 
The client-side contains the distractor task with the AR-setup while the server-side contains the distractor placement system and buttons to connect the client and to start the user study.

Additional scenes can be added to the "SceneManagement"-component. By adding them to the default scene group, the scene will be loaded additively on startup.
Otherwise, the scene can be added to a dedicated scene group that then can be loaded using code. 

### User Study 

The user study is divided into individual stages. Currently, there are two stages. The marker placement and the actual distractor task.
Each study stage requires two components, one for the server and one for the client. 
Both components agree on an "IStudyStageEvent"-type that defines the start and end of each study stage.
The "IStudyStageEvent" extends "ISerializer" by defining a boolean that marks whether the event is the start or the end of the current stage.

The abstract classes "ReceivingStudyStageComponent" and "SendingStudyStageComponent" already implement the necessary code to register the start and end event and expose additional methods to start or end the stage.
To include a new stage into the study, the stage components have to be added to their UserStudyManager (ClientSideUserStudyManager or ServerSideUserStudyManager).

Internally, the study managers set up all stages at startup and then start the first stage as soon as the request for the study start is sent from the server.
To make sure that both sides have properly ended the previous stage, the receiving side of the new stage has to send a start message to the sending side (happens automatically as soon as the previous stage ends).
This means that each stage starts and ends with its specific "IStudyStageEvent", handing the control back to the UserStudyManagers, who make sure that they are in sync before starting the next stage.


## Advanced topics

## Reference

## Samples

## Tutorials