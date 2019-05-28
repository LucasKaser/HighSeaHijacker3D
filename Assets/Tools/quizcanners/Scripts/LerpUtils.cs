﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace QuizCannersUtilities
{

    public interface ILinkedLerping
    {
        void Portion(LerpData ld);
        void Lerp(LerpData ld, bool canTeleport);
    }

    public class LerpData : IPEGI, IGotName, IGotCount, IPEGI_ListInspect
    {
        public float linkedPortion = 1;
        public float teleportPortion;
        private float _minPortion = 1;
        private int _resets;
        public string dominantParameter = "None";

        public float Portion(bool canTeleport = false) =>
            canTeleport ? Mathf.Max(teleportPortion, linkedPortion) : linkedPortion;

        public float MinPortion
        {
            get { return Mathf.Min(_minPortion, linkedPortion); }
            set { _minPortion = Mathf.Min(_minPortion, value); }
        }

        public int CountForInspector => _resets;

        public string NameForPEGI
        {
            get { return dominantParameter; }
            set { dominantParameter = value; }
        }

        public void Reset()
        {
            teleportPortion = 0;
            linkedPortion = 1;
            _minPortion = 1;
            _resets++;
        }

        #region Inspector
        
        public bool Inspect()
        {
            var changed = false;

            "Dominant Parameter".edit(ref dominantParameter).nl(ref changed);

            "Reboot calls".edit(ref _resets).nl(ref changed);

            "teleport portion: {0}".F(teleportPortion).nl();

            "min Portion {0} ".F(_minPortion).nl();

            return changed;
        }

        public bool PEGI_inList(IList list, int ind, ref int edited)
        {
            "Lerp DP: {0} [{1}]".F(dominantParameter, _resets).write();

            if (icon.Refresh.Click("Reset stats"))
            {
                dominantParameter = "None";
                _resets = 0;
            }

            if (icon.Enter.Click())
                edited = ind;

            return false;
        }

        #endregion
    }

    public interface IManageFading
    {
        void FadeAway();
        bool TryFadeIn();
    }

    public class LinkedLerp
    {

        public enum LerpSpeedMode
        {
            SpeedThreshold = 0,
            Unlimited = 1,
            LerpDisabled = 2,
            UnlinkedSpeed = 3
        }

        #region Abstract Base

        public abstract class BaseAnyValue : AbstractCfg, ILinkedLerping, IPEGI, IPEGI_ListInspect
        {

            public LerpSpeedMode lerpMode = LerpSpeedMode.SpeedThreshold;

            public virtual bool UsingLinkedThreshold =>
                (lerpMode == LerpSpeedMode.SpeedThreshold && Application.isPlaying);

            public virtual bool Enabled => lerpMode != LerpSpeedMode.LerpDisabled;

            protected virtual bool EaseInOutImplemented => false;

            protected bool easeInOut;

            protected bool defaultSet;
            public float speedLimit = 1;
            protected bool allowChangeParameters = true;

            protected abstract string Name { get; }

            #region Encode & Decode

            public override CfgEncoder Encode() {

                var cody = new CfgEncoder()
                    .Add_Bool("ch", allowChangeParameters);

                if (allowChangeParameters) {

                    if (EaseInOutImplemented)
                        cody.Add_Bool("eio", easeInOut);

                    cody.Add("lm", (int) lerpMode);

                    if (lerpMode == LerpSpeedMode.SpeedThreshold)
                        cody.Add("sp", speedLimit);
                }

                return cody;
            }

            public override bool Decode(string tg, string data)
            {
                switch (tg)
                {
                    case "ch":
                        allowChangeParameters = data.ToBool();
                        break;
                    case "sp":
                        speedLimit = data.ToFloat();
                        break;
                    case "lm":
                        lerpMode = (LerpSpeedMode) data.ToInt();
                        break;
                    case "eio":
                        easeInOut = data.ToBool();
                        break;
                    default: return false;
                }

                return true;
            }

            #endregion

            public void Lerp(LerpData ld, bool canTeleport = false)
            {
                if (!Enabled) return;

                float p;

                switch (lerpMode)
                {
                    case LerpSpeedMode.LerpDisabled:
                        p = 0;
                        break;
                    case LerpSpeedMode.UnlinkedSpeed:
                        p = 1;
                        if (Application.isPlaying)
                            Portion(ref p);

                        if (canTeleport)
                            p = Mathf.Max(p, ld.teleportPortion);

                        break;
                    default:
                        p = ld.Portion(canTeleport);
                        break;
                }

                LerpInternal(p);
                defaultSet = true;
            }

            public abstract bool LerpInternal(float linkedPortion);

            public virtual void Portion(LerpData ld)
            {
                if (UsingLinkedThreshold && Portion(ref ld.linkedPortion))
                    ld.dominantParameter = Name;
                else if (lerpMode == LerpSpeedMode.UnlinkedSpeed)
                {
                    float portion = 1;
                    Portion(ref portion);
                    ld.MinPortion = portion;
                }
            }

            public abstract bool Portion(ref float linkedPortion);

            #region Inspector
            
            public virtual bool PEGI_inList(IList list, int ind, ref int edited) {

                var changed = false;
                
                if (!allowChangeParameters)
                    Name.toggleIcon("Will this config contain new parameters", ref allowChangeParameters)
                        .changes(ref changed);
                else
                {
                    if (Application.isPlaying)
                        (Enabled ? icon.Active : icon.InActive).write(Enabled ? "Lerp Possible" : "Lerp Not Possible");

                    switch (lerpMode)
                    {
                        case LerpSpeedMode.SpeedThreshold:
                            (Name + " Thld").edit(170, ref speedLimit).changes(ref changed);
                            break;
                        case LerpSpeedMode.UnlinkedSpeed:
                            (Name + " Speed").edit(170, ref speedLimit).changes(ref changed);
                            break;
                        default:
                            (Name + " Mode").editEnum(120, ref lerpMode).changes(ref changed);
                            break;
                    }
                }

                if (icon.Enter.Click())
                    edited = ind;

                return changed;
            }

            public virtual bool Inspect()
            {

                var changed = "Edit".toggleIcon("Will this config contain new parameters", ref allowChangeParameters)
                    .nl();

                if (!allowChangeParameters) return changed;

                "Lerp Speed Mode ".editEnum(110, ref lerpMode).nl(ref changed);
                if (lerpMode == LerpSpeedMode.SpeedThreshold || lerpMode == LerpSpeedMode.UnlinkedSpeed)
                    "Lerp Speed for {0}".F(Name).edit(150, ref speedLimit).nl(ref changed);

                if (EaseInOutImplemented)
                    "Ease In/Out".toggleIcon(ref easeInOut).nl(ref changed);

                return changed;
            }

            #endregion

        }

        public abstract class BaseVector2Lerp : BaseAnyValue
        {
            public Vector2 targetValue;

            protected override bool EaseInOutImplemented => true;

            private float _easePortion = 0.1f;

            protected abstract Vector2 CurrentValue { get; set; }

            public override bool UsingLinkedThreshold => base.UsingLinkedThreshold && Enabled;

            public override bool LerpInternal(float linkedPortion)
            {
                if (CurrentValue != targetValue || !defaultSet)
                    CurrentValue = Vector2.Lerp(CurrentValue, targetValue, linkedPortion);
                else return false;

                return true;
            }

            public override bool Portion(ref float linkedPortion)
            {

                var magnitude = (CurrentValue - targetValue).magnitude;

                var modSpeed = speedLimit;

                if (easeInOut)
                {
                    _easePortion = Mathf.Lerp(_easePortion, magnitude > speedLimit * 0.5f ? 1 : 0.1f, Time.deltaTime * 2);
                    modSpeed *= _easePortion;
                }

                return modSpeed.SpeedToMinPortion(magnitude, ref linkedPortion);
            }

            #region Inspector

#if PEGI

            public override bool PEGI_inList(IList list, int ind, ref int edited)
            {
                if (base.PEGI_inList(list, ind, ref edited))
                {
                    targetValue = CurrentValue;
                    return true;
                }

                return false;
            }

            public override bool Inspect()
            {
                pegi.nl();

                var changed = false;

                if (base.Inspect().nl(ref changed))
                    targetValue = CurrentValue;

                if (lerpMode != LerpSpeedMode.LerpDisabled)
                    "Target".edit(ref targetValue).nl(ref changed);


                return changed;
            }
            #endif

            #endregion

            #region Encode & Decode

            public override CfgEncoder Encode()
            {
                var cody = new CfgEncoder()
                    .Add("b", base.Encode);
                if (allowChangeParameters)
                    cody.Add("t", CurrentValue);

                return cody;
            }

            public override bool Decode(string tg, string data)
            {
                switch (tg)
                {
                    case "b":
                        data.Decode_Delegate(base.Decode);
                        break;
                    case "t":
                        targetValue = data.ToVector2();
                        break;
                    default: return false;
                }

                return true;
            }

            #endregion

            protected BaseVector2Lerp()
            {
                lerpMode = LerpSpeedMode.LerpDisabled;
            }
        }

        public abstract class BaseFloatLerp : BaseAnyValue, IPEGI_ListInspect
        {

            protected abstract float TargetValue { get; set; }

            public abstract float Value { get; set; }

            protected virtual bool CanLerp => true;

            public override bool LerpInternal(float linkedPortion)
            {
                if (CanLerp && (!defaultSet || Value != TargetValue))
                    Value = Mathf.Lerp(Value, TargetValue, linkedPortion);
                else return false;

                return true;
            }

            public override bool Portion(ref float linkedPortion) =>
                speedLimit.SpeedToMinPortion(Value - TargetValue, ref linkedPortion);

            #region Inspect

#if PEGI
            public override bool Inspect()
            {
                var ret = base.Inspect();
                if (Application.isPlaying)
                    "{0} => {1}".F(Value, TargetValue).nl();
                return ret;
            }
#endif

            #endregion

            #region Encode & Decode

            public override CfgEncoder Encode() => new CfgEncoder()
                .Add("b", base.Encode);

            public override bool Decode(string tg, string data)
            {
                switch (tg)
                {
                    case "b":
                        data.Decode_Delegate(base.Decode);
                        break;
                    default: return false;
                }

                return true;
            }

            #endregion
        }

        public abstract class BaseMaterialTextureTransition : BaseFloatLerp
        {
            private float _portion;

            public ShaderProperty.FloatValue transitionProperty;
            public ShaderProperty.TextureValue currentTexturePrTextureValue;
            public ShaderProperty.TextureValue nextTexturePrTextureValue;
            //public int transitionPropertyName;
            //public int currentTexturePropertyName;
            //public int nextTexturePropertyName;

            private enum OnStart
            {
                Nothing = 0,
                ClearTexture = 1,
                LoadCurrent = 2
            }

            private OnStart _onStart = OnStart.Nothing;

            protected override float TargetValue
            {
                get { return Mathf.Max(0, _targetTextures.Count - 1); }
                set { }
            }

            public override float Value
            {
                get { return _portion; }
                set
                {
                    _portion = value;

                    while (_portion >= 1)
                    {
                        _portion -= 1;
                        if (_targetTextures.Count > 1)
                        {
                            _targetTextures.RemoveAt(0);
                            Current = _targetTextures[0];
                            if (_targetTextures.Count > 1)
                                Next = _targetTextures[1];
                        }
                    }

                    Material?.Set(transitionProperty, _portion);
                }
            }

            private readonly List<Texture> _targetTextures = new List<Texture>();

            public abstract Material Material { get; }

            private Texture Current
            {
                get { return Material?.Get(currentTexturePrTextureValue); }
                set { Material?.Set(currentTexturePrTextureValue, value); }
            }

            private Texture Next
            {
                get { return Material.Get(nextTexturePrTextureValue); }
                set { Material.Set(nextTexturePrTextureValue, value); }
            }

            public Texture TargetTexture
            {
                get { return _targetTextures.TryGetLast(); }

                set
                {
                    if (!value || !Material) return;

                    if (_targetTextures.Count == 0)
                    {
                        _targetTextures.Add(null);
                        _targetTextures.Add(value);
                        Current = null;
                        Next = value;
                    }
                    else
                    {

                        if (value == _targetTextures[0])
                        {
                            if (_targetTextures.Count > 1)
                            {
                                _targetTextures.Swap(0, 1);
                                Value = Mathf.Max(0, 1 - Value);
                                Current = Next;
                                Next = value;
                                _targetTextures.TryRemoveTill(2);
                            }
                        }
                        else if (_targetTextures.Count > 1 && value == _targetTextures[1])
                        {
                            _targetTextures.TryRemoveTill(2);
                        }
                        else
                        {
                            if (_targetTextures.Count == 1)
                            {
                                _targetTextures.Add(value);
                                Next = value;
                            }
                            else
                            {
                                if (_targetTextures[1] == value && _targetTextures.Count == 3)
                                    _targetTextures.RemoveAt(2);
                                else
                                    _targetTextures.ForceSet(2, value);
                            }
                        }
                    }
                }
            }

            protected BaseMaterialTextureTransition()
            {
                transitionProperty = CustomShaderParameters.TransitionPortion;
                currentTexturePrTextureValue = CustomShaderParameters.CurrentTexture;
                nextTexturePrTextureValue = CustomShaderParameters.NextTexture;
            }

            #region Inspector

#if PEGI
            public override bool Inspect()
            {
                var changed = base.Inspect();

                var tex = Current;
                if (allowChangeParameters)
                {

                    "On Start:".editEnum(60, ref _onStart).nl(ref changed);

                    if ("Texture[{0}]".F(_targetTextures.Count).edit(90, ref tex).nl(ref changed))
                        TargetTexture = tex;

                }
                else TargetTexture.write();

                return changed;
            }
#endif

            #endregion

            #region Encode & Decode

            public override CfgEncoder Encode()
            {

                var cody = new CfgEncoder().Add("b", base.Encode);
                if (allowChangeParameters)
                {
                    cody.Add_IfNotZero("onStart", (int) _onStart);
                    if (_onStart == OnStart.LoadCurrent)
                        cody.Add_Reference("s", _targetTextures.TryGetLast());
                }

                return cody;
            }

            public override bool Decode(string tg, string data)
            {
                switch (tg)
                {
                    case "b":
                        data.Decode_Delegate(base.Decode);
                        break;
                    case "s":
                        Texture tmp = null;
                        data.Decode_Reference(ref tmp);
                        TargetTexture = tmp;
                        break;
                    case "clear":
                        _onStart = OnStart.ClearTexture;
                        break;
                    case "onStart":
                        _onStart = (OnStart) data.ToInt();
                        break;
                    default: return false;
                }

                return true;
            }

            public override void Decode(string data)
            {
                _onStart = OnStart.Nothing;
                base.Decode(data);

                if (_onStart == OnStart.ClearTexture)
                {
                    Current = null;
                    Next = null;
                    _targetTextures.Clear();
                }
            }

            #endregion
        }

        public abstract class BaseShaderValue : BaseAnyValue, IGotName
        {

            public Material material;
            public Renderer rendy;
            protected string name;

            protected Material Material => material ? material : rendy.MaterialWhatever();

            protected override string Name => name;

            public string NameForPEGI
            {
                get { return name; }
                set { name = value; }
            }

            public sealed override bool LerpInternal(float linkedPortion)
            {
                if (LerpSubInternal(linkedPortion))
                    Set();
                else
                    return false;

                return true;
            }

            protected abstract bool LerpSubInternal(float linkedPortion);

            protected void Set() => Set(Material);

            public abstract void Set(Material on);

            protected BaseShaderValue(float startingSpeed = 1, Material m = null, Renderer renderer = null)
            {
                speedLimit = startingSpeed;
                material = m;
                rendy = renderer;
            }
        }

        public abstract class BaseColorValue : BaseAnyValue
        {
            protected override string Name => "Color";
            public Color targetValue = Color.white;
            public abstract Color Value { get; set; }

            public override bool Portion(ref float linkedPortion) =>
                speedLimit.SpeedToMinPortion(Value.DistanceRgba(targetValue), ref linkedPortion);

            public sealed override bool LerpInternal(float linkedPortion)
            {
                if (Enabled && (targetValue != Value || !defaultSet))
                    Value = Color.Lerp(Value, targetValue, linkedPortion);
                else return false;

                return true;
            }

            #region Encode & Decode

            public override CfgEncoder Encode() => new CfgEncoder()
                .Add("b", base.Encode)
                .Add("col", targetValue);

            public override bool Decode(string tg, string data)
            {
                switch (tg)
                {
                    case "b":
                        data.Decode_Delegate(base.Decode);
                        break;
                    case "col":
                        targetValue = data.ToColor();
                        break;
                    default: return false;
                }

                return true;
            }

            #endregion

            #region Inspector
            
            public override bool Inspect()
            {

                var changed = base.Inspect();

                pegi.edit(ref targetValue).nl(ref changed);
                
                return changed;
            }

            #endregion
        }

        #endregion

        #region Value Types

        public class FloatValue : BaseFloatLerp, IGotName
        {
            private readonly string _name = "Float value";

            public float targetValue;

            public bool minMax;

            public float min = 0;

            public float max = 1;

            public override float Value { get; set; }

            protected override float TargetValue
            {
                get { return targetValue; }
                set { targetValue = value; }
            }

            protected override string Name => _name;

            #region Inspect
            public string NameForPEGI
            {
                get { return _name; }
                set { }
            }
            
            public override bool PEGI_inList(IList list, int ind, ref int edited) {
                var changed = false;

                if (allowChangeParameters)
                {
                    int width = _name.ApproximateLengthUnsafe();
                    if (minMax)
                        _name.edit(width, ref targetValue, min, max).changes(ref changed);
                    else
                        _name.edit(width, ref targetValue).changes(ref changed);
                }

                if (icon.Enter.Click())
                    edited = ind;

                //base.PEGI_inList(list, ind, ref edited).changes(ref changed);

                return changed;
            }
            
            #endregion

            #region Encode & Decode

            public override CfgEncoder Encode()
            {
                var cody = new CfgEncoder()
                    .Add("b", base.Encode)
                    .Add("trgf", targetValue)
                    .Add_Bool("rng", minMax);
                if (minMax)
                    cody.Add("min", min)
                        .Add("max", max);

                return cody;
            }

            public override bool Decode(string tg, string data)
            {
                switch (tg)
                {
                    case "b": data.Decode_Delegate(base.Decode); break;
                    case "trgf": targetValue = data.ToFloat(); break;
                    case "rng": minMax = data.ToBool(); break;
                    case "min": min = data.ToFloat(); break;
                    case "max": max = data.ToFloat(); break;
                    default: return base.Decode(tg, data); // For compatibility reasons, should return false
                }

                return true;
            }

            #endregion

            public FloatValue()
            {
            }

            public FloatValue(string name)
            {
                _name = name;
            }

            public FloatValue(string name, float startValue)
            {
                _name = name;
                targetValue = startValue;
                Value = startValue;
            }

            public FloatValue(string name, float startValue, float lerpSpeed)
            {
                _name = name;
                targetValue = startValue;
                Value = startValue;
                speedLimit = lerpSpeed;
            }

            public FloatValue(string name, float startValue, float lerpSpeed, float min, float max)
            {
                _name = name;
                targetValue = startValue;
                Value = startValue;
                speedLimit = lerpSpeed;
                minMax = true;
                this.min = min;
                this.max = max;
            }
        }

        public class ColorValue : BaseColorValue, IGotName
        {
            private readonly string _name = "Float value";

            private Color currentValue;

            public override Color Value  {
                get { return currentValue; }
                set { currentValue = value; }
            }

            public string NameForPEGI
            {
                get { return _name; }
                set { }
            }
            
            public ColorValue()
            {
            }

            public ColorValue(string name)  {
                _name = name;
            }

        }

        #endregion

        #region Transform

        public class TransformLocalScale : TransformLocalPosition
        {
            protected override string Name => "Local Scale";

            public override Vector3 Value
            {
                get { return transform.localScale; }
                set { transform.localScale = value; }
            }

            public TransformLocalScale(Transform transform, float nspeed) : base(transform, nspeed)
            {
            }
        }

        public class TransformPosition : TransformLocalPosition
        {
            protected override string Name => "Position";

            public override Vector3 Value
            {
                get { return transform.position; }
                set { transform.position = value; }
            }

            public TransformPosition(Transform transform, float nspeed) : base(transform, nspeed)
            {
            }
        }

        public class TransformLocalPosition : BaseAnyValue
        {
            protected override string Name => "Local Position";
            public Transform transform;
            public Vector3 targetValue;

            public override bool Enabled => base.Enabled && transform;

            public virtual Vector3 Value
            {
                get { return transform.localPosition; }
                set { transform.localPosition = value; }
            }

            public TransformLocalPosition(Transform transform, float nspeed)
            {
                this.transform = transform;
                speedLimit = nspeed;
            }

            public override bool LerpInternal(float portion)
            {
                if (Enabled && Value != targetValue)
                    Value = Vector3.Lerp(Value, targetValue, portion);
                else return false;

                return true;
            }

            public override bool Portion(ref float portion) =>
                speedLimit.SpeedToMinPortion((Value - targetValue).magnitude, ref portion);

        }

        #endregion

        #region Rect Transform

        public class RectangleTransformAnchoredPositionValue : BaseVector2Lerp, IPEGI
        {
            public RectTransform rectTransform;

            public override bool Enabled => base.Enabled && rectTransform;

            protected override string Name => "Anchored Position";

            protected override Vector2 CurrentValue
            {
                get { return rectTransform ? rectTransform.anchoredPosition : targetValue; }
                set
                {
                    if (rectTransform)
                        rectTransform.anchoredPosition = value;
                }
            }

            public RectangleTransformAnchoredPositionValue(RectTransform rect, float nspeed)
            {
                rectTransform = rect;
                speedLimit = nspeed;
            }
        }

        public class RectangleTransformWidthHeight : RectangleTransformAnchoredPositionValue
        {

            protected override string Name => "Width Height";

            protected override Vector2 CurrentValue
            {
                get { return rectTransform ? rectTransform.sizeDelta : targetValue; }
                set { rectTransform.sizeDelta = value; }
            }

            public RectangleTransformWidthHeight(RectTransform rect, float speed) : base(rect, speed)
            {
            }
        }

        #endregion

        #region Material

        public class MaterialFloat : BaseShaderValue
        {
            private readonly ShaderProperty.FloatValue _property;

            public float Value
            {
                get { return _property.lastValue; }
                set
                {
                    _property.lastValue = value;
                    defaultSet = false;
                }
            }

            public float targetValue;

            public override void Set(Material mat)
            {
                if (mat)
                    mat.Set(_property);
                else
                    _property.GlobalValue = _property.lastValue;
            }

            public MaterialFloat(string nName, float startingValue, float startingSpeed = 1, Renderer renderer = null,
                Material m = null) : base(startingSpeed, m, renderer)
            {
                _property = new ShaderProperty.FloatValue(nName); 
                targetValue = startingValue;
                Value = startingValue;
            }

            public override bool Portion(ref float linkedPortion) =>
                speedLimit.SpeedToMinPortion(Value - targetValue, ref linkedPortion);

            protected override bool LerpSubInternal(float portion)
            {
                if (Enabled && (Value != targetValue || !defaultSet))
                {
                    _property.lastValue = Mathf.Lerp(Value, targetValue, portion);
                    return true;
                }

                return false;
            }

            #region Inspector

#if PEGI

            public override bool Inspect()
            {
                var changed = base.Inspect();

                "Target".edit(70, ref targetValue).nl(ref changed);

                if ("Value".edit(ref _property.lastValue).nl(ref changed))
                    Set();

                return changed;
            }

#endif

            #endregion


        }

        public class MaterialColor : BaseShaderValue
        {
            private readonly ShaderProperty.ColorValue _property;

            public Color Value
            {
                get { return _property.lastValue; }
                set
                {
                    _property.lastValue = value;
                    defaultSet = false;
                }
            }

            public Color targetValue;

            public override void Set(Material mat)
            {
                if (mat)
                    mat.Set(_property);
                else
                    _property.GlobalValue = Value;
            }

            public MaterialColor(string nName, Color startingValue, float startingSpeed = 1, Material m = null,
                Renderer renderer = null) : base(startingSpeed, m, renderer)
            {
                _property = new ShaderProperty.ColorValue(nName);
                Value = startingValue;
            }

            protected override bool LerpSubInternal(float portion)
            {
                if (Value != targetValue || !defaultSet)
                {
                    _property.lastValue = Color.Lerp(Value, targetValue, portion);
                    return true;
                }

                return false;
            }

            public override bool Portion(ref float portion) =>
                speedLimit.SpeedToMinPortion(Value.DistanceRgba(targetValue), ref portion);

        }

        public class GraphicMaterialTextureTransition : BaseMaterialTextureTransition
        {
            protected override string Name => "Texture Transition";

            private Graphic _graphic;

            public GraphicMaterialTextureTransition(float nSpeed = 1) : base()
            {
                speedLimit = nSpeed;
            }

            public Graphic Graphic
            {
                set
                {
                    if (value != _graphic)
                    {
                        _graphic = value;
                        if (Application.isPlaying)
                            _graphic.material = Object.Instantiate(_graphic.material);
                    }
                }
            }

            public override Material Material => _graphic?.material;
        }

        public class RendererMaterialTextureTransition : BaseMaterialTextureTransition
        {
            private Renderer _graphic;

            protected override string Name => "Renderer Texture Transition";

            public RendererMaterialTextureTransition(Renderer rendy, float nSpeed = 1) : base()
            {
                speedLimit = nSpeed;
                _graphic = rendy;
            }

            public Renderer Renderer
            {
                set
                {
                    if (value != _graphic)
                    {
                        _graphic = value;
                        if (Application.isPlaying) _graphic.material = UnityEngine.Object.Instantiate(_graphic.material);
                    }
                }
            }

            public override Material Material => _graphic?.MaterialWhatever();
        }

        #endregion

        #region UIElement Values

        public class GraphicAlpha : BaseFloatLerp
        {

            protected Graphic graphic;

            public Graphic Graphic
            {
                get { return graphic; }
                set
                {
                    graphic = value;
                    if (setZeroOnStart && !defaultSet)
                    {
                        graphic.TrySetAlpha(0);
                        defaultSet = true;
                    }
                }
            }

            public float targetValue;
            public bool setZeroOnStart = true;

            protected override float TargetValue
            {
                get { return targetValue; }
                set { targetValue = value; }
            }

            public override float Value
            {
                get { return graphic ? graphic.color.a : targetValue; }
                set { graphic.TrySetAlpha(value); }
            }

            protected override string Name => "Graphic Alpha";

            public GraphicAlpha()
            {
            }

            public GraphicAlpha(Graphic graphic)
            {
                this.graphic = graphic;
            }

            #region Encode & Decode

            public override void Decode(string data)
            {
                base.Decode(data);

                if (setZeroOnStart && !defaultSet)
                    graphic.TrySetAlpha(0);
            }

            public override CfgEncoder Encode() =>
                new CfgEncoder().Add("bb", base.Encode).Add_Bool("zero", setZeroOnStart);

            public override bool Decode(string tg, string data)
            {
                switch (tg)
                {
                    case "bb":
                        data.Decode_Delegate(base.Decode);
                        break;
                    case "zero":
                        setZeroOnStart = data.ToBool();
                        break;
                    default: return base.Decode(tg, data);
                }

                return true;
            }

            #endregion

            #region Inspect
            
            public override bool Inspect()
            {

                var changed = base.Inspect();

                "Set zero On Start".toggleIcon(ref setZeroOnStart).nl();

                return changed;
            }

            #endregion


        }

        public class GraphicColor : BaseColorValue
        {

            protected override string Name => "Graphic Color";

            public Graphic graphic;

            public override Color Value
            {
                get { return graphic ? graphic.color : targetValue; }
                set { graphic.color = value; }
            }

            public GraphicColor()
            {
            }

            public GraphicColor(Graphic graphic)
            {
                this.graphic = graphic;
            }

        }

        #endregion

    }

    public static class LinkedLerpingExtensions
    {

        public static void Portion<T>(this T[] list, LerpData ld) where T : ILinkedLerping
        {
            foreach (var e in list)
                if (!e.IsNullOrDestroyed_Obj())
                    e.Portion(ld);
        }

        public static void Lerp<T>(this T[] list, LerpData ld, bool canTeleport = false) where T : ILinkedLerping
        {
            foreach (var e in list)
                e.NullIfDestroyed()?.Lerp(ld, canTeleport);
        }

        public static void Portion<T>(this List<T> list, LerpData ld) where T : ILinkedLerping
        {

            foreach (var e in list)
                if (!e.IsNullOrDestroyed_Obj())
                    e.Portion(ld);

        }

        public static void Lerp<T>(this List<T> list, LerpData ld, bool canTeleport = false) where T : ILinkedLerping
        {
            foreach (var e in list)
                e.NullIfDestroyed()?.Lerp(ld, canTeleport);
        }

        public static void FadeAway<T>(this List<T> list) where T : IManageFading
        {
            if (list == null) return;

            foreach (var e in list)
                e.NullIfDestroyed()?.FadeAway();
        }

        public static bool TryFadeIn<T>(this List<T> list) where T : IManageFading
        {


            if (list == null) return false;

            var fadedIn = false;


            foreach (var e in list)
                if (!e.IsNullOrDestroyed_Obj())
                    fadedIn |= e.TryFadeIn();

            return fadedIn;
        }
    }

    public static class CustomShaderParameters
    {
        public static readonly ShaderProperty.VectorValue ImageProjectionPosition = new ShaderProperty.VectorValue("_imgProjPos");
        public static readonly ShaderProperty.TextureValue NextTexture = new ShaderProperty.TextureValue("_Next_MainTex");
        public static readonly ShaderProperty.TextureValue CurrentTexture = new ShaderProperty.TextureValue("_MainTex_Current");
        public static readonly ShaderProperty.FloatValue TransitionPortion = new ShaderProperty.FloatValue("_Transition");

        
        public static void Inspect()
        {
            "Image projection position".write_ForCopy(ImageProjectionPosition.NameForDisplayPEGI); pegi.nl();

            "Next Texture".write_ForCopy(NextTexture.NameForDisplayPEGI); pegi.nl();

            "Transition portion".write_ForCopy(TransitionPortion.NameForDisplayPEGI); pegi.nl();
        }
    }


    public static partial class QcMath
    {
        #region Lerps

        public static float SpeedToPortion(this float speed, float dist) =>
            dist != 0 ? Mathf.Clamp01(speed * Time.deltaTime / Mathf.Abs(dist)) : 1;

        public static bool SpeedToMinPortion(this float speed, float dist, ref float portion)
        {

            var nPortion = speed.SpeedToPortion(dist);
            if (!(nPortion < portion))
                return (1 - portion) < float.Epsilon && dist > 0;

            portion = nPortion;

            return true;

        }

        public static bool IsLerpingBySpeed(ref float from, float to, float speed)
        {
            if (from == to)
                return false;

            from = Mathf.LerpUnclamped(from, to, speed.SpeedToPortion(Mathf.Abs(from - to)));
            return true;
        }

        public static bool LerpBySpeed(ref float from, float to, float speed, out float portion)
        {
            if (from == to)
            {
                portion = 1;
                return false;
            }

            portion = speed.SpeedToPortion(Mathf.Abs(from - to));
            from = Mathf.LerpUnclamped(from, to, portion);

            return true;
        }

        public static float LerpBySpeed(float from, float to, float speed)
            => Mathf.LerpUnclamped(from, to, speed.SpeedToPortion(Mathf.Abs(from - to)));

        public static float LerpBySpeed(float from, float to, float speed, out float portion)
        {
            portion = speed.SpeedToPortion(Mathf.Abs(from - to));
            return Mathf.LerpUnclamped(from, to, portion);
        }

        public static bool LerpAngle_bySpeed(ref float from, float to, float speed)
        {
            var dist = Mathf.Abs(Mathf.DeltaAngle(from, to));
            if (dist <= float.Epsilon) return false;
            var portion = speed.SpeedToPortion(dist);
            from = Mathf.LerpAngle(from, to, portion);
            return true;
        }

        public static Vector2 LerpBySpeed(this Vector2 from, float toX, float toY, float speed)
        {
            var to = new Vector2(toX, toY);
            return Vector2.LerpUnclamped(from, to, speed.SpeedToPortion(Vector2.Distance(from, to)));
        }

        public static Vector2 LerpBySpeed(this Vector2 from, Vector2 to, float speed) =>
            Vector2.LerpUnclamped(from, to, speed.SpeedToPortion(Vector2.Distance(from, to)));

        public static Vector2 LerpBySpeed(this Vector2 from, Vector2 to, float speed, out float portion)
        {
            portion = speed.SpeedToPortion(Vector3.Distance(from, to));
            return Vector3.LerpUnclamped(from, to, portion);
        }

        public static Vector3 LerpBySpeed(this Vector3 from, Vector3 to, float speed) =>
            Vector3.LerpUnclamped(from, to, speed.SpeedToPortion(Vector3.Distance(from, to)));

        public static Vector3 LerpBySpeed(this Vector3 from, Vector3 to, float speed, out float portion)
        {
            portion = speed.SpeedToPortion(Vector3.Distance(from, to));
            return Vector3.LerpUnclamped(from, to, portion);
        }

        public static Vector4 LerpBySpeed(this Vector4 from, Vector4 to, float speed) =>
            Vector4.LerpUnclamped(from, to, speed.SpeedToPortion(Vector4.Distance(from, to)));

        public static Vector4 LerpBySpeed(this Vector4 from, Vector4 to, float speed, out float portion)
        {
            portion = speed.SpeedToPortion(Vector4.Distance(from, to));
            return Vector4.LerpUnclamped(from, to, portion);
        }

        public static Quaternion LerpBySpeed(this Quaternion from, Quaternion to, float speed) =>
            Quaternion.LerpUnclamped(from, to, speed.SpeedToPortion(Quaternion.Angle(from, to)));

        public static Quaternion LerpBySpeed(this Quaternion from, Quaternion to, float speed, out float portion)
        {
            portion = speed.SpeedToPortion(Quaternion.Angle(from, to));
            return Quaternion.LerpUnclamped(from, to, portion);
        }

        public static float DistanceRgb(this Color col, Color other)
            =>
                (Mathf.Abs(col.r - other.r) + Mathf.Abs(col.g - other.g) + Mathf.Abs(col.b - other.b));

        public static float DistanceRgba(this Color col, Color other) =>
                ((Mathf.Abs(col.r - other.r) + Mathf.Abs(col.g - other.g) + Mathf.Abs(col.b - other.b)) * 0.33f +
                 Mathf.Abs(col.a - other.a));
        
        public static Color LerpBySpeed(this Color from, Color to, float speed) =>
            Color.LerpUnclamped(from, to, speed.SpeedToPortion(from.DistanceRgb(to)));

        public static Color LerpRgb(this Color from, Color to, float speed, out float portion)
        {
            portion = speed.SpeedToPortion(from.DistanceRgb(to));
            to.a = from.a;
            return Color.LerpUnclamped(from, to, portion);
        }

        public static Color LerpRgba(this Color from, Color to, float speed, out float portion)
        {
            portion = speed.SpeedToPortion(from.DistanceRgba(to));
            return Color.LerpUnclamped(from, to, portion);
        }

        public static bool IsLerpingAlphaBySpeed<T>(this List<T> graphicList, float alpha, float speed) where T : Graphic
        {

            if (graphicList.IsNullOrEmpty()) return false;

            var changing = false;

            foreach (var i in graphicList)
                changing |= i.IsLerpingAlphaBySpeed(alpha, speed);

            return changing;
        }

        public static bool IsLerpingAlphaBySpeed<T>(this T img, float alpha, float speed) where T : Graphic
        {
            if (!img) return false;

            var changing = false;

            var col = img.color;
            col.a = LerpBySpeed(col.a, alpha, speed);

            img.color = col;
            changing |= col.a != alpha;

            return changing;
        }

        public static bool IsLerpingRgbBySpeed<T>(this T img, Color target, float speed) where T : Graphic
        {
            bool changing = false;

            if (img)
            {
                float portion;
                img.color = img.color.LerpRgb(target, speed, out portion);

                changing = portion < 1;
            }

            return changing;
        }

        #endregion

    }
}