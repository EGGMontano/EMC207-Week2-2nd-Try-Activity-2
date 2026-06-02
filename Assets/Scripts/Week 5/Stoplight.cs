using UnityEngine;

public class Stoplight : MonoBehaviour
{
    public bool isGreen = true;
    public float greenTime = 5f;
    public float redTime = 5f;

    private float timer = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (isGreen && timer >= greenTime)
        {
            isGreen = false;
            timer = 0f;
            Debug.Log("GREEN LIGHT");
        }
        else if (!isGreen && timer >= redTime)
        {
            isGreen = true;
            timer = 0f;
            Debug.Log("RED LIGHT");
        }
    }
}
