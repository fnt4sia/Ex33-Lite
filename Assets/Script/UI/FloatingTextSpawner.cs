using TMPro;
using UnityEngine;

public class FloatingTextSpawner : MonoBehaviour
{
    [Header("Setup")]
    public TMP_Text prefab;         
    public Rect spawnArea;          
    public Transform transformParent; 

    [Header("Text Behavior")]
    public float riseSpeed = 30f;   
    public float fadeSpeed = 1f;    
    public float lifetime = 1f;    

    public void SpawnText(string message)
    {
        Vector2 randomPos = new Vector2(
            Random.Range(spawnArea.xMin, spawnArea.xMax),
            Random.Range(spawnArea.yMin, spawnArea.yMax)
        );

        TMP_Text text = Instantiate(prefab, transformParent);
        text.text = message;
        text.rectTransform.anchoredPosition = randomPos;

        FloatingTextBehaviour behaviour = text.gameObject.AddComponent<FloatingTextBehaviour>();
        behaviour.Setup(riseSpeed, fadeSpeed, lifetime);
    }
}
