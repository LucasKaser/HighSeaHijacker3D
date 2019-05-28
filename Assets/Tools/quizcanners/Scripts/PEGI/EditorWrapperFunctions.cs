﻿#if !NO_PEGI && UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEditor.SceneManagement;
using QuizCannersUtilities;
using UnityEditorInternal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#pragma warning disable IDE1006 // Naming Styles

namespace PlayerAndEditorGUI {

    public static class ef {

        public enum EditorType { Mono, ScriptableObject, Material, Unknown }

        public static EditorType editorType = EditorType.Unknown;

        private static bool _lineOpen = false;
        private static int _selectedFold = -1;
        private static int _elementIndex;
        public static SerializedObject serObj;
        private static Editor _editor;
        private static PEGI_Inspector_Material _materialEditor;

        public static bool DefaultInspector() {
            newLine();

            EditorGUI.BeginChangeCheck();
            switch (editorType) {
                case EditorType.Material:  _materialEditor?.DrawDefaultInspector(); break;
                default: if (_editor!= null) _editor.DrawDefaultInspector(); break;
            }
            return EditorGUI.EndChangeCheck();

        }

        public static bool Inspect<T>(Editor editor) where T : MonoBehaviour
        {
            _editor = editor;

            editorType = EditorType.Mono;

            var o = (T)editor.target;
            var so = editor.serializedObject;
            pegi.inspectedTarget = editor.target;

            var go = o.gameObject;
            
            var pgi = o as IPEGI;

            if (pgi != null && !go.IsPrefab()) {

                start(so);

                if (!pegi.PopUpService.ShowingPopup()) {

                    #if UNITY_2018_3_OR_NEWER
                    var isPrefab = PrefabUtility.IsPartOfAnyPrefab(o);

                    if (isPrefab &&
                        PrefabUtility.HasPrefabInstanceAnyOverrides(PrefabUtility.GetNearestPrefabInstanceRoot(o),
                            false) &&
                        icon.Save.Click("Update Prefab"))
                        PrefabUtility.ApplyPrefabInstance(go, InteractionMode.UserAction);
                    #endif

                    if (pgi.Inspect())
                        ClearFromPooledSerializedObjects(o);

                    #if UNITY_2018_3_OR_NEWER
                    if (changes && isPrefab)
                        PrefabUtility.RecordPrefabInstancePropertyModifications(o);
                    #endif
                }

                if (changes)
                {
                    if (!Application.isPlaying) 
                        EditorSceneManager.MarkSceneDirty(go ? SceneManager.GetActiveScene() : go.scene);
                    
                    EditorUtility.SetDirty(o);

                    EditorUtility.SetDirty(go);
                }

                newLine();
                
                return changes;
            }

            editor.DrawDefaultInspector();

            return false;
        }

        public static bool Inspect_so<T>(Editor editor) where T : ScriptableObject
        {
            _editor = editor;

            editorType = EditorType.ScriptableObject;

            var o = (T)editor.target;
            var so = editor.serializedObject;
            pegi.inspectedTarget = editor.target;

            var pgi = o as IPEGI;
            if (pgi != null)
            {
                start(so);
              
                var changed = !pegi.PopUpService.ShowingPopup() && pgi.Inspect();
                end(o);
                return changed;
            }

            editor.DrawDefaultInspector();

            return false;
        }
        
        public static bool Inspect_Material(PEGI_Inspector_Material editor) {

            _materialEditor = editor;

            editorType = EditorType.Material;

            pegi.inspectedTarget = editor.unityMaterialEditor.target;

            var mat = editor.unityMaterialEditor.target as Material;

            start();

            var changed = !pegi.PopUpService.ShowingPopup() && editor.Inspect(mat);

            end(mat);
            
            return changes || changed;
        }

        public static bool toggleDefaultInspector() =>
            editorType == EditorType.Material 
                ? pegi.toggle(ref PEGI_Inspector_Material.drawDefaultInspector, icon.Config, icon.Debug, "Toggle Between regular and PEGI Material inspector", 20).nl() 
                : pegi.toggle(ref PEGI_Inspector_Base.drawDefaultInspector, icon.Config, icon.Debug, "Toggle Between regular and PEGI inspector", 20).nl();
          
        static void start(SerializedObject so = null) {
            _elementIndex = 0;
            PEGI_Extensions.focusInd = 0;
            _lineOpen = false;
            serObj = so;
            pegi.globChanged = false;
        }

        public static T ClearFromPooledSerializedObjects<T>(T obj) where T : Object {
            if (obj && SerializedObjects.ContainsKey(obj))
                SerializedObjects.Remove(obj);

            return obj;
        }

        static bool end(Object obj)
        {

            if (changes)
            {
                if (!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

                ClearFromPooledSerializedObjects(obj);

                EditorUtility.SetDirty(obj);
            }
            newLine();

            return changes;
        }
        
        private static bool change { get { pegi.globChanged = true; return true; } }

        private static bool Dirty(this bool val) { pegi.globChanged |= val; return val; }

        private static bool changes => pegi.globChanged;

        private static bool ignoreChanges(this bool changed)
        {
            if (changed)
                pegi.globChanged = false;
            return changed;
        }

        private static void BeginCheckLine() { checkLine(); EditorGUI.BeginChangeCheck(); }

        private static bool EndCheckLine() => EditorGUI.EndChangeCheck().Dirty(); 

        public static void checkLine()
        {
            if (_lineOpen) return;
            
            pegi.tabIndex = 0;
            EditorGUILayout.BeginHorizontal();
            _lineOpen = true;
            
        }

        public static void newLine()
        {
            if (!_lineOpen) return;
            
            _lineOpen = false;
            EditorGUILayout.EndHorizontal();
            
        }

        #region Foldout
        private static bool IsFoldedOut { get { return pegi.isFoldedOutOrEntered; } set { pegi.isFoldedOutOrEntered = value; } }

        private static bool StylizedFoldOut(bool foldedOut, string txt, string hint = "FoldIn/FoldOut")
        {

            BeginCheckLine();

            textAndToolTip.text = txt;
            textAndToolTip.tooltip = hint;
          
            foldedOut = EditorGUILayout.Foldout(foldedOut, textAndToolTip, true);
            EndCheckLine();

            return foldedOut; 
        }

        public static bool foldout(string txt, ref bool state)
        {
            state = StylizedFoldOut(state, txt);
            IsFoldedOut = state;
            return IsFoldedOut;
        }

        public static bool foldout(string txt, ref int selected, int current)
        {

            IsFoldedOut = (selected == current);

            if (StylizedFoldOut(IsFoldedOut, txt))
                selected = current;
            else
                if (IsFoldedOut) selected = -1;

            IsFoldedOut = selected == current;

            return IsFoldedOut;
        }
        
        public static bool foldout(string txt)
        {
            IsFoldedOut = foldout(txt, ref _selectedFold, _elementIndex);

            _elementIndex++;

            return IsFoldedOut;
        }

        #endregion

        #region Select
        public static bool select<T>(ref int no, List<T> lst, int width)
        {

            var listNames = new List<string>();
            var listIndexes = new List<int>();

            var current = -1;

            for (var j = 0; j < lst.Count; j++)
            {
                if (lst[j] == null) continue;
                
                if (no == j)
                    current = listIndexes.Count;
                listNames.Add("{0}: {1}".F(j,lst[j].ToPegiString()));
                listIndexes.Add(j);

            }

            if (select(ref current, listNames.ToArray(), width)) {
                no = listIndexes[current];
                return true;
            }

            return false;

        }

        public static bool select<T>(ref int no, CountlessCfg<T> tree) where T : ICfg , new()
        {
            List<int> indexes;
            var objs = tree.GetAllObjs(out indexes);
            var filtered = new List<string>();
            var current = -1;
            
            for (var i = 0; i < objs.Count; i++)
            {
                if (no == indexes[i])
                    current = i;
                filtered.Add("{0}: {1}".F(i, objs[i].ToPegiString()));
            }

            if (select(ref current, filtered.ToArray()))
            {
                no = indexes[current];
                return true;
            }
            return false;
        }

        public static bool select<T>(ref int no, Countless<T> tree)
        {
            List<int> indexes;
            var objs = tree.GetAllObjs(out indexes);
            var filtered = new List<string>();
            var current = -1;
            
            for (var i = 0; i < objs.Count; i++)
            {
                if (no == indexes[i])
                    current = i;
                filtered.Add(objs[i].ToPegiString());
            }

            if (select(ref current, filtered.ToArray()))
            {
                no = indexes[current];
                return true;
            }
            return false;
        }
        
        public static bool select(ref int no, string[] from, int width)
        {
            BeginCheckLine();
            no = EditorGUILayout.Popup(no, from, GUILayout.MaxWidth(width));
            return EndCheckLine();

        }

        public static bool select(ref int no, string[] from)
        {
            BeginCheckLine();
            no = EditorGUILayout.Popup(no, from);
            return EndCheckLine();
        }
        
        public static bool select(ref int no, Dictionary<int, string> from)
        {
            var options = new string[from.Count];

            var ind = -1;

            for (var i = 0; i < from.Count; i++)
            {
                var e = from.ElementAt(i);
                options[i] = e.Value;
                if (no == e.Key)
                    ind = i;
            }

            BeginCheckLine();
            var newInd = EditorGUILayout.Popup(ind, options, EditorStyles.toolbarDropDown);
            if (!EndCheckLine()) return false;
            
            no = from.ElementAt(newInd).Key;
            return true;
            
        }

        public static bool select(ref int no, Dictionary<int, string> from, int width)
        {
            var options = new string[from.Count];

            var ind = -1;

            for (var i = 0; i < from.Count; i++)
            {
                var e = from.ElementAt(i);
                options[i] = e.Value;
                if (no == e.Key)
                    ind = i;
            }


            BeginCheckLine();
            var newInd = EditorGUILayout.Popup(ind, options, EditorStyles.toolbarDropDown, GUILayout.MaxWidth(width));
            if (!EndCheckLine()) return false;
            
            no = from.ElementAt(newInd).Key;
            return true;
            
        }
        
        public static bool select(ref int index, Texture[] tex)
        {
            if (tex.Length == 0) return false;
            
            var before = index;
            var texNames = new List<string>();
            var texIndexes = new List<int>();

            var tmpInd = 0;
            for (var i = 0; i < tex.Length; i++)
                if (tex[i])
                {
                    texIndexes.Add(i);
                    texNames.Add("{0}: {1}".F(i, tex[i].name));
                    if (index == i) tmpInd = texNames.Count - 1;
                }

            BeginCheckLine();
            tmpInd = EditorGUILayout.Popup(tmpInd, texNames.ToArray());
            if (!EndCheckLine()) return false;

            if (tmpInd >= 0 && tmpInd < texNames.Count)
                index = texIndexes[tmpInd];

            return (before != index);
        }
   
        private static bool select_Type(ref Type current, IReadOnlyList<Type> others, Rect rect)
        {

            var names = new string[others.Count];

            var ind = -1;

            for (var i = 0; i < others.Count; i++)
            {
                var el = others[i];
                names[i] = el.ToPegiStringType();
                if (el != null && el == current)
                    ind = i;
            }

            BeginCheckLine();

            var newNo = EditorGUI.Popup(rect, ind, names);

            if (!EndCheckLine()) return false;
            
            current = others[newNo];
            return change;
        }

        private static bool select(ref Component current, IReadOnlyList<Component> others, Rect rect)
        {

            var names = new string[others.Count];

            var ind = -1;

            for (var i = 0; i < others.Count; i++)
            {
                var el = others[i];
                names[i] = i+": "+el.GetType().ToPegiStringType();
                if (el && el == current)
                    ind = i;
            }

            BeginCheckLine();

            var newNo = EditorGUI.Popup(rect, ind, names);

            if (!EndCheckLine()) return false;

            current = others[newNo];

            return change;
        }


        #endregion

        public static void Space()
        {
            checkLine();
            EditorGUILayout.Separator();
            newLine();
        }

        #region Edit

        #region Values
        public static bool edit(ref string text)
        {
            BeginCheckLine();
            text = EditorGUILayout.TextField(text);
            return EndCheckLine();
        }

        public static bool edit(ref string text, int width)
        {
            BeginCheckLine();
            text = EditorGUILayout.TextField(text, GUILayout.MaxWidth(width));
            return EndCheckLine();
        }

        public static bool edit(ref string[] texts, int no)
        {
            BeginCheckLine();
            texts[no] = EditorGUILayout.TextField(texts[no]);
            return EndCheckLine();
        }

        public static bool edit(List<string> texts, int no)
        {
            BeginCheckLine();
            texts[no] = EditorGUILayout.TextField(texts[no]);
            return EndCheckLine();
        }

        public static bool editPowOf2(ref int i, int min, int max)
        {
            BeginCheckLine();
            i = Mathf.ClosestPowerOfTwo((int)Mathf.Clamp(EditorGUILayout.IntField(i), min, max));
            return EndCheckLine();
        }


        public static bool editBig(ref string text)
        {
            BeginCheckLine();
            text = EditorGUILayout.TextArea(text, GUILayout.MaxHeight(100));
            return EndCheckLine();
        }


        public static bool edit<T>(ref T field) where T : UnityEngine.Object
        {
            BeginCheckLine();
            field = (T)EditorGUILayout.ObjectField(field, typeof(T), true);
            return EndCheckLine();
        }

        public static bool edit<T>(ref T field, int width) where T : UnityEngine.Object
        {
            BeginCheckLine();
            field = (T)EditorGUILayout.ObjectField(field, typeof(T), true, GUILayout.MaxWidth(width));
            return EndCheckLine();
        }

        public static bool edit<T>(ref T field, bool allowDrop) where T : UnityEngine.Object
        {
            BeginCheckLine();
            field = (T)EditorGUILayout.ObjectField(field, typeof(T), allowDrop);
            return EndCheckLine();
        }
        
        public static bool edit(string label, ref float val)
        {
            BeginCheckLine();
            val = EditorGUILayout.FloatField(label, val);
            return EndCheckLine();
        }

        public static bool edit(ref float val)
        {
            BeginCheckLine();
            val = EditorGUILayout.FloatField(val);
            return EndCheckLine();
        }
        
        public static bool edit(ref float val, int width)
        {
            BeginCheckLine();
            val = EditorGUILayout.FloatField(val, GUILayout.MaxWidth(width));
            return EndCheckLine();
        }

        public static bool edit(ref double val, int width)
        {
            BeginCheckLine();
            val = EditorGUILayout.DoubleField(val, GUILayout.MaxWidth(width));
            return EndCheckLine();
        }

        public static bool edit(ref double val)
        {
            BeginCheckLine();
            val = EditorGUILayout.DoubleField(val);
            return EndCheckLine();
        }
        
        public static bool edit(ref int val, int min, int max)
        {
            BeginCheckLine();
            val = EditorGUILayout.IntSlider(val, min, max); //Slider(val, min, max);
            return EndCheckLine();
        }

        public static bool edit(ref uint val, uint min, uint max)
        {
            BeginCheckLine();
            val = (uint)EditorGUILayout.IntSlider((int)val, (int)min, (int)max); //Slider(val, min, max);
            return EndCheckLine();
        }
        
        public static bool editPOW(ref float val, float min, float max)
        {

            checkLine();
            var before = Mathf.Sqrt(val);
            var after = EditorGUILayout.Slider(before, min, max);
            if (before != after)
            {
                val = after * after;
                return change;
            }
            return false;
        }

        public static bool edit(ref float val, float min, float max)
        {
            BeginCheckLine();
            val = EditorGUILayout.Slider(val, min, max);
            return EndCheckLine();
        }

        public static bool edit(ref Color col)
        {

            BeginCheckLine();
            col = EditorGUILayout.ColorField(col);
            return EndCheckLine();

        }

        public static bool edit(ref Color col, int width)
        {

            BeginCheckLine();
            col = EditorGUILayout.ColorField(col, GUILayout.MaxWidth(width));
            return EndCheckLine();

        }

        public static bool edit(ref Color col, GUIContent cnt, int width)
        {

            BeginCheckLine();
            col = EditorGUILayout.ColorField(cnt ,col, GUILayout.MaxWidth(width));
            return EndCheckLine();

        }

        public static bool editKey(ref Dictionary<int, string> dic, int key)
        {
            checkLine();
            var before = key;

            if (editDelayed(ref key, 40))
                return dic.TryChangeKey(before, key) ? change : false;
  
            return false;
        }
        
        public static bool edit(ref Dictionary<int, string> dic, int atKey)
        {
            var before = dic[atKey];
            if (editDelayed(ref before))
            {
                dic[atKey] = before;
                return change;
            }
            return false;
        }

        public static bool edit(ref int val)
        {
            BeginCheckLine();
            val = EditorGUILayout.IntField(val);
            return EndCheckLine();
        }

        public static bool edit(ref uint val)
        {
            BeginCheckLine();
            val = (uint)EditorGUILayout.IntField((int)val);
            return EndCheckLine();
        }

        public static bool edit(ref int val, int width)
        {
            BeginCheckLine();
            val = EditorGUILayout.IntField(val, GUILayout.MaxWidth(width));
            return EndCheckLine();
        }
        
        public static bool edit(ref uint val, int width)
        {
            BeginCheckLine();
            val = (uint)EditorGUILayout.IntField((int)val, GUILayout.MaxWidth(width));
            return EndCheckLine();
        }
        
        public static bool edit(string name, ref AnimationCurve val) {

            BeginCheckLine();
            val = EditorGUILayout.CurveField(name,val);
            return EndCheckLine();
        }

        public static bool edit(string label, ref Vector4 val)
        {
            BeginCheckLine();
            
            val = EditorGUILayout.Vector4Field(label, val);

            return EndCheckLine();
        }

        public static bool edit(string label, ref Vector2 val) {

            BeginCheckLine();
            val = EditorGUILayout.Vector2Field(label, val);
            return EndCheckLine();
        }

        public static bool edit(ref Vector2 val) => edit(ref val.x) || edit(ref val.y);
        
        public static bool edit(ref MyIntVec2 val) => edit(ref val.x) || edit(ref val.y);
        
        public static bool edit(ref MyIntVec2 val, int min, int max) => edit(ref val.x, min, max) || edit(ref val.y, min, max);
        
        public static bool edit(ref MyIntVec2 val, int min, MyIntVec2 max) => edit(ref val.x, min, max.x) || edit(ref val.y, min, max.y);
        
        public static bool edit(ref Vector4 val) => "X".edit(ref val.x).nl() || "Y".edit(ref val.y).nl() || "Z".edit(ref val.z).nl() || "W".edit(ref val.w).nl();
        #endregion

        #region Delayed

       // private static string _editedText;
       // private static string _editedHash = "";
        public static bool editDelayed(ref string text)
        {

            BeginCheckLine();
            text = EditorGUILayout.DelayedTextField(text);
            return EndCheckLine();

          /*  if (KeyCode.Return.IsDown())
            {
                if (text.GetHashCode().ToString() == _editedHash)
                {
                    checkLine();
                    EditorGUILayout.TextField(text);
                    text = _editedText;
                    return change;
                }
            }

            var tmp = text;
            if (edit(ref tmp).ignoreChanges())
            {
                _editedText = tmp;
                _editedHash = text.GetHashCode().ToString();
            }
            
            return false;//(String.Compare(before, text) != 0);*/
        }

        public static bool editDelayed(ref string text, int width)
        {

            BeginCheckLine();
            text = EditorGUILayout.DelayedTextField(text, GUILayout.MaxWidth(width));
            return EndCheckLine();

            /*
            if (KeyCode.Return.IsDown() && (text.GetHashCode().ToString() == _editedHash))
            {
                checkLine();
                EditorGUILayout.TextField(text);
                text = _editedText;
                return change;
            }

            var tmp = text;
            if (edit(ref tmp, width).ignoreChanges())
            {
                _editedText = tmp;
                _editedHash = text.GetHashCode().ToString();
            }



            return false;//(String.Compare(before, text) != 0);*/
        }


        public static bool editDelayed(ref int val)
        {

            BeginCheckLine();
            val = EditorGUILayout.DelayedIntField(val);
            return EndCheckLine();
        }

        // static int editedIntegerIndex;
            // static int editedInteger;
            public static bool editDelayed(ref int val, int width)
        {

            BeginCheckLine();
            val = EditorGUILayout.DelayedIntField(val, GUILayout.MaxWidth(width));
            return EndCheckLine();

           /* if (KeyCode.Return.IsDown() && (_elementIndex == editedIntegerIndex))
            {
                checkLine();
                EditorGUILayout.IntField(val, GUILayout.Width(width));
                val = editedInteger;
                _elementIndex++; editedIntegerIndex = -1;
                return change;
            }

            var tmp = val;
            if (edit(ref tmp).ignoreChanges())
            {
                editedInteger = tmp;
                editedIntegerIndex = _elementIndex;
            }

            _elementIndex++;

            return false;*/
        }

        //private static int _editedFloatIndex;
        //private static float _editedFloat;

        public static bool editDelayed(ref float val)
        {

            BeginCheckLine();
            val = EditorGUILayout.DelayedFloatField(val);
            return EndCheckLine();
        }

        public static bool editDelayed(ref float val, int width)
        {

            BeginCheckLine();
            val = EditorGUILayout.DelayedFloatField(val, GUILayout.MaxWidth(width));
            return EndCheckLine();

           /* if (KeyCode.Return.IsDown() && (_elementIndex == _editedFloatIndex))
            {
                checkLine();
                EditorGUILayout.FloatField(val, GUILayout.Width(width));
                val = _editedFloat;
                _elementIndex++;
                _editedFloatIndex = -1;
                return change;
            }

            var tmp = val;
            if (edit(ref tmp, width).ignoreChanges())
            {
                _editedFloat = tmp;
                _editedFloatIndex = _elementIndex;
            }

            _elementIndex++;

            return false;*/
        }

        public static bool editDelayed(ref double val)
        {

            BeginCheckLine();
            val = EditorGUILayout.DelayedDoubleField(val);
            return EndCheckLine();
        }

        public static bool editDelayed(ref double val, int width)
        {

            BeginCheckLine();
            val = EditorGUILayout.DelayedDoubleField(val, GUILayout.MaxWidth(width));
            return EndCheckLine();

           
        }


        #endregion

        #region Property

        private static GUIContent textImageTip = new GUIContent();

        public static bool edit_Property<T>(string label, Texture image, string tip, int width, Expression<Func<T>> memberExpression, UnityEngine.Object obj)
        {
            var changes = false;
            
            var serializedObject = (!obj ? ef.serObj : GetSerObj(obj)); 

            if (serializedObject == null) return false;

            var member = ((MemberExpression)memberExpression.Body).Member;
            var name = member.Name;

            textImageTip.text = label;
            textImageTip.image = image;
            textImageTip.tooltip = tip;
            
            var tps = serializedObject.FindProperty(name);

            if (tps == null) return changes;

            EditorGUI.BeginChangeCheck();

            if (width < 1)
                EditorGUILayout.PropertyField(tps, textImageTip, true);
            else
                EditorGUILayout.PropertyField(tps, textImageTip, true, GUILayout.MaxWidth(width));

            if (!EditorGUI.EndChangeCheck()) return changes;

            serializedObject.ApplyModifiedProperties();

            changes = change;

            return changes;
        }
        
        private static readonly Dictionary<Object, SerializedObject> SerializedObjects = new Dictionary<UnityEngine.Object, SerializedObject>();

        private static SerializedObject GetSerObj(Object obj)
        {
            SerializedObject so;

            if (SerializedObjects.TryGetValue(obj, out so))
                return so;

            so = new SerializedObject(obj);

            if (SerializedObjects.Count > 8)
                SerializedObjects.Clear();

            SerializedObjects.Add(obj, so);

            return so;

        }
        #endregion

        #endregion

        #region Toggle
        public static bool toggleInt(ref int val)
        {
            checkLine();
            var before = val > 0;
            if (toggle(ref before))
            {
                val = before ? 1 : 0;
                return change;
            }
            return false;
        }

        public static bool toggle(ref bool val)
        {
            BeginCheckLine();
            val = EditorGUILayout.Toggle(val, GUILayout.MaxWidth(40));
            return EndCheckLine();
        }

        public static bool toggle(int ind, CountlessBool tb)
        {
            var has = tb[ind];
            if (toggle(ref has))
            {
                tb.Toggle(ind);
                return true;
            }
            return false;
        }

        public static bool toggle(ref bool val, GUIContent cnt)
        {
            BeginCheckLine();
            val = EditorGUILayout.Toggle(cnt, val);
            return EndCheckLine();
        }

        #endregion

        #region Click
        public static bool Click(string label)
        {
            checkLine();
            return GUILayout.Button(label) && change;
        }
        
        public static bool Click(string label, GUIStyle style)
        {
            checkLine();
            return GUILayout.Button(label, style) && change;
        }

        public static bool Click(GUIContent content)
        {
            checkLine();
            return GUILayout.Button(content) && change;
        }

        public static bool Click(GUIContent content, GUIStyle style)
        {
            checkLine();
            return GUILayout.Button(content, style) && change;
        }

        public static bool Click(GUIContent content, int width, GUIStyle style)
        {
            checkLine();
            return GUILayout.Button(content, style, GUILayout.MaxWidth(width)) && change;
        }

        public static bool Click(Texture image, int width, GUIStyle style = null)   {
            if (style == null)
                style = PEGI_Styles.ImageButton;

            checkLine();
            return GUILayout.Button(image, style, GUILayout.MaxHeight(width), GUILayout.MaxWidth(width + 10)) && change;
        }

        public static bool ClickImage(GUIContent cnt, int width, GUIStyle style = null) => ClickImage(cnt, width, width, style);
        
        public static bool ClickImage(GUIContent cnt, int width, int height, GUIStyle style = null)
        {
            if (style == null)
                style = PEGI_Styles.ImageButton;

            checkLine();

            return GUILayout.Button(cnt, style, GUILayout.MaxWidth(width + 10), GUILayout.MaxHeight(height)).Dirty();
        }

        #endregion

        #region write

        private static GUIContent imageAndTip = new GUIContent();

        private static GUIContent ImageAndTip(Texture tex, string toolTip)
        {
            imageAndTip.image = tex;
            imageAndTip.tooltip = toolTip;
            return imageAndTip;
        }

        private static GUIContent ImageAndTip(Texture tex)
        {
            imageAndTip.image = tex;
            imageAndTip.tooltip = tex ? tex.name : "Null Image";
            return imageAndTip;
        }

        public static void write<T>(T field) where T : Object
        {
            checkLine();
            EditorGUILayout.ObjectField(field, typeof(T), false);
        }

        public static void write<T>(T field, int width) where T : Object
        {
            checkLine();
            EditorGUILayout.ObjectField(field, typeof(T), false, GUILayout.MaxWidth(width));
        }

        public static void write(GUIContent cnt, int width, int height)  {

            checkLine();
            GUI.enabled = false;
            Color.clear.SetBgColor();
            GUILayout.Button(cnt, PEGI_Styles.ImageButton, GUILayout.MaxWidth(width + 10), GUILayout.MaxHeight(height));
            pegi.PreviousBgColor();
            GUI.enabled = true;

        }

        public static void write(string text, int width)
        {

            checkLine();
            EditorGUILayout.LabelField(text, EditorStyles.miniLabel, GUILayout.MaxWidth(width));
        }

        public static void write(GUIContent cnt)
        {
            checkLine();
            EditorGUILayout.LabelField(cnt, PEGI_Styles.WrappingText);
        }

        public static void write(Texture tex, int width)
        {
           checkLine();

           GUILayout.Label(tex, GUILayout.MaxWidth(width), GUILayout.MaxHeight(width));
        }

        public static void write(Texture tex, string tip, int width)
        {
            checkLine();

            GUILayout.Label(ImageAndTip(tex, tip), GUILayout.MaxWidth(width), GUILayout.MaxHeight(width));
        }

        public static void write(Texture tex, string tip, int width, int height)
        {
            checkLine();

            GUILayout.Label(ImageAndTip(tex, tip), GUILayout.MaxWidth(width), GUILayout.MaxHeight(height));
        }

        public static void write(GUIContent cnt, int width)
        {
            checkLine();
            EditorGUILayout.LabelField(cnt, PEGI_Styles.WrappingText, GUILayout.MaxWidth(width));
        }

        public static void write(string text)
        {
            checkLine();
            EditorGUILayout.LabelField(text, PEGI_Styles.WrappingText);
        }

        public static void write(string text, GUIStyle style )  {
            checkLine();
            EditorGUILayout.LabelField(text, style);
        }

        public static void write(string text, int width, GUIStyle style) {
            checkLine();
            EditorGUILayout.LabelField(text, style, GUILayout.MaxWidth(width));
        }

        public static void write(GUIContent cnt, int width, GUIStyle style)
        {
            checkLine();
            EditorGUILayout.LabelField(cnt, style, GUILayout.MaxWidth(width));
        }

        public static void write(GUIContent cnt, GUIStyle style)
        {
            checkLine();
            EditorGUILayout.LabelField(cnt, style);
        }

        public static void writeHint(string text, MessageType type)  {
            checkLine();
            EditorGUILayout.HelpBox(text, type);
        }
        #endregion

        public static IEnumerable<T> DropAreaGUI<T>() where T : UnityEngine.Object
        {
            newLine();

            var evt = Event.current;
            var drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            
            GUILayout.Box("Drag & Drop");

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        yield break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var o in DragAndDrop.objectReferences)
                        {
                            var cnvrt = o as T;
                            if (cnvrt)
                                yield return cnvrt;
                        }
                    }
                    break;
            }
        }
        
        #region Reordable List
        
        private static readonly Dictionary<IList, ReorderableList> ReorderableList = new Dictionary<IList, ReorderableList>();

        static ReorderableList GetReordable<T>(this List<T> list, ListMetaData metaDatas)
        {
            ReorderableList rl;
            ReorderableList.TryGetValue(list, out rl);

            if (rl != null) return rl;
            
            rl = new ReorderableList(list, typeof(T), metaDatas == null || metaDatas.allowReorder, true, false, metaDatas == null || metaDatas.allowDelete);
            ReorderableList.Add(list, rl);

            rl.drawHeaderCallback += DrawHeader;
            rl.drawElementCallback += DrawElement;
            rl.onRemoveCallback += RemoveItem;
            
            return rl;
        }
        
        private static IList _currentReorderedList;
        private static Type _currentReorderedType;
        private static List<Type> _currentReorderedListTypes;
        private static TaggedTypesCfg _currentTaggedTypes;
        private static ListMetaData _listMetaData;

        private static bool GetIsSelected(int ind) => (_listMetaData != null) ? _listMetaData.GetIsSelected(ind) : pegi.selectedEls[ind];

        private static void SetIsSelected(int ind, bool val) {
            if (_listMetaData != null)
                _listMetaData.SetIsSelected(ind, val);
            else
                pegi.selectedEls[ind] = val;
        }
        
        public static bool reorder_List<T>(List<T> l, ListMetaData metas)
        {
            _listMetaData = metas;
    
            EditorGUI.BeginChangeCheck();

            if (_currentReorderedList != l) {

                _currentReorderedListTypes = typeof(T).TryGetDerivedClasses();

                if (_currentReorderedListTypes == null)
                {
                    _currentTaggedTypes = typeof(T).TryGetTaggedClasses();
                    if (_currentTaggedTypes != null)
                        _currentReorderedListTypes = _currentTaggedTypes.Types;
                }
                else _currentTaggedTypes = null;

                _currentReorderedType = typeof(T);
                _currentReorderedList = l;
                if (metas == null)
                    pegi.selectedEls.Clear();

            }



            l.GetReordable(metas).DoLayoutList();
            return EditorGUI.EndChangeCheck();
        }
        
        private static void DrawHeader(Rect rect) => GUI.Label(rect, "Ordering {0} {1}s".F(_currentReorderedList.Count.ToString(), _currentReorderedType.ToPegiStringType()));

        private static GUIContent textAndToolTip = new GUIContent();

        private static void DrawElement(Rect rect, int index, bool active, bool focused) {
            
            var el = _currentReorderedList[index];

            var selected = GetIsSelected(index);

            var after = EditorGUI.Toggle(new Rect(rect.x, rect.y, 30, rect.height), selected);

            if (after != selected)
                SetIsSelected(index, after);

            rect.x += 30;
            rect.width -= 30;

            if (el != null) {
                
                    var ty = el.GetType();

                    textAndToolTip.text = ty.ToString();
                    textAndToolTip.tooltip = el.ToPegiString();
                 

                    var uo = el as Object;
                    if (uo)
                    {
                        var mb = uo as Component;
                        var go = mb ? mb.gameObject : uo as GameObject;

                        if (!go)
                            EditorGUI.ObjectField(rect, textAndToolTip, uo, _currentReorderedType, true);
                        else
                        {
                            var mbs = go.GetComponents<Component>();

                            if (mbs.Length > 1) { 

                                //rect.width = 100;
                                //EditorGUI.ObjectField(rect, cont, uo, _currentReorderedType, true);
                                //rect.x += 100;
                                //rect.width = 100;

                                if (select(ref mb, mbs, rect))
                                    _currentReorderedList[index] = mb;
                            }
                            else
                                EditorGUI.ObjectField(rect, textAndToolTip, uo, _currentReorderedType, true);
                        }
                    }
                    else
                    {

                        if (_currentReorderedListTypes != null)
                        {

                            rect.width = 100;
                            EditorGUI.LabelField(rect, textAndToolTip);
                            rect.x += 100;
                            rect.width = 100;

                            if (select_Type(ref ty, _currentReorderedListTypes, rect))
                                _currentReorderedList.TryChangeObjectType(index, ty, _listMetaData);
                        } else
                            EditorGUI.LabelField(rect, textAndToolTip);
                    }
            }
            else
            {
                var ed = _listMetaData.TryGetElement(index);
                
                if (ed != null && ed.unrecognized) {

                    if (_currentTaggedTypes != null)
                    {

                        textAndToolTip.text = "UNREC {0}".F(ed.unrecognizedUnderTag);
                        textAndToolTip.tooltip = "Select New Class";
                        
                        rect.width = 100;
                        EditorGUI.LabelField(rect, textAndToolTip);
                        rect.x += 100;
                        rect.width = 100;

                        Type ty = null;

                        if (select_Type(ref ty, _currentReorderedListTypes, rect)) {
                            el = Activator.CreateInstance(ty);
                            _currentReorderedList[index] = el;

                            var std = el as ICfg;

                            if (std != null) 
                                 std.Decode(ed.SetRecognized().stdDta);
                            
                        }
                    }
                    
                } else 
                EditorGUI.LabelField(rect, "Empty {0}".F(_currentReorderedType.ToPegiStringType()));
            }
        }
        
        private static void RemoveItem(ReorderableList list)
        {
            var i = list.index;
            var el = _currentReorderedList[i];
            if (el != null && _currentReorderedType.IsUnityObject())
                _currentReorderedList[i] = null;
            else
                _currentReorderedList.RemoveAt(i);
        }
        
        #endregion

    }
}
#pragma warning restore IDE1006 // Naming Styles
#endif