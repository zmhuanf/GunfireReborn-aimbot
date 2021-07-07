using HeroCameraName;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using System;
using System.Diagnostics;
using UnityEngine;

namespace zcMod
{
    public static class BuildInfo
    {
        public const string Name = "zcMod";
        public const string Description = "Gunfire Reborn Aimbot";
        public const string Author = "Author:zhuchong; Modify:zmhuanf";
        public const string Company = "tms";
        public const string Version = "1.2.6";
        public const string DownloadLink = null;
    }

    public class zcMod : MelonMod
    {
        public static bool flag = true;
        public static bool repeat_lock = true;
        public static Stopwatch sw = new Stopwatch();

        public override void OnUpdate()
        {
            // 左Ctrl切换是否启用自动瞄准
            if (Input.GetKey(KeyCode.LeftControl) && repeat_lock)
            {
                flag = !flag;
                repeat_lock = false;
                sw.Restart();
            }
            if (sw.ElapsedMilliseconds >= 1000)
            {
                repeat_lock = true;
                sw.Stop();
            }
            // 获取所有怪物
            if (flag)
            {
                try
                {
                    List<NewPlayerObject> monsters = NewPlayerManager.GetMonsters();
                    if (monsters != null)
                    {
                        Vector3 campos = CameraManager.MainCamera.position;
                        Transform nearmons = null;
                        float neardis = 99999f;
                        foreach (NewPlayerObject monster in monsters)
                        {
                            Transform montrans = null;
                            try
                            {
                                montrans = monster.BodyPartCom.GetCenterTrans().parent.FindChild("Bip001 Neck").FindChild("Bip001 Head");
                            }
                            catch (Exception)
                            {
                                // 如无法获取怪物头部坐标则忽略该怪物
                            }
                            if (montrans == null)
                            {
                                continue;
                            }
                            // 获取距离
                            Vector3 vec = montrans.position - campos;
                            float curdis = vec.magnitude;
                            // 射线，判断是否可见
                            Ray ray = new Ray(campos, vec);
                            var hits = Physics.RaycastAll(ray, curdis);
                            bool visible = true;
                            foreach (var hit in hits)
                            {
                                if (hit.collider.gameObject.layer == 0 || hit.collider.gameObject.layer == 30 || hit.collider.gameObject.layer == 31) //&& hit.collider.name.Contains("_")
                                {
                                    visible = false;
                                    break;
                                }
                            }
                            // 获取最近怪物
                            if (visible)
                            {
                                if (curdis < neardis)
                                {
                                    neardis = curdis;
                                    nearmons = montrans;
                                }
                            }
                        }
                        // 调整摄像机
                        if (nearmons != null)
                        {
                            Vector3 objpos = HeroCameraManager.HeroObj.gameTrans.position;
                            Vector3 fw = nearmons.position - objpos;
                            fw.y = -0.1f;
                            Quaternion rot = Quaternion.LookRotation(fw);
                            HeroCameraManager.HeroObj.gameTrans.rotation = rot;
                            fw = nearmons.position - campos;
                            fw.y += 0.2f;
                            Quaternion rot2 = Quaternion.LookRotation(fw);
                            CameraManager.MainCamera.rotation = rot2;
                        }
                    }
                }
                catch (Exception)
                {
                    // 捕获所有错误
                }
            }
        }

        // 测试用函数
        public void Test(Transform t, int deep)
        {
            MelonLogger.Log("deep_" + deep + "_name:" + t.name);
            MelonLogger.Log("deep_" + deep + "_pos:" + t.position.ToString());
            deep += 1;
            for (int i = 0; i < t.GetChildCount(); ++i)
            {
                Transform t2 = t.GetChild(i);
                Test(t2, deep);
            }
        }
    }
}
