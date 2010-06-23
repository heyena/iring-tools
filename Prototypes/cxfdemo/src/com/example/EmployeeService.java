package com.example;

import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;

@Path("/")
public class EmployeeService
{
  @GET
  @Path("/get")
  @Produces("application/xml")
  public Employee getEmployee()
  {
    Employee employee = new Employee();
    employee.setFirstName("Hahn");
    employee.setLastName("Le");
    return employee;
  }
  
  @GET
  @Path("/echo/{firstName}/{lastName}")
  @Produces("application/xml")
  public Employee echoEmployee(@PathParam("firstName") String firstName, @PathParam("lastName") String lastName)
  {
    Employee employee = new Employee();
    employee.setFirstName(firstName);
    employee.setLastName(lastName);
    return employee;
  }
}
