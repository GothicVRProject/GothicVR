# Step 1 - Install the Nyras Prologue and UEVR 
* Install the Nyras Prologue from Steam
* Download UEVR from https://uevr.io/
* Follow the installation documentation: https://praydog.github.io/uevr-docs/index.htm
* Add file exemptions if necessary

 # Step 2 - Inject the Nyras Prologue with UEVR
* Start UEVR with Admin rights
* Start the Nyras Prologue and start a new game untill you are in the game
* Optional: Set the Nyras Prologue to windowed mode for easier configuration
  
  ![image](https://github.com/user-attachments/assets/f97ae881-51ca-486d-b1e3-deb5496c7f76)

*  Change back to UEVR
*  Select the Gothic Remake Nyras Prologue application
*  Select "OpenXR"
*  Press "Inject"
  
  ![image](https://github.com/user-attachments/assets/ca7b6d00-3691-4e81-bbe0-d1624d30d399)

*  You should change to the game automatically with VR mode active (in 3rd person)

 # Step 3 - Configure a virtual reality first person view
*  You will look the main character now over the shoould in 3rd person view
*  You can move the camera with the right controller joystick
*  Access the UEVR ingame menue with the "insert" button on your keyboard

  ![image](https://github.com/user-attachments/assets/29aa35f3-4eac-4181-b0ec-b8baec07d354)

*  Activate "Advanced Settings"
*  Go to "UObjectHook"

  ![image](https://github.com/user-attachments/assets/0fab4fdb-61b7-4bc2-8ca6-02275f2614b7)

*  Go to Main
*  Expand the Common Objects
*  Expand the "Acknowledged Pawn"
*  Press "Attach camera to (relative)"

  ![image](https://github.com/user-attachments/assets/7780db16-ba80-486d-b4af-b334c69bd722)

*  Adjust the camera offset so that the camera is in the main character's head a little in front of his eyes
*  Press "Save state"
*  You have know configured a first person view

 # Step 4 - Hide the main character's body for a more immersive experience
 *  Go to Common Objects and the "Acknowledged Pawn"
 *  Expand the "Components"

   ![image](https://github.com/user-attachments/assets/bf90ad62-692b-4303-8b10-c9394fe00ce8)

* Expand the GothicSkeletalMeshComponent
* Uncheck the "Visible" box
* Press "Save visibility state"

  ![image](https://github.com/user-attachments/assets/7f0dada3-0792-4233-b693-d3388e24b512)

* Your main character's mesh is now hidden (although the shadow is still there)
* You have achieved an immersive first person view

 # Step 5 - Adjust the user interface to your needs
 *  Go to "Runtime"
 *  Change the UI offset, size and distance to your needs
 *  All UI like hud, dialogs, chest interaction etc will be affacted at the same time

   ![image](https://github.com/user-attachments/assets/2174e2f3-bc04-4382-b079-4005573357b7)

 # Step 6 - Add motion control / objects like swords to your hands 
 *  Go to Common Objects and the "Acknowledged Pawn"
 *  Expand the "Properties"

   ![image](https://github.com/user-attachments/assets/72b83952-30e8-412d-8745-0e8b94ff300c)

 *  Expand "Children"
 *  Go to the object you want to attach to your hand (eg a sword or torch)
 *  Expand "Components"
 *  Go to the StaticMeshComponent of the object
 *  Press "Attach to" right or left hand

   ![image](https://github.com/user-attachments/assets/e9f5b09b-8cff-4385-afb4-f4f6d8d021be)

 *  Adjust it to your needs
 *  Check "Permanent change"
 *  Press "Save state"

  ![image](https://github.com/user-attachments/assets/55dbf1d0-08c4-4258-9fef-212a18526d95)

 *  The sword will now be in your according hand
 *  This change is mostly visually and not necessarily connected with real physics like damage dealing
