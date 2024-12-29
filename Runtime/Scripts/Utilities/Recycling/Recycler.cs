using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utilities.Recycling
{
    public static class Recycler
    {
        private static bool _isSetup;
        
        private static Dictionary<Type, Queue<IRecyclable>> _recycledObjects;
        private static Dictionary<Enum, Queue<GameObject>> _recycledEnumObjects;

        private static Transform _recyclingParentTransform;
        private static RectTransform _recyclingUIParentTransform;

        private static void SetupRecycling()
        {
            if (_isSetup)
                return;
            
            _recycledObjects = new Dictionary<Type, Queue<IRecyclable>>();
            _recycledEnumObjects = new Dictionary<Enum, Queue<GameObject>>();

            
            _recyclingParentTransform = new GameObject($"=== {nameof(Recycler).ToUpper()}PARENT ===").transform;
            Object.DontDestroyOnLoad(_recyclingParentTransform.gameObject);

            //Setup Canvas Parent
            //------------------------------------------------//
            var canvasParent = new GameObject($"=== {nameof(Canvas).ToUpper()}_{nameof(Recycler).ToUpper()}PARENT ===", typeof(Canvas));
            canvasParent.GetComponent<Canvas>().enabled = false;
            canvasParent.transform.SetParent(_recyclingParentTransform, false);
            _recyclingUIParentTransform = (RectTransform)canvasParent.transform;

            _isSetup = true;
        }

        //Type Recycling
        //============================================================================================================//
        
        public static void Recycle<T>(T toRecycle, bool reparent = true) where T: IRecyclable
        {
            if (_isSetup == false)
                SetupRecycling();

            var type = typeof(T);

            if (_recycledObjects.TryGetValue(type, out var recyclables) == false)
            {
                recyclables = new Queue<IRecyclable>();
                _recycledObjects.Add(type, recyclables);
            }

            toRecycle.IsRecycled = true;
            toRecycle.OnRecycled();
            toRecycle.gameObject.SetActive(false);

            if (reparent)
            {
                toRecycle.transform.SetParent(
                    toRecycle.transform is RectTransform ? _recyclingUIParentTransform : _recyclingParentTransform,
                    false);
            }
            
            recyclables.Enqueue(toRecycle);
        }
        
        public static bool TryGrab<T>(Transform parent, Vector3 localPosition, Quaternion localRotation, out T grabbed, bool setActive = true)  where T: IRecyclable
        {
            grabbed = default;
            
            if (_isSetup == false)
                return false;
            
            var type = typeof(T);
            if (_recycledObjects.TryGetValue(type, out var recyclables) == false)
                return false;

            if (recyclables.Count == 0)
                return false;

            var toReturn = (T)recyclables.Dequeue();
            toReturn.transform.SetParent(parent);
            toReturn.transform.localPosition = localPosition;
            toReturn.transform.localRotation = localRotation;

            if (setActive)
                toReturn.gameObject.SetActive(true);

            toReturn.IsRecycled = false;

            grabbed = toReturn;
            
            return true;
        }

        //Enum Recycling
        //============================================================================================================//
        
        public static void RecycleEnum<T>(T recycleType, GameObject toRecycle, bool reparent = true) where T: Enum
        {
            if (_isSetup == false)
                SetupRecycling();

            if (_recycledEnumObjects.TryGetValue(recycleType, out var recyclables) == false)
            {
                recyclables = new Queue<GameObject>();
                _recycledEnumObjects.Add(recycleType, recyclables);
            }

            toRecycle.gameObject.SetActive(false);

            if (reparent)
            {
                toRecycle.transform.SetParent(
                    toRecycle.transform is RectTransform ? _recyclingUIParentTransform : _recyclingParentTransform,
                    false);
            }
            
            recyclables.Enqueue(toRecycle);
        }

        public static bool TryGrabEnum<T>(T recycleType, Transform parent, Vector3 localPosition, Quaternion localRotation, out GameObject grabbed, bool setActive = true) where T : Enum
        {
            grabbed = null;
            
            if (_isSetup == false)
                return false;
            
            if (_recycledEnumObjects.TryGetValue(recycleType, out var recyclables) == false)
                return false;

            if (recyclables.Count == 0)
                return false;

            var toReturn = recyclables.Dequeue();
            toReturn.transform.SetParent(parent);
            toReturn.transform.localPosition = localPosition;
            toReturn.transform.localRotation = localRotation;

            if (setActive)
                toReturn.gameObject.SetActive(true);

            grabbed = toReturn;
            
            return true;
        }
        
        //============================================================================================================//
    }
}