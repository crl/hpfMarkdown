using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;

namespace hpfMarkdown
{
 

    public class Util
    {
        public static string GetSizeDesc(long total,bool color=true)
        {
            var kOrMG = "K";
            double size_k = Math.Round(total / 1024.0, 2);
            var size = size_k;
            if (size_k > 900f)
            {
                double size_mb = Math.Round(total / 1024.0 / 1024.0, 2);
                kOrMG = "M";
                size = size_mb;
                if (size_mb > 900f)
                {
                    size = Math.Round(size_mb / 1024.0, 2);
                    kOrMG = "G";
                }
            }

            var result = size + kOrMG;
            if (color)
            {
                return MD.Color(result, Color.Pink);
            }

            return result;
        }

        public static long GetSizeBySpliStr(string splitSizeStr)
        {
            if (string.IsNullOrEmpty(splitSizeStr))
            {
                return 0L;
            }
            var size=string.Join("",splitSizeStr.Split(','));

            if (long.TryParse(size, out long result))
            {
                return result;
            }

            return 0L;
        }
        public static string GetTimeSpan(TimeSpan timeSpan)
        {
            var totalDays = timeSpan.TotalDays;
            if (totalDays > 30)
            {
                return "1个月前";
            }

            if (totalDays > 14)
            {
                return "2周前";
            }

            if (totalDays > 7)
            {
                return "1周前";
            }

            var color = Color.Green;

            if (totalDays > 1)
            {
                return MD.Color("{0}天前", color, (int) Math.Floor(totalDays));
            }

            if (timeSpan.TotalHours > 1)
            {
                return MD.Color("{0}小时前", color, (int) Math.Floor(timeSpan.TotalHours));
            }

            if (timeSpan.TotalMinutes > 1)
            {
                return MD.Color("{0}分钟前", color, (int) Math.Floor(timeSpan.TotalMinutes));
            }

            if (timeSpan.TotalSeconds > 1)
            {
                return MD.Color("{0}秒前", color, (int) Math.Floor(timeSpan.TotalSeconds));
            }

            return MD.Color("1秒前",color);
        }

        public static bool HasComparer<T>(T[] assets, T asset) where T : Nameable
        {
            foreach (var item in assets)
            {
                if (item.Name == asset.Name) return true;
            }

            return false;
        }

        public static MergeInfoVO<T> Merge<T>(T[] newList, T[] oldList) where T: Nameable
        {
            var result=new MergeInfoVO<T>();

            List<T> oldTmp;
            if (oldList != null)
            {
                oldTmp = oldList.ToList();
            }
            else
            {
                oldTmp=new List<T>();
            }

            foreach (var item in newList)
            {
                var mergeVO=new MergeVO<T>();
                mergeVO.data = item;
                var index=oldTmp.FindIndex(i => i.Name == item.Name);

                if (index != -1)
                {
                    mergeVO.type = MergeType.Def;
                    mergeVO.oldData = oldTmp[index];

                    oldTmp.RemoveAt(index);
                }
                else
                {
                    result.addCount++;
                    mergeVO.type = MergeType.Add;
                }
                result.list.Add(mergeVO);
            }

            foreach (var item in oldTmp)
            {
                var mergeVO = new MergeVO<T>();
                mergeVO.type = MergeType.Remove;
                mergeVO.data = item;
                result.list.Add(mergeVO);
                result.removeCount++;
            }
            return result;
        }
    }
}