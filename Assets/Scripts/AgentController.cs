﻿using System.Collections;
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
    public EnviromenentType agentType;
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

    public void SetConfigs(Vector3Int position, GameController gameController) {
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
            this.position = gameController.MovePlayer(action, position, this);
        } else if(bombs > 0) {
            gameController.AddBomb(this, position);
        }

    }

    public void DecreaseBombs() {
        bombs--;
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(gameController.GetAgentPolarCoordinate(agentType));
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<int> actions = actionsOut.DiscreteActions;
        actions[0] = (int)currentAction;
        currentAction = ActionState.NONE;
    }

    private void Update() {
        ActionState currentAction = this.currentAction;

        if (Input.GetKeyDown(agentType==EnviromenentType.AGENT_2 ? KeyCode.RightControl : KeyCode.Space)) {
            currentAction = ActionState.BOMB;
        } else if (Input.GetKeyUp(agentType == EnviromenentType.AGENT_2 ? KeyCode.RightControl : KeyCode.Space)) {
            currentAction = ActionState.NONE;
        }
        if (Input.GetKeyDown(agentType == EnviromenentType.AGENT_2 ? KeyCode.RightArrow : KeyCode.D)) {
            currentAction = ActionState.RIGHT;
        } else if (Input.GetKeyUp(agentType == EnviromenentType.AGENT_2 ? KeyCode.RightArrow : KeyCode.D)) {
            currentAction = ActionState.NONE;
        }

        if (Input.GetKeyDown(agentType == EnviromenentType.AGENT_2 ? KeyCode.LeftArrow : KeyCode.A)) {
            currentAction = ActionState.LEFT;
        } else if (Input.GetKeyUp(agentType == EnviromenentType.AGENT_2 ? KeyCode.LeftArrow : KeyCode.A)) {
            currentAction = ActionState.NONE;
        }

        if (Input.GetKeyDown(agentType == EnviromenentType.AGENT_2 ? KeyCode.UpArrow : KeyCode.W)) {
            currentAction = ActionState.UP;
        } else if (Input.GetKeyUp(agentType == EnviromenentType.AGENT_2 ? KeyCode.UpArrow : KeyCode.W)) {
            currentAction = ActionState.NONE;
        }

        if (Input.GetKeyDown(agentType == EnviromenentType.AGENT_2 ? KeyCode.DownArrow : KeyCode.S)) {
            currentAction = ActionState.DOWN;
        } else if (Input.GetKeyUp(agentType == EnviromenentType.AGENT_2 ? KeyCode.DownArrow : KeyCode.S)) {
            currentAction = ActionState.NONE;
        }

        this.currentAction = currentAction;
    }
}
