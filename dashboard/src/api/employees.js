import { api } from './client.js'

export function getEmployees() {
  return api.get('/employees')
}

export function createEmployee(data) {
  return api.post('/employees', data)
}

export function setEmployeeStatus(id, isActive) {
  return api.patch(`/employees/${id}/status`, { isActive })
}