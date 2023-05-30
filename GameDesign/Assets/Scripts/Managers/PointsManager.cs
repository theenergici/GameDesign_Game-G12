using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsManager : MonoBehaviour
{
    private static PointsManager instance;

    private float maxPoints = 0;
    public float pointsPercentage{ get {
        if(maxPoints>0)
            return currentPoints / maxPoints; 
        else return 0;
        }
    }
    private int currentPoints;
    public bool IsNegative { get
        {
            return instance.currentPoints < 0;
        }
    }

    public static PointsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PointsManager>();
                if (instance == null)
                {
                    GameObject pointsObject = new GameObject(typeof(PointsManager).Name);
                    instance = pointsObject.AddComponent<PointsManager>();
                    instance.currentPoints = 0;
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (PointsManager.Instance != null && PointsManager.Instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;

            calculateMaxPoints();
        }
    }
    public void TriggerEvent_IncrementPoints(int points)
    {
        instance.currentPoints += points;
        Debug.Log($"Increasing points {points}, now is {instance.currentPoints}");
    }

    public void TriggerEvent_ResetPoints()
    {
        calculateMaxPoints();
        instance.currentPoints = 0;
    }

    public int GetCurrentPoints()
    {
        return instance.currentPoints;
    }

    private void calculateMaxPoints() {

        maxPoints = LevelSettingManager.Instance.getInitialMaxPoints();
    }

}