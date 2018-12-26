Easy Roads 3D (https://assetstore.unity.com/packages/tools/terrain/easyroads3d-pro-v3-469) is, IMO, an impressive road asset for Unity.   
CScape (https://assetstore.unity.com/packages/tools/modeling/cscape-city-system-86716) is a shader based building asset that allows me to get 90fps in VR, which was a priority requirement.   

CScape comes with it's own roads, however they are simple straight lines (ie: Manhattan NYC)   I liked Easy Roads 3D because of the curves and variations that are possible, but manually placing each building....  WOW, what a PITA.  So here is a set of scripts that will do most of the work for you.

Usage:
1) Place "BldgPlacementEditor" in the "Assets/Editor" folder.  This HAS to be here, like all editor scripts.
2) Place the "BldgPlacement" script where ever you wish.  (Organize your project however you like)
3) Add the "BldgPlacement" script anywhere you want. Personally I create a empty game object and attach the script to that.  Once you have the buildings set the way you wish, remove the script as it is not used at run-time
4) The "BldgPlacement" script has two properites:
  
  a) Building Tag
  
  b) Road Tag
  
  You will need to create two tags in your inspector, these are temporary tags that are only used while placing the buildings along the road.
  
  Any buildings/roads not tagged will not be affected.   This allows you to change only the roads and buildings that you want, instead of All-or-Nothing.
  Once you create the tags, then tag the buildings and roads you wish to affect (easy right?) 
  Make sure to untag your buildings and roads once they are in the positions you desire.
  
5) Press the "Place Buildings" button.   

That's it.

Options:
* minBetweenBuildings/maxBetweenBuildings (float): These allow a random range to add space between buildings.
* minBuildingBack/maxBuildingBack (float): These allow a random range to push the building back from the side of the street.
* percentBuildingBack: This affects the min/maxBuildingBack so that only a given percentage of buildings are moved back.

Notes: 
* There is currently an issue at intersections, so you will likely have two buildings placed on top of each other at each corner.    I am currently thinking about a solution to this (Nov 10, 2018)
* There is an odd situation in ref to moving buildings back.  Still working on this but wanted to include the code asap.
* After you have placed the buildings where you want, make sure to untag them so they are not moved at a later time.
