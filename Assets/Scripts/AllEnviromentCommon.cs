using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BombType {
    NORMAL
}
[System.Serializable]
public struct BombConfig {
    [Range(0f, 1f)]
    public float timeBeteweenExplosions;
    [Range(1, 10)]
    public int radius;
    public BombType type;
}

public class AllEnviromentCommon : MonoBehaviour {
    private static AllEnviromentCommon _instance;
    [SerializeField] private BombConfig[] bombsConfigs;

    public static AllEnviromentCommon Instance { get { return _instance; } }
    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    public BombConfig GetBombConfigs(BombType type) {
        foreach(var bomb in bombsConfigs) {
            if(bomb.type == type) {
                return bomb;
            }
        }
        return new BombConfig();
    } 
}
