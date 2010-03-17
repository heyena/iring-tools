package org.iringtools.client;

import com.google.gwt.core.client.EntryPoint;
import com.google.gwt.core.client.JavaScriptObject;
import com.google.gwt.core.client.JsonUtils;
import com.google.gwt.dom.client.Style.Unit;
import com.google.gwt.event.dom.client.ClickEvent;
import com.google.gwt.event.dom.client.ClickHandler;
import com.google.gwt.http.client.Request;
import com.google.gwt.http.client.RequestBuilder;
import com.google.gwt.http.client.RequestCallback;
import com.google.gwt.http.client.RequestException;
import com.google.gwt.http.client.Response;
import com.google.gwt.http.client.URL;
import com.google.gwt.json.client.JSONArray;
import com.google.gwt.json.client.JSONObject;
import com.google.gwt.user.client.Window;
import com.google.gwt.user.client.ui.Button;
import com.google.gwt.user.client.ui.DockLayoutPanel;
import com.google.gwt.user.client.ui.FlexTable;
import com.google.gwt.user.client.ui.HTML;
import com.google.gwt.user.client.ui.HorizontalPanel;
import com.google.gwt.user.client.ui.Label;
import com.google.gwt.user.client.ui.LayoutPanel;
import com.google.gwt.user.client.ui.ListBox;
import com.google.gwt.user.client.ui.RootLayoutPanel;
import com.google.gwt.user.client.ui.ScrollPanel;
import com.google.gwt.user.client.ui.SimplePanel;
import com.google.gwt.user.client.ui.VerticalPanel;
import com.google.gwt.user.client.ui.Widget;

public class XAppReport implements EntryPoint {
  //private String serviceUri = "http://127.0.0.1:56813/Service.svc/";
  private String serviceUri = "http://www.iringsandbox.org/XAppReportService/Service.svc/";
  private HorizontalPanel mainContentPanel = new HorizontalPanel();
  private ListBox reportNamesListBox = new ListBox();
  private Button goBtn = new Button("Go");
  private SimplePanel waitPanel = new SimplePanel();
  
  public void onModuleLoad() {
    LayoutPanel topPanel = new LayoutPanel();
    topPanel.setStyleName("topPanel");
    HorizontalPanel topContentPanel = new HorizontalPanel();
    topPanel.add(topContentPanel);
    Widget logo = new HTML("<img src='images/iRINGToolsLogo.png' style='height:40px;margin:5px 0 0 5px'/>");
    topContentPanel.add(logo);
    Label appTitle = new Label("X-Application Report");
    appTitle.addStyleName("appTitle");
    topContentPanel.add(appTitle);
    HorizontalPanel promptPanel = new HorizontalPanel();
    promptPanel.addStyleName("promptPanel");
    topContentPanel.add(promptPanel);
    Label promptLabel = new Label("Select a report: ");
    promptLabel.addStyleName("promptLabel");
    promptPanel.add(promptLabel);
    reportNamesListBox.setStyleName("reportNamesListBox");
    promptPanel.add(reportNamesListBox);
    goBtn.addClickHandler(new ClickHandler() {
      public void onClick(ClickEvent event) {
        getReport();
      }
    });
    goBtn.setStyleName("goBtn");
    promptPanel.add(goBtn);
    
    LayoutPanel mainPanel = new LayoutPanel();
    mainPanel.setStyleName("mainPanel");
    ScrollPanel mainScrollPanel = new ScrollPanel();
    mainScrollPanel.setStyleName("mainScrollPanel");
    mainScrollPanel.add(mainContentPanel);
    mainPanel.add(mainScrollPanel);
    waitPanel.setStyleName("waitPanel");
    waitPanel.add(new HTML("<div style='position:absolute;top:50%;left:50%'><img src='images/wait.gif' style='margin:-40px 0 0 -15px'/></div>"));
    mainPanel.add(waitPanel);
    
    DockLayoutPanel appPanel = new DockLayoutPanel(Unit.PX);
    appPanel.setStyleName("appPanel");
    appPanel.addNorth(topPanel, 50);
    appPanel.add(mainPanel);    
    RootLayoutPanel.get().add(appPanel);
    
    getReportNames();
  }
  
  private void startService() {    
    goBtn.setEnabled(false);
    reportNamesListBox.setEnabled(false);
    waitPanel.setVisible(true);
  }
  
  private void endService() {
    goBtn.setEnabled(true);
    reportNamesListBox.setEnabled(true);
    waitPanel.setVisible(false);
  }
  
  private void getReportNames() {
    String url = serviceUri + "list?sid=" + Math.random();
    
    try {
      startService();
      RequestBuilder builder = new RequestBuilder(RequestBuilder.GET, URL.encode(url));
      builder.sendRequest(null, new RequestCallback() {
        public void onError(Request request, Throwable exception) {
          Window.alert("Couldn't connect to server.");
          endService();
        }

        public void onResponseReceived(Request request, Response response) {
          if (response.getStatusCode() == 200) {
            try {
              JavaScriptObject javaScriptObject = JsonUtils.unsafeEval(response.getText());
              JSONArray reportNames = new JSONArray(javaScriptObject);
              
              if (reportNames != null) {
                refreshReportNames(reportNames);
              }
              
              endService();
            } catch (Exception ex) {
              Window.alert(ex.toString());
              endService();
            }
          } else {
            Window.alert("Can't get the status text from response.");
            endService();
          }
        }
      });
    } catch (RequestException ex) {
      Window.alert("Couldn't connect to server.");
      endService();
    } 
  }
  
  private void refreshReportNames(JSONArray reportNames) {
    reportNamesListBox.clear();
    
    for (int i = 0; i < reportNames.size(); i++) {
      reportNamesListBox.addItem(reportNames.get(i).isString().stringValue());
    }
  }

  private void getReport() {
    int selectedIndex = reportNamesListBox.getSelectedIndex(); 
    if (selectedIndex == -1) return;
    
    String reportName = reportNamesListBox.getItemText(selectedIndex);
    String url = serviceUri + "report?name=" + reportName + "&sid=" + Math.random();
    
    try {
      startService();
      RequestBuilder builder = new RequestBuilder(RequestBuilder.GET, URL.encode(url));
      builder.sendRequest(null, new RequestCallback() {
        public void onError(Request request, Throwable exception) {
          Window.alert("Couldn't connect to server.");
          endService();
        }

        public void onResponseReceived(Request request, Response response) {
          if (response.getStatusCode() == 200) {
            try {
              JavaScriptObject javaScriptObject = JsonUtils.unsafeEval(response.getText());
              JSONObject jsonObject = new JSONObject(javaScriptObject);
              
              if (jsonObject != null) {
                JSONArray companies = jsonObject.get("companies").isArray();
                populateReportingData(companies);
              }

              endService();
            } catch (Exception ex) {
              Window.alert(ex.toString());
              endService();
            }
          } else {
            Window.alert("Can't get the status text from response.");
            endService();
          }
        }
      });
    } catch (RequestException ex) {
      Window.alert("Couldn't connect to server.");
      endService();
    } 
  }
  
  private void populateReportingData(JSONArray companies) {    
    mainContentPanel.clear();
    
    for (int i = 0; i < companies.size(); i++) {
      JSONObject company = companies.get(i).isObject();
      String companyName = company.get("name").isString().stringValue();      
      JSONArray properties = company.get("dtoProperties").isArray();
      
      VerticalPanel panel = new VerticalPanel();
      mainContentPanel.add(panel);
      Label label = new Label(companyName);
      panel.add(label);
      FlexTable table = new FlexTable();
      panel.add(table);
      
      if (i == 0) {
        table.setStyleName("firstDtoTable");
      } else {
        table.setStyleName("nextDtoTable");
      }
      
      if (i == 0) {
        label.setStyleName("firstCompanyLabel");
      } else if (i % 2 == 0) {
        label.setStyleName("nextEvenCompanyLabel");
      } else {
        label.setStyleName("nextOddCompanyLabel");
      }      
      
      for (int j = 0; j < properties.size(); j++) {
        JSONObject property = properties.get(j).isObject();
        String propertyName = property.get("label").isString().stringValue();        
        JSONArray values = property.get("values").isArray();  
        
        table.setText(0, j, propertyName);
        table.getCellFormatter().addStyleName(0, j, "dtoTableCell");
        table.getRowFormatter().addStyleName(0, "dtoTableHeader");
        
        for (int k = 0; k < values.size(); k++) {
          String value = values.get(k).isString().stringValue();
          table.setText(k + 1, j, value);
          table.getCellFormatter().addStyleName(k + 1, j, "dtoTableCell");
          
          if (k % 2 == 0) {
            table.getRowFormatter().addStyleName(k + 1, "dtoTableEvenRow");
          } else {
            table.getRowFormatter().addStyleName(k + 1, "dtoTableOddRow");
          }
        }          
      }
    }
  }
}
