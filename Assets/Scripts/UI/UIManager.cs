using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public Text scoreText;
	public Text gameOverText;
	public Text restartHintText;
	public GameObject gameOverPanel;

	public void ShowHud()
	{
		if (gameOverPanel != null)
		{
			gameOverPanel.SetActive(false);
		}
	}

	public void ShowGameOver(int score, int bestScore)
	{
		if (gameOverPanel != null)
		{
			gameOverPanel.SetActive(true);
		}

		if (gameOverText != null)
		{
			gameOverText.text =
				"GAME OVER\n\n" +
				"Score: " + score.ToString().PadLeft(5, '0') + "\n" +
				"High Score: " + bestScore.ToString().PadLeft(5, '0') + "\n\n" +
				"Press R to Restart";
		}

		if (restartHintText != null)
		{
			restartHintText.text = "Restart";
		}
	}
}
