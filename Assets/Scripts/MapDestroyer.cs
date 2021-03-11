using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapDestroyer : MonoBehaviour {
    private Tilemap tilemap;
    [SerializeField] private Tile wallTile;
    [SerializeField] private Tile destructibleTile;
    [SerializeField] private GameObject explosionPrefab;
    private Vector3Int[] directions = new Vector3Int[] { Vector3Int.left, Vector3Int.right, Vector3Int.up, Vector3Int.down };
    private MapController mapController;
    private List<GameObject> explosions = new List<GameObject>();

    private GameController gameController;
    private void Start() {
        tilemap = this.transform.parent.Find("Grid").Find("TilemapGameplay").GetComponent<Tilemap>();
        mapController = GetComponent<MapController>();
        gameController = GetComponent<GameController>();
    }

    public void Explode(Vector2 worldPos, BombType bomb, AgentController bomber) {
        //int initialMatchId = gameController.matchId;
        Vector3Int originCell = tilemap.WorldToCell(worldPos);
        BombConfig bombConfig = AllEnviromentCommon.Instance.GetBombConfigs(bomb);
        int radius = bombConfig.radius;

        ExplodeCell(originCell, bombConfig, bomber, true);
        foreach (var direction in directions) {
            for (int i = 1; i < radius + 1; i++) {
                /*if (initialMatchId != gameController.matchId) {
                    return;
                }*/
                if (!ExplodeCell(originCell + i * direction, bombConfig, bomber)) {
                    break;
                }
            }
        }
    }

    private bool ExplodeCell(Vector3Int relativePos, BombConfig bombConfig, AgentController bomber, bool isFirstRow = false) {
        Vector3 pos = tilemap.GetCellCenterWorld(relativePos);

        Tile tile = tilemap.GetTile<Tile>(relativePos);
        if (tile == wallTile) {
            return false;
        }
        Collider2D collider = Physics2D.OverlapPoint(pos, LayerMask.GetMask("Bomb"));
        if (collider != null && !isFirstRow) {
            if (collider.tag == "Bomb") {
                collider.gameObject.GetComponent<Bomb>().SetCountdown(bombConfig.timeBeteweenExplosions);
            }
            return false;
        }


        GameObject newExplosion = GetExplosion(pos);

        // mapController.AddNewExplosionToMap(cellPos, bomber);
        gameController.AddExplosion(newExplosion, relativePos, bomber);

        if (tile == destructibleTile) {
            tilemap.SetTile(relativePos, null);
            //mapController.CreateNewItem(cellPos);
            //newExplosion.GetComponent<Explosion>().SetConfigs(mapController, cellPos, true);
            return false;
        }

        //newExplosion.GetComponent<Explosion>().SetConfigs(mapController, cellPos);

        return true;
    }

    GameObject GetExplosion(Vector3 pos) {
        foreach(var exp in explosions) {
            if(!exp.activeSelf) {
                exp.transform.position = pos;
                exp.SetActive(true);
                return exp;
            }
        }
        var newExplosion = Instantiate(explosionPrefab, pos, Quaternion.identity, this.transform.parent);
        explosions.Add(newExplosion);
        return newExplosion;
    }
}
