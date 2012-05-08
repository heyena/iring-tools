package org.iringtools.utility;

import java.util.ArrayList;
import java.util.Collection;
import java.util.Collections;
import java.util.Comparator;
import java.util.Iterator;
import java.util.List;

public class Utility
{

  public static String sqlTypeToCSharpType(String sqlType)
  {

    if (sqlType.equalsIgnoreCase("bit"))
    {
      return "Boolean";
    }
    else if (sqlType.equalsIgnoreCase("byte"))
    {
      return "Byte";
    }
    else if (sqlType.equalsIgnoreCase("char"))
    {
      return "Char";
    }
    else if (sqlType.equalsIgnoreCase("nchar"))
    {
      return "String";
    }
    else if (sqlType.equalsIgnoreCase("character"))
    {
      return "Char";
    }
    else if (sqlType.equalsIgnoreCase("varchar"))
    {
      return "String";
    }
    else if (sqlType.equalsIgnoreCase("varchar2"))
    {
      return "String";
    }
    else if (sqlType.equalsIgnoreCase("nvarchar"))
    {
      return "String";
    }
    else if (sqlType.equalsIgnoreCase("nvarchar2"))
    {
      return "String";
    }
    else if (sqlType.equalsIgnoreCase("text"))
    {
      return "String";
    }
    else if (sqlType.equalsIgnoreCase("ntext"))
    {
      return "String";
    }
    else if (sqlType.equalsIgnoreCase("xml"))
    {
      return "String";
    }
    else if (sqlType.equalsIgnoreCase("date"))
    {
      return "DateTime";
    }
    else if (sqlType.equalsIgnoreCase("datetime"))
    {
      return "DateTime";
    }
    else if (sqlType.equalsIgnoreCase("smalldatetime"))
    {
      return "DateTime";
    }
    else if (sqlType.equalsIgnoreCase("time"))
    {
      return "DateTime";
    }
    else if (sqlType.equalsIgnoreCase("timestamp"))
    {
      return "DateTime";
    }
    else if (sqlType.equalsIgnoreCase("dec"))
    {
      return "Double";
    }
    else if (sqlType.equalsIgnoreCase("decimal"))
    {
      return "Decimal";
    }
    else if (sqlType.equalsIgnoreCase("money"))
    {
      return "Double";
    }
    else if (sqlType.equalsIgnoreCase("smallmoney"))
    {
      return "Double";
    }
    else if (sqlType.equalsIgnoreCase("numeric"))
    {
      return "Double";
    }
    else if (sqlType.equalsIgnoreCase("float"))
    {
      return "Single";
    }
    else if (sqlType.equalsIgnoreCase("real"))
    {
      return "Double";
    }
    else if (sqlType.equalsIgnoreCase("int"))
    {
      return "Int32";
    }
    else if (sqlType.equalsIgnoreCase("integer"))
    {
      return "Int32";
    }
    else if (sqlType.equalsIgnoreCase("bigint"))
    {
      return "Int64";
    }
    else if (sqlType.equalsIgnoreCase("smallint"))
    {
      return "Int16";
    }
    else if (sqlType.equalsIgnoreCase("tinyint"))
    {
      return "Int16";
    }
    else if (sqlType.equalsIgnoreCase("number"))
    {
      return "Decimal";
    }
    else if (sqlType.equalsIgnoreCase("long"))
    {
      return "Int64";
    }
    else if (sqlType.equalsIgnoreCase("clob"))
    {
      return "String";
    }
    else if (sqlType.equalsIgnoreCase("blob"))
    {
      return "String";
    }
    else
    {
      return "String";
    }
  }

  public static String nameSafe(String name)
  {
    return name.replaceAll("^\\d*|\\W", "");
  }

  public static String titleCase(String value)
  {
    String returnValue = "";

    String[] words = value.split(" ");

    for (String word : words)
    {
      returnValue += word.substring(0, 1).toUpperCase();

      if (word.length() > 1)
      {
        returnValue += word.substring(1).toLowerCase();
      }
    }

    return returnValue;
  }
  
  public static <O, T> void searchAndInsert(List<O>  list, O element, T comparer)
  {
  		Comparator<O> comp = (Comparator<O>)comparer;
  		Iterator<O> iterator = list.iterator();
  		int index = 0;
  		boolean found = false;
  		while(iterator.hasNext()) {
  			index++;
  			if(iterator.equals(comp)){
  				found = true;
  				break;
  			}
  		}         	   
  		if (!found)
  		{
  			
  			list.add(index, element);
  		}

  }
  
}
