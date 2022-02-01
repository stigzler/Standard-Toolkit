﻿#region BSD License
/*
 * 
 * Original BSD 3-Clause License (https://github.com/ComponentFactory/Krypton/blob/master/LICENSE)
 *  © Component Factory Pty Ltd, 2006 - 2016, (Version 4.5.0.0) All rights reserved.
 * 
 *  New BSD 3-Clause License (https://github.com/Krypton-Suite/Standard-Toolkit/blob/master/LICENSE)
 *  Modifications by Peter Wagner(aka Wagnerp) & Simon Coghlan(aka Smurf-IV), et al. 2017 - 2022. All rights reserved. 
 *  
 */
#endregion


namespace Krypton.Toolkit
{
    internal class KryptonNumericUpDownActionList : DesignerActionList
    {
        #region Instance Fields
        private readonly KryptonNumericUpDown _numericUpDown;
        private readonly IComponentChangeService _service;
        #endregion

        #region Identity
        /// <summary>
        /// Initialize a new instance of the KryptonNumericUpDownActionList class.
        /// </summary>
        /// <param name="owner">Designer that owns this action list instance.</param>
        public KryptonNumericUpDownActionList(KryptonNumericUpDownDesigner owner)
            : base(owner.Component)
        {
            // Remember the text box instance
            _numericUpDown = owner.Component as KryptonNumericUpDown;

            // Cache service used to notify when a property has changed
            _service = (IComponentChangeService)GetService(typeof(IComponentChangeService));
        }
        #endregion

        #region Public
        /// <summary>Gets or sets the context menu strip.</summary>
        /// <value>The context menu strip.</value>
        public ContextMenuStrip ContextMenuStrip
        {
            get => _numericUpDown.ContextMenuStrip;

            set
            {
                if (_numericUpDown.ContextMenuStrip != value)
                {
                    _service.OnComponentChanged(_numericUpDown, null, _numericUpDown.ContextMenuStrip, value);

                    _numericUpDown.ContextMenuStrip = value;
                }
            }
        }

        /// <summary>
        /// Gets and sets the palette mode.
        /// </summary>
        public PaletteMode PaletteMode
        {
            get => _numericUpDown.PaletteMode;

            set
            {
                if (_numericUpDown.PaletteMode != value)
                {
                    _service.OnComponentChanged(_numericUpDown, null, _numericUpDown.PaletteMode, value);
                    _numericUpDown.PaletteMode = value;
                }
            }
        }

        /// <summary>
        /// Gets and sets the input control style.
        /// </summary>
        public InputControlStyle InputControlStyle
        {
            get => _numericUpDown.InputControlStyle;

            set
            {
                if (_numericUpDown.InputControlStyle != value)
                {
                    _service.OnComponentChanged(_numericUpDown, null, _numericUpDown.InputControlStyle, value);
                    _numericUpDown.InputControlStyle = value;
                }
            }
        }

        /// <summary>
        /// Gets and sets the increment value of the control.
        /// </summary>
        public decimal Increment
        {
            get => _numericUpDown.Increment;

            set
            {
                if (_numericUpDown.Increment != value)
                {
                    _service.OnComponentChanged(_numericUpDown, null, _numericUpDown.Increment, value);
                    _numericUpDown.Increment = value;
                }
            }
        }

        /// <summary>
        /// Gets and sets the increment value of the control.
        /// </summary>
        public decimal Maximum
        {
            get => _numericUpDown.Maximum;

            set
            {
                if (_numericUpDown.Maximum != value)
                {
                    _service.OnComponentChanged(_numericUpDown, null, _numericUpDown.Maximum, value);
                    _numericUpDown.Maximum = value;
                }
            }
        }

        /// <summary>
        /// Gets and sets the increment value of the control.
        /// </summary>
        public decimal Minimum
        {
            get => _numericUpDown.Minimum;

            set
            {
                if (_numericUpDown.Minimum != value)
                {
                    _service.OnComponentChanged(_numericUpDown, null, _numericUpDown.Minimum, value);
                    _numericUpDown.Minimum = value;
                }
            }
        }

        /// <summary>Gets or sets the font.</summary>
        /// <value>The font.</value>
        public Font Font
        {
            get => _numericUpDown.StateCommon.Content.Font;

            set
            {
                if (_numericUpDown.StateCommon.Content.Font != value)
                {
                    _service.OnComponentChanged(_numericUpDown, null, _numericUpDown.StateCommon.Content.Font, value);

                    _numericUpDown.StateCommon.Content.Font = value;
                }
            }
        }

        /// <summary>Gets or sets the corner radius.</summary>
        /// <value>The corner radius.</value>
        [DefaultValue(GlobalStaticValues.PRIMARY_CORNER_ROUNDING_VALUE)]
        public float CornerRadius
        {
            get => _numericUpDown.StateCommon.Border.Rounding;

            set
            {
                if (_numericUpDown.StateCommon.Border.Rounding != value)
                {
                    _service.OnComponentChanged(_numericUpDown, null, _numericUpDown.StateCommon.Border.Rounding, value);

                    _numericUpDown.StateCommon.Border.Rounding = value;
                }
            }
        }
        #endregion

        #region Public Override
        /// <summary>
        /// Returns the collection of DesignerActionItem objects contained in the list.
        /// </summary>
        /// <returns>A DesignerActionItem array that contains the items in this list.</returns>
        public override DesignerActionItemCollection GetSortedActionItems()
        {
            // Create a new collection for holding the single item we want to create
            DesignerActionItemCollection actions = new();

            // This can be null when deleting a control instance at design time
            if (_numericUpDown != null)
            {
                // Add the list of label specific actions
                actions.Add(new DesignerActionHeaderItem(@"Appearance"));
                actions.Add(new DesignerActionPropertyItem(@"ContextMenuStrip", @"Context Menu Strip", @"Appearance", @"The context menu strip for the control."));
                actions.Add(new DesignerActionPropertyItem(@"InputControlStyle", @"Style", @"Appearance", @"NumericUpDown display style."));
                actions.Add(new DesignerActionPropertyItem(@"Font", @"Font", @"Appearance", @"The numeric up down font."));
                actions.Add(new DesignerActionPropertyItem(@"CornerRadius", @"Corner Rounding Radius", @"Appearance", @"The corner rounding radius of the control."));
                actions.Add(new DesignerActionHeaderItem(@"Data"));
                actions.Add(new DesignerActionPropertyItem(@"Increment", @"Increment", @"Data", @"NumericUpDown increment value."));
                actions.Add(new DesignerActionPropertyItem(@"Maximum", @"Maximum", @"Data", @"NumericUpDown maximum value."));
                actions.Add(new DesignerActionPropertyItem(@"Minimum", @"Minimum", @"Data", @"NumericUpDown minimum value."));
                actions.Add(new DesignerActionHeaderItem(@"Visuals"));
                actions.Add(new DesignerActionPropertyItem(@"PaletteMode", @"Palette", @"Visuals", @"Palette applied to drawing"));
            }

            return actions;
        }
        #endregion
    }
}