using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace dotnetRDFInterfaceService
{
  public partial class _Default : System.Web.Mvc.ViewPage<System.Web.UI.Page>
    {

        protected void btnQuery_Click(object sender, EventArgs e)
        {
//            String query = this.rbSparql.SelectedValue + "?query=" + Uri.EscapeDataString(this.txtQuery.Text) + 
          String query = this.txtDefaultGraph0.Text + "?query=" + Uri.EscapeDataString(this.txtQuery.Text) + 
                "&default-graph-uri=" + Uri.EscapeDataString(this.txtDefaultGraph.Text) + "&timeout=" + 
                Uri.EscapeDataString(this.txtTimeout.Text) + "&partialResults=" + 
                Uri.EscapeDataString(this.chkPartialResults.Checked.ToString());
            Response.Redirect(query);
        }
    }
}
