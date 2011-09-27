import java.io.IOException;
import java.net.UnknownHostException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;

import javax.xml.bind.JAXBException;

import org.bson.BSON;
import org.bson.BSONObject;
import org.iringtools.directory.Application;
import org.iringtools.directory.Commodity;
import org.iringtools.directory.DataExchanges;
import org.iringtools.directory.Directory;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.Exchanges;
import org.iringtools.directory.Graph;
import org.iringtools.directory.Scope;
import org.iringtools.utility.JaxbUtils;

import com.mongodb.BasicDBObject;
import com.mongodb.DB;
import com.mongodb.DBCollection;
import com.mongodb.DBCursor;
import com.mongodb.DBObject;
import com.mongodb.Mongo;
import com.mongodb.MongoException;
import com.mongodb.WriteResult;

public class MongoTest {

	private BasicDBObject doc = null;

	private DBCollection collection = null;
	
	private DB db = null;
	
	private void buildConnection(String user,String pwd){

		try {
			Mongo mongo = new Mongo("localhost", 27017);
			// Select DB
			this.db = mongo.getDB("iRING");
			
			/*
			 * Will only works if the mongo server is running with --auth option
			 * command 
			 * c:/mongo/bin>mongod --auth --config "d:\mongo\bin\mongodb.config"
			 * --config required to write data into the specified path 
			 **/
			
			boolean auth = db.authenticate(user, pwd.toCharArray());
 			
			if(auth)
 			{
				// select the collection
 				this.collection = db.getCollection("directory");
 				
 				System.out.println("\nDatabase connected successfully  ["+ collection.getName() +"]");
 				
 			}else{
 				
 				System.out.println("\nYou are not authorized to access "+db);
 			}
			
		} catch (UnknownHostException e) {
			e.printStackTrace();
		} catch (MongoException e) {
			e.printStackTrace();
		}
	}
	
	public MongoTest(){
		
		buildConnection("admin","admin");
	}
	
	public Directory readDirectoryXML(String path){
		
		Directory directory = null;
		try {
			
			 directory = JaxbUtils.read(Directory.class, path);
			
		} catch (JAXBException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} 	
		 return directory;
		
	}
	
	public DBObject createDirectoryObject(Directory directory){
		
		//String jsonStr = null;

		DBObject dbobject = new BasicDBObject();
		
		if(directory.getItems().size()>0){

			Map directoryMap = new HashMap();
			ArrayList dir = new ArrayList();
			
			
			List<Scope> scopeList = directory.getItems();
			
			
			if(scopeList.size()>0){

				Map<String, Object> scopecollection = null;
				Map<String, Object> scopemap = null;
				Map<String, Object> dataxchangemap = null;

				for(Scope scope:scopeList ){
					
					scopemap = new HashMap<String, Object>();
					scopemap.put("name", scope.getName());

					
					List<Application> applicationList = scope.getApplicationData().getItems();
					if((applicationList.size()>0)){
						
						ArrayList apparr = new ArrayList();
						
						for(Application application:applicationList ){
							
							Map<String, Object> appmap = new HashMap<String, Object>();
							appmap.put("name", application.getName());
							appmap.put("id", application.getId());
							appmap.put("description",application.getDescription());
							appmap.put("baseUri", application.getBaseUri());
							
							ArrayList grapharr = new ArrayList();
							List<Graph> graphList = application.getGraphs().getItems();
							
							if(graphList.size()>0){
								for(Graph graph : graphList ){
									Map<String, Object> graphmap = new HashMap<String, Object>();
									graphmap.put("id", graph.getId());
									graphmap.put("name", graph.getName());
									graphmap.put("description", graph.getDescription());
									graphmap.put("commodity",graph.getCommodity());
									Map<String, Object> graphcollection = new HashMap<String, Object>();
									graphcollection.put("graph", graphmap);
									grapharr.add(graphcollection);
								}
							}
							
							appmap.put("graphs",grapharr);
							Map<String, Object> applicationcollection = new HashMap<String, Object>();
							applicationcollection.put("application", appmap);
							apparr.add(applicationcollection);
							
						}
						scopemap.put("applicationData", apparr);
						scopecollection = new HashMap<String, Object>();

						
						dataxchangemap = new HashMap<String, Object>();
						
						DataExchanges dataxchanges = scope.getDataExchanges();
						
						if(dataxchanges!=null){
							
							List<Commodity> commodityList = scope.getDataExchanges().getItems();
							Map<String, Object> commoditymap = null;
							
							for(Commodity commodity : commodityList){
								commoditymap = new HashMap<String, Object>();
								commoditymap.put("name", commodity.getName());
								
								Exchanges exchanges = commodity.getExchanges();
								
								if(exchanges!=null)
								{
									List<Exchange> exchangeList =exchanges.getItems();
									
									ArrayList exchangearr = new ArrayList();
									
									Map<String, Object> exchangemap = null;
									
									for(Exchange exchange : exchangeList){
										
										exchangemap = new HashMap<String, Object>();
										exchangemap.put("id",exchange.getId());
										exchangemap.put("name",exchange.getName());
										exchangemap.put("description",exchange.getDescription());
										exchangearr.add(exchangemap);
									}
									commoditymap.put("exchanges",exchangearr);	
								}
								
								
								dataxchangemap.put("commodity", commoditymap);
							}
							
							scopecollection.put("dataExchanges", dataxchangemap);
						}
						scopecollection.put("scope", scopemap);
					}
					dir.add(scopecollection);
				}

				
			}
			
			//System.out.println(directoryMap);
			
			directoryMap.put("directory", dir);
			
			dbobject.putAll(directoryMap);
			
			//System.out.println(dbobject);
			
			
		}

		return dbobject;
	}
	
	public Set<String> getCollections(){
		
		return db.getCollectionNames();
	}

	public long getRowCount(){
	
		return collection.count();
	}

	private DBCursor getCollectionsRecords(){
		
		DBCursor cursor = null;
		
		if(collection!=null){
			
			cursor = collection.find();
		}
		return cursor;
	}

	private long insertdbobject(DBObject dbObject){
		System.out.println("\n\n\t"+dbObject);
		//DBObject db = BasicDBObjectBuilder.start().add("project", document).get();
		WriteResult wr = collection.insert(dbObject);
		return wr.getN();
	} 
	
	private void getFilterRecords(String scopeName,String appName){
		
		System.out.println("\nGet Application Data where scope: "+scopeName+" app name : "+appName+"\n");
		
		DBObject query = new BasicDBObject();
		query.put("directory.scope.name", scopeName);
		query.put("directory.scope.applicationData.application.name",appName);
		
		DBObject fields = new BasicDBObject();
		fields.put("_id", 0);
		//fields.put("directory.scope.applicationData.application",1);
		//fields.put("directory.scope.name",1);
		//fields.put("directory.scope.applicationData.application",1);
		//fields.put("directory.scope.applicationData.application.graphs",1);
		fields.put("directory.scope.applicationData.application",1);
		//fields.put("directory.scope.appliocationData.application.graphs",1);
		
		//System.out.println(fields);
		//db.directory.findOne({"directory.scope.name":"44444","directory.scope.applicationData.application.name" : "CM"},{"_id" : 0 , "directory.scope.applicationData.application.graphs" : 1,"directory.scope.applicationData.application.id":1,"directory.scope.applicationData.application.name":1})
		//"directory.scope.applicationData.application.graphs" : 1,"directory.scope.applicationData.application.id":1,"directory.scope.applicationData.application.name":1})
		
		// To read all records
		/*DBCursor cursor = collection.find(query, fields, 0, 0);
		while(cursor.hasNext()){
			
			System.out.println(cursor.next());
		}*/
		
		DBObject application = collection.findOne(query, fields);
		
		// You have to filter it manually on the client side. which is bad
		if(application!=null){

			/*System.out.println(dboo.keySet());*/
			//System.out.println("return map: "+application.toMap());
			
			System.out.println(application);
		}
		/*
		System.out.println("match : "+query+"\n");
		System.out.println("fields : "+fields+"\n");
		*/
		
		//find({"directory.scope.name" : "44444" , "directory.scope.applicationData.application.name" : "ABC"},{"_id" : 0 , "directory.scope.applicationData.application" : 1},0,0)
		//find({"directory.scope.name" : "44444"},{$pop:{"directory.scope.applicationData.application":1}})
		// db.media.update({"Title":"One Piece"},{$set:{"Title":"my Title"}})
	}

	private void updateScopeName(String OldName,String ScopeName){
		
		DBObject query = new BasicDBObject();
		query.put("directory.scope.name", OldName);
		
		DBObject query1 = new BasicDBObject();
		query1.put("directory.scope.name", ScopeName);
	
		try{
			
			WriteResult wr = collection.update(query,query1,false,false);
			
			System.out.println("Errrro"+wr.getError());
			
		}catch(Exception ex){
			
			ex.printStackTrace();

		}
		
                
	}

	public static void main(String args[]){
		
		MongoTest testObj = new MongoTest();
		
		if(testObj.getRowCount()==0){
			
			Directory directory = testObj.readDirectoryXML("C:\\iringtools3.0\\services\\WebContent\\WEB-INF\\data\\directory.xml");
			
			DBObject dbobject = testObj.createDirectoryObject(directory);
			
			testObj.insertdbobject(dbobject);
			
		}else{
		
			DBCursor cursor = testObj.getCollectionsRecords();
			
			//System.out.println("Total records : "+cursor.count());

			while(cursor.hasNext()){
				
				System.out.println("\n\t"+cursor.next());
			}
		}
		
		// To Get The Filter Data
		// testObj.getFilterRecords("12345_000","ABC");

		// To update record
		   testObj.updateScopeName("12345_000","1111111");
		

		
			
		
	}

}