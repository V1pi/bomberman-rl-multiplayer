using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum EnviromenentType {
    BOMB, 
    PLAYER,
    PLAYER_BOMB,
    NONE,
    IGNORE,
    EXTRA_BOMB,
    EXPLOSION,
    WALL,
    DESTRUCTABLE,
    OWN,
    OWN_BOMB,
    AGENT_1,
    AGENT_2
}

public class MapController : MonoBehaviour {
    int[][] map = new int[19][];
    private Tilemap tilemap;
    public Vector3Int posBegin;
    private MapDestroyer mapDestroyer;
    private MapGenerator mapGenerator;
    [SerializeField] Tile wall;
    [SerializeField] Tile destructable;
    private float endingTime = 0;
    private Vector3Int leftPosEnding = Vector3Int.zero;
    private Vector3Int rightPosEnding = new Vector3Int(18, 12, 0);
    private bool toggle = true;
    private GameController gameController;
    private Dictionary<EnviromenentType, Vector3Int> agentsPos = new Dictionary<EnviromenentType, Vector3Int>();

    private void Awake() {
        Transform grid = this.transform.parent.Find("Grid");
        tilemap = grid.Find("TilemapGameplay").GetComponent<Tilemap>();
        Transform posBeginTransform = grid.Find("PosBegin");
        posBegin = tilemap.WorldToCell(posBeginTransform.position);
        mapDestroyer = GetComponent<MapDestroyer>();
        mapGenerator = GetComponent<MapGenerator>();
        gameController = GetComponent<GameController>();
    }

    public void GenerateNewMap() {
        ResetMap();
        mapGenerator.CreateNewMap();
    }

    public void ExplodeBomb(Vector2 worldPos, BombType bomb, AgentController bomber) {
        mapDestroyer.Explode(worldPos, bomb, bomber);
    }

    public void ResetMap() {
        for (int i = 0; i < 19; i++) {
            map[i] = new int[13];
            for (int j = 0; j < 13; j++) {
                map[i][j] = (int) EnviromenentType.NONE;
                tilemap.SetTile(new Vector3Int(i, j, 0) + posBegin, null);
            }
        }
        toggle = true;
        leftPosEnding = Vector3Int.zero;
        rightPosEnding = new Vector3Int(18, 12, 0);
        endingTime = 0;
    }

    public bool MovePlayer(EnviromenentType agentType, Vector3Int oldPos, Vector3Int newPos) {
        EnviromenentType newPosType = (EnviromenentType)map[newPos.x][newPos.y];
        EnviromenentType oldPosType = (EnviromenentType)map[oldPos.x][oldPos.y];
        if (newPosType == EnviromenentType.EXPLOSION) {
            gameController.KillAgent(oldPos);
        } else {
            if(newPosType == EnviromenentType.NONE) {
                agentsPos[agentType] = newPos;
                if (oldPosType == EnviromenentType.PLAYER_BOMB) {
                    AddToMap(EnviromenentType.BOMB, oldPos);
                } else {
                    AddToMap(EnviromenentType.NONE, oldPos);
                }
                AddToMap(EnviromenentType.PLAYER, newPos);
                return true;
            }
        }
        return false;
    }

    private void AddToMap(EnviromenentType type, Vector3Int cellPos, AgentController bomber = null) {
        EnviromenentType oldPos = (EnviromenentType)map[cellPos.x][cellPos.y];
        if (type == EnviromenentType.EXPLOSION && (oldPos == EnviromenentType.PLAYER || oldPos == EnviromenentType.PLAYER_BOMB)) {
            gameController.KillAgent(cellPos, bomber);
        } else {
            map[cellPos.x][cellPos.y] = (int)type;
        }
    }

    public int[][] GetMap() {
        int[][] newMap = new int[19][];
        for (int i = 0; i < 19; i++) {
            newMap[i] = new int[13];
            for (int j = 0; j < 13; j++) {
                newMap[i][j] = map[i][j];
            }
        }
        return newMap;
    }


    public int[][] GetPlayerMap() {
        int[][] newMap = new int[19][];
        for(int i = 0; i < 19; i++) {
            newMap[i] = new int[13];
            for (int j = 0; j < 13; j++) {
                newMap[i][j] = map[i][j];
            }
        }
        foreach(var key in agentsPos.Keys) {
            Vector3Int pos = agentsPos[key];
            newMap[pos.x][pos.y] = (int)key;
        }
        return newMap;
    }

    public void AddExplosion(Vector3Int relativePos, AgentController bomber) {
        Vector3Int cellPos = relativePos - posBegin;
        AddToMap(EnviromenentType.EXPLOSION, cellPos, bomber);
    }

    public bool CanPlantBomb(Vector3Int cellPos) {
        EnviromenentType type = (EnviromenentType)map[cellPos.x][cellPos.y];
        return type != EnviromenentType.BOMB && type != EnviromenentType.PLAYER_BOMB;
    }

    public void AddNewPlayerToMap(EnviromenentType agent, Vector3Int cellPos) {
        agentsPos[agent] = cellPos;
        AddToMap(EnviromenentType.PLAYER, cellPos);
    }

    public void AddNewObjectToMap(EnviromenentType type, Vector3Int cellPos) {
        Vector3Int relativePos = cellPos + posBegin;
        switch(type) {
            case EnviromenentType.WALL:
                tilemap.SetTile(relativePos, wall);
                break;
            case EnviromenentType.DESTRUCTABLE:
                tilemap.SetTile(relativePos, destructable);
                break;
        }
        AddToMap(type, cellPos);
    }

    public Vector3 GetWorldFromCellPos(Vector3Int cellPos) {
        Vector3Int relativePos = cellPos + posBegin;
        return tilemap.GetCellCenterWorld(relativePos);
    }

    public Vector3Int GetCellFromRelativePos(Vector3Int relativePos) {
        return relativePos - posBegin;
    }

    public Vector3Int GetRelativeFromCellPos(Vector3Int cellPos) {
        return cellPos + posBegin;
    }

    public void AddNewEndingTile(Vector3Int cellPos) {
        Vector3Int relativePos = cellPos + posBegin;
        tilemap.SetTile(relativePos, wall);
        AddToMap(EnviromenentType.EXPLOSION, cellPos);
    }

    public void AddNewObjectToMap(EnviromenentType type, int x, int y) {

        AddNewObjectToMap(type, new Vector3Int(x, y, 0));
    }

    public void EndingGame(float eachStep) {
        if(leftPosEnding.x > rightPosEnding.x) {
            return;
        }
        endingTime += Time.deltaTime;
        if(endingTime >= eachStep) {
            AddNewEndingTile(leftPosEnding);
            AddNewEndingTile(rightPosEnding);
            bool tempToogle = this.toggle;
            
            leftPosEnding = ToggleEnding(leftPosEnding, tempToogle);
            rightPosEnding = ToggleEnding(rightPosEnding, !tempToogle, false);
            
            endingTime = 0;
        }
    }

    private Vector3Int ToggleEnding(Vector3Int pos, bool toggle, bool isLeft = true) {
        if(toggle) {
            pos.y++;
        } else {
            pos.y--;
        }

        if(toggle && pos.y > 12) {
            if(isLeft) {
                pos.x++;
            } else {
                pos.x--;
            }
            pos.y = 12;
            this.toggle = !this.toggle;
        } 
        if(!toggle && pos.y < 0) {
            if (isLeft) {
                pos.x++;
            } else {
                pos.x--;
            }
            pos.y = 0;
        }
        return pos;
    }
}
