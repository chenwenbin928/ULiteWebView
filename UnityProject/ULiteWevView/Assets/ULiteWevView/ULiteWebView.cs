﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Jing.ULiteWebView
{
    /// <summary>
    /// Code By Jing 2018-03-04
    /// EMail:122300890@qq.com
    /// </summary>
    public class ULiteWebView : MonoBehaviour
    {

        [Header("距离屏幕上边缘距离")]
        public int top = 0;
        [Header("距离屏幕下边缘距离")]
        public int bottom = 0;
        [Header("距离屏幕左边缘距离")]
        public int left = 0;
        [Header("距离屏幕右边缘距离")]
        public int right = 0;

        AULite4Platform _ulite;
        Dictionary<string, Action<String>> _jsActionsDic = new Dictionary<string, Action<string>>();

        void Start()
        {
#if !UNITY_EDITOR
    #if UNITY_ANDROID
                    _ulite = new ULiteAndroidWebView(getFullName(this.gameObject));
    #elif UNITY_IOS
                _ulite = new ULiteIosWebView(getFullName(this.gameObject));
    #endif
#endif
        }


        void Update()
        {

        }

        /// <summary>
        /// 显示
        /// </summary>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public void Show()
        {            
            if (null == _ulite)
            {
                return;
            }

            _ulite.Show(top, bottom, left, right);
        }

        /// <summary>
        /// 使用WebView加载指定的URL，访问网页用Http://开头
        /// </summary>
        /// <param name="url">访问的URL地址</param>
        public void LoadUrl(string url)
        {
            if (null == _ulite)
            {
                Application.OpenURL(url);
                return;
            }

            _ulite.LoadUrl(url);
        }

        /// <summary>
        /// 访问StreamingAssets文件夹中存放的资源
        /// </summary>
        /// <param name="filePath">相对于StreamingAssets目录的文件路径，以"/"开头</param>
        public void LoadLocal(string filePath)
        {
#if UNITY_ANDROID
            filePath = "file:///android_asset" + filePath;
#else
            filePath = "file://" + Application.streamingAssetsPath + filePath;
#endif
            LoadUrl(filePath);
        }

        /// <summary>
        /// 关闭ULiteWebView关联的WebView
        /// </summary>
        public void Close()
        {
            if (null == _ulite)
            {
                return;
            }

            _ulite.Close();
        }

        /// <summary>
        /// 请求当前WebView页面中对应的JS方法
        /// </summary>
        /// <param name="funName">Fun name.</param>
        /// <param name="msg">Message.</param>
        public void CallJS(string funName, string msg)
        {
            if (null == _ulite)
            {
                return;
            }

            _ulite.CallJS(funName, msg);
        }

        void OnJsCall(string msg){
            Debug.Log("js call unity: " + msg);
            string iName = null;
            string paramsStr = null;

            try
            {
                int flag = msg.IndexOf("?");
                if (-1 == flag)
                {
                    iName = msg;
                }
                else
                {
                    iName = msg.Substring(0, flag);
                    paramsStr = msg.Substring(flag + 1);
                }

                if (_jsActionsDic.ContainsKey(iName))
                {
                    _jsActionsDic[iName](paramsStr);
                }
            }
            catch(Exception e)
            {
                Debug.Log(string.Format("ULiteWebView：Wrong JS Msg [{0}]", msg));
            }
        }

        

        /// <summary>
        /// 注册供JS调用的方法
        /// </summary>
        /// <param name="funName">方法名：JS通过该方法名调用对应方法</param>
        /// <param name="fun">方法</param>
        public void RegistJsInterfaceAction(string interfaceName, Action<String> action){
            _jsActionsDic[interfaceName] = action;
        }


        /// <summary>
        /// 注销供JS调用的方法
        /// </summary>
        /// <param name="interfaceName">方法名：JS通过该方法名调用对应方法</param>
        /// <param name="action">方法</param>
        public void UnregistJsInterfaceAction(string interfaceName, Action<String> action)
        {
            if (_jsActionsDic.ContainsKey(interfaceName))
            {
                _jsActionsDic.Remove(interfaceName);
            }
        }


        /// <summary>
        /// 获取GameObject的完整路径名
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string getFullName(GameObject obj)
        {
            string fullName = gameObject.name;
            Transform temp = transform;
            do
            {
                temp = temp.parent;
                if (null != temp)
                {
                    fullName = temp.name + "/" + fullName;
                }
            }
            while (temp != null);
            return fullName;
        }
    }

    abstract class AULite4Platform
    {

        /// <summary>
        /// 显示
        /// </summary>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        abstract public void Show(int top, int bottom, int left, int right);

        /// <summary>
        /// 加载页面
        /// </summary>
        /// <param name="url"></param>
        abstract public void LoadUrl(string url);

        /// <summary>
        /// 关闭
        /// </summary>
        abstract public void Close();

        /// <summary>
        /// 调用JS
        /// </summary>
        /// <param name="funName">方法名</param>
        /// <param name="msg">消息内容</param>
        abstract public void CallJS(string funName, string msg);


    }

    class ULiteAndroidWebView : AULite4Platform
    {
        AndroidJavaObject _ajo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObjectName">Game object name.</param>
        public ULiteAndroidWebView(string gameObjectName)
        {
            _ajo = new AndroidJavaObject("com.jing.unity.ulitewebview.ULiteWebView");
            _ajo.Call("registCallBackGameObjectName", gameObjectName);
        }

        public override void CallJS(string funName, string msg)
        {
            _ajo.Call("callJs", funName, msg);
        }

        public override void Close()
        {
            _ajo.Call("close");
        }

        public override void LoadUrl(string url)
        {
            _ajo.Call("loadUrl", url);
        }

        public override void Show(int top, int bottom, int left, int right)
        {
            _ajo.Call("show", top, bottom, left, right);
        }
    }

    class ULiteIosWebView : AULite4Platform
    {
        [DllImport("__Internal")]
        private static extern void _registCallBackGameObjectName(string gameObjectName);

        [DllImport("__Internal")]
        private static extern void _show(int top, int bottom, int left, int right);

        [DllImport("__Internal")]
        private static extern void _loadUrl(string url);

        [DllImport("__Internal")]
        private static extern void _close();

        [DllImport("__Internal")]
        private static extern void _callJS(string funName, string msg);

        
        public ULiteIosWebView(string gameObjectName){
            _registCallBackGameObjectName(gameObjectName);
        }

        public override void CallJS(string funName, string msg)
        {
            _callJS(funName,msg);
        }

        public override void Close()
        {
            _close();
        }

        public override void LoadUrl(string url)
        {
            _loadUrl(url);
        }

        public override void Show(int top, int bottom, int left, int right)
        {
            _show(top, bottom, left, right);
        }
    }
}
