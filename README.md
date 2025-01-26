# X4SaveEditor

## Edit X4 Save Files.

### Features:
  - Change mineral respawn time in existing save files

Note: This is a work in progress. Please backup your save files before using this tool.

### Usage:

1. Download the latest release from the [releases page]
1. Run the executable
1. Open a save file (must be decompressed xml from .gz)
1. Load the save file (See bottom left corner)
1. Click on the "Change Mineral Respawn Rates" button
1. Change the values as needed
1. Click on the "Save" button
1. Compress the file back to .gz
1. Replace the original save file with the modified one

Note:
The game will not load the save file if the xml is not compressed back to .gz

Currently the mechanism is comparing the input value to default value and get a ratio, then modify the ratio of all resources matching the 
search criteria. 

The default values are: 
- RespawnTime: 36000
- MaxResource: 49500

So, if you want to double the respawn time, you should input 72000. If you want to double the max resource, you should input 99000.

Then, for any other resources that didn't use this default value, the new value will be calculated based on the ratio of the default value.

For example, if you have a resource with RespawnTime 5000 and MaxResource 6000, and you input 72000, 99000
the new RespawnTime will be 10000 (5000*(72000/36000)) and the new MaxResource will be 12000 (6000*(99000/49500)).

## Other notes

You may edit any field you want too, but no adding items or removing items yet. Don't have time to do that. 

## Memory usage

For a 2GiB save it may use up to 28 GiB RAM to hold all the data.
Make sure you have enough RAM!