﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using Newtonsoft.Json;

using System.Reflection;
using System.Collections;
using System.Collections.ObjectModel;

using ICities;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;

using Craxy.CitiesSkylines.ToggleTrafficLights.Tools;

namespace NetworkInterface
{
    public class Network
    {
        public static Queue<int> selectedNodeIds = new Queue<int>(4);

        public static void UpdateSelectedIds(int nodeId)
        {
            if (!selectedNodeIds.Contains(nodeId))
            {
                if (selectedNodeIds.Count == 4)
                {
                    selectedNodeIds.Dequeue();
                }
                selectedNodeIds.Enqueue(nodeId);
            }
            try
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                    "Selected Intersection IDs: " + JsonConvert.SerializeObject(selectedNodeIds));
            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error,
                    e.Message);
            }
        }

        public static NetNode SelectNode(int index)
        {
            if (index > 3 || index < 0)
            {
                throw new Exception("Node index must be an integer between 0 and 3!");
            }
            if ((selectedNodeIds.Count - 1) < index)
            {
                throw new Exception("Node index out of bounds; set of lights not fully selected!");
            }
            int nodeId = selectedNodeIds.ElementAt(index);
            return Craxy.CitiesSkylines.ToggleTrafficLights.Tools.ToggleTrafficLightsTool.GetNetNode(nodeId);
        }

        public object HandleRequest(string jsonRequest)
        {
            object retObj = null;
            Request request;
            // parse the message according to Request formatting
            try
            {
                request = JsonConvert.DeserializeObject<Request>(jsonRequest);
            }
            catch (Exception e)
            {
                throw new Exception("Error: request not properly formatted: " + e.Message);
            }

            // got well formatted message, now process it
            if (request.Method == MethodType.GET)
            {
                retObj = GetObject(request.Object);
            }
            else if (request.Method == MethodType.SET)
            {

            }
            else if (request.Method == MethodType.EXECUTE)
            {

            }
            else if (request.Method == MethodType.GETDENSITY)
            {
                object nodeIdObj = GetObject(request.Object);
                int nodeId = Convert.ToInt32(nodeIdObj);
                retObj = GetSegmentDensity(nodeId, 0);
            }
            else if (request.Method == MethodType.GETSTATE)
            {
                object nodeIdObj = GetObject(request.Object);
                int nodeId = Convert.ToInt32(nodeIdObj);
                retObj = GetNodeState(nodeId);
            }
            else if (request.Method == MethodType.SETSTATE)
            {

            }
            else
            {
                throw new Exception("Error: unsupported method type!");
            }

            return retObj;
        }

        public object GetSegmentDensity(int nodeId, int segId)
        {
            byte density = 0;
            NetNode node = SelectNode(nodeId);
            return density;
        }

        public object GetNodeState(int nodeId)
        {
            NetNode node = SelectNode(nodeId);
            NetSegment[] segments = 
                {
NetManager.instance.m_segments.m_buffer[node.m_segment0],
NetManager.instance.m_segments.m_buffer[node.m_segment1],
NetManager.instance.m_segments.m_buffer[node.m_segment2],
NetManager.instance.m_segments.m_buffer[node.m_segment3],
NetManager.instance.m_segments.m_buffer[node.m_segment4],
NetManager.instance.m_segments.m_buffer[node.m_segment5],
NetManager.instance.m_segments.m_buffer[node.m_segment6],
NetManager.instance.m_segments.m_buffer[node.m_segment7]
};

            /*
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                "" + node.m_segment0 +
                ", " + node.m_segment1 +
                ", " + node.m_segment2 +
                ", " + node.m_segment3 +
                ", " + node.m_segment4 +
                ", " + node.m_segment5 +
                ", " + node.m_segment6 +
                ", " + node.m_segment7);
                */

            Dictionary<string, object> retObj = new Dictionary<string, object>();
            int i = 0;
            foreach (NetSegment seg in segments)
            {
                NetSegment theSeg = seg;
                if (theSeg.m_flags > 0)
                {
                    Dictionary<string, RoadBaseAI.TrafficLightState> segDict = new Dictionary<string, RoadBaseAI.TrafficLightState>();
                    RoadBaseAI.TrafficLightState vehicleState;
                    RoadBaseAI.TrafficLightState pedState;
                    RoadBaseAI.GetTrafficLightState((ushort)nodeId, ref theSeg,
                        SimulationManager.instance.m_currentFrameIndex,
                        out vehicleState,
                        out pedState);
                    segDict.Add("vehicle", vehicleState);
                    segDict.Add("pedestrian", vehicleState);
                    retObj.Add("segment" + i, segDict);
                }
                i++;
            }
            return retObj;
        }

        public object GetObject(NetworkObject obj)
        {
            object retObj = null;

            // get required/dependent context now (recursively)
            Type contextType = null;
            object ctx = null;
            if (obj.Dependency != null)
            {
                ctx = GetObject(obj.Dependency);
            }
            if (ctx != null)
            {
                contextType = ctx as Type;
                if (contextType != null)
                {
                    ctx = null;
                }
                else
                {
                    contextType = ctx.GetType();
                }
                if (obj.IsStatic)
                {
                    ctx = null;
                }
            }

            /*
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
            "Getting: " + obj.Name + " of type: " + obj.Type + " from context:" + ctx);
            */

            // get object data now
            if (obj.Type == ObjectType.CLASS)
            {
                Type t = GetAssemblyType(obj.Assembly, obj.Name);
                if (t == null)
                {
                    throw new Exception("Couldn't get: " + obj.Name + " from assembly: " + obj.Assembly);
                }
                retObj = t;
            }
            else if (obj.Type == ObjectType.MEMBER || obj.Type == ObjectType.METHOD)
            {
                retObj = GetObjectMember(contextType, ctx, obj);
            }
            else if (obj.Type == ObjectType.PARAMETER) // do we need this type?
            {
            }
            else
            {
                throw new Exception("Usupported object type: "+obj.Type);
            }

            // set the value of the object if it exists
            if (obj.Value != null)
            {
                // need to figure out here how to decide what to do
                Type t = Type.GetType(obj.ValueType);
                if (t == null)
                {
                    t = GetAssemblyType(obj.Assembly, obj.ValueType);
                    retObj = Enum.Parse(t, obj.Value);  // won't always just be an enum...
                }
                else
                {
                    retObj = Convert.ChangeType(obj.Value, t);
                }
            }

            return retObj;
        }

        public object GetObjectMember(Type contextType, object ctx, NetworkObject obj)
        {
            object retObj = null;
            // make sure we have context!
            if (contextType != null)
            {
                // get parameters (if they exist)
                List<object> parameters = new List<object>();
                if (obj.Parameters != null)
                {
                    for (int i = 0; i < obj.Parameters.Count; i++)
                    {
                        object param = GetObject(obj.Parameters.ElementAt(i));
                        DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                            "Got parameter: " + param.ToString());
                        parameters.Add(param);
                    }
                }
                // now actually get the member
                MemberInfo[] mia = contextType.GetMember(obj.Name);
                foreach (var mi in mia)
                {
                    if (mi.MemberType == MemberTypes.Method)
                    {
                        MethodInfo methodInfo = (MethodInfo)mi;
                        if (methodInfo != null)
                        {
                            if (methodInfo.IsGenericMethod)
                            {
                                methodInfo = ((MethodInfo)mi).MakeGenericMethod(contextType);
                            }
                            retObj = methodInfo.Invoke(ctx, parameters.ToArray());
                        }
                        break;
                    }
                    else if (mi.MemberType == MemberTypes.Property)
                    {
                        PropertyInfo pi = (PropertyInfo)mi;
                        if (pi != null)
                        {
                            MethodInfo methodInfo = pi.GetAccessors()[0];
                            if (methodInfo != null)
                            {
                                retObj = methodInfo.Invoke(ctx, null);
                            }
                        }
                        break;
                    }
                    else if (mi.MemberType == MemberTypes.Field)
                    {
                        FieldInfo fi = (FieldInfo)mi;
                        if (fi != null)
                        {
                            retObj = fi.GetValue(ctx);
                        }
                        break;
                    }
                }
            }
            return retObj;
        }

        public Type GetAssemblyType(string assemblyName, string typeName)
        {
            return Assembly.Load(assemblyName).GetType(typeName);
        }

        public void SetValueFromString(object target, string propertyName, string propertyValue)
        {
            PropertyInfo pi = target.GetType().GetProperty(propertyName);
            Type t = pi.PropertyType;

            if (t.IsGenericType &&
                t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (propertyValue == null)
                {
                    pi.SetValue(target, null, null);
                    return;
                }
                t = new NullableConverter(pi.PropertyType).UnderlyingType;
            }
            pi.SetValue(target, Convert.ChangeType(propertyValue, t), null);
        }
    }
}
