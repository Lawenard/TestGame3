using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings", order = 1)]
public class GameSettings : ScriptableObject
{
    public Vector3 partMaxScale;
    public float
        growSpeed, errorMargin,
        initialSize, minSize,
        perfectMargin, perfectDelay,
        perfectGrow, perfectShrink,
        perfectTowerGrow, perfectTowerShrink,
        errorShowTime;
}
