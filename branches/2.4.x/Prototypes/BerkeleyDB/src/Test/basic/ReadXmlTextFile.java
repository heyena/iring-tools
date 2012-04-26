package Test.basic;

import java.io.BufferedReader;
import java.io.DataInputStream;
import java.io.FileInputStream;
import java.io.InputStreamReader;

public class ReadXmlTextFile
{
	String getXmlFromFile(){
		String xmlDoc="";

		try{
			  FileInputStream fstream = new FileInputStream("bin/data/sample.txt");

			  DataInputStream in = new DataInputStream(fstream);
			  BufferedReader br = new BufferedReader(new InputStreamReader(in));
			  String strLine="";
			  while ((strLine = br.readLine()) != null)   {
				  System.out.println(strLine);
				  xmlDoc = xmlDoc + strLine;
			  }
			  
			  in.close();
		}catch (Exception e){
			  System.err.println("Error: " + e.getMessage());
		}
		return xmlDoc;
	}
	
}