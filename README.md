# Save Blueprint Requirements

A rough mod to save the requirements for blueprints that are currently being built.
It can be used to keep track of all the blueprints in all of the environments and what resources they require.

The mod saves the output to two files:
* Blueprints.txt
* Blueprints.json

The .txt file is the formatted version, the .json file is the database.  

The files will be located in the plugin's install directory, which will be ```<Steam Directory>\steamapps\common\Card Survival Tropical Island\BepInEx\plugins\SaveBlueprintRequirements\```

The files will be created/updated when the "Update" hotkey is pressed or the player travels to a different environment (traveling or entering a structure.)

For modders that wish to create a better mod, see [Note to Modders](#note-to-modders) at the end of this document for research notes.

# File Output:

## Example
```
Eastern Grasslands: 
    Trapping Pit
        Shovel (1)
        
        Heavy Stone (4)
        Long Stick (6)
        Palm Fronds (12)
        Shovel (3)
        Stone (16)
        
Jungle: 
    Empty Skep
        Palm Fronds (6)
        Palm Weave (8)
        
        Manure (2)
        Mud Pile (2)
        Palm Fronds (6)
        Palm Weave (8)
```       
## Overview

The output is as follows:
* The Environment the blueprint is in.
* The name of the blueprint
* The requirements for the current stage.
* A blank line
* All of the requirements to complete the blueprint, including the remaining items for the current stage.

## Viewing the File
It is recommended to use a text editor that refreshes an opened file when that file has changed.  Visual Studio Code from Microsoft supports automatic file refresh and is free.  

https://code.visualstudio.com/


# Limitations
I debated releasing this mod as it is so rough; however, I've found it to be quite useful so others may as well.

The limitations are listed below.

## Updates
The data is only updated when leaving an environment or the user presses the update hotkey.  The default update key is "Page Up" and can be changed in the config.  Mouse3 is a good option for mice with side buttons.

## Changing Game Saves
If changing saves or starting a new game, the .json file must be deleted or data from the previous game will be shown.

## No Improvements
Improvements such as expansions on huts or paths, etc. are not included.


# Configuration

The following settings are in the configuration file.  See [Changing the Configuration](#changing-the-configuration) below.

|Setting|Default|Description|
|--|--|--|
|InlineFormat|false|If true, will put all needed resources in a single line.|
|FileBaseName|Blueprints|The name of the exported files, minus the extension.|
|Hotkey|PageUp|The hotkey used to write the blueprint info on demand.|

# Changing the Configuration
All options are contained in the config file which is located at ```<Steam Directory>\steamapps\common\Card Survival Tropical Island\BepInEx\config\SaveBlueprintRequirements.cfg```.

The .cfg file will not exist until the mod is installed and then the game is run.

To reset the config, delete the config file.  A new config will be created the next time the game is run.

# Installation 
This mod requires the BepInEx mod loader.

## BepInEx Setup
If BepInEx has already been installed, skip this section.

Download BepInEx from https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x64_5.4.21.0.zip

* Extract the contents of the BepInEx zip file into the game's directory:
```<Steam Directory>\steamapps\common\Card Survival Tropical Island```

    __Important__:  The .zip file *must* be extracted to the root folder of the game.  If BepInEx was extracted correctly, the following directory will exist: ```<Steam Directory>\steamapps\common\Card Survival Tropical Island\BepInEx```.  This is a common install issue.

* Run the game.  Once the main menu is shown, exit the game.
    
* In the BepInEx folder, there will now be a "plugins" directory.

## Mod Setup
* Download the SaveBlueprintRequirements.zip.  
    * If on Nexumods.com, download from the Files tab.
    * Otherwise, download from https://github.com/NBKRedSpy/CardSurvival-SaveBlueprintRequirements/releases/

* Extract the contents of the zip file into the ```BepInEx/plugins``` folder.

* Run the Game.  The mod will now be enabled.

# Uninstalling

## Uninstall
This resets the game to an unmodded state.

Delete the BepInEx folder from the game's directory
```<Steam Directory>\steamapps\common\Card Survival Tropical Island\BepInEx```

## Uninstalling This Mod Only

This method removes this mod, but keeps the BepInEx mod loader and any other mods.

Delete the ```SaveBlueprintRequirements.dll``` from the ```<Steam Directory>\steamapps\common\Card Survival Tropical Island\BepInEx\plugins``` directory.

# Compatibility
Safe to add and remove from existing saves.


# Note to Modders

This section discusses some of the findings when doing research for this mod.  Be aware that some of it is from memory, and some paths were not fully researched.

Card Survival stores the current game's cards in two ways:  All of the environments in a dehydrated state and the current environment in a hydrated state.

The hydrated state is stored in GameManager.CurrentEnvironment and all the cards in GameManager.AllCards are all of the cards for the current environment.

The rest of the environments are in a "save" format and contain keys to the cards and the states of those cards.  If I remember correctly those environments are stored in GameManager.EnvironmentsData.

Note that while the current environment is in the EnvironmentsData collection, it appears to be a stale copy.  When the game is loaded, the current environment's entry in GameManager.EnvironmentsData is effectively ignored since the current environment is loaded from different data.  The save data is updated when the player leaves an environment.

Source code for this mod can be found at https://github.com/NBKRedSpy/CardSurvival-SaveBlueprintRequirements/

