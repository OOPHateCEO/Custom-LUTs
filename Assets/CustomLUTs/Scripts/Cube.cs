using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : ScriptableObject
{
    //public Texture2D tex;
    //public RenderTexture tex;
    public string lutName;
    public List<Color> colorValues;
    public ComputeBuffer valuesBuffer;
    public int size;
}