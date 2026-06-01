using UnityEngine;

public class GameSpeedController : MonoBehaviour
{
	[Header("Speed")]
	public float startSpeed = 15f;
	public float speedIncreaseRate = 0.12f;
	public float scoreStepIncrease = 1f;
	public float maxSpeed = 35f;

	public float CurrentSpeed { get; private set; }
	public int SpeedLevel { get; private set; }

	private void Awake()
	{
		ResetSpeed();
	}

	public void ResetSpeed()
	{
		CurrentSpeed = startSpeed;
		UpdateSpeedLevel();
	}

	public void Tick(float deltaTime)
	{
		CurrentSpeed = Mathf.Min(maxSpeed, CurrentSpeed + speedIncreaseRate * deltaTime);
		UpdateSpeedLevel();
	}

	public void BumpForScoreStep()
	{
		CurrentSpeed = Mathf.Min(maxSpeed, CurrentSpeed + scoreStepIncrease);
		UpdateSpeedLevel();
	}

	private void UpdateSpeedLevel()
	{
		if (CurrentSpeed >= 30f)
		{
			SpeedLevel = 4;
		}
		else if (CurrentSpeed >= 20f)
		{
			SpeedLevel = 3;
		}
		else if (CurrentSpeed >= 10f)
		{
			SpeedLevel = 2;
		}
		else
		{
			SpeedLevel = 1;
		}
	}
}
