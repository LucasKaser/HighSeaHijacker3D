using UnityEditor;
using PlayerAndEditorGUI;
using Playtime_Painter.Examples;

#if UNITY_EDITOR

namespace WaterFoam
{

    [CustomEditor(typeof(FoamyWater))]
    public class FoamyWaterDrawer : PEGI_Inspector<FoamyWater>
    {
    }

    [CustomEditor(typeof(GodMode))]
    public class GodModeDrawer : PEGI_Inspector<GodMode>
    {
    }


}
#endif