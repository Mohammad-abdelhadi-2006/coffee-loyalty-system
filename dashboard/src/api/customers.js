import { api } from './client.js'

// Searches for an existing customer record by their phone number, encoding the parameter to prevent URL injection
export function findByPhone(phone) {
  return api.get(`/customers?phone=${encodeURIComponent(phone)}`)
}

// Sends a POST request to create a new customer entry with their full name and phone number
export function createCustomer(fullName, phoneNumber) {
  return api.post('/customers', { fullName, phoneNumber })
}

// Fetches the order history for a specific customer ID, limiting the returned results (defaults to 10)
export function getCustomerOrders(id, limit = 10) {
  return api.get(`/customers/${id}/orders?limit=${limit}`)
}