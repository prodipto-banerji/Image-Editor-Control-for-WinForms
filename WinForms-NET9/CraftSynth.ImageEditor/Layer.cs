using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Layer contains a GraphicsList of DrawObjects (SkiaSharp)
    /// </summary>
    public class Layer : IDisposable
    {
        private string _name = string.Empty;
        private bool _isDirty;
        private bool _visible;
        private bool _active;
        private GraphicsList _graphicsList = new GraphicsList();
        private bool _disposed;

        public string LayerName { get => _name; set => _name = value ?? string.Empty; }
        public GraphicsList Graphics { get => _graphicsList; set => _graphicsList = value; }
        public bool IsVisible { get => _visible; set => _visible = value; }
        public bool IsActive { get => _active; set => _active = value; }
        public bool Dirty { get { if (!_isDirty) _isDirty = _graphicsList.Dirty; return _isDirty; } set { _graphicsList.Dirty = false; _isDirty = false; } }

        private const string entryLayerName = "LayerName";
        private const string entryLayerVisible = "LayerVisible";
        private const string entryLayerActive = "LayerActive";
        private const string entryObjectType = "ObjectType";
        private const string entryGraphicsCount = "GraphicsCount";

        public void SaveToStream(SerializationInfo info, int orderNumber)
        {
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}", entryLayerName, orderNumber), _name);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}", entryLayerVisible, orderNumber), _visible);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}", entryLayerActive, orderNumber), _active);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}", entryGraphicsCount, orderNumber), _graphicsList.Count);
            for (int i = 0; i < _graphicsList.Count; i++)
            {
                object o = _graphicsList[i];
                info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryObjectType, orderNumber, i), o.GetType().FullName);
                ((DrawObject)o).SaveToStream(info, orderNumber, i);
            }
        }

        public void LoadFromStream(SerializationInfo info, int orderNumber)
        {
            _graphicsList = new GraphicsList();
            _name = info.GetString(string.Format(CultureInfo.InvariantCulture, "{0}{1}", entryLayerName, orderNumber));
            _visible = info.GetBoolean(string.Format(CultureInfo.InvariantCulture, "{0}{1}", entryLayerVisible, orderNumber));
            _active = info.GetBoolean(string.Format(CultureInfo.InvariantCulture, "{0}{1}", entryLayerActive, orderNumber));
            int n = info.GetInt32(string.Format(CultureInfo.InvariantCulture, "{0}{1}", entryGraphicsCount, orderNumber));
            for (int i = 0; i < n; i++)
            {
                string typeName = info.GetString(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryObjectType, orderNumber, i));
                object drawObject = Assembly.GetExecutingAssembly().CreateInstance(typeName)!;
                ((DrawObject)drawObject).LoadFromStream(info, orderNumber, i);
                _graphicsList.Append((DrawObject)drawObject);
            }
        }

        internal void Draw(SKCanvas canvas)
        {
            _graphicsList.Draw(canvas);
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
                    if (_graphicsList != null)
                    {
                        for (int i = 0; i < _graphicsList.Count; i++)
                        {
                            _graphicsList[i]?.Dispose();
                        }
                    }
                }
                _disposed = true;
            }
        }

        ~Layer() { Dispose(false); }
    }
}
