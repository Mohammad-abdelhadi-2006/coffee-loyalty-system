import { api } from './client.js'

export function findByPhone(phone) {
  return api.get(`/customers?phone=${encodeURIComponent(phone)}`)
}

export function createCustomer(fullName, phoneNumber) {
  return api.post('/customers', { fullName, phoneNumber })
}

export function getCustomerOrders(id, limit = 10) {
  return api.get(`/customers/${id}/orders?limit=${limit}`)
}