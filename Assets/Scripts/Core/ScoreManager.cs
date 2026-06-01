using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
	public Text scoreText;
	public Text finalScoreText;
	public GameSpeedController speedController;
	public float pointsPerSecond = 10f;
	public int scoreStep = 100;
	public int zeroPadding = 5;
	public string highScoreKey = "DinoRunner3DHighScore";

	private float score;
	private int lastStepScore;
	private float flashTimer;
	private bool newBestPlayed;

	public int Score
	{
		get { return Mathf.FloorToInt(score); }
	}

	public int BestScore { get; private set; }

	private void Awake()
	{
		BestScore = PlayerPrefs.GetInt(highScoreKey, 0);
	}

	public void ResetScore()
	{
		score = 0f;
		lastStepScore = 0;
		flashTimer = 0f;
		newBestPlayed = false;
		BestScore = PlayerPrefs.GetInt(highScoreKey, 0);
		UpdateScoreText();
	}

	public void Tick(float deltaTime, float currentSpeed)
	{
		score += deltaTime * pointsPerSecond;

		if (Score > BestScore)
		{
			BestScore = Score;
			PlayerPrefs.SetInt(highScoreKey, BestScore);

			if (!newBestPlayed && GameManager.Instance != null)
			{
				GameManager.Instance.PlayScoreSound();
				newBestPlayed = true;
			}
		}

		if (Score > 0 && Score / scoreStep > lastStepScore / scoreStep)
		{
			lastStepScore = Score;
			flashTimer = 1f;

			if (speedController != null)
			{
				speedController.BumpForScoreStep();
			}

			if (GameManager.Instance != null)
			{
				GameManager.Instance.PlayScoreSound();
			}
		}

		if (flashTimer > 0f)
		{
			flashTimer -= deltaTime;
		}

		UpdateScoreText();
	}

	public void FinishScore()
	{
		if (Score > BestScore)
		{
			BestScore = Score;
			PlayerPrefs.SetInt(highScoreKey, BestScore);
			PlayerPrefs.Save();
		}

		if (finalScoreText != null)
		{
			finalScoreText.text =
				"GAME OVER\n\n" +
				"Score: " + FormatScore(Score) + "\n" +
				"High Score: " + FormatScore(BestScore) + "\n\n" +
				"Press R to Restart";
		}
	}

	private void UpdateScoreText()
	{
		if (scoreText != null)
		{
			string scoreColor = flashTimer > 0f ? "#FFF4D0" : "#3E2F1E";
			scoreText.text =
				"<color=" + scoreColor + ">SCORE " + FormatScore(Score) + "</color>\n" +
				"<size=20><color=#8A673B>HIGH " + FormatScore(BestScore) + "</color></size>";
		}
	}

	private string FormatScore(int value)
	{
		return Mathf.Max(0, value).ToString().PadLeft(zeroPadding, '0');
	}
}
