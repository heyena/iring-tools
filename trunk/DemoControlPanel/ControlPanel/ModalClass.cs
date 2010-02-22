// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace ControlPanel
{
    public abstract class ModalClass
    {
    public void Show(DialogStyle style)

        {
            if (_isShowing)
                throw new InvalidOperationException();
            _isShowing = true;
            EnsurePopup(style);
            _popup.IsOpen = true;
            Application.Current.Host.Content.Resized += OnPluginSizeChanged;
        }
        public void Close()
        {
            _isShowing = false;
            if (_popup != null)
            {
                _popup.IsOpen = false;
                Application.Current.Host.Content.Resized -= OnPluginSizeChanged;
            }
        }       

        // Override this method to add your content to the dialog
        protected abstract FrameworkElement GetContent();
        // Override this method if you want to do something (e.g. call Close) when you click 
        // outside of the content
        protected virtual void OnClickOutside() { }

        // A Grid is the child of the Popup. If it is modal, it will contain a Canvas, which
        // will be sized to fill the plugin and prevent mouse interaction with the elements
        // outside of the popup. (Keyboard interaction is still possible, but hopefully when
        // Silverlight 2 RTMs, you can disable the root to take care of that.) The Grid isn't
        // strictly needed if there is always a Canvas, but it is handy for centering the content.
        //
        // The other child of the Grid is the content of the popup. This is obtained from the
        // GetContent method.
        private void EnsurePopup(DialogStyle style)
        {
            if (_popup != null)
                return;
            _popup = new Popup();
            _grid = new Grid();
            _popup.Child = _grid;
            if (style != DialogStyle.NonModal)
            {
                // If Canvas.Background != null, you cannot click through it
                _canvas = new Canvas();
                _canvas.MouseLeftButtonDown += (sender, args) => { OnClickOutside(); };
                if (style == DialogStyle.Modal)
                {
                    _canvas.Background = new SolidColorBrush(Colors.Transparent);
                }
                else if (style == DialogStyle.ModalDimmed)
                {

                    _canvas.Background = new SolidColorBrush(Color.FromArgb(0x20, 0x80, 0x80, 0x80));
                }
                _grid.Children.Add(_canvas);
            }
            _grid.Children.Add(_content = GetContent());
            UpdateSize();
        } 

        private void OnPluginSizeChanged(object sender, EventArgs e)
        {
            UpdateSize();
        }

        private void UpdateSize()
        {

            _grid.Width  = Application.Current.Host.Content.ActualWidth;
            _grid.Height = Application.Current.Host.Content.ActualHeight; 

            if (_canvas != null)
            {

                _canvas.Width = _grid.Width;
                _canvas.Height = _grid.Height;
            }
        }
        private bool _isShowing;
        private Popup _popup;
        private Grid _grid;
        private Canvas _canvas;
        private FrameworkElement _content;
    }

    public enum DialogStyle
    {
        NonModal,
        Modal,
        ModalDimmed

    };

}


