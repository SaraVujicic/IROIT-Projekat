namespace EmployeeManagement.Application.DTOs;

public record CreateEmployeeDTO(string FirstName, string LastName, string Phone, string Username, string Password, string Role, string Department, int? ManagerId);
public record UpdateEmployeeDTO(string FirstName, string LastName, string Phone, string Department);
public record AssignManagerDTO(int ManagerId);
public record LoginDTO(string Username, string Password);
public record EmployeeResponseDTO(int Id, string FirstName, string LastName, string Phone, string Role, string Department, int? ManagerId);
public record AuthResponseDTO(string Token, int Id, string Role, string FirstName, string LastName);
