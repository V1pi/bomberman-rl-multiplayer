using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum GameState {
    RUNNING,
    RESETTING
}

public class GameController : MonoBehaviour {
    [Range(10, 60)]
    [SerializeField] private float matchTime = 30f;
    [Range(1, 10)]
    [SerializeField] private float timeToEnding = 5f;
    private Vector3Int[] spawnPositions = new Vector3Int[] { new Vector3Int(1,1,0), new Vector3Int(17, 11, 0), new Vector3Int(1, 11, 0), new Vector3Int(17, 1, 0) };
    private MapController mapController;
    private float timer = 0;
    private GameState currentGameState = GameState.RUNNING;
    private List<GameObject> items = new List<GameObject>();
    [SerializeField] private GameObject[] prefabs;
    [Range(1,4)]
    [SerializeField] private int nAgentsInGame = 1;
    private AgentController[] agents;
    private int nDeads = 0;
    private Dictionary<EnviromenentType, int> nKillsPerAgent = new Dictionary<EnviromenentType, int>();
    private Dictionary<ActionState, Vector3Int> directions = new Dictionary<ActionState, Vector3Int>();
    public GameObject bombPrefab;
    private List<GameObject> bombs = new List<GameObject>();
    private bool isMapDead = false;

    private void Awake() {
        mapController = GetComponent<MapController>();
        agents = new AgentController[nAgentsInGame];
        directions[ActionState.UP] = Vector3Int.up;
        directions[ActionState.DOWN] = Vector3Int.down;
        directions[ActionState.LEFT] = Vector3Int.left;
        directions[ActionState.RIGHT] = Vector3Int.right;
    }

    private void Start() {
        DoReset();
    }

    private void ResetEnviroment() {
        currentGameState = GameState.RESETTING;
    }

    private void DoReset() {
        foreach(GameObject item in items) {
            item.SetActive(false);
        }
        items.Clear();
        timer = 0;
        nDeads = 0;
        isMapDead = false;
        nKillsPerAgent = new Dictionary<EnviromenentType, int>();
        mapController.GenerateNewMap();
        InitializeAgents();
        currentGameState = GameState.RUNNING;
    }
    private void InitializeAgents() {
        for (int i = 0; i < nAgentsInGame; i++) {
            Vector3 worldPos = mapController.GetWorldFromCellPos(spawnPositions[i]);
            GameObject agent = null;
            
            if (agents[i] != null) {
                agent = agents[i].gameObject;
                agent.transform.position = worldPos;
                agent.SetActive(true);
            } else {
                agent = Instantiate(prefabs[i], worldPos, Quaternion.identity, this.transform.parent);
            }
            nKillsPerAgent[agent.GetComponent<AgentController>().agentType] = 0;
            mapController.AddNewPlayerToMap(agent.GetComponent<AgentController>().agentType, spawnPositions[i]);
            agent.GetComponent<AgentController>().SetConfigs(spawnPositions[i], this);
            agents[i] = agent.GetComponent<AgentController>();
        }
    }

    private void Update() {
        
        if(currentGameState == GameState.RUNNING) {
            timer += Time.deltaTime;
            if(timer >= matchTime - timeToEnding && timer <= matchTime) {
                mapController.EndingGame(2 / (247 / timeToEnding));
            }
            if(timer >= matchTime) {
                ResetEnviroment();
                timer = 0;
            }
            if (nDeads == nAgentsInGame - 1) {
                foreach (AgentController agent in agents) {
                    if (agent.gameObject.activeSelf) {
                        if(nKillsPerAgent[agent.agentType] != 0) { // Matou e terminou vivo
                            agent.AddReward(1);
                        } else {
                            // agent.AddReward(-1f); // last man alive
                        }

                        agent.gameObject.SetActive(false);
                        ResetEnviroment();
                    }
                }
            }
        }
        if(currentGameState == GameState.RESETTING) {
            timer += Time.deltaTime;
            if(timer >= 0.5f) {
                DoReset();
            }
        }
    }

    public Vector3Int MovePlayer(ActionState action, Vector3Int lastPos, AgentController agent) {
        if (action != ActionState.NONE) {
            Vector3Int newPos = lastPos + directions[action];
            if(newPos.x >= 0 && newPos.x < 19 && newPos.y >= 0 && newPos.y < 13 &&
                mapController.MovePlayer(agent.agentType, lastPos, newPos)) {
                agent.transform.position = mapController.GetWorldFromCellPos(newPos);
                return newPos;
            }
        }
        return lastPos;
    }

    public void KillAgent(Vector3Int cellPos, AgentController bomber = null) {
        nDeads++;
        foreach (AgentController agent in agents) {
            if(agent.position == cellPos && agent.gameObject.activeSelf) {
                if (bomber != null && agent.agentType != bomber.agentType) { // Morreu pq alguem matou ele
                    nKillsPerAgent[bomber.agentType]++;
                    //agent.AddReward(-1);
                    /*bomber.AddReward(0.2f);
                    agent.AddReward(-0.8f);*/
                } else {
                    // Morreu pelo mapa
                    /*if(bomber == null) {
                        isMapDead = true;
                    } else { // Se matou

                    }*/
                }

                agent.gameObject.SetActive(false);
            }
        }
        
        if (nDeads >= nAgentsInGame) {
            ResetEnviroment();
        }
    }

    public void ExplodeBomb(Vector2 worldPos, BombType bomb, AgentController bomber) {
        mapController.ExplodeBomb(worldPos, bomb, bomber);
    }

    public void RemoveExplosion(Vector3Int cellPos) {
        mapController.AddNewObjectToMap(EnviromenentType.NONE, cellPos);
    }

    public void AddExplosion(GameObject explosion, Vector3Int relativePos, AgentController bomber) {
        mapController.AddExplosion(relativePos, bomber);
        explosion.GetComponent<Explosion>().SetConfigs(this, mapController.GetCellFromRelativePos(relativePos));
        items.Add(explosion);
    }

    public int[][] GetMap() {
        int[][] map = mapController.GetPlayerMap();
        return map;
    }

    public Vector3Int[] GetAllBombsMap() {
        int[][] map = mapController.GetMap();
        List<Vector3Int> bombs = new List<Vector3Int>();
        for(int i =0; i < 19; i++) {
            for(int j = 0; j < 13; j++) {
                if (map[i][j] == (int) EnviromenentType.BOMB || map[i][j] == (int)EnviromenentType.PLAYER_BOMB) {
                    bombs.Add(new Vector3Int(i, j, 1));
                }
            }
        }
        while (bombs.Count != 8) {
            bombs.Add(Vector3Int.zero);
        }
        return bombs.ToArray();
    }

    public void AddBomb(AgentController bomber, Vector3Int position) {
        if(mapController.CanPlantBomb(position)) {
            bomber.DecreaseBombs();
            mapController.AddNewObjectToMap(EnviromenentType.PLAYER_BOMB, position);
            GameObject newBomb = GetBomb(mapController.GetWorldFromCellPos(position));
            newBomb.GetComponent<Bomb>().SetConfigs(bomber);
            items.Add(newBomb);
        }
    }

    GameObject GetBomb(Vector3 pos) {
        foreach (var exp in bombs) {
            if (!exp.activeSelf) {
                exp.transform.position = pos;
                exp.SetActive(true);
                exp.GetComponent<Bomb>().ResetConfigs();
                return exp;
            }
        }
        var newExplosion = Instantiate(bombPrefab, pos, Quaternion.identity, this.transform.parent);
        bombs.Add(newExplosion);
        return newExplosion;
    }
}
