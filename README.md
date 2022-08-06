## Custom Assets Library Plugin
The Custom Assets Library Plugin (CALP) is the successor to EAL and EAR which converts packs to use the TaleWeave Interface.
Supplementary functionality like Auras, Effects, Filters, Transformations, Animations and Sound has been moved to the
new Custom Assets Library Plugin Integrated Extension (CALPIE) plugin.

## Installing With R2ModMan
This package is designed specifically for R2ModMan and Talespire. 
You can install them via clicking on "Install with Mod Manager" or using the r2modman directly.

## Player Usage
This plugin used for players allows TaleSpire to search for TaleWeaver content in the R2Modman Directory.
This will allow direct download of TaleWeaver packs via R2Modman.

### Default Keyboard Shortcuts

CALP has a bunch of keyboard shortcuts for triggering additional functionality. The default keys can be reconfigured to other
keys by editing the R2ModMan configuration for the CALP plugin. Keyboard shortcuts are divided into two sub-sections: functionality
triggers and asset spawn modifeirs. 

### Asset Types

```
+-----------------+-----------+-------------------+--------------------+--------------------+----------------------------------------------------+
| Kind            | Armature  | Shader            | Transparency       | Hide               | Comments                                           |
+-----------------+-----------+-------------------+--------------------+--------------------+----------------------------------------------------+
+-----------------+-----------+-------------------+--------------------+--------------------+----------------------------------------------------+
| Any             |    Yes    | From Asset Bundle | Supported          | Not Supported      | Applies to the armature meshes only                |
+-----------------+-----------+-------------------+--------------------+--------------------+----------------------------------------------------+
| Creature        |    No     | TS/Creature       | Not Supported      | Supported          | This is the default for minis                      |
+-----------------+-----------+-------------------+--------------------+--------------------+----------------------------------------------------+
| Audio           |    No     | TS/Creature       | Not Supported      | Supported          | Mini starts playing audio when place on board*     |
+-----------------+-----------+-------------------+--------------------+--------------------+----------------------------------------------------+
| Auras           |    No     | From Asset Bundle | Supported          | Supported*         | Not yet implemented in CAPL*                       |
+-----------------+-----------+-------------------+--------------------+--------------------+----------------------------------------------------+
| Effect          |    No     | From Asset Bundle | Supported          | Supported*         | Used to create minis with non-TS shaders*          |
+-----------------+-----------+-------------------+--------------------+--------------------+----------------------------------------------------+
| Transform       |    No     | TS/Creature       | Not Supported      | Supported          | Not yet implemented in CAPL*                       |
+-----------------+-----------+-------------------+--------------------+--------------------+----------------------------------------------------+
| Prop            |    No     | TS/Placeable      | Not Supported      | Supported          | Not yet implemented in CAPL                        |
+-----------------+-----------+-------------------+--------------------+--------------------+----------------------------------------------------+
| Tile            |    No     | TS/Placable       | Not Supported      | Not Supported      | Not yet implemented in CAPL                        |
+-----------------+-----------+-------------------+--------------------+--------------------+----------------------------------------------------+
| (Multi) Slab    |    N/A    | TS/Placable       | Not Supported      | Not Supported      | Partially supported. Height bar does not work*     |
+-----------------+-----------+-------------------+--------------------+--------------------+----------------------------------------------------+
| Encounter       |    N/A    | Based On Asset    | Based On Asset     | Supported          | Not yet implemented in CAPL*                       |
+-----------------+-----------+-------------------+--------------------+--------------------+----------------------------------------------------+
```

Functionality marked by ```*``` required CALPIE to function.

## Developer Usage (Asset Creators)

Inbuilt into this package is a Binary Index writer which will generate a TaleWeaver compliant index file mimicing the interface of TaleWeaver. This is incredibly experimental and is being worked on. With CALP, EAR compliant files can be converted to TW compliance. This allows asset creators to keep their current workflow.

After CALP runs, an `index.json` and `index` files will be generated with some re-organizing. In the root of `index.json` file will contain an `assetPackId` variable which can be used to copy over into TaleWeaver's Steamapps folder.

### Unregister Batch File

If you update an asset (e.g. custom mini) you may or may not need to re-register it. If the asset's info.txt file and portrait.png file remained the same, you can just copy your new asset over the old one and the next time TS runs, it will use the updated version. If you did modify the info.txt or portrait.png file or added new assets to the pack then you will need to re-register the pack. To make this process easier, the Unregister batch file is provdied. Double click on this batch file to return the pack to its unregistered condition and it will get re-registered when TS is started again.

### Tweaking Packs

For testing purpose it is not possible to tweak the pack's index.json file (which is basically core TS version of most info.txt properties). To do this the pack needs to have been registered (or pre-registered). If the setting to delete the index.json is not turned on, the index.json file will remain after registration. To tweak it, follow these steps:

```
1. Ensure that in the CALP R2ModMan config you have Build Index Mode set to rebuildIndexIfMissing.
2. Rename the Assets folder back to CustomData
3. Delete the index (but not the index.json) file. Do not delete the Portraits folder.
4. Edit the index.json file
5. Restart TS to re-register the pack. This process will recognize the fact that you have a index.json file present and use it instead.
```

### Register For Vanilla Batch File

To help content developers use their custom assets even on Vanilla Talespire (non-modded Talespire), CALP generates a RegisterForVanilla.Bat file in the root of each pack. Using this batch file creates a directory link to be created in the Taleweaver directory that points to the pack location. Talespire will be convinced that the content is in the Taleweaver folder even though it is actually elsewhere. In order to use this batch file, you will need to do the following:

```
1. Open up a Command Prompt using the Run As Administrator. A normal Command Prompt does not have permissions to make links.
2. Execute the batch file with the location of your Taleweaver directory. For example: RegisterForVanilla E:\Steam\steamapps\common\TaleSpire\Taleweaver
```

To remove the link, just go to the Talwweaver folder and delete the corresponding sub-folder (not the d71427a1-5535-4fa7-82d7-4ca1e75edbfd folder since that is the core TS folder).

### Info.Txt Files

Below is a list of the currently supported proeprties in the info.txt file. Any of the old EAR properties (which are not yet listed here) are accepted but will be ignored.

```
{
  "name": "Assasin",
  "prefab": "",
  "kind": "Effect",
  "category": "Creature",
  "groupName": "Human",
  "description": "Assasin",
  "tags": "Human,Rogue,Assasin",
  "author": "Lord Ashes",
  "version": "1.0",
  "comment": "Maximo source",
  "size": 1.0,
  "code": "",
  "meshAdjustments":
  {
    "size": "1.0,1.0,1.0",
    "rotationOffset": "0.0,0.0,0.0",
    "positionOffset": "0.0,0.0,0.0"
  }
}
```

All properties are optional and will take the default if not provided. The "prefab" property allow using a different prefab name than the asset bundle name. If empty the
prefab will default to the asset bundle name.

## Binary index writer usage (BepInEx Dev)
the index writer as a supplied interface and method to write the pack. PackContent data structure does not use blob references or ECS upon initial construction. 
```CSharp
CustomAssetsLibrary.CustomAssetLib.generate(folder);
```
After completion, the WritePack method will convert it into the ECS Blob structure and write the index file into the modpack directory.

## Changelog
```
2.2.0: Bug fix for assets without info.txt
2.2.0: Added more registration logging
2.1.0: Added SmartJsonConvert to allow plugin to work in different locale regions without needing to switch region settings before running Talespire.
2.0.0: Moved supplementary functionality to CALPIE.
1.6.0: Added Effect hide functionality.
1.6.0: Added checks for improper asset bundles with visual warning and list in the logs.
1.5.0: Added scripts for unregistering and registering for vanilla.
1.5.0: Added option to tweak index.json files.
1.4.1: Updated documentation. No plugin change.
1.4.0: Added animation functionality
1.4.0: Added sound functionality
1.4.0: Added spawn modification keys
1.3.1: Improved logs
1.3.1: Bug Fix: Null Reference on ghosts
1.3.0: Bug Fix: Endless bases on load
1.3.0: Bug Fix: Spawn size for non-medium creatures
1.2.1: Corrected Thunderstore posting
1.2.0: Effects support trasparency.
1.2.0: Effects are restored on board load.
1.2.0: Bug Fix: AssetBundle Shader is not applied to Creature.
1.1.0: Fixed Effects support.
1.1.0: Fixed Index Out Of Bounds issue.
1.1.0: Added pre and post callbacks to implement additional features.
1.1.0: Temporarily removed dependency on Asset Data plugin.
1.0.1: Added missing Default Portrait into pack. No plugin change.
1.0.0: Official Release, CAL & CAP integrated and working.
```

Shoutout to Hollofoxes [Patreons](https://www.patreon.com/HolloFox) recognising their
mighty contribution to his caffeine addiction:
- John Fuller
- [Tales Tavern](https://talestavern.com/) - MadWizard

Also checkout LordAshes [donation page](https://lordashes.ca/TalespireDonate/Donate.php)
