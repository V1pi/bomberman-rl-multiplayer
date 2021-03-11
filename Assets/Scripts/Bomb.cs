using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {
	[SerializeField] private BombType type;
	[SerializeField] private float countdown = 2f;
	private float timer;
	AgentController bomber;
	GameController gameController;

    private void Start() {
		gameController = this.transform.parent.GetComponentInChildren<GameController>();
		timer = countdown;
	}

    void Update() {
		timer -= Time.deltaTime;

		if (timer <= 0f) {
			this.bomber.RecoverBomb();
			gameController.ExplodeBomb(transform.position, type, bomber);
			this.gameObject.SetActive(false);
		}
	}

	public void ResetConfigs() {
		timer = countdown;
	}

	public void SetConfigs(AgentController bomber) {
		this.bomber = bomber;
    }

	public void SetCountdown(float countdown) {
		this.timer = countdown;
    }
}
