using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Collection of Layers used to organize the drawing surface (SkiaSharp)
    /// </summary>
    [Serializable]
    public class Layers : ISerializable, IDisposable
    {
        private ArrayList layerList = new ArrayList();
        private bool _isDirty;
        private bool _disposed;

        public bool Dirty
        {
            get
            {
                if (!_isDirty)
                {
                    foreach (Layer l in layerList)
                    {
                        if (l.Dirty) { _isDirty = true; break; }
                    }
                }
                return _isDirty;
            }
        }

        private const string entryCount = "LayerCount";
        private const string entryLayer = "LayerType";

        public Layers() { }

        public int ActiveLayerIndex
        {
            get
            {
                int i = 0;
                foreach (Layer l in layerList)
                {
                    if (l.IsActive) break; i++;
                }
                return i;
            }
        }

        protected Layers(SerializationInfo info, StreamingContext context)
        {
            layerList = new ArrayList();
            int n = info.GetInt32(entryCount);
            for (int i = 0; i < n; i++)
            {
                string typeName = info.GetString(string.Format(CultureInfo.InvariantCulture, "{0}{1}", entryLayer, i));
                object _layer = Assembly.GetExecutingAssembly().CreateInstance(typeName)!;
                ((Layer)_layer).LoadFromStream(info, i);
                layerList.Add(_layer);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(entryCount, layerList.Count);
            int i = 0;
            foreach (Layer l in layerList)
            {
                info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}", entryLayer, i), l.GetType().FullName);
                l.SaveToStream(info, i);
                i++;
            }
        }

        public void Draw(SKCanvas canvas)
        {
            foreach (Layer l in layerList)
            {
                if (l.IsVisible) l.Draw(canvas);
            }
        }

        public bool Clear()
        {
            bool result = (layerList.Count > 0);
            foreach (Layer l in layerList) l.Graphics.Clear();
            if (layerList.Count > 0)
            {
                for (int i = layerList.Count - 1; i >= 0; i--)
                {
                    layerList.RemoveAt(i);
                }
            }
            CreateNewLayer("Default");
            if (result) _isDirty = false;
            return result;
        }

        public int Count => layerList.Count;
        public Layer this[int index]
        {
            get
            {
                if (index < 0 || index >= layerList.Count) return null!;
                return (Layer)layerList[index];
            }
        }

        public void Add(Layer obj)
        {
            layerList.Add(obj);
        }

        public void CreateNewLayer(string name)
        {
            if (layerList.Count > 0) ((Layer)layerList[ActiveLayerIndex]).IsActive = false;
            var l = new Layer { IsVisible = true, IsActive = true, LayerName = name, Graphics = new GraphicsList() };
            Add(l);
        }

        public void InactivateAllLayers()
        {
            foreach (Layer l in layerList)
            {
                l.IsActive = false;
                l.Graphics?.UnselectAll();
            }
        }

        public void MakeLayerInvisible(int p)
        {
            if (p > -1 && p < layerList.Count) ((Layer)layerList[p]).IsVisible = false;
        }

        public void MakeLayerVisible(int p)
        {
            if (p > -1 && p < layerList.Count) ((Layer)layerList[p]).IsVisible = true;
        }

        public void SetActiveLayer(int p)
        {
            if (p > -1 && p < layerList.Count)
            {
                ((Layer)layerList[p]).IsActive = true;
                ((Layer)layerList[p]).IsVisible = true;
            }
        }

        public void RemoveLayer(int p)
        {
            if (layerList.Count == 1) return;
            if (p > -1 && p < layerList.Count)
            {
                ((Layer)layerList[p]).Graphics.Clear();
                layerList.RemoveAt(p);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (Layer layer in layerList) layer?.Dispose();
                }
                _disposed = true;
            }
        }

        ~Layers() { Dispose(false); }
    }
}
