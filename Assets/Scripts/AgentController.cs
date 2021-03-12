using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public enum ActionState {
    NONE,
    LEFT,
    RIGHT,
    UP,
    DOWN,
    BOMB
}

public class AgentController : Agent {
    public int id;
    public Vector3Int position;
    private ActionState currentAction = ActionState.NONE;
    private GameController gameController;
    [SerializeField] private int maxBombs = 2;
    private int bombs;

    public override void OnEpisodeBegin() {
        this.gameObject.SetActive(true);
        bombs = maxBombs;
        currentAction = ActionState.NONE;
    }

    public void SetConfigs(int id, Vector3Int position, GameController gameController) {
        this.id = id;
        this.position = position;
        this.gameController = gameController;
    }

    public void RecoverBomb() {
        if(bombs < maxBombs) {
            bombs++;
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers) {
        ActionSegment<int> actions = actionBuffers.DiscreteActions;
        ActionState action = (ActionState) actions[0];
        if(action != ActionState.BOMB) {
            this.position = gameController.MovePlayer(action, position, this.transform);
        } else if(bombs > 0) {
            gameController.AddBomb(this, position);
        }

    }

    public void DecreaseBombs() {
        bombs--;
    }

    public override void CollectObservations(VectorSensor sensor) {
        int[][] map = gameController.GetOwnMap(position);
        for(int i = 0; i < 19; i++) {
            for (int j = 0; j < 13; j++) {
                sensor.AddObservation(map[i][j]/11);
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<int> actions = actionsOut.DiscreteActions;
        actions[0] = (int)currentAction;
        currentAction = ActionState.NONE;
    }

    private void Update() {
        ActionState currentAction = this.currentAction;

        if (Input.GetKeyDown(id==1 ? KeyCode.RightControl : KeyCode.Space)) {
            currentAction = ActionState.BOMB;
        } else if (Input.GetKeyUp(id == 1 ? KeyCode.RightControl : KeyCode.Space)) {
            currentAction = ActionState.NONE;
        }
        if (Input.GetKeyDown(id == 1 ? KeyCode.RightArrow : KeyCode.D)) {
            currentAction = ActionState.RIGHT;
        } else if (Input.GetKeyUp(id == 1 ? KeyCode.RightArrow : KeyCode.D)) {
            currentAction = ActionState.NONE;
        }

        if (Input.GetKeyDown(id == 1 ? KeyCode.LeftArrow : KeyCode.A)) {
            currentAction = ActionState.LEFT;
        } else if (Input.GetKeyUp(id == 1 ? KeyCode.LeftArrow : KeyCode.A)) {
            currentAction = ActionState.NONE;
        }

        if (Input.GetKeyDown(id == 1 ? KeyCode.UpArrow : KeyCode.W)) {
            currentAction = ActionState.UP;
        } else if (Input.GetKeyUp(id == 1 ? KeyCode.UpArrow : KeyCode.W)) {
            currentAction = ActionState.NONE;
        }

        if (Input.GetKeyDown(id == 1 ? KeyCode.DownArrow : KeyCode.S)) {
            currentAction = ActionState.DOWN;
        } else if (Input.GetKeyUp(id == 1 ? KeyCode.DownArrow : KeyCode.S)) {
            currentAction = ActionState.NONE;
        }

        this.currentAction = currentAction;
    }
}
