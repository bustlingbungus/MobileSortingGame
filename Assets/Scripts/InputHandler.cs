using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public struct PlayerInput
{
    public PlayerInput(Vector2 screenPosition)
    {
        position = screenPosition;    
    }

    public Vector2 position;
}


public class InputHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputs = new List<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        inputs.Clear();

        // @TODO sophisticate this
        HandleMobileInputs();
        HandlePCInput();
    }


    


    public List<PlayerInput> inputs;



    void HandleMobileInputs()
    {
        foreach (Touch touch in Input.touches)
        {
            inputs.Add(new PlayerInput(touch.position));
        }
    }


    void HandlePCInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Input.mousePosition;
            inputs.Add(new PlayerInput(pos));
        }
    }


    public static bool ScreenPositionCollidesWithObject(Vector2 screenPoint, GameObject obj)
    {
        Collider2D col = obj.GetComponent<Collider2D>();
        if (col == null) return false;
        Vector2 point = Camera.main.ScreenToWorldPoint(screenPoint);
        return col.OverlapPoint(point);
    }

}
