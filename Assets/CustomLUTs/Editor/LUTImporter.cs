using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

[ScriptedImporter(1, "cube")]
public class LUTImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        Cube cube = ScriptableObject.CreateInstance<Cube>();

        string regex = "^[0-1].[0-9]* [0-1].[0-9]* [0-1].[0-9]*";
        StreamReader SR = new StreamReader(ctx.assetPath);

        cube.colorValues =  new List<Color>();

        while(!SR.EndOfStream)
        {
            string ln = SR.ReadLine();
            if(Regex.Match(ln, regex).Success)
            {
                string[] coords = new string[3];
                coords  = ln.Split(' ');
                cube.colorValues.Add(new Color(
                    float.Parse(coords[0]),
                    float.Parse(coords[1]),
                    float.Parse(coords[2])));
            }
        }
        SR.Close();


        float _cuberoot = Mathf.Pow( cube.colorValues.Count, 1f/3f);
        
        if(_cuberoot % 1 > 0.00001f)
        {
            Debug.LogError("Unable to parse LUT, possibly incorrect format.");
            return;
        }

        int LUTSize = (int)_cuberoot;
        cube.lutName = Path.GetFileName(ctx.assetPath);
        cube.size = LUTSize;

        ctx.AddObjectToAsset("LUT", cube);
    }
}

