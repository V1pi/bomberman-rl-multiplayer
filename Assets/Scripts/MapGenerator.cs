using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour {
    MapController mapController;
    private void Awake() {
        mapController = GetComponent<MapController>();
    }
    public void CreateNewMap() {
        for (int x = 0; x < 19; x++) {
            for (int y = 0; y < 13; y++) {
                if(x > 1 && x < 17 && x % 2 == 0) {
                    if (y > 1 && y < 11 && y % 2 == 0) {
                        mapController.AddNewObjectToMap(EnviromenentType.WALL, x, y);
                    }
                }
                if(x == 0 || y == 0 || x == 18 || y == 12) {
                    mapController.AddNewObjectToMap(EnviromenentType.WALL, x, y);
                }
            }
        }
    }
}
