using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int CalculateFinalScore(int goldCount)
    {
        //Beta verzióhoz egy egyszerû változat, késõbb bõvítve lesz
        int basePoints = goldCount * 5;
        return basePoints;
    }
}