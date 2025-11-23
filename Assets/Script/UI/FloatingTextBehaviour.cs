using TMPro;
using UnityEngine;

public class FloatingTextBehaviour : MonoBehaviour
{
    private TMP_Text textObj;
    private float timer;
    private float riseSpeed;
    private float fadeSpeed;
    private float lifetime;
    private Color originalColor;

    public void Setup(float rise, float fade, float time)
    {
        textObj = GetComponent<TMP_Text>();
        riseSpeed = rise;
        fadeSpeed = fade;
        lifetime = time;
        originalColor = textObj.color;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Move up
        transform.localPosition += Vector3.up * riseSpeed * Time.deltaTime;

        // Fade out
        float alpha = Mathf.Lerp(originalColor.a, 0, timer / lifetime);
        textObj.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

        // Destroy after lifetime
        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
