using UnityEngine;

public class CountScore : MonoBehaviour
{
    public TMPro.TMP_Text scoreText;
    public int score = 0;
    public float pointsPerSecond = 2f;

    private float timer = 0f;
    private bool isCounting = false;

    void Update()
    {
        if (!isCounting) return;

        timer += Time.deltaTime;

        if (timer >= 1f / pointsPerSecond)
        {
            score += 1;
            timer = 0f;
            scoreText.text = "Score: " + score;
        }
    }

    public void StartCounting()
    {
        isCounting = true;
    }

    public void StopCounting()
    {
        isCounting = false;
    }
}
