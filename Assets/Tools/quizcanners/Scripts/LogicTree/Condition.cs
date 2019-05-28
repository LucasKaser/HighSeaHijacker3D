﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using PlayerAndEditorGUI;
using QuizCannersUtilities;

namespace STD_Logic
{

    public interface IAmConditional {
        bool CheckConditions(Values values);
    }
    
    public enum ConditionType {Above, Below, Equals, RealTimePassedAbove, RealTimePassedBelow, VirtualTimePassedAbove, VirtualTimePassedBelow, NotEquals }

    [DerivedList(typeof(ConditionLogicBool), typeof(ConditionLogicInt), typeof(TestOnceCondition))]
    public class ConditionLogic : ValueIndex, IPEGI_ListInspect  {

        #region Encode & Decode
        public override CfgEncoder Encode() => new CfgEncoder().Add("ind", EncodeIndex());

        public override bool Decode(string tg, string data) => true;
        #endregion

        public virtual bool TryForceConditionValue(Values values, bool toTrue) => false;

        public virtual bool TestFor(Values values) => false;

        public virtual int IsItClaimable( int dir, Values values) => -2;
        
        public override bool IsBoolean => false;

        protected override bool SearchTriggerSameType => false;
        
        #region Inspector
        #if !NO_PEGI

        public static Values inspectedTarget = null;

        public override bool PEGI_inList_Sub(IList list, int ind, ref int inspecte)
        {
            var changed = FocusedField_PEGI(ind, "Cond");

            Trigger.Usage.Inspect(this);

            return changed;
        }

        #endif
        #endregion

        public ConditionLogic()
        {
            if (TriggerGroup.Browsed != null)
                groupIndex = TriggerGroup.Browsed.IndexForPEGI;
        }

    }

    public class ConditionLogicBool : ConditionLogic {

        public bool compareValue;

        #region Encode & Decode
        public override CfgEncoder Encode() => new CfgEncoder()
            .Add_IfTrue("b", compareValue)
            .Add("ind", EncodeIndex());

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "b": compareValue = data.ToBool(); break;
                case "ind": data.Decode_Delegate(DecodeIndex); break;
                default: return false;
            }
            return true;
        }
        #endregion

        #region Inspect
        #if !NO_PEGI

        public override string NameForDisplayPEGI => "if {0}{1}".F(compareValue ? "" : "NOT ",base.NameForDisplayPEGI);

        #endif
        #endregion

        public override bool TryForceConditionValue(Values values, bool toTrue)
        {
            SetBool(values, toTrue ? compareValue : !compareValue);
            LogicMGMT.currentLogicVersion++;
            return true;
        }

        protected override bool SearchTriggerSameType => true;

        public override bool TestFor(Values values) => GetBool(values) == compareValue;

        public override int IsItClaimable(int dir, Values st) => (dir > 0) == (compareValue) ?  1 : -2;
        
        public override bool IsBoolean => true;

    }

    public class ConditionLogicInt : ConditionLogic {
        public ConditionType type;
        public int compareValue;

        #region Encode & Decode
        public override CfgEncoder Encode() => new CfgEncoder()
            .Add_IfNotZero("v", compareValue)
            .Add_IfNotZero("ty", (int)type)
            .Add("ind", EncodeIndex);

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "v": compareValue = data.ToInt(); break;
                case "ty": type = (ConditionType)data.ToInt(); break;
                case "ind": data.Decode_Delegate(DecodeIndex); break;
                default: return false;
            }
            return true;
        }
        #endregion

        #region Inspect
#if !NO_PEGI

        public override string NameForDisplayPEGI {
            get  {
                var name = "If {0} {1} {2}".F(base.NameForDisplayPEGI, type.GetName(), compareValue);
                return name;
            }
        }

#endif
        #endregion

        protected override bool SearchTriggerSameType => true;

        public override bool TryForceConditionValue(Values value,bool toTrue) {

            if (TestFor(value) == toTrue)
                return true;
            
            if (toTrue) {
                switch (type) { 
                    case ConditionType.Above:                   SetInt(value, compareValue + 1);                                                break;
                    case ConditionType.Below:                   SetInt(value, compareValue - 1);                                                break;
                    case ConditionType.Equals:                  SetInt(value, compareValue);                                                    break;
                    case ConditionType.NotEquals:               if (GetInt(value) == compareValue) value.ints[groupIndex].Add(triggerIndex, 1); break;
                    case ConditionType.RealTimePassedAbove:     SetInt(value, LogicMGMT.RealTimeNow() - compareValue - 1);                      break;
                    case ConditionType.RealTimePassedBelow:     SetInt(value, LogicMGMT.RealTimeNow());                                         break;
                    case ConditionType.VirtualTimePassedAbove:  SetInt(value, (int)Time.time - compareValue - 1);                               break;
                    case ConditionType.VirtualTimePassedBelow:  SetInt(value, (int)Time.time);                                                  break;
                }
            } else {
                switch (type) {
                    case ConditionType.Above:                   SetInt(value, compareValue - 1);                                                break;
                    case ConditionType.Below:                   SetInt(value, compareValue + 1);                                                break;
                    case ConditionType.Equals:                  SetInt(value, compareValue + 1);                                                break;
                    case ConditionType.NotEquals:               SetInt(value, compareValue);                                                    break;
                    case ConditionType.RealTimePassedAbove:     SetInt(value, LogicMGMT.RealTimeNow());                                         break;
                    case ConditionType.RealTimePassedBelow:     SetInt(value, LogicMGMT.RealTimeNow() - compareValue - 1);                      break;
                    case ConditionType.VirtualTimePassedAbove:  SetInt(value, (int)Time.time );                                                 break;
                    case ConditionType.VirtualTimePassedBelow:  SetInt(value, (int)Time.time - compareValue - 1);                               break;
                }
            }

            LogicMGMT.currentLogicVersion++;

            return true;
        }

        public override bool TestFor(Values st)
        {
            int timeGap;

            switch (type)
            {
                case ConditionType.Above:                   if (GetInt(st) > compareValue) return true;                     break;
                case ConditionType.Below:                   if (GetInt(st) < compareValue) return true;                     break;
                case ConditionType.Equals:                  if (GetInt(st) == compareValue) return true;                    break;
                case ConditionType.NotEquals:               if (GetInt(st) != compareValue) return true;                    break;

                case ConditionType.VirtualTimePassedAbove:
                                                            timeGap = (int)Time.time - GetInt(st);
                                                            if (timeGap > compareValue) return true;
                                                            LogicMGMT.inst.AddTimeListener(compareValue - timeGap);         break;

                case ConditionType.VirtualTimePassedBelow:
                                                            timeGap = (int)Time.time - GetInt(st);
                                                            if (timeGap < compareValue) {
                                                                LogicMGMT.inst.AddTimeListener(compareValue - timeGap);
                                                                return true;
                                                            }
                                                                                                                            break;
                case ConditionType.RealTimePassedAbove:
                                                            timeGap = (LogicMGMT.RealTimeNow() - GetInt(st));
                                                            if (timeGap > compareValue) return true;
                                                                LogicMGMT.inst.AddTimeListener(compareValue - timeGap);     break;

                case ConditionType.RealTimePassedBelow:
                                                            timeGap = (LogicMGMT.RealTimeNow() - GetInt(st));
                                                            if (timeGap < compareValue) {
                                                                LogicMGMT.inst.AddTimeListener(compareValue - timeGap);
                                                                return true;
                                                            }                                                               break;

            }
            return false;
        }

        public override int IsItClaimable(int dir, Values st) {
            switch (type) {
                case ConditionType.Above:   if ((GetInt(st) < compareValue) && (dir > 0)) return (compareValue - GetInt(st) + 1);       break;
                case ConditionType.Below:   if ((GetInt(st) > compareValue) && (dir < 0)) return (GetInt(st) - compareValue + 1);       break;
                case ConditionType.Equals:  if ((GetInt(st) > compareValue) == (dir < 0)) return Mathf.Abs(GetInt(st) - compareValue);  break;
            }

            return -2;
        }
    }
    
    public class TestOnceCondition : ConditionLogicBool
    {

        #region Inspector
        #if !NO_PEGI
        public override bool PEGI_inList(IList list, int ind, ref int edited)
        {
            bool changed = FocusedField_PEGI(ind, "Cond");

            Trigger.Usage.Inspect(this);

            changed |= SearchAndAdd_Triggers_PEGI(0);

            return base.Inspect() || changed;
        }
        #endif
        #endregion

        public override bool TestFor(Values st)
        {
            bool value = base.TestFor(st);

            if (value)
                ResultType.SetBool.Apply(compareValue ? 0 : 1, this, st);

            return value;
        }

    }

    public static class ConditionExtensions
    {
        public static bool Test_And_For(this List<IAmConditional> lst, Values vals)
        {

            if (lst == null)
                return true;

            foreach (var e in lst)
                if (e != null && !e.CheckConditions(vals))
                    return false;


            return true;
        }

        public static bool IsTrue(this IAmConditional cond) => cond.CheckConditions(Values.global);

        public static bool TryTestCondition(this object obj)
        {
            var cnd = obj as IAmConditional;
            if (cnd == null) return true;
            return cnd.IsTrue();
        }

        public static string GetName(this ConditionType type)
        {
            switch (type) {
                case ConditionType.Equals:                  return "==";
                case ConditionType.Above:                   return ">";
                case ConditionType.Below:                   return "<";
                case ConditionType.NotEquals:               return "!=";
                case ConditionType.VirtualTimePassedAbove:  return "Game_Time passed > ";
                case ConditionType.VirtualTimePassedBelow:  return "Game_Time passed < ";
                case ConditionType.RealTimePassedAbove:     return "Real_Time passed > ";
                case ConditionType.RealTimePassedBelow:     return "Real_Time passed < ";
            }

            return "!!!Unrecognized Condition Type";
        }

    }

}