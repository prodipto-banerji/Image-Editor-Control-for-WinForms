using System;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Layer edit class for managing layer state in dialogs
    /// </summary>
    public class LayerEdit
    {
        private string _name = string.Empty;
        private bool _visible;
        private bool _active;
        private bool _new;
        private bool _deleted;

        /// <summary>
        /// Layer Name
        /// </summary>
        public string LayerName
        {
            get { return _name; }
            set { _name = value ?? string.Empty; }
        }

        /// <summary>
        /// IsVisible is True if this Layer is visible, else False
        /// </summary>
        public bool LayerVisible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        /// <summary>
        /// IsActive is True if this is the active Layer, else False
        /// </summary>
        public bool LayerActive
        {
            get { return _active; }
            set { _active = value; }
        }

        /// <summary>
        /// True if Layer was added in the dialog
        /// </summary>
        public bool LayerNew
        {
            get { return _new; }
            set { _new = value; }
        }

        /// <summary>
        /// True if Layer was deleted in the dialog
        /// </summary>
        public bool LayerDeleted
        {
            get { return _deleted; }
            set { _deleted = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public LayerEdit()
        {
            _name = string.Empty;
            _visible = true;
            _active = false;
            _new = false;
            _deleted = false;
        }

        /// <summary>
        /// Constructor with initial values
        /// </summary>
        /// <param name="name">Layer name</param>
        /// <param name="visible">Layer visibility</param>
        /// <param name="active">Layer active state</param>
        public LayerEdit(string name, bool visible = true, bool active = false)
        {
            _name = name ?? string.Empty;
            _visible = visible;
            _active = active;
            _new = false;
            _deleted = false;
        }

        /// <summary>
        /// Creates a copy of this layer edit instance
        /// </summary>
        /// <returns>A new LayerEdit instance with the same values</returns>
        public LayerEdit Clone()
        {
            return new LayerEdit(_name, _visible, _active)
            {
                LayerNew = _new,
                LayerDeleted = _deleted
            };
        }

        /// <summary>
        /// Returns a string representation of the layer edit
        /// </summary>
        /// <returns>String containing layer information</returns>
        public override string ToString()
        {
            return $"LayerEdit: {_name} (Visible: {_visible}, Active: {_active}, New: {_new}, Deleted: {_deleted})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object
        /// </summary>
        /// <param name="obj">The object to compare with the current object</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object? obj)
        {
            if (obj is LayerEdit other)
            {
                return _name == other._name &&
                       _visible == other._visible &&
                       _active == other._active &&
                       _new == other._new &&
                       _deleted == other._deleted;
            }
            return false;
        }

        /// <summary>
        /// Serves as the default hash function
        /// </summary>
        /// <returns>A hash code for the current object</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(_name, _visible, _active, _new, _deleted);
        }
    }
}