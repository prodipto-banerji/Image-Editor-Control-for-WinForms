using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// List of graphic objects using SkiaSharp
    /// </summary>
    [Serializable]
    public class GraphicsList : IDisposable
    {
        private ArrayList graphicsList = new ArrayList();
        private bool _isDirty;
        private bool _disposed;

        public bool Dirty
        {
            get
            {
                if (_isDirty == false)
                {
                    foreach (DrawObject o in graphicsList)
                    {
                        if (o.Dirty) { _isDirty = true; break; }
                    }
                }
                return _isDirty;
            }
            set
            {
                foreach (DrawObject o in graphicsList) o.Dirty = false;
                _isDirty = false;
            }
        }

        public IEnumerable<DrawObject> Selection
        {
            get
            {
                foreach (DrawObject o in graphicsList)
                {
                    if (o.Selected) yield return o;
                }
            }
        }

        private const string entryCount = "ObjectCount";
        private const string entryType = "ObjectType";

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
                    if (graphicsList != null)
                    {
                        for (int i = 0; i < graphicsList.Count; i++)
                        {
                            (graphicsList[i] as DrawObject)?.Dispose();
                        }
                    }
                }
                _disposed = true;
            }
        }

        ~GraphicsList() { Dispose(false); }

        public void LoadFromStream(SerializationInfo info, int orderNumber)
        {
            graphicsList = new ArrayList();
            int numberObjects = info.GetInt32(string.Format(CultureInfo.InvariantCulture, "{0}{1}", entryCount, orderNumber));
            for (int i = 0; i < numberObjects; i++)
            {
                string typeName = info.GetString(string.Format(CultureInfo.InvariantCulture, "{0}{1}", entryType, i));
                object drawObject = Assembly.GetExecutingAssembly().CreateInstance(typeName)!;
                ((DrawObject)drawObject).LoadFromStream(info, orderNumber, i);
                graphicsList.Add(drawObject);
            }
        }

        public void SaveToStream(SerializationInfo info, int orderNumber)
        {
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}", entryCount, orderNumber), graphicsList.Count);
            int i = 0;
            foreach (DrawObject o in graphicsList)
            {
                info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}", entryType, i), o.GetType().FullName);
                o.SaveToStream(info, orderNumber, i);
                i++;
            }
        }

        /// <summary>
        /// Draw all objects
        /// </summary>
        public void Draw(SKCanvas canvas)
        {
            int count = graphicsList.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var o = (DrawObject)graphicsList[i];
                // Optionally check intersection vs current canvas bounds, but Skia doesn't expose ClipBounds easily.
                o.Draw(canvas);
                if (o.Selected) o.DrawTracker(canvas);
            }
        }

        public bool Clear()
        {
            bool result = (graphicsList.Count > 0);
            if (graphicsList.Count > 0)
            {
                for (int i = graphicsList.Count - 1; i >= 0; i--)
                {
                    (graphicsList[i] as DrawObject)?.Dispose();
                    graphicsList.RemoveAt(i);
                }
            }
            if (result) _isDirty = false;
            return result;
        }

        public int Count => graphicsList.Count;

        public DrawObject this[int index]
        {
            get
            {
                if (index < 0 || index >= graphicsList.Count) return null!;
                return (DrawObject)graphicsList[index];
            }
        }

        public int SelectionCount
        {
            get
            {
                int n = 0;
                foreach (DrawObject o in graphicsList) if (o.Selected) n++;
                return n;
            }
        }

        public DrawObject GetSelectedObject(int index)
        {
            int n = -1;
            foreach (DrawObject o in graphicsList)
            {
                if (o.Selected)
                {
                    n++;
                    if (n == index) return o;
                }
            }
            return null!;
        }

        public void Add(DrawObject obj)
        {
            graphicsList.Sort();
            foreach (DrawObject o in graphicsList) o.ZOrder++;
            graphicsList.Insert(0, obj);
        }
        public void AddAsInitialGraphic(DrawObject obj) => graphicsList.Add(obj);
        public void Append(DrawObject obj) => graphicsList.Add(obj);

        public void SelectInRectangle(SKRect r)
        {
            UnselectAll();
            foreach (DrawObject o in graphicsList)
            {
                if (o.IntersectsWith(r)) o.Selected = true;
            }
        }

        public void UnselectAll()
        {
            foreach (DrawObject o in graphicsList) o.Selected = false;
        }

        public void SelectAll()
        {
            foreach (DrawObject o in graphicsList) o.Selected = true;
        }

        public bool DeleteSelection()
        {
            bool result = false;
            int n = graphicsList.Count;
            for (int i = n - 1; i >= 0; i--)
            {
                if (((DrawObject)graphicsList[i]).Selected)
                {
                    graphicsList.RemoveAt(i);
                    result = true;
                }
            }
            if (result) _isDirty = true;
            return result;
        }

        public void DeleteLastAddedObject()
        {
            if (graphicsList.Count > 0) graphicsList.RemoveAt(0);
        }

        public void Replace(int index, DrawObject obj)
        {
            if (index >= 0 && index < graphicsList.Count)
            {
                graphicsList.RemoveAt(index);
                graphicsList.Insert(index, obj);
            }
        }

        public void RemoveAt(int index) => graphicsList.RemoveAt(index);

        public bool MoveSelectionToFront()
        {
            ArrayList tempList = new ArrayList();
            int n = graphicsList.Count;
            for (int i = n - 1; i >= 0; i--)
            {
                if (((DrawObject)graphicsList[i]).Selected)
                {
                    tempList.Add(graphicsList[i]);
                    graphicsList.RemoveAt(i);
                }
            }
            n = tempList.Count;
            for (int i = 0; i < n; i++) graphicsList.Insert(0, tempList[i]);
            if (n > 0) _isDirty = true;
            return (n > 0);
        }

        public bool MoveSelectionToBack()
        {
            ArrayList tempList = new ArrayList();
            int n = graphicsList.Count;
            for (int i = n - 1; i >= 0; i--)
            {
                if (((DrawObject)graphicsList[i]).Selected)
                {
                    tempList.Add(graphicsList[i]);
                    graphicsList.RemoveAt(i);
                }
            }
            n = tempList.Count;
            for (int i = n - 1; i >= 0; i--) graphicsList.Add(tempList[i]);
            if (n > 0) _isDirty = true;
            return (n > 0);
        }
    }
}
