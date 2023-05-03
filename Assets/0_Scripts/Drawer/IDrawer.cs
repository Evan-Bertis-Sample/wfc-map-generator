using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDrawer
{
    // Modifies a texture via reference
    // Does not apply changes the texture, the user is expected to called the Texture2D.Apply() method afterr modifying a texture
    public void Draw(Texture2D tex);
}
