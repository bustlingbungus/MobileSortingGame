using System;
using System.Collections.Generic;
using UnityEngine;

public class TemplateImage : MonoBehaviour
{
    [SerializeField]
    float EnlargedScreenCoverage = 0.9f;

    [SerializeField]
    int MinimizedHeight = 150;


    [SerializeField]
    Vector2 EnglargedPosition = new Vector2(0f, 0f);
    [SerializeField]
    Vector2 MinimizedPosition = new Vector2(0f, -3f);


    Vector2 PreviousScale = Vector3.zero;
    Vector2 TargetScale = Vector3.zero;

    public string ImagePath = "Assets/Images/Mona Lisa.png";
    SpriteRenderer spriteRenderer;
    Sprite sprite;


    InputHandler input;





    enum State
    {
        Start,
        Pixelation,
        EnlargedView,
        Minimized
    }


    [SerializeField]
    float StartViewTimer = 1f;
    float start_timer;

    [SerializeField]
    int maxTiles = 5;
    int init_res;
    Texture2D init_tex = null;


    [SerializeField]
    float PixelationTime = 0.5f;
    [SerializeField]
    float PostPixelViewTime = 0.5f;
    float pixel_time;
    [SerializeField]
    float ViewTransitionTimer = 0.25f;
    float view_timer;

    State viewState = State.Start;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // attempt to get sprite reference
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            sprite = ImageToTexture.LoadSprite(ImagePath);
            spriteRenderer.sprite = sprite;
        }
        else
        {
            Debug.LogError("Could not find sprite renderer on template image!");
            Destroy(gameObject);
            return;
        }


        GameObject ih = GameObject.FindGameObjectWithTag("InputHandler");
        if (ih != null) input = ih.GetComponent<InputHandler>();

        EnterState(State.Start);
    }

    // Update is called once per frame
    void Update()
    {
        switch (viewState)
        {
            case State.Start:
                StartUpdate();
                break;
            case State.Pixelation:
                PixelUpdate();
                break;
            case State.Minimized:
                MinimizedUpdate();
                break;
            case State.EnlargedView:
                MaximizedUpdate();
                break;
        }
    }



    Vector2 GetTargetScale(State state)
    {
        float sc;

        if (state == State.Minimized) sc = (float)MinimizedHeight / (float)sprite.texture.height;
        else
        {
            if (sprite.texture.width > sprite.texture.height) sc = (Screen.width * EnlargedScreenCoverage) / sprite.texture.width;
            else sc = (Screen.height * EnlargedScreenCoverage) / sprite.texture.height;
        }

        return new Vector2(sc, sc);
    }



    void EnterState(State state)
    {
        

        switch (state)
        {
            case State.Start:
                start_timer = StartViewTimer;
                transform.position = EnglargedPosition;
                transform.localScale = GetTargetScale(state);
                break;
            case State.Pixelation:
                pixel_time = PixelationTime;
                init_res = Math.Max(sprite.texture.width, sprite.texture.height);
                init_tex = sprite.texture;
                break;
            case State.Minimized:
                PreviousScale = GetTargetScale(viewState);
                TargetScale = GetTargetScale(state);
                view_timer = ViewTransitionTimer;
                break;
            case State.EnlargedView:
                PreviousScale = GetTargetScale(viewState);
                TargetScale = GetTargetScale(state);
                view_timer = ViewTransitionTimer;
                break;
        }

        viewState = state;
    }


    void StartUpdate()
    {
        if (start_timer <= 0f) EnterState(State.Pixelation);
        else start_timer -= Time.deltaTime;
    }

    void PixelUpdate()
    {
        float t = pixel_time / PixelationTime;
        t = Mathf.Lerp(0f, 0.05f, t);
        float tiles = Mathf.Lerp((float)init_res, (float)maxTiles, 1f - t);
        float downscale_ratio = tiles / (float)init_res;

        Texture2D down_tex = ImageToTexture.DownscaleSpriteTexture(init_tex, downscale_ratio);
        if (down_tex != null)
        {
            sprite = Sprite.Create(down_tex, new Rect(0, 0, down_tex.width, down_tex.height), new Vector2(0.5f, 0.5f));
            spriteRenderer.sprite = sprite;
        }

        if (pixel_time <= -PostPixelViewTime) EnterState(State.Minimized);
        else pixel_time = Mathf.Max(-PostPixelViewTime, pixel_time - Time.deltaTime);
    }

    void MinimizedUpdate()
    {
        view_timer = Mathf.Max(0f, view_timer - Time.deltaTime);
        float t = 1f - (view_timer / ViewTransitionTimer);
        
        // lerp to position
        Vector3 newPos = Vector3.Lerp(EnglargedPosition, MinimizedPosition, t);
        transform.position = newPos;

        // lerp to scale
        Vector3 newScale = Vector3.Lerp(PreviousScale, TargetScale, t);
        transform.localScale = newScale;


        // handle input
        if (CheckIfClicked()) EnterState(State.EnlargedView);
    }


    void MaximizedUpdate()
    {
        view_timer = Mathf.Max(0f, view_timer - Time.deltaTime);
        float t = 1f - (view_timer / ViewTransitionTimer);
        
        // lerp to position
        Vector3 newPos = Vector3.Lerp(MinimizedPosition, EnglargedPosition, t);
        transform.position = newPos;

        // lerp to scale
        Vector3 newScale = Vector3.Lerp(PreviousScale, TargetScale, t);
        transform.localScale = newScale;

        if (CheckIfClicked()) EnterState(State.Minimized);
    }


    bool CheckIfClicked()
    {
        bool clicked = false;

        if (input != null)
        {
            // Debug.Log(input.inputs.Count);
            foreach (PlayerInput cmd in input.inputs)
            {
                if (InputHandler.ScreenPositionCollidesWithObject(cmd.position, gameObject))
                {
                    Debug.Log(cmd.position);
                    clicked = !clicked;
                }
            }
        }

        return clicked;
    }
}
