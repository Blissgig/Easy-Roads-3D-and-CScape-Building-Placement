using System.Collections.Generic;
using UnityEngine;
using EasyRoads3Dv3;
using System.Linq;
using CScape;

[ExecuteInEditMode]
public class BldgPlacement : MonoBehaviour
{
    //Tags are used so that the developer can control the which roads and buildings are affected by this process
    public string buildingTag = "";
    public string roadTag = "";
    public float minBetweenBuildings = 0;
    public float maxBetweenBuildings = 0;
    public float minBuildingBack = 0;
    public float maxBuildingBack = 0;
    public byte percentBuildingBack = 100; //This affects the min/maxBuildingBack so that only a percentage of these buildings are affected


    private List<GameObject> buildings;
    private const byte buildingSizeMultiplier = 3;  //CScape uses a 1 to 3 setting for their buildings, so this is to deal with that.
    
    
    public void PlaceBuildings()
    {
        try
        {
            if (buildingTag.Trim().Length == 0 || roadTag.Trim().Length == 0)
            {
                Debug.Log("You must have building and road tags set");
                return;
            }

            //To insure the percentage is never over 100
            if (percentBuildingBack > 100)
            {
                percentBuildingBack = 100;
            }


            //Use only tagged roads and buildings, that way if you have sections the way you want them, this code will not affect the untagged assets
            buildings = GameObject.FindGameObjectsWithTag(buildingTag).ToList();
            List<GameObject> roads = GameObject.FindGameObjectsWithTag(roadTag).ToList();
            ERRoadNetwork roadNetwork = new ERRoadNetwork();
            ERRoad[] ERroads = roadNetwork.GetRoads();
            string roadName = "";


            //Move the buildings out of the way
            foreach (GameObject building in buildings)
            {
                building.transform.position = new Vector3(-100, 0, -100);
            }


            foreach (ERRoad road in ERroads)
            {
                roadName = road.GetName();

                //Make sure the road is in the tagged list
                if (RoadInList(roads, roadName))
                {
                    Vector3[] markersRight = road.GetSplinePointsRightSide();
                    Vector3[] markersLeft = road.GetSplinePointsLeftSide();
                    Vector3[] markersCenter = road.GetSplinePointsCenter();
                    Vector3[] markersOffset = markersCenter;
                    
                    PlaceBuildings(markersRight, markersCenter, true, road); 

                    PlaceBuildings(markersLeft, markersCenter, false, road); 
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log("Place Building Ex: " + ex.Message);
        }
    }

    private void PlaceBuildings(Vector3[] markersSide, Vector3[] markersCenter, bool rightSide, ERRoad road)
    {
        try
        {
            //Process:
            //Get a building from the list, 
            //Measure the building's width
            //Get the current sidePoint's position and count points until the difference is greater than/equal to the width of the building.  If the end of the side markers is reached, don't place building
            //Get the angle based on the two points
            //Place the building at the center point of those two points
            //Move the building back away from edge of sidewalk (Min/Max values in the editors)
            //Optional: Move buildings back from road
            //Optional: Allow space between buildings


            float offset = 0;
            int currentMarker = 0;
            bool isProcessing = true;
            bool isFound = false;       //Check if the building fits on this street-block
            float buildingDistance;
            const float increment = 0.2f;  //The value incremented back from the road.

            do
            {
                //If there are no buildings left, no need to prcess anything, obviously.
                if (buildings.Count == 0)
                {
                    isProcessing = false;
                    break;
                }

                isFound = false;  //reset

                Vector3 startMarker = markersSide[currentMarker];
                GameObject building = buildings[buildings.Count - 1];  //Get the last one, and build in reverse.  (allows removal from List without any issues)

                int buildingDepth = (building.GetComponent<BuildingModifier>().buildingDepth * buildingSizeMultiplier);
                int buildingWidth = (building.GetComponent<BuildingModifier>().buildingWidth * buildingSizeMultiplier);
                

                //loop to find the end position
                for (int i = (currentMarker + 1); i < markersSide.Length; i++)
                {
                    Vector3 endMarker = markersSide[i];
                    buildingDistance = Vector3.Distance(startMarker, endMarker);
                    

                    if (buildingDistance >= buildingDepth)
                    {
                        Vector3[] vectors = new Vector3[] { startMarker, endMarker };
                        Vector3 centerPoint = CenterOfVectors(vectors);
                        bool isRoad = false;
                        offset = 0; //Reset

                        do
                        {
                            isRoad = isInRoad(road.GetName(), centerPoint);

                            //If this is on the road, then we need to move back a little bit until it's not touching the road
                            if (isRoad)
                            {
                                offset += increment;  
								var x = (currentMarker + i) / 2;
								centerPoint = markersCenter[x] + (markersCenter[x] - centerPoint) * offset;
                            }

                        } while (isRoad);

                        //Optional: Push the building back farther from the road
                        if (minBuildingBack > 0 || maxBuildingBack > 0)
                        {
                            var percent = Random.Range(1, 100);

                            //This option allow a minimal amount of buildings to be moved back
                            if (percent <= percentBuildingBack)
                            {
                                var offsetAddition = Random.Range(minBuildingBack, maxBuildingBack);
                                if (offset == 0)
                                {
                                    offsetAddition += (road.GetWidth() * .1f);
                                }
                                offset += offsetAddition;
                            }
                        }

                        
                        //if there is an offset value, push the building back farther
                        if (offset > 0)
                        {
                            startMarker = markersCenter[currentMarker] + (markersCenter[currentMarker] - markersSide[currentMarker]) * -(offset / 2);
                            endMarker = markersCenter[i] + (markersCenter[i] - markersSide[i]) * -(offset / 2);
                        }

                        //Need to move back the start and end markers
                        if (rightSide)
                        {
                            building.transform.position = startMarker;
                            building.transform.rotation = Quaternion.LookRotation(endMarker - startMarker, Vector3.up);
                        }
                        else
                        {
                            building.transform.position = endMarker;
                            building.transform.rotation = Quaternion.LookRotation(startMarker - endMarker, Vector3.up);
                        }

                        isFound = true;
                        currentMarker = i;
                        break;
                    }
                }

                if (isFound)
                {
                    buildings.Remove(building);  //Only remove this from the list if the building was placed
                }
                else
                {
                    isProcessing = false;
                }

                
                //Optional: Allow space between buildings
                if (this.minBetweenBuildings > 0 && this.maxBetweenBuildings > 0)
                {
                    isFound = false;  //Reset
                    float distanceBetweenBuildings = Random.Range(minBetweenBuildings, maxBetweenBuildings);
                    Vector3 startPoint = markersSide[currentMarker];

                    for (int i = (currentMarker + 1); i < markersSide.Length; i++)
                    {
                        Vector3 endPoint = markersSide[i];

                        var buildingSeparation = Vector3.Distance(startPoint, endPoint);

                        if (buildingSeparation >= distanceBetweenBuildings)
                        {
                            currentMarker = i;
                            isFound = true;
                            break;
                        }
                    }

                    //In case we ran out of room for this side of the road, stop processing
                    isProcessing = isFound;
                }
                
            } while (isProcessing);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Place Buildings Ex: " + ex.Message);
        }
    }

    private bool isInRoad(string roadName, Vector3 centerPoint)
    {
        bool isFound = false;

        float radius = 0.1f;
        Collider[] hitColliders = Physics.OverlapSphere(centerPoint, radius);


        if (hitColliders.Length > 0)
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].name.ToLower() == roadName.ToLower())
                {
                    isFound = true;
                    break;
                }
            }
        }
        
        return isFound;
    }

    private bool RoadInList(List<GameObject> roads, string roadName)
    {
        bool bReturn = false;

        foreach (GameObject road in roads)
        {
            if (road.name.ToLower() == roadName.ToLower())
            {
                bReturn = true;
                break;
            }
        }
        
        return bReturn;
    }

    private Vector3 CenterOfVectors(Vector3[] vectors)
    {
        Vector3 sum = Vector3.zero;
        if (vectors == null || vectors.Length == 0)
        {
            return sum;
        }

        foreach (Vector3 vec in vectors)
        {
            sum += vec;
        }
        return sum / vectors.Length;
    }

}
