import { api } from './client.js'

// Fetches the complete list of registered employees from the backend server
export function getEmployees() {
  return api.get('/employees')
}

// Submits a payload to create a new employee profile in the database
export function createEmployee(data) {
  return api.post('/employees', data)
}

// Updates the active/inactive status flag of a specific employee by their unique ID
export function setEmployeeStatus(id, isActive) {
  return api.patch(`/employees/${id}/status`, { isActive })
}