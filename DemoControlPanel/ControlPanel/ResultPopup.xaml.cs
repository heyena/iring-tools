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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Browser;

namespace ControlPanel
{
  public partial class ResultPopup : UserControl
  {
    ModalClass _modal;
    //bool isMouseCaptured;
    //double mouseVerticalPosition;
    //double mouseHorizontalPosition;

    public ResultPopup()
    {
      InitializeComponent();
      _modal = new MyDialog();
      _modal.Show(DialogStyle.ModalDimmed);
    }

    private void btnCloseResults_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Collapsed;
      if (_modal != null)
      {
        _modal.Close();
      }
    }

    private void btnSaveResults_Click(object sender, RoutedEventArgs e)
    {

      
      string myTextFile = this.tblResults.Text;
      HtmlDocument doc = HtmlPage.Document;
      HtmlElement downloadData = doc.GetElementById("downloadData");
      downloadData.SetAttribute("value", myTextFile);

      HtmlElement fileName = doc.GetElementById("fileName");
      fileName.SetAttribute("value", "FileToSAve.txt"); 
      doc.Submit("generateFileForm");
      this.Visibility = Visibility.Collapsed;
      
    }
    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
      UserControl item = sender as UserControl;
      //if (isMouseCaptured)
      //{
       
      //  // Calculate the current position of the object.
      //  double deltaV = e.GetPosition(null).Y - mouseVerticalPosition;
      //  double deltaH = e.GetPosition(null).X - mouseHorizontalPosition;
      //  double newTop = deltaV + (double)item.GetValue(Canvas.TopProperty);
      //  double newLeft = deltaH + (double)item.GetValue(Canvas.LeftProperty);

      //  // Set new position of object.
      //  item.SetValue(Canvas.TopProperty, newTop);
      //  item.SetValue(Canvas.LeftProperty, newLeft);

      //  // Update position global variables.
      //  mouseVerticalPosition = e.GetPosition(null).Y;
      //  mouseHorizontalPosition = e.GetPosition(null).X;
      //}
    }
  }
}
