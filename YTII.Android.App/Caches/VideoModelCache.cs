#region file_header

// QuickVideoInfo - YTII.Android.App - VideoModelCache.cs
// 
// This file is licensed to you under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express 
// or implied.  See the License for the specific language governing permissions and limitations under the License.
//  

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Android.Util;
using YTII.ModelFactory.Models;

namespace YTII.Droid.App.Caches
{
    public class VideoModelCache<T> : IVideoModelCache<T>
        where T : IVideoModel
    {
        const int MaxItems = 20;

        static readonly Dictionary<string, T> CachedList = new Dictionary<string, T>();

        static readonly Queue<string> IdOrderQueue = new Queue<string>(20);


        internal int ItemCount => CachedList.Count;

        internal int CacheHits { get; private set; }

        internal int CacheMisses { get; private set; }


        public void Add(T item)
        {
            if (item == null || string.IsNullOrEmpty(item.VideoId))
            {
                Log.Info($"YTII.{nameof(VideoModelCache<T>)}.{nameof(Add)}", $"Cannot Add Null Item/VideoId");
                return;
            }

            if (CachedList.Count >= MaxItems)
                CachedList.Remove(IdOrderQueue.Dequeue());

            if (!IdOrderQueue.Contains(item.VideoId))
                IdOrderQueue.Enqueue(item.VideoId);

            if (!CachedList.ContainsKey(item.VideoId))
                CachedList.Add(item.VideoId, item);

            Log.Info($"YTII.{nameof(VideoModelCache<T>)}.{nameof(Add)}", $"Cache Item Added");
        }

        public bool IsCached(string videoId)
        {
            var isCached = CachedList.ContainsKey(videoId);

            if (isCached)
                CacheHits++;
            else
                CacheMisses++;

            return isCached;
        }

        public T GetItem(string videoId)
        {
            if (!CachedList.ContainsKey(videoId))
                return default(T);

            try
            {
                var tempQueue = IdOrderQueue.Where(i => i != videoId).Reverse().ToList();
                IdOrderQueue.Clear();

                foreach (var i in tempQueue)
                    IdOrderQueue.Enqueue(i);

                IdOrderQueue.Enqueue(videoId);
            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(VideoModelCache<T>)}.{nameof(GetItem)}.MoveIdToBottom", ex.Message);
            }

            Log.Info($"YTII.{nameof(VideoModelCache<T>)}.{nameof(GetItem)}", $"Found Cached Video Item");
            return CachedList[videoId];
        }
    }
}