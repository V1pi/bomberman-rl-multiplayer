using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {
    GameController gameController;
    Vector3Int cellPos;
    public void DestroyExplosion() {
        gameController.RemoveExplosion(cellPos);
        this.gameObject.SetActive(false);
    }

    public void SetConfigs(GameController gameController, Vector3Int cellPos) {
        this.gameController = gameController;
        this.cellPos = cellPos;
    }
}
