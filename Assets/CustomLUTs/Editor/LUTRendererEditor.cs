using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// [ExecuteAlways]
// [CustomEditor(typeof(LUTRenderer))]
// public class LUTRendererEditor : Editor
// {    
//     LUTRenderer LR;
//     public override void OnInspectorGUI()
//     {
//         //if(LR == null)
//         LR = (LUTRenderer)target;

//         EditorGUI.BeginChangeCheck();
//         LR.Blend = EditorGUILayout.Slider("Blend", LR.Blend,0,1);
//         if(EditorGUI.EndChangeCheck())
//         {
//             LR.UpdateBlend();
//         }


//         EditorGUI.BeginChangeCheck();
//         LR.LUT = EditorGUILayout.ObjectField(LR.LUT, typeof(Cube), false) as Cube;
//         if(EditorGUI.EndChangeCheck())
//         {
//             LR.reInitialize();
//         }
//     }
// }
