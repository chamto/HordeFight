using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Test_ChangeSprite : MonoBehaviour
{
    private SpriteRenderer m_SpriteRenderer;
    private Rect buttonPos1;
    private Rect buttonPos2;

    void Start()
    {
        m_SpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        buttonPos1 = new Rect(10.0f, 10.0f, 200.0f, 30.0f);
        buttonPos2 = new Rect(10.0f, 50.0f, 200.0f, 30.0f);
    }

    private void OnDrawGizmos()
    {
        if (null == m_SpriteRenderer) return;

        int vidx = 0;
        Vector2[] vertices = m_SpriteRenderer.sprite.vertices;
        foreach (Vector2 vv in vertices)
        {
            DebugWide.PrintText(vv, Color.black, "v " + vidx);
            vidx++;
        }
    }


    void OnGUI()
    {
        //Press this Button to show the sprite triangles (in the Scene tab)
        if (GUI.Button(buttonPos1, "Draw Debug"))
            DrawDebug();
        //Press this Button to edit the vertices obtained from the Sprite
        if (GUI.Button(buttonPos2, "Perform OverrideGeometry"))
            ChangeSprite();
        
    }

    // Show the sprite triangles
    void DrawDebug()
    {
        Sprite sprite = m_SpriteRenderer.sprite;

        ushort[] triangles = sprite.triangles;
        Vector2[] vertices = sprite.vertices;
        int a, b, c;

        // draw the triangles using grabbed vertices
        for (int i = 0; i < triangles.Length; i = i + 3)
        {
            a = triangles[i];
            b = triangles[i + 1];
            c = triangles[i + 2];

            //To see these you must view the game in the Scene tab while in Play mode
            Debug.DrawLine(vertices[a], vertices[b], Color.red, 60.0f); //초단위 지속시간 
            Debug.DrawLine(vertices[b], vertices[c], Color.red, 60.0f);
            Debug.DrawLine(vertices[c], vertices[a], Color.red, 60.0f);


        }
        string temp = "";
        foreach(ushort aa in triangles)
        {
            temp += " ," + aa;
        }
        DebugWide.LogBlue(temp);

        int vidx = 0;
        foreach (Vector2 aa in vertices)
        {
            DebugWide.LogBlue(vidx + " " + aa);
            vidx++;
        }

    }

    // Edit the vertices obtained from the sprite.  Use OverrideGeometry to
    // submit the changes.
    void ChangeSprite()
    {
        //Fetch the Sprite and vertices from the SpriteRenderer
        Sprite sprite = m_SpriteRenderer.sprite;
        Vector2[] spriteVertices = sprite.vertices;


        for (int i = 0; i < spriteVertices.Length; i++)
        {
            spriteVertices[i].x = Mathf.Clamp(
                (sprite.vertices[i].x - sprite.bounds.center.x -
                    (sprite.textureRectOffset.x / sprite.texture.width) + sprite.bounds.extents.x) /
                (2.0f * sprite.bounds.extents.x) * sprite.rect.width,
                0.0f, sprite.rect.width);

            spriteVertices[i].y = Mathf.Clamp(
                (sprite.vertices[i].y - sprite.bounds.center.y -
                    (sprite.textureRectOffset.y / sprite.texture.height) + sprite.bounds.extents.y) /
                (2.0f * sprite.bounds.extents.y) * sprite.rect.height,
                0.0f, sprite.rect.height);

            // Make a small change to the second vertex
            if (i == 2)
            {
                //Make sure the vertices stay inside the Sprite rectangle
                if (spriteVertices[i].x < sprite.rect.size.x - 5)
                {
                    spriteVertices[i].x = spriteVertices[i].x + 5;
                }
                else spriteVertices[i].x = 0;
            }
        }
        //Override the geometry with the new vertices
        sprite.OverrideGeometry(spriteVertices, sprite.triangles);

    }
}
