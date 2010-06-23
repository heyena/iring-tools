package com.example;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;

@XmlAccessorType(XmlAccessType.FIELD)
@XmlRootElement(name = "Employee")
public class Employee {
  @XmlElement(name = "FirstName")
  protected String firstName;
  
  @XmlElement(name = "LastName")
  protected String lastName;
  
	public String getFirstName()
  {
    return firstName;
  }
  
  public void setFirstName(String firstName)
  {
    this.firstName = firstName;
  }
  
  public String getLastName()
  {
    return lastName;
  }
  
  public void setLastName(String lastName)
  {
    this.lastName = lastName;
  }
}
