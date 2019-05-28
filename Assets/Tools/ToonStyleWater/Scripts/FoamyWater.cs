using System;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WaterFoam
{

    [ExecuteInEditMode]
    public class FoamyWater : MonoBehaviour, IPEGI, ICfg
    {

        #region Variables

        public Texture2D foamTexture;
        public float MyTime = 0;
        public bool stopTime = false;
        public bool modifyProjectSettings = false;

        // Linked Lerp class allows all the values to transition to a new configuration during the same interval
        LinkedLerp.ColorValue shadowColor = new LinkedLerp.ColorValue("Shadow Color");
        LinkedLerp.ColorValue fogColor = new LinkedLerp.ColorValue("Fog Color");
        LinkedLerp.ColorValue skyColor = new LinkedLerp.ColorValue("Sky Color");
        LinkedLerp.FloatValue _thickness = new LinkedLerp.FloatValue("Thickness", 80, 200, 5, 300);
        LinkedLerp.FloatValue _simulationSpeed = new LinkedLerp.FloatValue("Simulation Speed", 1, 40, 0.01f, 10);
        LinkedLerp.FloatValue _noise = new LinkedLerp.FloatValue("Noise", 0.4f);
        LinkedLerp.FloatValue upscale = new LinkedLerp.FloatValue("Scale", 1, 30, 1, 64);
        LinkedLerp.FloatValue shadowStrength = new LinkedLerp.FloatValue("Shadow Strength", 1);
        LinkedLerp.FloatValue shadowDistance = new LinkedLerp.FloatValue("Shadow Distance", 100, 500, 10, 1000);
        LinkedLerp.FloatValue fogDistance = new LinkedLerp.FloatValue("Fog Distance", 100, 500, 0.01f, 1000);
        LinkedLerp.FloatValue fogDensity = new LinkedLerp.FloatValue("Fog Density", 0.01f, 0.01f, 0.00001f, 0.1f);

        // Shader PRoperty class holds the ID of the parameter to set it faster, which is good for performance
        ShaderProperty.VectorValue foamTimeAndPosition = new ShaderProperty.VectorValue("_foamParams");
        ShaderProperty.VectorValue foamDynamics = new ShaderProperty.VectorValue("_foamDynamics");
        ShaderProperty.ColorValue shadowColorProperty = new ShaderProperty.ColorValue("_ShadowColor");
        ShaderProperty.TextureValue foamTextureProperty = new ShaderProperty.TextureValue("_Foam");

        #endregion

        #region Configurations

        private WaterConfig activeConfig;

        public FoamyWaterConfigs configs;

        WaterConfig EditConfiguration(WaterConfig val) {

            if (val == activeConfig)
                pegi.SetBgColor(Color.green);

            if (!val.data.IsNullOrEmpty())
            {
                if (icon.Play.Click(val.data))
                {
                    Decode(val.data);
                    activeConfig = val;
                }
            }
            else if (icon.SaveAsNew.Click())
                val.data = Encode().ToString();

            pegi.edit(ref val.name);

            if (activeConfig == null || activeConfig == val)
            {
                if (icon.Save.Click())
                    val.data = Encode().ToString();
            }
            else if (!val.data.IsNullOrEmpty() && icon.Delete.Click())
                val.data = null;

            pegi.RestoreBGcolor();

            return val;
        }

        #endregion

        // Inspect (IPEGI interface) allows you too customize

        private int inspectedProperty = -1;

        public bool Inspect()
        {

            bool changed = false;

            bool notInspectingProperty = inspectedProperty == -1;

            _thickness.enter_Inspect_AsList(ref inspectedProperty, 0).nl(ref changed);

            if (notInspectingProperty)
                "Noise:".edit(ref _noise.targetValue, 0, 2).nl(ref changed);

            upscale.enter_Inspect_AsList(ref inspectedProperty, 1).nl(ref changed);
            _simulationSpeed.enter_Inspect_AsList(ref inspectedProperty, 2).nl(ref changed);

            if (notInspectingProperty)
            {
                "Control Project Settings".toggleIcon(ref modifyProjectSettings).nl();

                "Shadow:".write(PEGI_Styles.ListLabel);
                pegi.nl();
                "_color:".edit(60, ref shadowColor.targetValue).nl(ref changed);
            }


            if (modifyProjectSettings)
            {
                shadowDistance.enter_Inspect_AsList(ref inspectedProperty, 3).nl(ref changed);

                bool fog = RenderSettings.fog;

                if (notInspectingProperty && "Fog (Recommended)".toggleIcon(ref fog, true).changes(ref changed))
                        RenderSettings.fog = fog;
                

                if (fog)
                {
                    var fogMode = RenderSettings.fogMode;
                    
                    if (notInspectingProperty)
                    {
                        "Fog Color".edit(60, ref fogColor.targetValue).nl();
                        
                        if ("Fog Mode".editEnum(ref fogMode).nl())
                            RenderSettings.fogMode = fogMode;
                    }
                    
                    if (fogMode == FogMode.Linear)
                        fogDistance.enter_Inspect_AsList(ref inspectedProperty, 4).nl(ref changed);
                    else
                        fogDensity.enter_Inspect_AsList(ref inspectedProperty, 5).nl(ref changed);
                }

                if (notInspectingProperty)
                    "Sky Color".edit(60, ref skyColor.targetValue).nl(ref changed);

            }

            pegi.nl();

            if (notInspectingProperty)
            {
                "Water foam mask".edit(ref foamTexture).nl(ref changed);

                if (configs)
                {
                    if (icon.UnLinked.Click("Disconnect config"))
                        configs = null;
                    else
                        "Configurations".edit_List(ref configs.configurations, EditConfiguration).changes(ref changed);
                }
                else
                {
                    "Configs".edit(90, ref configs);

                    if (icon.Create.Click("Create new Config"))
                        configs = UnityUtils.CreateScriptableObjectAsset<FoamyWaterConfigs, FoamyWaterConfigs>(
                            "Tools/ToonStyleWater", "Water Config");

                    pegi.nl();
                }

            }


            if (Application.isPlaying)
            {
                "Changes will not be saved when exiting play mode".writeOneTimeHint("FoamCfgChangeLoss");

                if (ld.linkedPortion < 1)
                {
                    "Lerping {0}".F(ld.dominantParameter).write();
                    ("Each parameter has a transition speed. THis text shows which parameter sets speed for others (the slowest one). " +
                     "If Transition is too slow, increase this parameter's speed").fullWindowDocumentationClick();
                    pegi.nl();
                }
            }

            if (changed)
            {
                UpdateShaderProperties();
#if UNITY_EDITOR
                if (Application.isPlaying == false)
                {
                    SceneView.RepaintAll();
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
#endif
            }

            return changed;
        }

        #region Encode & Decode

        // Encode and Decode class lets you store configuration of this class in a string 

        public CfgEncoder Encode()
        {
            var cody = new CfgEncoder()
                .Add_Bool("s", stopTime)
                .Add("thl", _thickness)
                .Add("spd", _simulationSpeed)
                .Add("ns", _noise.targetValue)
                .Add("sh", shadowStrength.targetValue)
                .Add("scol", shadowColor.targetValue)
                .Add("sdst", shadowDistance)
                .Add("us", upscale)
                .Add("sc", skyColor.targetValue)
                .Add_Bool("fg", RenderSettings.fog);

            if (RenderSettings.fog)
                cody.Add("fogCol", fogColor.targetValue)
                    .Add("fogD", fogDistance)
                    .Add("fogDen", fogDensity);

            return cody;
        }

        public void Decode(string data) => new CfgDecoder(data).DecodeTagsFor(this);

        public bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "s": stopTime = data.ToBool();  break;
                case "thl": _thickness.Decode(data);  break;
                case "spd": _simulationSpeed.Decode(data); break;
                case "ns": _noise.targetValue = data.ToFloat(); break;
                case "sh": shadowStrength.targetValue = data.ToFloat(); break;
                case "scol": shadowColor.targetValue = data.ToColor(); break;
                case "sdst": shadowDistance.Decode(data); break;
                case "us": upscale.Decode(data); break;
                case "sc": skyColor.targetValue = data.ToColor(); break;
                case "fg": if (modifyProjectSettings) RenderSettings.fog = data.ToBool(); break;
                case "fogD": fogDistance.Decode(data); break;
                case "fogDen": fogDensity.Decode(data); break;
                case "fogCol": fogColor.targetValue = data.ToColor(); break;
                default: return false;
            }

            return true;
        }

        #endregion

        public void UpdateShaderProperties()
        {
            foamDynamics.GlobalValue = new Vector4(_thickness.Value, _noise.Value, upscale.Value, 300 - _thickness.Value);
            shadowColorProperty.GlobalValue = shadowColor.Value;
        }

        private void OnEnable()
        {

            foamTextureProperty.GlobalValue = foamTexture;
            UpdateShaderProperties();
        }

        LerpData ld = new LerpData();

        

        void Update()
        {

            if (!stopTime)
                MyTime += Time.deltaTime * _simulationSpeed.Value;

            foamTimeAndPosition.GlobalValue = new Vector4(MyTime, MyTime * 0.6f, transform.position.y, 0);

            // Reset portion to be 1
            ld.Reset();

            // Find slowest property
            shadowColor.Portion(ld);
            _thickness.Portion(ld);
            _simulationSpeed.Portion(ld);
            _noise.Portion(ld);
            upscale.Portion(ld);
            shadowStrength.Portion(ld);
            shadowDistance.Portion(ld);
            fogColor.Portion(ld);
            skyColor.Portion(ld);
            fogDensity.Portion(ld);
            fogDistance.Portion(ld);

            // Lerp all the properties
            shadowColor.Lerp(ld);
            _thickness.Lerp(ld);
            _simulationSpeed.Lerp(ld);
            _noise.Lerp(ld);
            upscale.Lerp(ld);
            shadowStrength.Lerp(ld);
            shadowDistance.Lerp(ld);
            fogColor.Lerp(ld);
            skyColor.Lerp(ld);
            fogDensity.Lerp(ld);
            fogDistance.Lerp(ld);


            if (modifyProjectSettings) {
                RenderSettings.fogColor = fogColor.Value;

                if (RenderSettings.fog)
                {
                    RenderSettings.fogEndDistance = fogDistance.Value;
                    RenderSettings.fogDensity = fogDensity.Value;
                }



                //RenderSettings. = fogDistance.Value;
                RenderSettings.ambientSkyColor = skyColor.Value;
                QualitySettings.shadowDistance = shadowDistance.Value;
            }

            UpdateShaderProperties();

        }
    }
}