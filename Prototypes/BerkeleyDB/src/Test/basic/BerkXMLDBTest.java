package Test.basic;

import com.sleepycat.dbxml.XmlException;
import com.sleepycat.dbxml.XmlManager;
import com.sleepycat.dbxml.XmlContainer;
import com.sleepycat.dbxml.XmlQueryContext;
import com.sleepycat.dbxml.XmlQueryExpression;
import com.sleepycat.dbxml.XmlResults;
import com.sleepycat.dbxml.XmlValue;

class BerkXMLDBTest
{
    XmlContainer myContainer = null;
    XmlManager myManager = null;
    String theContainer = "iring.dbxml";
    String abcXmlContent = null;
    
    BerkXMLDBTest(){
    	try{
    		myManager = new XmlManager();
    	}catch (Exception e) {
    	    
    	}
    }
    
    
	private void cleanup(XmlManager mgr, XmlContainer cont) {
	try {
	    if (cont != null)
		cont.delete();
	    if (mgr != null)
		mgr.delete();
	} catch (Exception e) {
	    
	}
    }

    
    
    public static void main(String args[]) throws Throwable
    {
    	BerkXMLDBTest berkXMLDBTest = new BerkXMLDBTest();
    	//berkXMLDBTest.removeContainer();
    	//berkXMLDBTest.createOpenAndInsert();
    	//berkXMLDBTest.insertContent("abc1.xml",berkXMLDBTest.abcXmlContent);
    	//berkXMLDBTest.viewXmlContent("abc.xml");
    	//erkXMLDBTest.updateXml();
    	berkXMLDBTest.queryXml();
    	
    }
    public void removeContainer(){
    	try{
    		
    		myManager.removeContainer(theContainer);
    	    System.out.println("Container '"+theContainer+"' deleted successfully");

    	}catch (XmlException e) {
	    	e.printStackTrace();
	    } catch (Exception e) {
	    	e.printStackTrace();
	    }finally {
		    cleanup(myManager, myContainer);
		}
    	
    }
    public void createOpenAndInsert(){
	    try {
		    
		    myManager.createContainer(theContainer);
		    
		    myContainer = myManager.openContainer(theContainer);
	    } catch (XmlException e) {
	    	e.printStackTrace();
	    } catch (Exception e) {
	    	e.printStackTrace();
	    }finally {
		    cleanup(myManager, myContainer);
		}
	    abcXmlContent = (new ReadXmlTextFile()).getXmlFromFile();
	    insertContent("abc.xml",abcXmlContent);

    }
    
    public void insertContent(String xmlName, String xmlContent){
	    try {
	    	
	    	myContainer = myManager.openContainer(theContainer);
		    myContainer.putDocument(xmlName, xmlContent);
		    
		    System.out.println("Xml added successfully with name :"+xmlName);

	    }
    	catch (XmlException e) {
	    	e.printStackTrace();
	    } catch (Exception e) {
	    	e.printStackTrace();
	    }finally {
		    cleanup(myManager, myContainer);
		}
    }
    
    public void viewXmlContent(String xmlName){
	    try {
		    
		    myContainer = myManager.openContainer(theContainer);
		    System.out.println(myContainer.getNumDocuments());
		    System.out.println(myContainer.getDocument(xmlName).getContentAsString());
	    } catch (XmlException e) {
	    	e.printStackTrace();
	    } catch (Exception e) {
	    	e.printStackTrace();
	    }finally {
		    cleanup(myManager, myContainer);
		}
    }
    
  
    public void updateXml(){
    	try {
		    myManager = new XmlManager();
		    
		    myContainer = myManager.openContainer(theContainer);
		   	
		    //Insert Node in Between
		    //General Format : insert nodes [nodes] [keyword] [position]
		    //String query="insert nodes <city>New Delhi</city> after doc('dbxml:/iring.dbxml/abc.xml')/abc/scope[name='example 1']/type";
		   	
		    //Rename Node
		    //String query = "rename node doc('dbxml:/iring.dbxml/abc.xml')/abc/scope[name='example 1']/city as 'cityName'";
		    
		    //Replace Node as well as content
		    //String query="replace node doc('dbxml:/iring.dbxml/abc.xml')/abc/scope[name='example 1']/cityName with <country>India</country>";
		    
		    String query="replace value of node doc('dbxml:/iring.dbxml/abc.xml')/abc/scope[country='India']/country with 'Spain'";
		    
		    //Delete Node
		    //String query="delete nodes doc('dbxml:/iring.dbxml/abc.xml')/abc/scope[name='example 1']/country";
		   	
		    XmlQueryContext qc = myManager.createQueryContext();
		    
		    XmlQueryExpression expr = myManager.prepare(query, qc);
		    expr.execute(qc);

		    expr.delete();
	    	qc=null;
    	} catch (XmlException e) {
	    	e.printStackTrace();
	    } catch (Exception e) {
	    	e.printStackTrace();
	    }finally {
		    cleanup(myManager, myContainer);
		}
	    viewXmlContent("abc.xml");
    }
    
    
    public void queryXml(){
    	try {
		    myManager = new XmlManager();
		    
		    myContainer = myManager.openContainer(theContainer);
		    //To retrieve something from the entire DB
		    String query ="collection('iring.dbxml')/abc/scope/name";
		    
		    //To retrieve some value from an the entire DB
		    //String query ="collection('iring.dbxml')/abc/scope[name='example 1']/type/string()";
		   
		    //To retrieve the entire xml document
		    //String query = "doc('dbxml:/iring.dbxml/abc.xml')";
		   
		    //To retrieve some value from an xml
		    //String query ="doc('dbxml:/iring.dbxml/abc.xml')/abc/scope[name='example 1']/type";
		    
		   
		    XmlQueryContext qc = myManager.createQueryContext();
		    
		    
		    XmlQueryExpression expr = myManager.prepare(query, qc);
		    XmlResults res = expr.execute(qc);
		    
		    XmlValue value = new XmlValue();
		    while ((value = res.next()) != null) {
		    	System.out.println(value.asString());
		    }

		    res.delete();
		    expr.delete();
	    } catch (XmlException e) {
	    	e.printStackTrace();
	    } catch (Exception e) {
	    	e.printStackTrace();
	    }finally {
		    cleanup(myManager, myContainer);
		}
    }
}

