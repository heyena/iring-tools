package com.example;

public class EmployeeService
{
  public Employee getEmployee()
  {
    Employee employee = new Employee();
    employee.setFirstName("Hahn");
    employee.setLastName("Le");
    return employee;
  }
  
  public Employee echoEmployee(String firstName, String lastName)
  {
    Employee employee = new Employee();
    employee.setFirstName(firstName);
    employee.setLastName(lastName);
    return employee;
  }
}
